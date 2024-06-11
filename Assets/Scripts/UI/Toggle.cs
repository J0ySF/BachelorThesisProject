using UI.Settings.Toggle;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Implements a toggle view via an Unity button whose "activeness" aspect is controlled by a toggle controller.
    /// </summary>
    public sealed class Toggle : MonoBehaviour, IView
    {
        [SerializeField] private Button button;

        /// <summary>The on toggle color.</summary>
        [SerializeField] private Color onColor;

        /// <summary>The off toggle color.</summary>
        [SerializeField] private Color offColor;

        /// <summary>The off highlight color.</summary>
        [SerializeField] private Color highlightOff;

        /// <summary>The on highlight color.</summary>
        [SerializeField] private Color highlightOn;

        /// <summary>The off pressed color.</summary>
        [SerializeField] private Color pressedOff;

        /// <summary>The on pressed color.</summary>
        [SerializeField] private Color pressedOn;

        private IController _controller;

        public IController Controller
        {
            get => _controller;
            set
            {
                _controller = value;
                OnValueChange(_controller.Value); // Update the visualization.
            }
        }

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public void OnValueChange(bool value)
        {
            // Set the on/off color based on the value.
            var colors = button.colors;
            colors.normalColor = value ? onColor : offColor;
            colors.highlightedColor = value ? highlightOn : highlightOff;
            colors.pressedColor = value ? pressedOn : pressedOff;
            button.colors = colors;
        }

        private void Start()
        {
            // Toggle the controller's value and update the visualization.
            button.onClick.AddListener(() => OnValueChange(_controller.Toggle()));
        }
    }
}