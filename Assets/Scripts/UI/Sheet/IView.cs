using Score;

namespace UI.Sheet
{
    /// <summary>
    /// Visualizes a score's sheet, handles a sheet controller's position updates and sends scrolling events.
    /// </summary>
    public interface IView
    {
        /// <summary>The controller associated to this view.</summary>
        public IController Controller { get; set; }

        /// <summary>Controls if the view is interactable.</summary>
        public bool Interactable { get; set; }
        
        /// <summary>Loads the given score's graphics data into view.</summary>
        /// <param name="graphics">The score's graphics data.</param>
        /// <param name="firstValidPosition">The sheet's first valid scrolling position.</param>
        /// <param name="lastValidPosition">The sheet's last valid scrolling position.</param>
        public void Load(Graphics graphics, int firstValidPosition, int lastValidPosition);

        /// <summary>Updates the scrolling position visualization based on the updated value.</summary>
        public void OnPositionChange(int position);
    }
}