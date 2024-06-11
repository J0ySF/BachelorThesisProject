using UI.Settings.Slider;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Implements a slider view via an Unity slider whose value aspect is controlled by a slider controller.
    /// </summary>
    public sealed class Slider : MonoBehaviour, IView
    {
        [SerializeField] private UnityEngine.UI.Slider slider;
        [SerializeField] private Text text;
        [SerializeField] private string textFormat;

        /// <summary>The minimum slider value.</summary>
        /// <para>This is used to be able to stop the slider at a minimum value that is greater that the total sliding
        /// range, the effect is that when this value is grater than the actual minimum slider value, an area will
        /// remain always filled.</para>
        [SerializeField] private float minimumValue;

        /// <summary>The maximum slider value.</summary>
        /// <para>This is used to be able to stop the slider at a maximum value that is lesser that the total sliding
        /// range, the effect is that when this value is lesser than the actual maximum slider value, an area will
        /// remain always empty.</para>
        [SerializeField] private float maximumValue;

        /// <summary>The value to multiply/divide by when respectively sending/recieving values from the controller.
        /// </summary>
        [SerializeField] private float valueMultiplier;

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

        public void OnValueChange(float value)
        {
            // Set the value and text based on the value.

            slider.enabled = false; // Disable to avoid onValueChanged events when setting the value.

            text.text = string.Format(textFormat, value);
            slider.value = value / valueMultiplier; // Convert from controller range to slider range.

            slider.enabled = true; // Re-enable.
        }

        private void Start()
        {
            // Set the controller's value and update the visualization.
            slider.onValueChanged.AddListener(
                v => OnValueChange(Controller.Value =
                    // Clamp the slider value and convert from slider range to controller range.
                    Mathf.Clamp(v, minimumValue, maximumValue) * valueMultiplier)
            );
        }
    }
}