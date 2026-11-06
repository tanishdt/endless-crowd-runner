using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Enemies
{
    /// <summary>
    /// AI controller enabling armed enemy stickmen to fire bullets at the approaching player crowd.
    /// </summary>
    public class EnemyShooter : MonoBehaviour
    {
        [Header("Shooting Parameters")]
        [SerializeField] private float fireInterval = 1.3f;
        [SerializeField] private float shootRange = 36.0f;
        [SerializeField] private float bulletSpeed = 22.0f;

        private float _fireTimer;
        private EnemyGroup _group;
        private CrowdController _cachedCrowd;

        public void Initialize(EnemyGroup group, float interval = 1.3f, float range = 36f, float speed = 22f)
        {
            _group = group;
            fireInterval = interval;
            shootRange = range;
            bulletSpeed = speed;
            _fireTimer = Random.Range(0.2f, fireInterval);
        }

        private void Update()
        {
            if (_group == null || _group.IsDefeated || _group.ShooterCount <= 0) return;

            if (_cachedCrowd == null)
            {
                _cachedCrowd = FindObjectOfType<CrowdController>();
                if (_cachedCrowd == null) return;
            }

            Vector3 crowdPos = _cachedCrowd.CentroidPosition;
            float distZ = transform.position.z - crowdPos.z;

            // Only fire if player crowd is approaching within range ahead
            if (distZ > 2.0f && distZ <= shootRange)
            {
                _fireTimer -= Time.deltaTime;
                if (_fireTimer <= 0f)
                {
                    _fireTimer = fireInterval;
                    FireAtPlayer();
                }
            }
        }

        private void FireAtPlayer()
        {
            if (_cachedCrowd == null || _cachedCrowd.RawUnitCount == 0) return;
            if (PoolManager.Instance == null || PoolManager.Instance.Bullets == null) return;

            // Pick a shooter position from group
            Vector3 origin = _group.GetRandomShooterPosition();

            // Pick a target unit within player crowd
            int randomUnitIdx = Random.Range(0, _cachedCrowd.Units.Count);
            StickmanUnit targetUnit = _cachedCrowd.Units[randomUnitIdx];
            Vector3 targetPos = targetUnit != null ? targetUnit.transform.position : _cachedCrowd.CentroidPosition;

            Vector3 direction = (targetPos - origin).normalized;

            Bullet bullet = PoolManager.Instance.Bullets.Get(origin, Quaternion.LookRotation(direction));
            bullet.Fire(direction, bulletSpeed, 1, false, 2.5f);

            SoundManager.Instance?.PlaySound(SoundEffectType.GunshotPistol);
        }
    }
}
