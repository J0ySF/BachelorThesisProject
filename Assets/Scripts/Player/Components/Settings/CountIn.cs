using UI.Settings.Toggle;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's count in option to an UI toggle.
    /// </summary>
    public sealed class CountIn : State.Component, IController
    {
        public IView View { get; set; }
        
        public bool Value
        {
            get => State.CountIn;
            set => SetCountIn(value);
        }

        public CountIn(State state) : base(state)
        {
        }

        protected override void OnCountInUpdate(bool v) => View?.OnValueChange(v);
    }
}