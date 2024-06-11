namespace Player
{
    /// <summary>
    /// Indicates a player's playback state
    /// </summary>
    public enum Playback
    {
        /// <summary>
        /// Refers to when the player is not playing but a score is loaded.
        /// </summary>
        Paused,
        /// <summary>
        /// Refers to when the player is counting in playback is about to start.
        /// <para>Due to an AlphaTab bug which glitches the audio if playback is paused during count-in, this state
        /// should not be changed under any circumstances unless indicated by AlphaTab that count-in is done.
        /// This means that the count-in should be never interrupted under any circumstance.</para>
        /// </summary>
        CountingIn,
        /// <summary>
        /// Refers to when the player is playing a loaded score.
        /// </summary>
        Playing,
        /// <summary>
        /// Refers to when the player is seeking through a score.
        /// </summary>
        Seeking,
    }
}
