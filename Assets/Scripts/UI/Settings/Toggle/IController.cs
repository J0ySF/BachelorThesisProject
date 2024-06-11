namespace UI.Settings.Toggle
{
    /// <summary>
    /// Controls a toggle view's visualized value and handles the view's events.
    /// </summary>
    public interface IController
    {
        /// <summary>The view associated to this controller.</summary>
        IView View { get; set; }

        /// <summary>The controller's reported value.</summary>
        bool Value { get; set; }

        /// <summary>Switches the reported value on/off.</summary>
        bool Toggle() => Value = !Value;
    }
}