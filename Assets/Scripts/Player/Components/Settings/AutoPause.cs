using UI.Settings.Toggle;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's auto pause option to an UI toggle.
    /// </summary>
    public sealed class AutoPause : State.Component, IController
    {
        public IView View { get; set; }
        
        public bool Value
        {
            get => State.AutoPause;
            set => SetAutoPause(value);
        }

        public AutoPause(State state) : base(state)
        {
        }

        protected override void OnAutoPauseUpdate(bool v) => View?.OnValueChange(v);
    }
}