namespace UI.Sheet
{
    /// <summary>
    /// Controls a score sheet view's scrolling position and handles it's scrolling events.
    /// </summary>
    public interface IController
    {
        /// <summary>The view associated to this controller.</summary>
        IView View { get; set; }

        /// <summary>The controller's reported scrolling position.</summary>
        int Position { get; }

        /// <summary>Indicates that the view has initiated a scrolling operation and is set to the given position.
        /// </summary>
        void StartScroll(int position);

        /// <summary>Indicates that the view is continuing an already initiated scrolling operation and is set to the
        /// given position.</summary>
        void ScrollUpdate(int position);

        /// <summary>Indicates that the view has ended a scrolling operation and is set to the given position.
        /// </summary>
        void EndScroll(int position);
    }
}