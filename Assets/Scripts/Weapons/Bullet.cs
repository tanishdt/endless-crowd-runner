using UnityEngine;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Enemies;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Visuals;

namespace EndlessCrowdRunner.Weapons
{
    /// <summary>
    /// High-performance pooled projectile for player crowd and enemy weapons.
    /// Uses non-allocating distance and sphere checks.
    /// </summary>
    public class Bullet : MonoBehaviour, IPoolable
    {
        [Header("Components")]
        [SerializeField] private Renderer bulletRenderer;
        [SerializeField] private TrailRenderer trailRenderer;

        private Vector3 _direction;
        private float _speed;
        private int _damage;
        private bool _isPlayerBullet;
        private float _lifeTimer;
        private float _maxLifetime;
        private static readonly Collider[] HitBuffer = new Collider[8];

        private void Awake()
        {
            if (bulletRenderer == null) bulletRenderer = GetComponentInChildren<Renderer>();
            if (trailRenderer == null) trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        public void Fire(Vector3 dir, float speed, int damage, bool isPlayer, float maxLife = 2.0f)
        {
            _direction = dir.normalized;
            _speed = speed;
            _damage = damage;
            _isPlayerBullet = isPlayer;
            _maxLifetime = maxLife;
            _lifeTimer = maxLife;

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.startColor = isPlayer ? Color.yellow : Color.red;
                trailRenderer.endColor = isPlayer ? new Color(1f, 0.8f, 0f, 0f) : new Color(1f, 0f, 0f, 0f);
            }

            if (bulletRenderer != null)
            {
                bulletRenderer.sharedMaterial = VisualAssetFactory.GetBulletMaterial(isPlayer);
            }
        }

        public void OnSpawn()
        {
            _lifeTimer = 2.0f;
        }

        public void OnDespawn()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _lifeTimer -= dt;

            if (_lifeTimer <= 0f)
            {
                Despawn();
                return;
            }

            Vector3 nextPos = transform.position + _direction * (_speed * dt);
            CheckCollisions(transform.position, nextPos);
            transform.position = nextPos;
        }

        private void CheckCollisions(Vector3 currentPos, Vector3 nextPos)
        {
            if (_isPlayerBullet)
            {
                // Check hits against enemy stickmen and bosses
                int count = Physics.OverlapSphereNonAlloc(nextPos, 0.45f, HitBuffer);
                for (int i = 0; i < count; i++)
                {
                    Collider col = HitBuffer[i];
                    if (col == null) continue;

                    // Boss hit check
                    BossController boss = col.GetComponentInParent<BossController>();
                    if (boss != null && !boss.IsDefeated)
                    {
                        boss.TakeDamage(_damage);
                        SpawnHitEffect(nextPos, Color.yellow);
                        Despawn();
                        return;
                    }

                    // Enemy unit hit check
                    EnemyUnit enemy = col.GetComponentInParent<EnemyUnit>();
                    if (enemy != null && enemy.hp > 0)
                    {
                        enemy.TakeDamage(_damage);
                        SpawnHitEffect(nextPos, Color.yellow);
                        Despawn();
                        return;
                    }
                }
            }
            else
            {
                // Enemy bullet hitting player units
                int count = Physics.OverlapSphereNonAlloc(nextPos, 0.45f, HitBuffer);
                for (int i = 0; i < count; i++)
                {
                    Collider col = HitBuffer[i];
                    if (col == null) continue;

                    StickmanUnit unit = col.GetComponentInParent<StickmanUnit>();
                    if (unit != null)
                    {
                        unit.Die();
                        SpawnHitEffect(nextPos, Color.red);
                        Despawn();
                        return;
                    }
                }
            }
        }

        private void SpawnHitEffect(Vector3 pos, Color color)
        {
            if (PoolManager.Instance != null && PoolManager.Instance.HitEffects != null)
            {
                var fx = PoolManager.Instance.HitEffects.Get(pos, Quaternion.identity);
                fx.Play(color);
            }
        }

        private void Despawn()
        {
            PoolManager.Instance?.Bullets.Return(this);
        }
    }
}
