using System;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
using Gizmos = Popcron.Gizmos;
#endif

namespace Tracking
{
    /// <summary>
    /// Represents a GameObject whose transform is handled to match a real life counterpart via markers placed on the
    /// GameObject and the real life object in the same locations.
    /// </summary>
    public sealed class TrackedObject : MonoBehaviour
    {
        /// <summary>Transform whose children are all marker instances with unique Ids.</summary>
        [SerializeField] private Transform markersParent;

        /// <summary>Whether to track as a static or dynamic object.</summary>
        [SerializeField] private bool dynamicTracking;

        /// <summary>The tracking timeout for this object's markers.</summary>
        [SerializeField] private float markerTimeout;

        private Marker[] _markers;

        private void Start()
        {
            // Setup markers
            _markers = new Marker[markersParent.childCount];
            for (var i = 0; i < markersParent.childCount; i++)
                _markers[i] = markersParent.GetChild(i).GetComponent<Marker>();
        }

        /// <summary>This object's markers.</summary>
        public IReadOnlyList<Marker> Markers => _markers;

        /// <summary>Whether to track as a static or dynamic object.</summary>
        public bool DynamicTracking => dynamicTracking;

        /// <summary>The tracking timeout for this object's markers.</summary>
        public float MarkerTimeout => markerTimeout;

        /// <summary>The event invoked when this object has begun to be tracked.</summary>
        public event Action<TrackedObject> BeginTracking;

        /// <summary>The event invoked when this object has stopped being tracked.</summary>
        public event Action<TrackedObject> EndTracking;

        private bool _isTracked = true; // Set true by default so that when debugging wihout tracking enabled it's
        // considered tracked.

        /// <summary>Indicates if the object is being tracked or not.</summary>
        public bool IsTracked
        {
            get => _isTracked;
            set
            {
                if (_isTracked == value) return;

                // Set value and invoke the required event.
                _isTracked = value;
                if (value) BeginTracking?.Invoke(this);
                else EndTracking?.Invoke(this);
            }
        }

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        private void Update()
        {
            // Visualize the markers positions in relation to the tracked object
            var c = _isTracked ? Color.magenta : Color.magenta / 2;
            for (var i = 0; i < markersParent.childCount; i++)
            {
                var t = markersParent.GetChild(i).position;
                Gizmos.Line(markersParent.position, t, c);
                Gizmos.Sphere(t, 0.03f, c);
            }
            Gizmos.Sphere(markersParent.position, 0.03f, _isTracked ? Color.yellow : Color.yellow / 2);
        }
#endif
    }
}