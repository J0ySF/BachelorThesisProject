namespace Score
{
    /// <summary>
    /// A beat's note.
    /// </summary>
    /// <param name="Duration">The note's tick duration.</param>
    /// <param name="String">The note's string index.
    /// <para>The index is zero based and starts from the lightest string progressing towards the heaviest string.
    /// </para></param>
    /// <param name="Fret">The note's fret position on the given string.</param>
    /// <param name="MidiValue">The note's corresponding MIDI value.</param>
    public sealed record Note(int Duration, int String, int Fret, int MidiValue);
}