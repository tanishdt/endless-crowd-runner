using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Visuals;

namespace EndlessCrowdRunner.Enemies
{
    /// <summary>
    /// Represents an individual red enemy stickman on the road.
    /// Supports melee clash cancellation and shooting behavior.
    /// </summary>
    public class EnemyUnit : MonoBehaviour, IPoolable
    {
        [Header("Components")]
        [SerializeField] private StickmanAnimator animator;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform weaponMount;
        [SerializeField] private Renderer meshRenderer;

        public bool isShooter = false;
        public int unitValue = 1;
        public int hp = 1;

        private EnemyGroup _parentGroup;
        private GameObject _weaponVisual;

        public Transform WeaponMount => weaponMount != null ? weaponMount : transform;

        private void Awake()
        {
            if (visualRoot == null) visualRoot = transform.Find("Visual") ?? transform;
            if (animator == null) animator = GetComponent<StickmanAnimator>() ?? gameObject.AddComponent<StickmanAnimator>();
            animator.Initialize(visualRoot);
            if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();
        }

        public void Setup(EnemyGroup parent, bool shooter = false)
        {
            _parentGroup = parent;
            isShooter = shooter;
            hp = 1;
            unitValue = 1;

            if (meshRenderer != null)
            {
                meshRenderer.sharedMaterial = VisualAssetFactory.GetEnemyMaterial();
            }

            if (isShooter)
            {
                if (_weaponVisual != null) Destroy(_weaponVisual);
                _weaponVisual = VisualAssetFactory.CreateEnemyGunModel(WeaponMount);
            }
            else if (_weaponVisual != null)
            {
                Destroy(_weaponVisual);
                _weaponVisual = null;
            }

            animator.SetBaseScale(Vector3.one);
            animator.PlaySpawnBounce(0.2f);
        }

        public void OnSpawn()
        {
            hp = 1;
            unitValue = 1;
            isShooter = false;
        }

        public void OnDespawn()
        {
            if (_weaponVisual != null)
            {
                Destroy(_weaponVisual);
                _weaponVisual = null;
            }
        }

        public void TakeDamage(int damage)
        {
            hp -= damage;
            if (hp <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            if (PoolManager.Instance != null && PoolManager.Instance.DeathEffects != null)
            {
                var fx = PoolManager.Instance.DeathEffects.Get(transform.position + Vector3.up * 0.6f, Quaternion.identity);
                fx.Play(new Color(1f, 0.2f, 0.2f));
            }

            if (_parentGroup != null)
            {
                _parentGroup.OnUnitDefeated(this);
            }

            PoolManager.Instance?.EnemyUnits.Return(this);
        }
    }
}
