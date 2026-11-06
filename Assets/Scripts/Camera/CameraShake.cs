using UnityEngine;

namespace EndlessCrowdRunner.Camera
{
    /// <summary>
    /// Handles procedural impulse screen shake for explosive boss impacts and collisions.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float _shakeTimer = 0f;
        private float _shakeDuration = 0f;
        private float _shakeMagnitude = 0f;

        public Vector3 ShakeOffset { get; private set; } = Vector3.zero;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Shake(float duration = 0.25f, float magnitude = 0.3f)
        {
            _shakeDuration = duration;
            _shakeTimer = duration;
            _shakeMagnitude = magnitude;
        }

        private void Update()
        {
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float progress = _shakeTimer / _shakeDuration;
                float damper = Mathf.Clamp01(progress);

                float x = (Random.value * 2f - 1f) * _shakeMagnitude * damper;
                float y = (Random.value * 2f - 1f) * _shakeMagnitude * damper;

                ShakeOffset = new Vector3(x, y, 0f);
            }
            else
            {
                ShakeOffset = Vector3.zero;
            }
        }
    }
}
