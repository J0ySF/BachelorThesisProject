using System.Collections.Generic;
using System.Linq;
using Leap.Unity;
using UnityEngine;
using Varjo.XR;
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
using Gizmos = Popcron.Gizmos;
#endif

namespace Tracking
{
    /// <summary>
    /// Tracks a series of objects' in a scene by trying to match their markers to real world counterparts.
    /// </summary>
    public sealed class ObjectsTracker : MonoBehaviour
    {
        /// <summary>The objects tracked in the scene.</summary>
        [SerializeField] private TrackedObject[] trackedTransforms;

        private void Start()
        {
            VarjoMixedReality.StartRender(); //TODO: move to a more appropriate place

            // Initialize the marker tracking.
            VarjoMarkers.EnableVarjoMarkers(true);

            // Set up every marker's properties.
            foreach (var t in trackedTransforms)
            foreach (var marker in t.Markers)
            {
                VarjoMarkers.SetVarjoMarkerTimeout(marker.Id, (long)(t.MarkerTimeout * 1000));
                if (t.DynamicTracking)
                    VarjoMarkers.AddVarjoMarkerFlags(marker.Id, VarjoMarkerFlags.DoPrediction);
            }
        }

        private void Update()
        {
            if (VarjoMarkers.IsVarjoMarkersEnabled())
            {
                // Get all currently tracked markers.
                VarjoMarkers.GetVarjoMarkers(out var varjoMarkers);

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
                // Draw all currently tracked markers' positions.
                foreach (var varjoMarker in varjoMarkers)
                    Gizmos.Sphere(
                        varjoMarker.pose.position,
                        0.03f,
                        Color.Lerp(Color.black, Color.green, varjoMarker.confidence)
                    );
#endif

                foreach (var t in trackedTransforms)
                {
                    // Find all markers on this object that are being currently tracked and store nominal and actual
                    // positions.
                    var sourcePositions = new List<Vector3>(t.Markers.Count);
                    var destinationPositions = new List<Vector3>(t.Markers.Count);
                    foreach (var marker in t.Markers)
                    foreach (var varjoMarker in varjoMarkers.Where(varjoMarker => marker.Id == varjoMarker.id))
                    {
                        sourcePositions.Add(marker.Position);
                        destinationPositions.Add(varjoMarker.pose.position);
                    }

                    // Perform tracking only if at least 3 markers are tracked.
                    var performTracking = sourcePositions.Count >= 3;
                    if (performTracking)
                        t.transform.SetMatrix(BestFit.Fit(sourcePositions, destinationPositions));

                    t.IsTracked = performTracking; // Update the tracked state.
                }
            }
            else
                foreach (var t in trackedTransforms) // Set all objects to not tracked state.
                    t.IsTracked = false;
        }
    }
}