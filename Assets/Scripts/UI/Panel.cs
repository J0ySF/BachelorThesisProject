using UnityEngine;

namespace UI
{
    /// <summary>
    /// Class that represents UI panels whose rendering and interaction can be enabled and disabled.
    /// <para>Note that the GameObject itself stays active, this class acts on the panel's canvas.</para>
    /// </summary>
    public class Panel : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        
        /// <summary>Enabled and disables the panel's canvas.</summary>
        public virtual bool Enabled
        {
            get => canvas.enabled;
            set => canvas.enabled = value;
        }
    }
}