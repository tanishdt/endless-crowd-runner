using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Environment.Obstacles
{
    /// <summary>
    /// Wall spanning the road width with a passable gap.
    /// Units that do not steer through the gap hit the wall and are crushed.
    /// </summary>
    public class WallObstacle : MonoBehaviour
    {
        [Header("Gap Settings")]
        [SerializeField] private float gapCenterX = 0f;
        [SerializeField] private float gapWidth = 3.8f;
        [SerializeField] private float wallThickness = 1.0f;

        public float GapCenterX => gapCenterX;
        public float GapWidth => gapWidth;

        public void Setup(float gapX, float width)
        {
            gapCenterX = gapX;
            gapWidth = Mathf.Max(2.5f, width);
        }

        private void Update()
        {
            CrowdController crowd = FindObjectOfType<CrowdController>();
            if (crowd == null) return;

            float wallZ = transform.position.z;
            float crowdZ = crowd.CentroidPosition.z;

            // Only check when crowd is at wall Z boundary
            if (Mathf.Abs(crowdZ - wallZ) > wallThickness + crowd.CrowdRadius) return;

            float halfGap = gapWidth * 0.5f;
            float gapMin = gapCenterX - halfGap;
            float gapMax = gapCenterX + halfGap;

            var units = crowd.Units;
            for (int i = units.Count - 1; i >= 0; i--)
            {
                StickmanUnit unit = units[i];
                if (unit == null) continue;

                Vector3 uPos = unit.transform.position;
                if (Mathf.Abs(uPos.z - wallZ) <= wallThickness * 0.5f)
                {
                    // If unit is outside the gap opening
                    if (uPos.x < gapMin || uPos.x > gapMax)
                    {
                        crowd.RemoveUnits(1);
                        SoundManager.Instance?.PlaySound(SoundEffectType.UnitDeath);
                        break;
                    }
                }
            }
        }
    }
}
