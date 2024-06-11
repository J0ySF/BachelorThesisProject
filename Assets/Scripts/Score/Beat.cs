#nullable enable

namespace Score
{
    /// <summary>
    /// A score's beat, which represents a discrete point in time when either a note or pause appear.
    /// </summary>
    /// <param name="Tick">The beat's audio position in a score.</param>
    /// <param name="SheetPosition">The beat's horizional sheet position in a score.</param>
    /// <param name="Note">The note playing on this beat.
    /// <para>Either one or no note can play on a beat, if no note is present it usually means that this is a pause beat
    /// </para></param>
    /// <param name="Next">The following beat in the score.
    /// <para>When the next beat is null, this means that the score's end has been reached.</para></param>
    /// <param name="TimeSignatureNumerator">The score's time signature numerator.</param> //TODO: move to bars and make beats share bars
    /// <param name="TimeSignatureDenominator">The score's time signature denominator.</param> //TODO: move to bars and make beats share bars
    // ReSharper disable once NotAccessedPositionalProperty.Global
    public sealed record Beat(int Tick, int SheetPosition, Note? Note, Beat? Next, int TimeSignatureNumerator, int TimeSignatureDenominator)
    {
        /// <summary>Shorthand to check if the beat is the score's end beat.</summary>
        public bool IsEndBeat => Next == null;
    }
}