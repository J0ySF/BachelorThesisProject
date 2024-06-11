using UnityEngine;

namespace Tracking
{
    /// <summary>
    /// Represents a markers placed that must be placed on a GameObject in a position relative to it's real life
    /// counterpart.
    /// </summary>
    public sealed class Marker : MonoBehaviour
    {
        private void Start()
        {
            Id = int.Parse(name);
        }

        /// <summary>The marker's id..</summary>
        public int Id { get; private set; }
        
        /// <summary>The marker's relative position to the tracked object.</summary>
        public Vector3 Position => transform.localPosition;
    }
}