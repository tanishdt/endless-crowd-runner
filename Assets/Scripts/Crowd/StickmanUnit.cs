using UnityEngine;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Visuals;
using EndlessCrowdRunner.Weapons;

namespace EndlessCrowdRunner.Crowd
{
    /// <summary>
    /// Represents an individual stickman in the player's crowd.
    /// Manages formation positioning, weapon visuals, health, and pooling lifecycle.
    /// </summary>
    public class StickmanUnit : MonoBehaviour, IPoolable
    {
        [Header("Components")]
        [SerializeField] private StickmanAnimator animator;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform weaponMount;
        [SerializeField] private Renderer meshRenderer;

        [Header("Unit Properties")]
        public int unitValue = 1; // 1 for regular, 5 for heavy gunner
        public int formationIndex = 0;
        public bool isHeavy = false;

        private Vector3 _targetLocalPosition;
        private GameObject _equippedWeaponVisual;
        private WeaponType _currentWeaponType = WeaponType.None;

        public Transform WeaponMount => weaponMount != null ? weaponMount : transform;
        public Vector3 TargetLocalPosition
        {
            get => _targetLocalPosition;
            set => _targetLocalPosition = value;
        }

        private void Awake()
        {
            if (visualRoot == null) visualRoot = transform.Find("Visual") ?? transform;
            if (animator == null) animator = GetComponent<StickmanAnimator>() ?? gameObject.AddComponent<StickmanAnimator>();
            animator.Initialize(visualRoot);

            if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();
        }

        public void OnSpawn()
        {
            unitValue = 1;
            isHeavy = false;
            formationIndex = 0;
            _targetLocalPosition = Vector3.zero;
            transform.localPosition = Vector3.zero;
            animator.SetBaseScale(Vector3.one);
            animator.PlaySpawnBounce(0.22f);
            SetWeaponVisual(WeaponType.None);
        }

        public void OnDespawn()
        {
            SetWeaponVisual(WeaponType.None);
        }

        public void SetupAsHeavy(int value = 5)
        {
            isHeavy = true;
            unitValue = value;
            transform.localScale = Vector3.one * 1.45f;
            animator.SetBaseScale(Vector3.one * 1.45f);
        }

        public void UpdatePosition(float lerpSpeed, float jitterMagnitude, float jitterSpeed, float deltaTime)
        {
            // Organic running jitter
            float t = Time.time * jitterSpeed + formationIndex * 0.77f;
            Vector3 jitter = new Vector3(
                Mathf.Sin(t) * jitterMagnitude,
                0f,
                Mathf.Cos(t * 1.3f) * jitterMagnitude
            );

            Vector3 destination = _targetLocalPosition + jitter;
            transform.localPosition = Vector3.Lerp(transform.localPosition, destination, deltaTime * lerpSpeed);
        }

        public void UpdateAnimation(float forwardSpeed, float deltaTime)
        {
            animator.UpdateAnimation(forwardSpeed, deltaTime);
        }

        public void SetWeaponVisual(WeaponType weaponType)
        {
            _currentWeaponType = weaponType;
            if (weaponMount == null) return;

            // Clear previous weapon model
            if (_equippedWeaponVisual != null)
            {
                Destroy(_equippedWeaponVisual);
                _equippedWeaponVisual = null;
            }

            if (weaponType != WeaponType.None)
            {
                _equippedWeaponVisual = VisualAssetFactory.CreateWeaponModel(weaponType, weaponMount);
            }
        }

        /// <summary>
        /// Kills the unit and plays particle burst.
        /// </summary>
        public void Die()
        {
            if (PoolManager.Instance != null && PoolManager.Instance.DeathEffects != null)
            {
                var fx = PoolManager.Instance.DeathEffects.Get(transform.position + Vector3.up * 0.6f, Quaternion.identity);
                fx.Play(Color.cyan);
            }

            PoolManager.Instance?.PlayerUnits.Return(this);
        }
    }
}
