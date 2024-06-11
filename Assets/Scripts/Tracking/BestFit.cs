using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Tracking.Old;
using UnityEngine;

namespace Tracking
{
    public static class BestFit
    {
        /// <summary>
        /// Compute the best fit transformation matrix that transforms the source positions into destination positions.
        /// <para>Both lists should be the same lenght.</para>
        /// </summary>
        public static Matrix4x4 Fit(IReadOnlyList<Vector3> sourcePositions, IReadOnlyList<Vector3> destinationPositions)
        {
            // TODO: implement in a less expensive way, this works but is VERY, VERY suboptimal.
            
            // Convert into multidimensional arrays.
            var actuals = new double[destinationPositions.Count, 3];
            var nominals = new double[destinationPositions.Count, 3];
            for (var i = 0; i < destinationPositions.Count; i++)
            {
                actuals[i, 0] = destinationPositions[i].x;
                actuals[i, 1] = destinationPositions[i].y;
                actuals[i, 2] = destinationPositions[i].z;
                nominals[i, 0] = sourcePositions[i].x;
                nominals[i, 1] = sourcePositions[i].y;
                nominals[i, 2] = sourcePositions[i].z;
            }

            // Convert into matrixes.
            var a = Matrix<double>.Build.DenseOfArray(actuals);
            var n = Matrix<double>.Build.DenseOfArray(nominals);

            // Compute the best fit transformation matrix.
            var t = new Transform3D(a, n);
            t.CalcTransform(n, a);
            var m = t.TransformMatrix;

            // Copy the transformation matrix into an Unity matrix.
            var matrix = Matrix4x4.zero;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    matrix[i, j] = (float)m[i, j];
                }
            }

            return matrix;
        }
    }
}