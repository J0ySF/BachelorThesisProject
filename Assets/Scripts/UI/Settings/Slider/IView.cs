namespace UI.Settings.Slider
{
    /// <summary>
    /// Visualizes a slider controller's value and sends it slider events.
    /// </summary>
    public interface IView
    {
        /// <summary>The controller associated to this view.</summary>
        public IController Controller { get; set; }
        
        /// <summary>Updates the view's state based on the updated value.</summary>
        public void OnValueChange(float value);
    }
}