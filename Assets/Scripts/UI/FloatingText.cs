using UnityEngine;
using EndlessCrowdRunner.Pooling;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Pooled floating text popup (+10, -5, CLEARED, WEAPON UPGRADE) that floats up and fades.
    /// </summary>
    public class FloatingText : MonoBehaviour, IPoolable
    {
        [SerializeField] private TextMesh textMesh;
        [SerializeField] private float floatSpeed = 3.0f;
        [SerializeField] private float lifetime = 0.85f;

        private float _timer;
        private Color _baseColor = Color.white;

        private void Awake()
        {
            if (textMesh == null) textMesh = GetComponent<TextMesh>();
        }

        public void Setup(string text, Color color)
        {
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
            }
            _baseColor = color;
            _timer = lifetime;
            transform.localScale = Vector3.one * 1.3f;
        }

        public void OnSpawn()
        {
            _timer = lifetime;
        }

        public void OnDespawn()
        {
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _timer -= dt;

            // Float up
            transform.position += Vector3.up * (floatSpeed * dt);

            // Face main camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            // Settle scale
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, dt * 6.0f);

            // Fade alpha
            if (textMesh != null)
            {
                float alpha = Mathf.Clamp01(_timer / (lifetime * 0.5f));
                Color c = _baseColor;
                c.a = alpha;
                textMesh.color = c;
            }

            if (_timer <= 0f)
            {
                PoolManager.Instance?.FloatingTexts?.Return(this);
            }
        }
    }
}
