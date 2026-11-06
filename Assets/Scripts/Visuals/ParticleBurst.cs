using UnityEngine;
using EndlessCrowdRunner.Pooling;

namespace EndlessCrowdRunner.Visuals
{
    /// <summary>
    /// Pooled particle burst for unit spawns, deaths, bullet impacts, and explosions.
    /// Auto-recycles back to pool upon completion.
    /// </summary>
    public class ParticleBurst : MonoBehaviour, IPoolable
    {
        [SerializeField] private ParticleSystem ps;

        private float _lifetime = 0.6f;
        private float _timer = 0f;

        private void Awake()
        {
            if (ps == null) ps = GetComponent<ParticleSystem>();
        }

        public void Play(Color color, float scale = 1.0f)
        {
            if (ps == null) return;

            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.2f * scale;

            transform.localScale = Vector3.one * scale;
            _lifetime = main.startLifetime.constantMax;
            _timer = _lifetime;

            ps.Clear();
            ps.Play();
        }

        public void OnSpawn()
        {
            _timer = _lifetime;
        }

        public void OnDespawn()
        {
            if (ps != null)
            {
                ps.Stop();
                ps.Clear();
            }
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                PoolManager.Instance?.DeathEffects?.Return(this);
            }
        }
    }
}
