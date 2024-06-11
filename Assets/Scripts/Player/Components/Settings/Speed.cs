using UI.Settings.Slider;

namespace Player.Components.Settings
{
    /// <summary>
    /// Bridges a player's speed setting to an UI toggle.
    /// </summary>
    public sealed class Speed : State.Component, IController
    {
        public IView View { get; set; }

        public float Value
        {
            get => State.Speed;
            set => SetSpeed(value);
        }

        public Speed(State state) : base(state)
        {
        }

        protected override void OnSpeedUpdate(float v) => View?.OnValueChange(v);
    }
}