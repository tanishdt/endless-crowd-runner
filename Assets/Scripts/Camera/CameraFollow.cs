using UnityEngine;
using EndlessCrowdRunner.Crowd;

namespace EndlessCrowdRunner.Camera
{
    /// <summary>
    /// Smooth third-person over-the-shoulder portrait camera follow.
    /// Dynamically expands framing as crowd grows to keep all stickmen on-screen.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private CrowdController targetCrowd;
        [SerializeField] private Vector3 baseOffset = new Vector3(0f, 13f, -12f);
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.5f, 6.0f);

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeedX = 8.0f;
        [SerializeField] private float smoothSpeedZ = 20.0f;

        [Header("Dynamic Zoom")]
        [SerializeField] private float maxZoomOutDistance = 7.0f;
        [SerializeField] private float zoomOutUnitsThreshold = 250f;

        private float _currentX;
        private Vector3 _currentVelocity;

        public void SetTarget(CrowdController crowd)
        {
            targetCrowd = crowd;
            if (crowd != null)
            {
                _currentX = crowd.CentroidPosition.x;
                Vector3 initPos = crowd.CentroidPosition + baseOffset;
                transform.position = initPos;
                transform.LookAt(crowd.CentroidPosition + lookOffset);
            }
        }

        private void LateUpdate()
        {
            if (targetCrowd == null) return;

            Vector3 targetPos = targetCrowd.CentroidPosition;

            // Smooth X tracking with damping for juicy feel
            _currentX = Mathf.Lerp(_currentX, targetPos.x, Time.deltaTime * smoothSpeedX);

            // Dynamic camera pull-back based on crowd count
            int count = targetCrowd.CurrentCount;
            float zoomFactor = Mathf.Clamp01(count / zoomOutUnitsThreshold);
            Vector3 dynamicOffset = baseOffset + new Vector3(0f, zoomFactor * 4.0f, -zoomFactor * maxZoomOutDistance);

            Vector3 desiredPosition = new Vector3(_currentX + dynamicOffset.x, targetPos.y + dynamicOffset.y, targetPos.z + dynamicOffset.z);

            // Apply camera shake if present
            if (CameraShake.Instance != null)
            {
                desiredPosition += CameraShake.Instance.ShakeOffset;
            }

            transform.position = desiredPosition;

            // Look slightly ahead of crowd
            Vector3 lookTarget = new Vector3(_currentX, targetPos.y, targetPos.z) + lookOffset;
            transform.rotation = Quaternion.Euler(38.0f, 0f, 0f);
        }
    }
}
