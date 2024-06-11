using System;
using System.Collections.Generic;
using Tracking;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace UI.Fretboard
{
    /// <summary>
    /// Visualizes interaction cues on a fretboard to visualize a player's playback state according to tracking
    /// positions.
    /// </summary>
    public sealed class Overlay : MonoBehaviour
    {
        [SerializeField] private TrackedObject fretboard;

        [SerializeField] private FretboardConfiguration fretboardConfiguration;

        [SerializeField] private Indicator indicatorPrefab;
        //[SerializeField] private Transform fretboardWarningPanel;

        private OrderedPool<Indicator> _indicatorsPool;

        /// <summary>
        /// Ordered pool controller.
        /// </summary>
        private sealed class IndicatorsPoolController : IOrderedPoolController<Indicator>
        {
            private readonly Overlay _parent;
            public IndicatorsPoolController(Overlay parent) => _parent = parent;

            public Indicator Instantiate(int index)
            {
                // Instantiate a new indicator and name it with it's index.
                var item = Object.Instantiate(_parent.indicatorPrefab, _parent.transform, false);
                item.name = $"Indicator#{index}";
                item.Disable();
                return item;
            }

            public void Rewind(Indicator item) => item.Disable();
        }

        private void Start() =>
            // Setup the indicators pool.
            _indicatorsPool = new OrderedPool<Indicator>(5, new IndicatorsPoolController(this));

        /// <summary>The latest descriptions to display.</summary>
        public IEnumerable<IndicatorDescription> DisplayValues { get; set; } = Array.Empty<IndicatorDescription>();

        private void Update()
        {
            // Hide the indicators shown the previous frame.
            _indicatorsPool.Rewind();

            if (!fretboard.IsTracked) return; // If the fretboard is tracked

            foreach (var description in DisplayValues) // for each description
            {
                var indicator = _indicatorsPool.Extract(); // extract an indicator and set it up for display.

                indicator.Enable(
                    description.Fret > 0
                        ? fretboardConfiguration.StringFretPosition(description.String, description.Fret)
                        : null,
                    fretboardConfiguration.PluckingPosition(description.String),
                    fretboardConfiguration.Rotation,
                    description.Convergence,
                    description.CountIn
                );
            }
        }
    }
}