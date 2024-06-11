using UI;

namespace Player.Components.Panels
{
    /// <summary>
    /// Controls when a player's settings panel is enabled.
    /// </summary>
    public sealed class SettingsPanel : State.Component
    {
        private readonly Panel _panel;

        public SettingsPanel(State state, Panel panel) : base(state)
        {
            _panel = panel;
            _panel.Enabled = false;
        }
        
        public override void OnStartLoading() => _panel.Enabled = false;
        
        public override void OnEndLoading() => _panel.Enabled = true;
        
        protected override void OnPlaybackUpdate(Playback v) => _panel.Enabled = v == Playback.Paused;
    }
}