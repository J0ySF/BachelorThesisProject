using UI.Settings.Toggle;

namespace UI
{
    /// <summary>
    /// Utility class to bind views and controller togheter.
    /// </summary>
    public static class ViewController
    {
        public static void Bind(IView view, IController controller)
        {
            view.Controller = controller;
            controller.View = view;
        }
        
        public static void Bind(Settings.Slider.IView view, Settings.Slider.IController controller)
        {
            view.Controller = controller;
            controller.View = view;
        }
        
        public static void Bind(Sheet.IView view, Sheet.IController controller)
        {
            view.Controller = controller;
            controller.View = view;
        }
    }
}