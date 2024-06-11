using System.Collections.Generic;
using System.Linq;
using AlphaTab.Model;
using AlphaTab.Rendering.Utils;
using UnityEngine;

namespace Score
{
    /// <summary>
    /// A score's beat map, which represents all beats contained in the score and handles beat operations.
    /// </summary>
    /// <param name="Beats">The score's beats in order of appearance.</param>
    /// <param name="Tempo">The score's tempo.</param>
    public sealed record BeatMap(IReadOnlyList<Beat> Beats, int Tempo)
    {
        /// <summary>Finds the last beat that plays before or at the given tick.</summary>
        /// <para>Ticks less than zero are considered invalid.</para>
        public Beat LastBeatBeforeOrAtTick(int tick)
        {
            Debug.Assert(tick >= 0);
            return Beats.Last(beat => beat.Tick <= tick); // TODO: implement with logarithmic search.
        }

        /// <summary>Finds the last beat that plays before or at the given sheet position.</summary>
        /// <para>Sheet position less than the first beat's position are considered invalid.</para>
        public Beat LastBeatBeforeOrAtSheetPosition(int sheetPosition)
        {
            Debug.Assert(sheetPosition >= Beats[0].SheetPosition);
            return Beats.Last(beat => beat.SheetPosition <= sheetPosition); // TODO: implement with logarithmic search.
        }

        /// <summary>Converts between tick and sheet position.</summary>
        /// <para>Ticks less than zero are considered invalid.</para>
        public int TickToSheetPosition(int tick)
        {
            var currentBeat = LastBeatBeforeOrAtTick(tick);

            if (currentBeat.IsEndBeat) return currentBeat.SheetPosition;

            // If beat has successor, get the interpolated sheet positions between the current and next beat.
            // The interpolation parameter is calculated from the min-max normalization of the current tick betweem the
            // current and next beat's ticks.
            var nextBeat = currentBeat.Next;
            return (int)Mathf.Lerp(currentBeat.SheetPosition, nextBeat!.SheetPosition,
                (float)(tick - currentBeat.Tick) / (nextBeat.Tick - currentBeat.Tick));
        }

        /// <summary>Converts between sheet position and tick.</summary>
        /// <para>Sheet position less than the first beat's position are considered invalid.</para>
        public int SheetPositionToTick(int sheetPosition)
        {
            var currentBeat = LastBeatBeforeOrAtSheetPosition(sheetPosition);

            if (currentBeat.IsEndBeat) return currentBeat.Tick;

            // If beat has successor, get the interpolated tick position between the current and next beat.
            // The interpolation parameter is calculated from the min-max normalization of the current sheet position
            // betweem the current and next beat's sheet positions.
            var nextBeat = currentBeat.Next;
            return (int)Mathf.Lerp(currentBeat.Tick, nextBeat!.Tick,
                (float)(sheetPosition - currentBeat.SheetPosition) /
                (nextBeat.SheetPosition - currentBeat.SheetPosition));
        }

        /// <summary>Load the beatmap from the given score and graphics data.</summary>
        public static BeatMap Load(Staff staff, Graphics graphics, BoundsLookup sheetBoundsLookup)
        {
            // Calculate the total beat count, the +1 is there to account for a "fake" beat at the end of the score. 
            var beatCount = staff.Bars.Sum(x => x.Voices[0].Beats.Count) + 1;

            var beats = new Beat[beatCount];

            var lastBeat = staff.Bars.Last().Voices[0].Beats.Last(); // The last "real" beat in the score.

            // "Fake" beat at the end if the beat chain,
            var nextBeat = beats[beatCount - 1] =
                new Beat(
                    // with tick immediately after the "real" last beat's end,
                    (int)(lastBeat.AbsolutePlaybackStart + lastBeat.PlaybackDuration),
                    graphics.Width, // positioned at the end of the score sheet,
                    null, null, // without a note or next beat,
                    (int)lastBeat.Voice.Bar.MasterBar.TimeSignatureNumerator, (int)lastBeat.Voice.Bar.MasterBar.TimeSignatureDenominator // with the last bar's time signature.
                );

            // The cursor used during iteration.
            var beatCursor = lastBeat;
            // Iterate through score beats, starting from the last "real" beat and ending at the first.
            // Reverse iteration in necessary to build the Next beat chain with immutable structures.
            for (var i = beatCount - 2; i >= 0; i--)
            {
                // The first note's data on this AlphaTab beat (if it exists).
                var atNote = beatCursor!.Notes.Count > 0 ? beatCursor.Notes[0] : null;

                var note = atNote != null // If there is at least one note on this AlphaTab beat create a new note
                    ? new Note(
                        (int)(beatCursor.PlaybackDuration * atNote.DurationPercent),
                        (int)atNote.String - 1,
                        (int)atNote.Fret,
                        (int)staff.Tuning[^(int)atNote.String] + (int)atNote.Fret
                    )
                    : null; // else create none.

                // Find this beat's sheet position.
                var sheetPosition = Graphics.BeatPosition(sheetBoundsLookup.FindBeat(beatCursor)!.VisualBounds);

                // "Real" beat before the previously inserted beat.
                beats[i] = new Beat(
                    (int)beatCursor.AbsolutePlaybackStart,
                    sheetPosition,
                    note,
                    nextBeat,
                    (int)beatCursor.Voice.Bar.MasterBar.TimeSignatureNumerator, 
                    (int)beatCursor.Voice.Bar.MasterBar.TimeSignatureDenominator
                );

                // Progress the iteration aux variables.
                nextBeat = beats[i];
                beatCursor = beatCursor.PreviousBeat;
            }

            return new BeatMap(beats, (int)staff.Track.Score.Tempo);
        }
    }
}