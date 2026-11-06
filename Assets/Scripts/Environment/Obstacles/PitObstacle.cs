using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Environment.Obstacles
{
    /// <summary>
    /// Pit / hole hazard on the roadway.
    /// Any stickmen running over the pit fall in and are lost.
    /// </summary>
    public class PitObstacle : MonoBehaviour
    {
        [Header("Pit Dimensions")]
        [SerializeField] private float pitWidth = 4.0f;
        [SerializeField] private float pitLength = 5.0f;

        public void Setup(float width, float length)
        {
            pitWidth = width;
            pitLength = length;
        }

        private void Update()
        {
            CrowdController crowd = FindObjectOfType<CrowdController>();
            if (crowd == null) return;

            Vector3 pitPos = transform.position;
            float crowdZ = crowd.CentroidPosition.z;

            if (Mathf.Abs(crowdZ - pitPos.z) > (pitLength * 0.5f) + crowd.CrowdRadius) return;

            float minX = pitPos.x - (pitWidth * 0.5f);
            float maxX = pitPos.x + (pitWidth * 0.5f);
            float minZ = pitPos.z - (pitLength * 0.5f);
            float maxZ = pitPos.z + (pitLength * 0.5f);

            var units = crowd.Units;
            for (int i = units.Count - 1; i >= 0; i--)
            {
                StickmanUnit unit = units[i];
                if (unit == null) continue;

                Vector3 pos = unit.transform.position;
                if (pos.x >= minX && pos.x <= maxX && pos.z >= minZ && pos.z <= maxZ)
                {
                    crowd.RemoveUnits(1);
                    SoundManager.Instance?.PlaySound(SoundEffectType.UnitDeath);
                    break;
                }
            }
        }
    }
}
