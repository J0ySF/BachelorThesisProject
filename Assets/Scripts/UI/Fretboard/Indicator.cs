using System.Linq;
using UnityEngine;

namespace UI.Fretboard
{
    /// <summary>
    /// Represent an interaction cue on a fretboard. The closer to zero the distance value is, the more the indicator
    /// converges on the playing position to indicate timing.
    /// </summary>
    public sealed class Indicator : MonoBehaviour
    {
        [SerializeField] private Transform frettingSpotTransform;

        [SerializeField] private MeshRenderer frettingSpotCenter;
        [SerializeField] private LineRenderer frettingSpotCircle;

        [SerializeField] private Transform pluckingSpotTransform;

        [SerializeField] private MeshRenderer pluckingSpotCenterDot;
        [SerializeField] private MeshRenderer pluckingSpotLeftDot;
        [SerializeField] private MeshRenderer pluckingSpotRightDot;

        // Values in millimeters
        [SerializeField] private float startFrettingCircleRadius = 40f;
        [SerializeField] private float frettingCenterDiameter = 6f;

        [SerializeField] private float startPluckingDotsDistance = 64f;
        [SerializeField] private float pluckingDotsDiameter = 8f;

        [SerializeField] private Gradient radiusProgressGradient;
        [SerializeField] private Gradient dotProgressGradient;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;

        /// <summary>Disables the indicator.</summary>
        public void Disable() => frettingSpotTransform.localScale = pluckingSpotTransform.localScale = Vector3.zero;

        /// <summary>Enables the indicator with the given configuration.</summary>
        public void Enable(Vector3? frettingPosition, Vector3 pluckingPosition, Quaternion rotation,
            float convergence, bool countIn)
        {
            var radiusColor = radiusProgressGradient.Evaluate(convergence) * (countIn ? inactiveColor : activeColor);
            var dotColor = dotProgressGradient.Evaluate(convergence) * (countIn ? inactiveColor : Color.white);

            if (frettingPosition is not null)
            {
                // Update the indicator radius' color based on convergence using a gradient.
                frettingSpotCenter.material.color = dotColor;
                frettingSpotCircle.startColor = radiusColor;
                frettingSpotCircle.endColor = radiusColor;

                // Set the indicator radius' geometry based on convergence.
                // When fully closed should match the center's radius.
                var circleRadius = Mathf.Lerp(startFrettingCircleRadius, frettingCenterDiameter / 4.0f, convergence);
                var pos = CirclePoints.Select(p => p * circleRadius).ToArray();
                frettingSpotCircle.SetPositions(pos);

                // Set visible at location/rotation
                frettingSpotTransform.localScale = Vector3.one;
                frettingSpotTransform.position = frettingPosition.Value;
                frettingSpotTransform.rotation = rotation;
            }
            else
            {
                // Set invisible at location
                frettingSpotTransform.localScale = Vector3.zero;
            }

            // Set the top and bottom plucking bars' positions and colors based on convergence.
            var dotDistance = Mathf.Lerp(startPluckingDotsDistance, 0, convergence);
            pluckingSpotLeftDot.transform.localPosition = new Vector3(-dotDistance, 0);
            pluckingSpotRightDot.transform.localPosition = new Vector3(dotDistance, 0);
            pluckingSpotCenterDot.material.color = dotColor;
            pluckingSpotLeftDot.material.color = pluckingSpotRightDot.material.color = radiusColor;

            // Set visible at location/rotation
            pluckingSpotTransform.localScale = Vector3.one;
            pluckingSpotTransform.position = pluckingPosition;
            pluckingSpotTransform.rotation = rotation;
        }

        private void Start()
        {
            // Configure the elements' scale.

            frettingSpotCenter.transform.localScale =
                new Vector3(frettingCenterDiameter, frettingCenterDiameter, frettingCenterDiameter);

            frettingSpotCircle.widthMultiplier = frettingCenterDiameter / 2.0f;
            frettingSpotCircle.positionCount = CirclePoints.Length;

            pluckingSpotCenterDot.transform.localScale = pluckingSpotLeftDot.transform.localScale =
                pluckingSpotRightDot.transform.localScale = new Vector3(pluckingDotsDiameter, pluckingDotsDiameter);
        }

        /// <summary>Hardcoded list of circle points.</summary>
        private static readonly Vector3[] CirclePoints =
        {
            new Vector3(1.0000000f, 0),
            new Vector3(0.9807853f, -0.1950903f),
            new Vector3(0.9238795f, -0.3826834f),
            new Vector3(0.8314696f, -0.5555702f),
            new Vector3(0.7071068f, -0.7071068f),
            new Vector3(0.5555702f, -0.8314696f),
            new Vector3(0.3826834f, -0.9238795f),
            new Vector3(0.1950903f, -0.9807853f),
            new Vector3(0, -1.0000000f),
            new Vector3(-0.1950903f, -0.9807853f),
            new Vector3(-0.3826834f, -0.9238795f),
            new Vector3(-0.5555702f, -0.8314696f),
            new Vector3(-0.7071068f, -0.7071068f),
            new Vector3(-0.8314696f, -0.5555702f),
            new Vector3(-0.9238795f, -0.3826834f),
            new Vector3(-0.9807853f, -0.1950903f),
            new Vector3(-1.0000000f, 0),
            new Vector3(-0.9807853f, 0.1950903f),
            new Vector3(-0.9238795f, 0.3826834f),
            new Vector3(-0.8314696f, 0.5555702f),
            new Vector3(-0.7071068f, 0.7071068f),
            new Vector3(-0.5555702f, 0.8314696f),
            new Vector3(-0.3826834f, 0.9238795f),
            new Vector3(-0.1950903f, 0.9807853f),
            new Vector3(0, 1.0000000f),
            new Vector3(0.1950903f, 0.9807853f),
            new Vector3(0.3826834f, 0.9238795f),
            new Vector3(0.5555702f, 0.8314696f),
            new Vector3(0.7071068f, 0.7071068f),
            new Vector3(0.8314696f, 0.5555702f),
            new Vector3(0.9238795f, 0.3826834f),
            new Vector3(0.9807853f, 0.1950903f)
        };
    }
}