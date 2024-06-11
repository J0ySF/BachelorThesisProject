namespace UI.Fretboard
{
    /// <summary>
    /// Represents a fretboard indicator's display information.
    /// </summary>
    /// <param name="String">The string onto which to display.</param>
    /// <param name="Fret">The fret onto which to display.</param>
    /// <param name="Convergence">How close to being played the represented note is (0 when furthest to 1 when playing)
    /// <param name="CountIn">Indicates if the indicator refers to a note played during count-in.</param>
    /// .</param>
    public record IndicatorDescription(int String, int Fret, float Convergence, bool CountIn);
}