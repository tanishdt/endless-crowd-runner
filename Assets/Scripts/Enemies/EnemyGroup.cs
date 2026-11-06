using System.Collections.Generic;
using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Visuals;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Enemies
{
    /// <summary>
    /// Holds a packed mob of enemy stickmen standing on the road.
    /// Displays a floating count badge and resolves melee combat clash with the player crowd.
    /// </summary>
    public class EnemyGroup : MonoBehaviour
    {
        [Header("Group Info")]
        [SerializeField] private int initialCount = 15;
        [SerializeField] private bool hasShooters = false;
        [SerializeField] private float groupSpacing = 0.65f;

        [Header("Visual Badge")]
        [SerializeField] private TextMesh countText;
        [SerializeField] private Renderer badgeRenderer;

        private readonly List<EnemyUnit> _units = new List<EnemyUnit>(128);
        private readonly List<EnemyUnit> _shooters = new List<EnemyUnit>(16);
        private bool _isDefeated = false;
        private EnemyShooter _shooterAI;
        private CrowdController _cachedCrowd;

        public int CurrentCount => _units.Count;
        public int ShooterCount => _shooters.Count;
        public bool IsDefeated => _isDefeated;
        public IReadOnlyList<EnemyUnit> Units => _units;

        private void Awake()
        {
            if (countText == null) countText = GetComponentInChildren<TextMesh>();
            if (badgeRenderer == null) badgeRenderer = GetComponentInChildren<Renderer>();
        }

        public void Setup(int count, bool shooters = false, float spacing = 0.65f)
        {
            initialCount = Mathf.Max(1, count);
            hasShooters = shooters;
            groupSpacing = spacing;
            _isDefeated = false;

            ClearUnits();
            SpawnMob();
            UpdateBadge();

            if (hasShooters && _shooters.Count > 0)
            {
                if (_shooterAI == null) _shooterAI = gameObject.AddComponent<EnemyShooter>();
                _shooterAI.Initialize(this);
            }
            else if (_shooterAI != null)
            {
                Destroy(_shooterAI);
                _shooterAI = null;
            }
        }

        private void SpawnMob()
        {
            if (PoolManager.Instance == null || PoolManager.Instance.EnemyUnits == null) return;

            int shooterCount = hasShooters ? Mathf.Clamp(Mathf.RoundToInt(initialCount * 0.25f), 1, 6) : 0;
            int shootersSpawned = 0;

            for (int i = 0; i < initialCount; i++)
            {
                Vector3 offset = FormationHelper.GetFormationOffset(i, groupSpacing);
                Vector3 worldPos = transform.position + offset;

                EnemyUnit unit = PoolManager.Instance.EnemyUnits.Get(worldPos, Quaternion.Euler(0f, 180f, 0f));
                unit.transform.SetParent(transform);

                bool isShooter = false;
                // Place shooters in back ranks
                if (shootersSpawned < shooterCount && i >= initialCount - shooterCount)
                {
                    isShooter = true;
                    shootersSpawned++;
                    _shooters.Add(unit);
                }

                unit.Setup(this, isShooter);
                _units.Add(unit);
            }
        }

        private void Update()
        {
            if (_isDefeated || _units.Count == 0) return;

            if (_cachedCrowd == null)
            {
                _cachedCrowd = FindObjectOfType<CrowdController>();
                if (_cachedCrowd == null) return;
            }

            Vector3 crowdPos = _cachedCrowd.CentroidPosition;
            float crowdRadius = _cachedCrowd.CrowdRadius;
            float groupZ = transform.position.z;

            // Check if player crowd has physically reached this group
            if (crowdPos.z + crowdRadius >= groupZ - 1.2f && crowdPos.z - crowdRadius <= groupZ + 3.0f)
            {
                // Also check horizontal alignment
                float distLeftRight = Mathf.Abs(crowdPos.x - transform.position.x);
                if (distLeftRight <= crowdRadius + (groupSpacing * 2.5f))
                {
                    ResolveCombatClash();
                }
            }
        }

        /// <summary>
        /// Resolves melee collision: player and enemy units cancel each other 1:1.
        /// </summary>
        private void ResolveCombatClash()
        {
            if (_cachedCrowd == null || _cachedCrowd.CurrentCount <= 0 || _units.Count == 0) return;

            // Cancel out units rapidly during the clash
            int clashBatch = Mathf.Min(4, _units.Count, _cachedCrowd.CurrentCount);

            for (int i = 0; i < clashBatch; i++)
            {
                if (_units.Count == 0 || _cachedCrowd.CurrentCount == 0) break;

                // Remove 1 enemy
                int lastEnemyIdx = _units.Count - 1;
                EnemyUnit enemy = _units[lastEnemyIdx];
                _units.RemoveAt(lastEnemyIdx);
                _shooters.Remove(enemy);
                enemy.Die();

                // Remove 1 player unit
                _cachedCrowd.RemoveUnits(1);
            }

            UpdateBadge();

            if (_units.Count == 0)
            {
                DefeatGroup();
            }
        }

        public void OnUnitDefeated(EnemyUnit unit)
        {
            _units.Remove(unit);
            _shooters.Remove(unit);
            UpdateBadge();

            if (_units.Count == 0 && !_isDefeated)
            {
                DefeatGroup();
            }
        }

        private void DefeatGroup()
        {
            if (_isDefeated) return;
            _isDefeated = true;

            SoundManager.Instance?.PlaySound(SoundEffectType.ExplosionSmall);
            FloatingTextManager.Instance?.SpawnFloatingText("CLEARED!", transform.position + Vector3.up * 2f, Color.yellow);

            if (countText != null)
            {
                countText.text = "0";
                countText.gameObject.SetActive(false);
            }

            if (badgeRenderer != null)
            {
                badgeRenderer.gameObject.SetActive(false);
            }
        }

        public void UpdateBadge()
        {
            if (countText != null)
            {
                countText.text = _units.Count.ToString();
            }

            // Keep badge floating directly above the group
            if (badgeRenderer != null)
            {
                float height = 2.4f + Mathf.Sqrt(_units.Count) * 0.15f;
                badgeRenderer.transform.localPosition = new Vector3(0f, height, 0f);
            }
        }

        public Vector3 GetRandomShooterPosition()
        {
            if (_shooters.Count > 0)
            {
                int idx = Random.Range(0, _shooters.Count);
                if (_shooters[idx] != null) return _shooters[idx].transform.position + Vector3.up * 0.8f;
            }
            return transform.position + Vector3.up * 1.0f;
        }

        public void ClearUnits()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null)
                {
                    PoolManager.Instance?.EnemyUnits.Return(_units[i]);
                }
            }
            _units.Clear();
            _shooters.Clear();
        }

        private void OnDestroy()
        {
            ClearUnits();
        }
    }
}
