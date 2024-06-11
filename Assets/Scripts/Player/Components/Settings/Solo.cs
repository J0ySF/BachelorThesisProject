using UI.Settings.Toggle;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's solo option to an UI toggle.
    /// </summary>
    public sealed class Solo : State.Component, IController
    {
        public IView View { get; set; }
        
        public bool Value
        {
            get => State.Solo;
            set => SetSolo(value);
        }

        public Solo(State state) : base(state)
        {
        }

        protected override void OnSoloUpdate(bool v) => View?.OnValueChange(v);
    }
}