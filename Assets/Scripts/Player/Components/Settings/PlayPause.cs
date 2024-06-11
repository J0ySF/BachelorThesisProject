using Score;
using UI.Settings.Toggle;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's playback state to an UI toggle to toggle between playing and pausing.
    /// </summary>
    public sealed class PlayPause : State.Component, IController
    {
        public IView View { get; set; }

        private BeatMap _beatMap;
        
        public bool Value
        {
            get => State.Playback == Playback.Playing;
            set
            {
                // Set playing if true else paused.
                var state = value ? (State.CountIn ? Playback.CountingIn : Playback.Playing) : Playback.Paused;
                // If paused snap the state's tick to the closest beat before or at the state's tick.
                if (state == Playback.Paused) SetTick(_beatMap.LastBeatBeforeOrAtTick(State.Tick).Tick);
                // Reflect changes and update the playback state.
                OnPlaybackUpdate(state);
                SetPlayback(state);
            }
        }

        public PlayPause(State state) : base(state)
        {
        }

        public override void OnLoad(BeatMap beatMap) => _beatMap = beatMap;
        
        protected override void OnPlaybackUpdate(Playback v)
        {
            View?.OnValueChange(v == Playback.Playing); // Set true if playing else false.
            View!.Interactable = v is Playback.Playing or Playback.Paused;
        }
    }
}