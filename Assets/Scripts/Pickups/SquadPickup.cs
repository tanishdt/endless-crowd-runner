using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Audio;
using EndlessCrowdRunner.UI;

namespace EndlessCrowdRunner.Pickups
{
    /// <summary>
    /// Pickable squad bonus beacon adding 10 stickmen or 1 heavy gunner worth 5 units.
    /// </summary>
    public class SquadPickup : MonoBehaviour
    {
        [SerializeField] private PickupType pickupType = PickupType.Squad10;
        [SerializeField] private float rotateSpeed = 80.0f;
        [SerializeField] private float bobHeight = 0.2f;

        private Vector3 _startPos;
        private bool _isCollected = false;

        public void Setup(PickupType type)
        {
            pickupType = type;
            _isCollected = false;
        }

        private void Start()
        {
            _startPos = transform.position;
        }

        private void Update()
        {
            if (_isCollected) return;

            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
            float y = _startPos.y + Mathf.Sin(Time.time * 3.0f) * bobHeight;
            transform.position = new Vector3(_startPos.x, y, _startPos.z);

            CrowdController crowd = FindObjectOfType<CrowdController>();
            if (crowd == null) return;

            Vector3 crowdPos = crowd.CentroidPosition;
            float distZ = Mathf.Abs(crowdPos.z - transform.position.z);
            float distX = Mathf.Abs(crowdPos.x - transform.position.x);

            if (distZ < 1.2f && distX <= crowd.CrowdRadius + 1.2f)
            {
                Collect(crowd);
            }
        }

        private void Collect(CrowdController crowd)
        {
            if (_isCollected) return;
            _isCollected = true;

            if (pickupType == PickupType.Squad10)
            {
                crowd.AddUnits(10, transform.position);
                FloatingTextManager.Instance?.SpawnFloatingText("+10 SQUAD!", transform.position + Vector3.up * 2f, Color.green);
                SoundManager.Instance?.PlaySound(SoundEffectType.Powerup);
            }
            else if (pickupType == PickupType.HeavyGunner)
            {
                crowd.AddUnits(1, transform.position, isHeavy: true);
                FloatingTextManager.Instance?.SpawnFloatingText("HEAVY GUNNER (+5)!", transform.position + Vector3.up * 2f, Color.yellow);
                SoundManager.Instance?.PlaySound(SoundEffectType.Powerup);
            }

            gameObject.SetActive(false);
            Destroy(gameObject, 0.5f);
        }
    }
}
