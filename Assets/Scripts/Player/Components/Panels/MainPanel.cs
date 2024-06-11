using UI;

namespace Player.Components.Panels
{
    /// <summary>
    /// Controls when a player's main panel is enabled.
    /// </summary>
    public sealed class MainPanel : State.Component
    {
        private readonly Panel _panel;

        public MainPanel(State state, Panel panel) : base(state)
        {
            _panel = panel;
            OnStartLoading();
        }
        
        public override void OnStartLoading() => _panel.Enabled = false;

        public override void OnEndLoading() => _panel.Enabled = true;
    }
}