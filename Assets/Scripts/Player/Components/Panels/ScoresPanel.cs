using UI;
using UI.Settings.Toggle;

namespace Player.Components.Panels
{
    /// <summary>
    /// Controls when a player's tracks panel is enabled via an UI toggle and state events.
    /// </summary>
    public sealed class ScoresPanel : State.Component, IController
    {
        public IView View { get; set; }
        
        private bool _enabled;
        
        public bool Value
        {
            get => _enabled;
            set
            {
                _enabled = value;
                _panel.Enabled = value;
            }
        }

        private readonly Panel _panel;

        public ScoresPanel(State state, Panel panel) : base(state)
        {
            _panel = panel;
        }

        public override void OnStartLoading()
        {
            Value = false;
            View?.OnValueChange(Value);
        }

        protected override void OnPlaybackUpdate(Playback v)
        {
            if (v != Playback.Playing && v != Playback.CountingIn) return;
            Value = false;
            View?.OnValueChange(Value);
        }
    }
}