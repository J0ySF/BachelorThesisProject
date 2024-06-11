using UI.Settings.Toggle;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's metronome option to an UI toggle.
    /// </summary>
    public sealed class Metronome : State.Component, IController
    {
        public IView View { get; set; }
        
        public bool Value
        {
            get => State.Metronome;
            set => SetMetronome(value);
        }

        public Metronome(State state) : base(state)
        {
        }

        protected override void OnMetronomeUpdate(bool v) => View?.OnValueChange(v);
    }
}