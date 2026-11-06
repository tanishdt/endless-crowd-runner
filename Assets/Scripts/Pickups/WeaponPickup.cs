using UnityEngine;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Audio;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Visuals;

namespace EndlessCrowdRunner.Pickups
{
    /// <summary>
    /// Pickable crate on the road granting weapon upgrades (Pistol -> SMG -> Shotgun -> AR -> Minigun).
    /// </summary>
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponType targetWeapon = WeaponType.Pistol;
        [SerializeField] private float rotateSpeed = 90.0f;
        [SerializeField] private float bobHeight = 0.25f;

        private Vector3 _startPos;
        private bool _isCollected = false;

        public WeaponType TargetWeapon => targetWeapon;

        private void Start()
        {
            _startPos = transform.position;
        }

        public void Setup(WeaponType weapon)
        {
            targetWeapon = weapon;
            _isCollected = false;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Idle spin and bob
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
            float y = _startPos.y + Mathf.Sin(Time.time * 3.5f) * bobHeight;
            transform.position = new Vector3(_startPos.x, y, _startPos.z);

            // Distance check to player crowd
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

            WeaponSystem ws = crowd.GetComponent<WeaponSystem>();
            if (ws != null)
            {
                ws.UpgradeWeapon(targetWeapon);
            }

            if (VisualAssetFactory.Instance != null)
            {
                // Particle feedback
                var fx = VisualAssetFactory.CreateSparkEffect(transform.position, Color.yellow);
            }

            gameObject.SetActive(false);
            Destroy(gameObject, 0.5f);
        }
    }
}
