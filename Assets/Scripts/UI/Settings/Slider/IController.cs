namespace UI.Settings.Slider
{
    /// <summary>
    /// Controls a slider view's visualized value and handles the view's events.
    /// </summary>
    public interface IController
    {
        /// <summary>The view associated to this controller.</summary>
        IView View { get; set; }

        /// <summary>The controller's reported value.</summary>
        float Value { get; set; }
    }
}