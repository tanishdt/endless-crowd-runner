using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Environment.Obstacles
{
    /// <summary>
    /// Rotating blade obstacle. Slices through any crowd stickmen in its path.
    /// </summary>
    public class SpinningBlade : MonoBehaviour
    {
        [Header("Blade Movement")]
        [SerializeField] private float rotationSpeed = 360.0f;
        [SerializeField] private bool oscillateHorizontally = true;
        [SerializeField] private float sweepAmplitude = 3.5f;
        [SerializeField] private float sweepSpeed = 2.0f;
        [SerializeField] private float bladeRadius = 1.2f;

        private Vector3 _centerPos;
        private Transform _rotor;

        private void Awake()
        {
            _centerPos = transform.position;
            _rotor = transform.Find("Rotor") ?? transform;
        }

        private void Start()
        {
            _centerPos = transform.position;
        }

        private void Update()
        {
            // Rotate blade
            if (_rotor != null)
            {
                _rotor.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
            }

            // Oscillate side to side
            if (oscillateHorizontally)
            {
                float x = _centerPos.x + Mathf.Sin(Time.time * sweepSpeed) * sweepAmplitude;
                transform.position = new Vector3(x, _centerPos.y, _centerPos.z);
            }

            // Check contact with player crowd units
            CrowdController crowd = FindObjectOfType<CrowdController>();
            if (crowd == null) return;

            Vector3 bladePos = transform.position;
            float distZ = Mathf.Abs(crowd.CentroidPosition.z - bladePos.z);

            if (distZ <= bladeRadius + crowd.CrowdRadius)
            {
                var units = crowd.Units;
                for (int i = units.Count - 1; i >= 0; i--)
                {
                    StickmanUnit unit = units[i];
                    if (unit == null) continue;

                    float distSq = (unit.transform.position - bladePos).sqrMagnitude;
                    if (distSq <= bladeRadius * bladeRadius)
                    {
                        crowd.RemoveUnits(1);
                        SoundManager.Instance?.PlaySound(SoundEffectType.UnitDeath);
                        break; // Chop 1 unit per frame to avoid instant wipeout
                    }
                }
            }
        }
    }
}
