using System;
using System.Collections.Generic;
using Score;
using UI.Fretboard;
using static Configuration;

namespace Player.Components
{
    /// <summary>
    /// Controls a fretboard overlay to visualize a player's playback state.
    /// </summary>
    public sealed class Fretboard : State.Component
    {
        /// <summary>Set to true to display indicators.</summary>
        private bool _show;

        /// <summary>The overlay used to show the indicators.</summary>
        private readonly Overlay _overlay;

        private int _resumeFromCountInTick;

        public Fretboard(State state, Overlay overlay) : base(state) => _overlay = overlay;

        protected override void OnPlaybackUpdate(Playback v)
        {
            if (v == Playback.CountingIn) _resumeFromCountInTick = State.Tick;
            else _resumeFromCountInTick = -1;
        }

        protected override void OnTickUpdate(int _)
        {
            UpdateDisplay();
        }

        /// <summary>Update the overlay to display the current indicator state.</summary>
        private void UpdateDisplay() => _overlay.DisplayValues = ComputeIndicatorDescriptions();

        public override void OnStartLoading()
        {
            _show = false;
            UpdateDisplay();
        }

        public override void OnEndLoading()
        {
            _show = true;
            UpdateDisplay();
        }

        /// <summary>The current score's beatmap.</summary>
        private BeatMap _beatMap;

        public override void OnLoad(BeatMap beatMap) => _beatMap = beatMap;

        /// <summary>Compute all indicator descriptions that should be shown in this instant.</summary>
        private IEnumerable<IndicatorDescription> ComputeIndicatorDescriptions()
        {
            // Check if indicators are enabled.
            if (!_show) yield break;

            // Calculate how many ticks in the future to look in.
            var lookAheadDistance = (int)(LookAheadBeats * AlphaTab.Midi.MidiUtils.QuarterTime);

            var tick = State.Tick;
            var lookAheadEnd = tick + lookAheadDistance;

            // Iterate through all beats until one too far is found or the last beat is reached
            var beat = _beatMap.LastBeatBeforeOrAtTick(Math.Max(tick, 0));
            while (!beat!.IsEndBeat && beat.Tick <= lookAheadEnd)
            {
                Note note;
                if ((note = beat.Note) != null)
                {
                    // Compute the playing convergence.
                    float convergence;
                    if (beat.Tick <= tick && beat.Tick + note.Duration > tick) convergence = 1;
                    else convergence = 1 - (beat.Tick - tick) / (float)lookAheadDistance;

                    yield return new IndicatorDescription(note.String, note.Fret, convergence,
                        beat.Tick < _resumeFromCountInTick);
                }

                beat = beat.Next;
            }
        }
    }
}