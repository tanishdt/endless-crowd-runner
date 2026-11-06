using UnityEngine;

namespace EndlessCrowdRunner.Crowd
{
    /// <summary>
    /// Computes tight, organic disk formation positions using Fermat's spiral / phyllotaxis.
    /// Units pack densely without overlapping and expand/contract naturally with count changes.
    /// </summary>
    public static class FormationHelper
    {
        // Golden angle in radians: ~137.507764 degrees
        private const float GoldenAngle = 2.39996323f;

        /// <summary>
        /// Calculates the local offset (X, Z) for the unit at unitIndex within a crowd.
        /// </summary>
        /// <param name="unitIndex">0-based index of the unit.</param>
        /// <param name="spacing">Distance multiplier between units.</param>
        /// <param name="aspectRatio">Width-to-depth compression ratio (e.g. 1.25 for wider crowd).</param>
        /// <returns>Local 3D offset relative to the crowd centroid.</returns>
        public static Vector3 GetFormationOffset(int unitIndex, float spacing, float aspectRatio = 1.2f)
        {
            if (unitIndex == 0)
                return Vector3.zero;

            // Fermat spiral: radius proportional to sqrt of index
            float radius = spacing * Mathf.Sqrt(unitIndex);
            float angle = unitIndex * GoldenAngle;

            float x = radius * Mathf.Cos(angle) * aspectRatio;
            float z = radius * Mathf.Sin(angle);

            return new Vector3(x, 0.0f, z);
        }

        /// <summary>
        /// Estimates the outer radius of a crowd with unitCount stickmen.
        /// </summary>
        public static float GetCrowdRadius(int unitCount, float spacing, float aspectRatio = 1.2f)
        {
            if (unitCount <= 1) return spacing * 0.5f;
            return spacing * Mathf.Sqrt(unitCount) * Mathf.Max(1.0f, aspectRatio);
        }
    }
}
