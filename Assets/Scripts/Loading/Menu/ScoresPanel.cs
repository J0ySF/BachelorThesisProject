using UI;
using UnityEngine;

namespace Loading.Menu
{
    /// <summary>
    /// Overloads the enabled property to move the panel in the right position when enabled.
    /// </summary>
    public sealed class ScoresPanel : Panel
    {
        [SerializeField] private Transform enablePosition;

        public override bool Enabled
        {
            get => canvas.enabled;
            set
            {
                canvas.enabled = value;
                if (value) transform.position = enablePosition.position;
            }
        }
    }
}