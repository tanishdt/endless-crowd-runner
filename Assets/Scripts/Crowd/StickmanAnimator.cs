using UnityEngine;

namespace EndlessCrowdRunner.Crowd
{
    /// <summary>
    /// High-performance procedural running animation for stickmen.
    /// Eliminates Animator/Mecanim overhead for 300+ units on mobile.
    /// </summary>
    public class StickmanAnimator : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private float bobFrequency = 14.0f;
        [SerializeField] private float bobHeight = 0.12f;
        [SerializeField] private float tiltAngle = 8.0f;
        [SerializeField] private float rollWobble = 5.0f;

        private Transform _visualRoot;
        private float _phaseOffset;
        private float _spawnBounceTimer = -1f;
        private float _spawnBounceDuration = 0.25f;
        private Vector3 _baseScale = Vector3.one;

        public void Initialize(Transform visualRoot)
        {
            _visualRoot = visualRoot != null ? visualRoot : transform;
            _baseScale = _visualRoot.localScale;
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        public void PlaySpawnBounce(float duration = 0.25f)
        {
            _spawnBounceDuration = duration;
            _spawnBounceTimer = 0f;
            if (_visualRoot != null)
            {
                _visualRoot.localScale = Vector3.zero;
            }
        }

        public void UpdateAnimation(float runSpeed, float deltaTime)
        {
            if (_visualRoot == null) return;

            // Handle spawn scale bounce
            if (_spawnBounceTimer >= 0f)
            {
                _spawnBounceTimer += deltaTime;
                float t = Mathf.Clamp01(_spawnBounceTimer / _spawnBounceDuration);
                // Overshoot bounce curve: f(t) = sin(t * pi * 0.5) + bounce
                float scaleMod = Mathf.Sin(t * Mathf.PI * 0.5f);
                if (t < 0.7f)
                    scaleMod *= 1.2f;
                else
                    scaleMod = Mathf.Lerp(1.2f, 1.0f, (t - 0.7f) / 0.3f);

                _visualRoot.localScale = _baseScale * scaleMod;

                if (t >= 1f)
                {
                    _spawnBounceTimer = -1f;
                    _visualRoot.localScale = _baseScale;
                }
            }

            // Procedural running bob & tilt
            if (runSpeed > 0.1f)
            {
                float time = Time.time * (bobFrequency * (runSpeed / 10.0f)) + _phaseOffset;
                float bobY = Mathf.Abs(Mathf.Sin(time)) * bobHeight;
                float wobbleZ = Mathf.Sin(time * 0.5f) * rollWobble;

                _visualRoot.localPosition = new Vector3(0f, bobY, 0f);
                _visualRoot.localRotation = Quaternion.Euler(tiltAngle, 0f, wobbleZ);
            }
            else
            {
                _visualRoot.localPosition = Vector3.zero;
                _visualRoot.localRotation = Quaternion.identity;
            }
        }

        public void SetBaseScale(Vector3 scale)
        {
            _baseScale = scale;
            if (_visualRoot != null && _spawnBounceTimer < 0f)
            {
                _visualRoot.localScale = scale;
            }
        }
    }
}
