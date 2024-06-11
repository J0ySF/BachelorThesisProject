using System;
using UnityEngine;
using Varjo.XR;

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
using Gizmos = Popcron.Gizmos;
#endif

namespace Tracking
{
    /// <summary>
    /// Keeps track of a fretboard's positions (in string/fret coordinates) in a scene. String indices begin from 0 and
    /// are refer to the strings in lightest to heaviest order.
    /// <para>This component is intended to be attatched to a GameObject that represents a fretboard, and the parameters
    /// need to be configured to match said fretboard. The positions parent should be centered on the center of the 12th
    /// fret.</para>
    /// </summary>
    public sealed class FretboardConfiguration : MonoBehaviour
    {
        /// <summary>The instrument's string count.</summary>
        [SerializeField] private int stringCount = 4;

        /// <summary>The instrument's fret count.</summary>
        [SerializeField] private int fretCount = 24;

        /// <summary>The total instrument lenght from the start to the end of the playable string section.</summary>
        [SerializeField] private float bodyScale = 0.8636f;

        /// <summary>The string spacing at the nut (near the headstock).</summary>
        [SerializeField] private float nutSpacing = 0.01f;

        /// <summary>The string spacing at the bridge (the opposite side of the nut).</summary>
        [SerializeField] private float bridgeSpacing = 0.015f;

        /// <summary>The distance from the bridge where plucking happens.</summary>
        [SerializeField] private float pluckingOffset = 0.015f;

        /// <summary>The transform to which to add the position transforms for each coordinate.
        /// <para>This should be a children of this GameObject to be used correctly (since this GameObject might get
        /// scaled during tracking.</para></summary>
        [SerializeField] private Transform positionsParent;

        private Transform[,] _stringFretPositions;
        private Transform[] _pluckingPositions;

        private void Start()
        {
            // Set up all string/fret positions.

            var o = new GameObject(); // Create a dummy GameObject to clone for each string/fret position.

            _stringFretPositions =
                new Transform[stringCount, fretCount + 1]; // +1 to account for the "0th fret" (the nut).
            _pluckingPositions = new Transform[stringCount];

            for (var fret = 0; fret <= fretCount; fret++)
            {
                // The normalized fret position from the nut.
                var fretPositionPercentage = (float)(1.0 / Math.Pow(2.0, fret / 12.0));

                // Position the 12th fret on the Transform's center and scale accordingly.
                var fretPosition = (0.5f - fretPositionPercentage) * bodyScale;

                // Lerp the string spacing between the nut and bridge spacing based on the normalized fret position.
                var stringSpacing = Mathf.Lerp(bridgeSpacing, nutSpacing, fretPositionPercentage);

                for (var @string = 0; @string < stringCount; @string++)
                {
                    // Instiantiate the current position.
                    var stringFretPositionIstance = Instantiate(o, positionsParent, false);
                    stringFretPositionIstance.name = $"String{@string}Fret{fret}";

                    // Calculate the vertical string offset, taking into account potential odd string numbers.
                    var verticalStringOffset =
                        -(@string + 1) + (float)stringCount / 2 + (stringCount % 2 == 0 ? 0.5f : 1);

                    var stringPosition = stringSpacing * verticalStringOffset;

                    // Position the transform locally and store its reference.
                    stringFretPositionIstance.transform.localPosition = new Vector3(fretPosition, stringPosition, 0.0f);
                    _stringFretPositions[@string, fret] = stringFretPositionIstance.transform;
                }
            }

            // The actual plucking position from the 12th fret.
            var horizontalPluckingPosition = bodyScale / 2.0f - pluckingOffset;

            // Lerp the string spacing between the nut and bridge spacing based on the normalized plucking position.
            var pluckingStringSpacing = Mathf.Lerp(nutSpacing, bridgeSpacing, (bodyScale - pluckingOffset) / bodyScale);

            for (var @string = 0; @string < stringCount; @string++)
            {
                // Instiantiate a plucking position.
                var pluckingPositionIstance = Instantiate(o, positionsParent, false);
                pluckingPositionIstance.name = $"PluckingPosition{@string}";

                // Calculate the vertical string offset, taking into account potential odd string numbers.
                var verticalStringOffset =
                    -(@string + 1) + (float)stringCount / 2 + (stringCount % 2 == 0 ? 0.5f : 1);

                var stringPosition = pluckingStringSpacing * verticalStringOffset;

                pluckingPositionIstance.transform.localPosition =
                    new Vector3(horizontalPluckingPosition, stringPosition, 0.0f);
                _pluckingPositions[@string] = pluckingPositionIstance.transform;
            }

            Destroy(o); // Destroy the dummy.
        }

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        private void Update()
        {

            // Draw debug coordinates
            foreach (var p in _stringFretPositions)
                Gizmos.Sphere(
                    p.position,
                    0.001f,
                    Color.yellow
                );
            foreach (var p in _pluckingPositions)
                Gizmos.Sphere(
                    p.position,
                    0.001f,
                    Color.yellow
                );
        }
#endif

        /// <summary>Returns the string/fret's intersection position.</summary>
        public Vector3 StringFretPosition(int @string, int fret) => _stringFretPositions[@string, fret].position;

        /// <summary>Returns the string's plucking position.</summary>
        public Vector3 PluckingPosition(int @string) => _pluckingPositions[@string].position;
 
        /// <summary>The fretboard's rotation</summary>
        public Quaternion Rotation => positionsParent.rotation;
    }
}