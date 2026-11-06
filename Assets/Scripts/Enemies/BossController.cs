using System;
using UnityEngine;
using UnityEngine.UI;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Camera;
using EndlessCrowdRunner.Audio;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Visuals;

namespace EndlessCrowdRunner.Enemies
{
    /// <summary>
    /// Massive boss blocking the road with a large floating health bar.
    /// Can be shot down by crowd weapons or clashed with in high-stakes melee.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int maxHp = 250;
        [SerializeField] private int currentHp = 250;
        [SerializeField] private float attackInterval = 3.2f;

        [Header("UI & Visuals")]
        [SerializeField] private TextMesh nameText;
        [SerializeField] private TextMesh hpText;
        [SerializeField] private Transform healthBarFill;
        [SerializeField] private Transform visualRoot;

        private bool _isDefeated = false;
        private float _attackTimer;
        private Vector3 _originalScale = Vector3.one;
        private CrowdController _cachedCrowd;

        public event Action<BossController> OnBossDefeated;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public bool IsDefeated => _isDefeated;

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (visualRoot == null) visualRoot = transform.Find("Visual") ?? transform;
        }

        public void Setup(int hp, string bossName = "TITAN CRUSHER")
        {
            maxHp = Mathf.Max(50, hp);
            currentHp = maxHp;
            _isDefeated = false;
            _attackTimer = attackInterval;

            if (nameText != null) nameText.text = bossName;
            UpdateHealthBar();
        }

        public void TakeDamage(int damage)
        {
            if (_isDefeated) return;

            currentHp -= damage;
            UpdateHealthBar();

            // Punch scale bounce on hit
            transform.localScale = _originalScale * 1.05f;

            if (currentHp <= 0)
            {
                Die();
            }
        }

        private void Update()
        {
            if (_isDefeated) return;

            // Recover scale from hit pulse
            transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.deltaTime * 10f);

            if (_cachedCrowd == null)
            {
                _cachedCrowd = FindObjectOfType<CrowdController>();
                if (_cachedCrowd == null) return;
            }

            Vector3 crowdPos = _cachedCrowd.CentroidPosition;
            float distZ = transform.position.z - crowdPos.z;

            // Periodic shockwave attack while crowd is approaching
            if (distZ > 3.0f && distZ < 40.0f)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _attackTimer = attackInterval;
                    ExecuteGroundSlam();
                }
            }

            // Melee clash if crowd reaches boss position
            if (distZ <= 1.5f && distZ >= -2.0f)
            {
                ResolveBossClash();
            }
        }

        private void ExecuteGroundSlam()
        {
            CameraShake.Instance?.Shake(0.35f, 0.4f);
            SoundManager.Instance?.PlaySound(SoundEffectType.BossSlam);

            // Ground slam visual shockwave ring
            VisualAssetFactory.CreateShockwaveRing(transform.position + Vector3.up * 0.1f);
        }

        private void ResolveBossClash()
        {
            if (_isDefeated || _cachedCrowd == null || _cachedCrowd.CurrentCount <= 0) return;

            // Boss HP cancels player units 1:1
            int clash = Mathf.Min(currentHp, _cachedCrowd.CurrentCount, 15);
            currentHp -= clash;
            _cachedCrowd.RemoveUnits(clash);
            UpdateHealthBar();

            CameraShake.Instance?.Shake(0.2f, 0.3f);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            if (_isDefeated) return;
            _isDefeated = true;
            currentHp = 0;

            CameraShake.Instance?.Shake(0.5f, 0.6f);
            SoundManager.Instance?.PlaySound(SoundEffectType.ExplosionLarge);

            // Boss defeat rewards
            if (_cachedCrowd != null)
            {
                _cachedCrowd.AddUnits(30, transform.position);
            }

            FloatingTextManager.Instance?.SpawnFloatingText("BOSS DEFEATED! +30 SQUAD", transform.position + Vector3.up * 3f, Color.yellow);

            OnBossDefeated?.Invoke(this);

            // Big death burst
            if (PoolManager.Instance != null && PoolManager.Instance.DeathEffects != null)
            {
                var fx = PoolManager.Instance.DeathEffects.Get(transform.position + Vector3.up * 2f, Quaternion.identity);
                fx.Play(Color.red, 3.0f);
            }

            Destroy(gameObject, 0.3f);
        }

        private void UpdateHealthBar()
        {
            float fillRatio = Mathf.Clamp01((float)currentHp / maxHp);

            if (healthBarFill != null)
            {
                healthBarFill.localScale = new Vector3(fillRatio, 1f, 1f);
            }

            if (hpText != null)
            {
                hpText.text = $"{Mathf.Max(0, currentHp)} / {maxHp}";
            }
        }
    }
}
