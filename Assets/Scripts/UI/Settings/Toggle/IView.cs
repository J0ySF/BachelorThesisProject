namespace UI.Settings.Toggle
{
    /// <summary>
    /// Visualizes a toggle controller's value and sends it toggle events.
    /// </summary>
    public interface IView
    {
        /// <summary>The controller associated to this view.</summary>
        public IController Controller { get; set; }
        
        /// <summary>Controls if the view is interactable.</summary>
        public bool Interactable { get; set; }
        
        /// <summary>Updates the view's state based on the updated value.</summary>
        public void OnValueChange(bool value);
    }
}