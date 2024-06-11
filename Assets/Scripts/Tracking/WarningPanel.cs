using UI;
using UnityEngine;

namespace Tracking
{
    /// <summary>
    /// Displays a warning to altert when the tracked object is not being tracked.
    /// </summary>
    public sealed class WarningPanel : Panel
    {
        [SerializeField] private TrackedObject trackedObject;

        public void Start()
        {
            Enabled = false;
            trackedObject.BeginTracking += _ => Enabled = false;
            trackedObject.EndTracking += _ => Enabled = true;
        }
    }
}