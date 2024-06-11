using Minis;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static Configuration;

namespace Audio
{
    public sealed class MidiInput : MonoBehaviour
    {
        /// <summary>Midi note count according to the standard.</summary>
        private const int MidiNoteCount = 128;

        /// <summary>All currently active notes.</summary>
        private readonly bool[] _activeNotes = new bool[MidiNoteCount];

        /// <summary>Array representing all inactive notes buffering timers.</summary>
        /// <para>When a note is not active, it's timer gets decremented every frame until the value is less or equal to
        /// 0, at which point the note is considered not active.</para>
        private readonly float[] _remainingActiveness = new float[MidiNoteCount];

        /// <summary>Array representing all inactive notes buffering timers.</summary>
        public bool Active(int note, bool distinguishOctaves = true)
        {
            if (distinguishOctaves) return note is < MidiNoteCount && _remainingActiveness[note] > 0;
            return _remainingActiveness.Where((_, i) => note % 12 == i % 12 && _remainingActiveness[i] > 0).Any();
        }

        private void Start()
        {
            // Subscribe to detect MIDI device changes.
            // Only one device at the time has been tested
            // TODO: test behavior when more than one MIDI device is connected
            InputSystem.onDeviceChange += (device, change) =>
            {
                if (change != InputDeviceChange.Added) return;
                if (device is not MidiDevice midiDevice) return;

                // Activate/deactivate notes
                midiDevice.onWillNoteOn += (note, _) => _activeNotes[note.noteNumber] = true;
                midiDevice.onWillNoteOff += note => _activeNotes[note.noteNumber] = false;
            };
        }

        private void Update()
        {
            for (var i = 0; i < MidiNoteCount; i++)
            {
                // Refresh all active notes' buffering timers
                if (_activeNotes[i]) _remainingActiveness[i] = NoteBufferingDuration;
                // Decrement all inactive notes' buffering timers
                else _remainingActiveness[i] -= Time.deltaTime;
            }
        }
        
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        /// <summary>Small debugging utility to display note states: green notes are active, yellow notes are inactive
        /// but still buffered and blue notes are inactive.</summary>
        private void OnGUI()
        {
            var activeStyle = new GUIStyle
            {
                normal =
                {
                    textColor = Color.green
                }
            };

            var bufferedStyle = new GUIStyle
            {
                normal =
                {
                    textColor = Color.yellow
                }
            };

            var unactiveStyle = new GUIStyle
            {
                normal =
                {
                    textColor = Color.blue
                }
            };

            var x = 0;
            // The range we are interested in.
            for (var i = 28; i <= 67; i++)
            {
                GUI.Label(
                    new Rect(10 + 30 * x, 10, 30, 30),
                    NoteNames[i],
                    _activeNotes[i] ? activeStyle : _remainingActiveness[i] > 0 ? bufferedStyle : unactiveStyle
                );
                x++;
            }
        }

        private static readonly string[] NoteNames =
        {
            "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "A0", "A#0", "B0",
            "C1", "C#1", "D1", "D#1", "E1", "F1", "F#1", "G1", "G#1", "A1", "A#1", "B1",
            "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B2",
            "C3", "C#3", "D3", "D#3", "E3", "F3", "F#3", "G3", "G#3", "A3", "A#3", "B3",
            "C4", "C#4", "D4", "D#4", "E4", "F4", "F#4", "G4", "G#4", "A4", "A#4", "B4",
            "C5", "C#5", "D5", "D#5", "E5", "F5", "F#5", "G5", "G#5", "A5", "A#5", "B5",
            "C6", "C#6", "D6", "D#6", "E6", "F6", "F#6", "G6", "G#6", "A6", "A#6", "B6",
            "C7", "C#7", "D7", "D#7", "E7", "F7", "F#7", "G7", "G#7", "A7", "A#7", "B7",
            "C8", "C#8", "D8", "D#8", "E8", "F8", "F#8", "G8", "G#8", "A8", "A#8", "B8",
            "C9", "C#9", "D9", "D#9", "E9", "F9", "F#9", "G9"
        };
#endif
    }
}