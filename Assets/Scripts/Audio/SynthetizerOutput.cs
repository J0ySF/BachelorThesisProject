using System;
using AlphaTab;
using AlphaTab.Core.EcmaScript;
using AlphaTab.Synth;
using UnityEngine;
using Array = System.Array;

namespace Audio
{
    /// <summary>
    /// Bridges between AlphaTab's audio synthesis and Unity's audio system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implements MonoBehaviour to receive the <c>Start</c> and <c>OnAudioFilterRead</c> events.
    /// </para>
    /// <para>
    /// Implements ISynthOutput to be able to act as an AlphaTab synthesizer output.
    /// </para>
    /// <para>
    /// This class supposes that the project settings' audio channel count are set to 2.
    /// </para>
    /// <para>
    /// No <c>ISynthOutput</c> functions should be called before the <c>Start</c> event is called, and the
    /// <c>ISynthOutput</c> specific functions are best left to AlphaTab to handle.
    /// </para>
    /// </remarks>
    public sealed class SynthesizerOutput : MonoBehaviour, ISynthOutput
    {
        /// <summary>AlphaTab hardcoded value which refers to the synthesizer's sample buffer size.</summary>
        private const int SynthesizerBufferSize = 4096;

        /// <summary>AlphaTab hardcoded value which refers to the synthesizer's audio channel count.</summary>
        private const int SynthesizerChannelCount = 2;

        /// <summary>Refers to the audio output's sample rate.</summary>
        /// <para>Set during the <c>Start</c> Event to the project settings' val</para>
        public double SampleRate { get; private set; }

        /// <summary>Values representing the Unity DSP buffer size for this process' running istance.</summary>
        private int _dspSegmentLenght;

        /// <summary>Values representing the amount of Unity DSP buffers that fit in a single
        /// <c>SynthesizerBufferSize</c>.</summary>
        private int _dspSegmentsCount;

        // State variables

        /// <summary>Stores the DPS data arriving from AlphaTab.</summary>
        private float[] _buffer;

        /// <summary>Keeps track of which buffer segment is being read from.</summary>
        private int _bufferSegmentIndex;

        /// <summary>True if currently playing.</summary>
        private bool _playing;

        /// <summary>Used to make state access from AlphaSynth callbacks mutually exclusive to the Unity callbacks.
        /// <para>This is needed because the OnAudioFilterRead events are called on a separate thread than the Unity
        /// main thread.</para></summary>
        private readonly object _stateLock = new();

        private void Start()
        {
            // Get all relevant Unity audio parameters 
            SampleRate = AudioSettings.outputSampleRate;
            AudioSettings.GetDSPBufferSize(out _dspSegmentLenght, out var _);
            _dspSegmentLenght *= SynthesizerChannelCount;

            // Both valid assumptions given that DSP buffer size are usually power of 2s and the SynthesizerBufferSize
            // should be larger than the Unity DSP buffer size, otherwise there would be a more input lag than
            // anticipated.
            Debug.Assert(SynthesizerBufferSize >= _dspSegmentLenght);
            Debug.Assert(SynthesizerBufferSize % _dspSegmentLenght == 0);
            _dspSegmentsCount = SynthesizerBufferSize / _dspSegmentLenght;

            // Inizialize the audio buffering data structure.
            _buffer = new float[SynthesizerBufferSize];
            _bufferSegmentIndex = _dspSegmentsCount;
        }

        private readonly EventEmitter _ready = new();
        private readonly EventEmitterOfT<double> _samplesPlayed = new();
        private readonly EventEmitter _sampleRequest = new();

        /// <summary>Event emitter that tells AlphaTab that the output is ready for playback.</summary>
        public IEventEmitter Ready => _ready;

        /// <summary>Event emitter that tells AlphaTab that a number of samples have just been played.</summary>
        public IEventEmitterOfT<double> SamplesPlayed => _samplesPlayed;

        /// <summary>Event emitter that tells AlphaTab that the want to receive new samples.</summary>
        public IEventEmitter SampleRequest => _sampleRequest;

        /// <summary>Meant to be called by AlphaTab to add audio samples.</summary>
        public void AddSamples(Float32Array f)
        {
            lock (_stateLock) // Lock the state to avoid audio glitches due to race conditions.
            {
                // Copy all the samples into the segmented buffer
                Buffer.BlockCopy(f.Data, 0, _buffer, 0, f.Data.Length * sizeof(float));
                _bufferSegmentIndex = 0;
            }
        }

        /// <summary>Required by the interface but not used.</summary>
        public void Activate()
        {
        }

        /// <summary>Opens the audio output.</summary>
        public void Open(double _) => _ready.Trigger();


        /// <summary>Clears the buffer to avoid accidental playback of previously stored values.</summary>
        public void ResetSamples()
        {
            lock (_stateLock) // Lock the state to avoid audio glitches due to race conditions.
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _bufferSegmentIndex = _dspSegmentsCount;
                if (_playing) _sampleRequest.Trigger();
            }
        }

        /// <summary>Starts the audio output.</summary>
        public void Play()
        {
            lock (_stateLock) // Lock the state to avoid audio glitches due to race conditions.
            {
                _playing = true;
                _sampleRequest.Trigger();
            }
        }

        /// <summary>Pauses the audio output.</summary>
        public void Pause()
        {
            lock (_stateLock) // Lock the state to avoid audio glitches due to race conditions.
            {
                _playing = false;
            }
        }

        /// <summary>Called by Unity when new audio samples are needed.</summary>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            Debug.Assert(channels == SynthesizerChannelCount);

            lock (_stateLock) // Lock the state to avoid audio glitches due to race conditions.
            {
                // Do not play if not samples are available or if not playing. 
                if (!_playing || _bufferSegmentIndex == _dspSegmentsCount) return;

                // Write a samples segment into the Unity DSP buffer and shift the current segments window.
                Buffer.BlockCopy(
                    _buffer, _bufferSegmentIndex++ * _dspSegmentLenght * sizeof(float),
                    data, 0,
                    data.Length * sizeof(float)   
                );

                // Notify that we just played a DSP window size amount of samples.
                _samplesPlayed.Trigger((int)((float)_dspSegmentLenght / SynthesizerChannelCount));

                if (_bufferSegmentIndex == _dspSegmentsCount) // Reached the end of the buffer
                {
                    // Clear the buffer and request a new batch of samples.
                    _sampleRequest.Trigger();
                }
            }
        }

        /// <summary>Required by the interface but not used.</summary>
        public void Destroy()
        {
        }
    }
}