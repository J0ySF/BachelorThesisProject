using AlphaTab.Synth;
using Loading;
using Score;

namespace Player.Components
{
    /// <summary>
    /// Controls the audio playback via AlphaTab's <c>AlphaSynth</c> class.
    /// </summary>
    public sealed class Synthesizer : State.Component
    {
        private readonly AlphaSynth _synth;

        /// <summary>The currently loaded score's audio instrument channel.</summary>
        private int _instrumentChannel;

        /// <summary>The currently loaded score's beatmap.</summary>
        private BeatMap _beatMap;

        /// <summary>Signal from <c>AlphaSynth</c>'s separate thread which indicates that the the audio playback has
        /// reached its end (which corresponds to the end of the score).</summary>
        private bool _pollFinished;

        private int _countInTarget, _countInOffset;


        public Synthesizer(State state, ISynthOutput synthOutput) : base(state)
        {
            // Set up the AlphaSynth instance and play end event.
            _synth = new AlphaSynth(synthOutput, 0.0);
            _synth.LoadSoundFont(FileSystem.LoadSoundFontData(), false);
            _synth.Finished.On(() => _pollFinished = true);
        }

        protected override void OnPlaybackUpdate(Playback v)
        {
            // Clear any old flag
            _pollFinished = false;

            // Change playing state
            if (v is Playback.Playing or Playback.CountingIn)
            {
                _synth.TickPosition = State.Tick;
                if (v is Playback.CountingIn)
                {
                    _countInTarget = State.Tick;
                    _countInOffset = (int)AlphaTab.Midi.MidiUtils.QuarterTime *
                                     _beatMap.LastBeatBeforeOrAtTick(State.Tick)
                                         .TimeSignatureNumerator; // account for time signature
                }

                _synth.Play();
            }
            else _synth.Pause();
        }

        protected override void OnSoloUpdate(bool v) => _synth.SetChannelSolo(_instrumentChannel, v);
        protected override void OnMuteUpdate(bool v) => _synth.SetChannelMute(_instrumentChannel, v);
        protected override void OnMetronomeUpdate(bool v) => _synth.MetronomeVolume = v ? 1 : 0;
        protected override void OnCountInUpdate(bool v) => _synth.CountInVolume = v ? 1 : 0;
        protected override void OnSpeedUpdate(float v) => _synth.PlaybackSpeed = v;

        public override void OnLoad(Score.Audio audio)
        {
            // Load the audio data.
            _instrumentChannel = audio.InstrumentChannel;
            _synth.LoadMidiFile(audio.MidiFile);

            // Set the correct solo and mute channels for this score.
            _synth.ResetChannelStates();
            _synth.SetChannelSolo(_instrumentChannel, State.Solo);
            _synth.SetChannelMute(_instrumentChannel, State.Mute);
            // Set the rest of the synth parameters.
            _synth.MetronomeVolume = State.Metronome ? 1 : 0;
            _synth.CountInVolume = State.CountIn ? 1 : 0;
            _synth.PlaybackSpeed = State.Speed;
        }

        public override void OnLoad(BeatMap beatMap) => _beatMap = beatMap;

        public override void Poll()
        {
            if (State.Playback == Playback.CountingIn)
                SetTick(_countInTarget - _countInOffset + (int)_synth.TickPosition);

            // Run only if the tick is progressing normally.
            if (State.Playback is not (Playback.Playing or Playback.CountingIn) || _synth.IsPlayingCountIn) return;

            // Set the latest known synth tick and state
            SetTick((int)_synth.TickPosition);
            SetPlayback(Playback.Playing);


            // Reset to the start when the end is reached, remove the finished notification.
            if (!_pollFinished) return;
            _pollFinished = false;
            State.ResetPlayback();
        }
    }
}