using System;
using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Enemies;
using EndlessCrowdRunner.Audio;
using EndlessCrowdRunner.UI;

namespace EndlessCrowdRunner.Weapons
{
    /// <summary>
    /// Manages player crowd weapon tier, auto-targeting forward enemies, and projectile firing.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private WeaponType currentWeapon = WeaponType.None;
        [SerializeField] private CrowdController crowdController;

        private WeaponData _currentData;
        private float _fireTimer;

        public event Action<WeaponType, string> OnWeaponUpgraded;

        public WeaponType CurrentWeapon => currentWeapon;
        public WeaponData CurrentData => _currentData;

        private void Awake()
        {
            if (crowdController == null) crowdController = GetComponent<CrowdController>();
            SetWeapon(WeaponType.None);
        }

        public void SetWeapon(WeaponType type)
        {
            currentWeapon = type;
            _currentData = WeaponData.GetDefault(type);
            _fireTimer = 0f;

            crowdController?.SyncWeaponVisuals(type);
            OnWeaponUpgraded?.Invoke(type, _currentData.weaponName);
        }

        public void UpgradeWeapon(WeaponType newType)
        {
            if (newType > currentWeapon)
            {
                SetWeapon(newType);
                SoundManager.Instance?.PlaySound(SoundEffectType.Powerup);
                FloatingTextManager.Instance?.SpawnFloatingText($"{_currentData.weaponName.ToUpper()}!", transform.position + Vector3.up * 2f, Color.cyan);
            }
        }

        private void Update()
        {
            if (currentWeapon == WeaponType.None || _currentData == null) return;
            if (crowdController == null || crowdController.RawUnitCount == 0) return;

            _fireTimer += Time.deltaTime;
            float interval = 1.0f / Mathf.Max(0.1f, _currentData.fireRate);

            if (_fireTimer >= interval)
            {
                _fireTimer = 0f;
                TryAutoFire();
            }
        }

        /// <summary>
        /// Scans forward cone for enemies or boss and fires bullets.
        /// </summary>
        private void TryAutoFire()
        {
            Vector3 crowdPos = crowdController.CentroidPosition;
            Vector3 aimTarget;

            // First check if a boss is ahead
            BossController boss = FindObjectOfType<BossController>();
            if (boss != null && !boss.IsDefeated && boss.transform.position.z > crowdPos.z && (boss.transform.position.z - crowdPos.z) <= _currentData.range)
            {
                aimTarget = boss.transform.position + Vector3.up * 1.5f;
            }
            else
            {
                // Find closest active enemy group ahead
                EnemyGroup closestGroup = null;
                float minDistZ = float.MaxValue;
                EnemyGroup[] groups = FindObjectsOfType<EnemyGroup>();

                for (int i = 0; i < groups.Length; i++)
                {
                    EnemyGroup g = groups[i];
                    if (g == null || g.IsDefeated || g.CurrentCount == 0) continue;

                    float distZ = g.transform.position.z - crowdPos.z;
                    if (distZ > 1.0f && distZ <= _currentData.range && distZ < minDistZ)
                    {
                        minDistZ = distZ;
                        closestGroup = g;
                    }
                }

                if (closestGroup != null)
                {
                    aimTarget = closestGroup.transform.position + Vector3.up * 0.8f;
                }
                else
                {
                    // No targets in range
                    return;
                }
            }

            ExecuteVolley(aimTarget);
        }

        private void ExecuteVolley(Vector3 targetPos)
        {
            if (PoolManager.Instance == null || PoolManager.Instance.Bullets == null) return;

            // Fire from the front units (up to 4 shooters simultaneously for punchy feeling)
            int shooterCount = Mathf.Clamp(crowdController.RawUnitCount / 3, 1, 4);

            for (int s = 0; s < shooterCount; s++)
            {
                int unitIdx = Mathf.Min(s, crowdController.RawUnitCount - 1);
                StickmanUnit unit = crowdController.Units[unitIdx];
                Vector3 origin = unit != null ? unit.WeaponMount.position : crowdController.CentroidPosition + Vector3.up * 0.8f;

                // Fire pellets (1 for most weapons, 5 for shotgun)
                for (int p = 0; p < _currentData.pelletsPerShot; p++)
                {
                    Vector3 baseDir = (targetPos - origin).normalized;
                    if (_currentData.spreadAngle > 0.01f)
                    {
                        float yaw = UnityEngine.Random.Range(-_currentData.spreadAngle, _currentData.spreadAngle);
                        float pitch = UnityEngine.Random.Range(-_currentData.spreadAngle * 0.5f, _currentData.spreadAngle * 0.5f);
                        baseDir = Quaternion.Euler(pitch, yaw, 0f) * baseDir;
                    }

                    Bullet b = PoolManager.Instance.Bullets.Get(origin, Quaternion.LookRotation(baseDir));
                    b.Fire(baseDir, _currentData.bulletSpeed, _currentData.damage, true);
                }
            }

            SoundManager.Instance?.PlaySound(_currentData.fireSound);
        }
    }
}
