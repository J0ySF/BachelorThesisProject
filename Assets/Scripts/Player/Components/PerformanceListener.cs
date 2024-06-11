using Audio;
using Score;
using UnityEngine;
using static Configuration;

namespace Player.Components
{
    /// <summary>
    /// Controls a player's playback by pausing playback if the MIDI input does not match a note when it should be
    /// playing.
    /// </summary>
    public sealed class PerformanceListener : State.Component
    {
        private readonly MidiInput _midiInput;

        private BeatMap _beatMap;
        private Beat _snapBeat, _waitingNoteBeat;

        public PerformanceListener(State state, MidiInput midiInput) : base(state)
        {
            _midiInput = midiInput;

            OnPlaybackUpdate(State.Playback);
            OnAutoPauseUpdate(State.AutoPause);
        }

        public override void OnLoad(BeatMap beatMap)
        {
            _beatMap = beatMap;
        }

        /// <summary> Fetch the next note to wait for, either from a given beat onwards or from the last beat before
        /// the current tick.</summary>
        private void FetchNextWaitingNote(Beat searchFrom = null)
        {
            _waitingNoteBeat = searchFrom ?? _beatMap.LastBeatBeforeOrAtTick(State.Tick);
            while (_waitingNoteBeat is { Note: null })
            {
                _snapBeat = _waitingNoteBeat;
                _waitingNoteBeat = _waitingNoteBeat.Next;
            }

            _snapBeat = _waitingNoteBeat;
        }

        private void CheckState()
        {
            if (_beatMap is not null && State.Playback != Playback.CountingIn) FetchNextWaitingNote();
        }

        protected override void OnPlaybackUpdate(Playback _) => CheckState();

        protected override void OnAutoPauseUpdate(bool _) => CheckState();

        protected override void OnTickUpdate(int _)
        {
            if (State.Playback != Playback.Playing) CheckState();
        }

        public override void Poll()
        {
            if (_waitingNoteBeat == null || State.Playback != Playback.Playing || !State.AutoPause) return;

            // If the player state is in a configuration where the player is playing, check for matching MIDI inputs.
            
            // Get the current tick's case in regards to the waiting note.
            var beatDuration = 60.0f / _beatMap.Tempo / State.Speed;
            var delayTicks = (int)(NoteDelay * AlphaTab.Midi.MidiUtils.QuarterTime / beatDuration);
            var detectionRadiusTicks = (int)(NoteDetectionRadius * AlphaTab.Midi.MidiUtils.QuarterTime / beatDuration);
            var adjustedTick = State.Tick - delayTicks;
            
            if (adjustedTick < _waitingNoteBeat.Tick - detectionRadiusTicks) return; // Not yet in listening range.
            if (adjustedTick > _waitingNoteBeat.Tick + detectionRadiusTicks ||
                _waitingNoteBeat.Next?.Tick <= adjustedTick) // Outside the valid note range, stop the playback/
            {
                SetPlayback(Playback.Paused);
                SetTick(_snapBeat.Tick); // Snap to tick/
                return;
            }

            // If detected a valid input, progress to the next note.
            if (_midiInput.Active(_waitingNoteBeat.Note!.MidiValue, NoteDistinguishOctaves))
                FetchNextWaitingNote(_waitingNoteBeat.Next);
        }

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        /// <summary>Small debugging utility to display detection ranges.</summary>
        public override void OnGUI()
        {
            if (_waitingNoteBeat == null) return;

            var beatDuration = 60.0f / _beatMap.Tempo / State.Speed;
            var delayTicks = (int)(NoteDelay * AlphaTab.Midi.MidiUtils.QuarterTime / beatDuration);
            var detectionRadiusTicks = (int)(NoteDetectionRadius * AlphaTab.Midi.MidiUtils.QuarterTime / beatDuration);

            Color color;
            var adjustedTick = State.Tick - delayTicks;
            if (adjustedTick < _waitingNoteBeat.Tick - detectionRadiusTicks ||
                adjustedTick > _waitingNoteBeat.Tick + detectionRadiusTicks) color = Color.blue;
            else if (adjustedTick < _waitingNoteBeat.Tick) color = Color.green;
            else color = Color.magenta;

            var x = _waitingNoteBeat.Note!.MidiValue - 28;
            GUI.Box(
                new Rect(10 + 30 * x, 50, 30, 30),
                "###",
                new GUIStyle
                {
                    normal =
                    {
                        textColor = color
                    }
                }
            );
        }
#endif
    }
}