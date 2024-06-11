using AlphaTab.Midi;

namespace Score
{
    /// <summary>
    /// A score's audio related data.
    /// </summary>
    /// <param name="MidiFile">The MIDI file representing the score's audio playback.
    /// <para>This data is meant for handling via the AlphaTab library.</para></param>
    /// <param name="InstrumentChannel">The controlled instrument's MIDI channel.</param>
    public sealed record Audio(MidiFile MidiFile, int InstrumentChannel)
    {
        /// <summary>Load the MIDI data from the given score data.</summary>
        public static Audio Load(AlphaTab.Model.Score score, int instrumentChannel)
        {
            // Generate the MIDI data.
            var midiFile = new MidiFile();
            var handler = new AlphaSynthMidiFileHandler(midiFile);
            var generator = new MidiFileGenerator(score, null, handler);
            generator.Generate();

            return new Audio(midiFile, instrumentChannel);
        }
    }
}
