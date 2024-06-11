using UI.Settings.Toggle;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's mute option to an UI toggle.
    /// </summary>
    public sealed class Mute : State.Component, IController
    {
        public IView View { get; set; }
        
        public bool Value
        {
            get => State.Mute;
            set => SetMute(value);
        }
        
        public Mute(State state) : base(state)
        {
        }

        protected override void OnMuteUpdate(bool v) => View?.OnValueChange(v);
    }
}