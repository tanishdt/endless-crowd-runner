using System;
using System.Collections.Generic;
using UnityEngine;
using EndlessCrowdRunner.Difficulty;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Crowd
{
    /// <summary>
    /// Coordinates the player's stickman crowd, forward running movement,
    /// horizontal touch/mouse drag steering, formation packing, and unit counts.
    /// </summary>
    public class CrowdController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig config;
        [SerializeField] private WeaponSystem weaponSystem;

        [Header("State")]
        [SerializeField] private float currentRunSpeed = 11.0f;
        [SerializeField] private float targetX = 0.0f;
        [SerializeField] private float currentX = 0.0f;

        private readonly List<StickmanUnit> _units = new List<StickmanUnit>(512);
        private Camera _mainCamera;
        private Vector3 _lastPointerPos;
        private bool _isDragging = false;
        private float _lastGateTime = -1f;

        public event Action<int> OnCountChanged;
        public event Action OnCrowdEliminated;

        public int CurrentCount
        {
            get
            {
                int total = 0;
                for (int i = 0; i < _units.Count; i++)
                {
                    total += _units[i].unitValue;
                }
                return total;
            }
        }

        public int RawUnitCount => _units.Count;
        public IReadOnlyList<StickmanUnit> Units => _units;
        public Vector3 CentroidPosition => transform.position;
        public float CurrentRunSpeed => currentRunSpeed;
        public float CrowdRadius => FormationHelper.GetCrowdRadius(_units.Count, config != null ? config.unitSpacing : 0.65f);

        private void Awake()
        {
            _mainCamera = Camera.main;
            if (weaponSystem == null) weaponSystem = GetComponent<WeaponSystem>();
        }

        public void Initialize(GameConfig gameConfig)
        {
            config = gameConfig;
            currentRunSpeed = config.baseRunSpeed;
            targetX = 0.0f;
            currentX = 0.0f;
            transform.position = Vector3.zero;

            ClearCrowd();
            AddUnits(config.startingCount, transform.position);
        }

        public void SetRunSpeed(float speed)
        {
            currentRunSpeed = speed;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            HandleInput();
            UpdateMovement(dt);
            UpdateUnits(dt);
        }

        /// <summary>
        /// Reads touchscreen drag or mouse drag to steer crowd horizontally.
        /// </summary>
        private void HandleInput()
        {
            Vector3 currentPointerPos = Vector3.zero;
            bool pointerDown = false;
            bool pointerHeld = false;

            // Touch input on mobile devices
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                currentPointerPos = touch.position;
                if (touch.phase == TouchPhase.Began) pointerDown = true;
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) pointerHeld = true;
            }
            // Mouse input on PC/Editor
            else
            {
                currentPointerPos = Input.mousePosition;
                if (Input.GetMouseButtonDown(0)) pointerDown = true;
                if (Input.GetMouseButton(0)) pointerHeld = true;
            }

            if (pointerDown)
            {
                _isDragging = true;
                _lastPointerPos = currentPointerPos;
            }
            else if (pointerHeld && _isDragging)
            {
                float deltaX = currentPointerPos.x - _lastPointerPos.x;
                _lastPointerPos = currentPointerPos;

                float screenNormalizedDelta = deltaX / Screen.width;
                float sensitivity = config != null ? config.dragSensitivity : 1.35f;
                float roadSpan = config != null ? config.roadWidth : 10.0f;

                targetX += screenNormalizedDelta * roadSpan * sensitivity;
            }
            else if (!pointerHeld)
            {
                _isDragging = false;
            }

            // Clamp target horizontal position within road half-width, accounting for crowd radius
            float halfRoad = (config != null ? config.roadWidth : 10.0f) * 0.5f;
            float maxOffset = Mathf.Max(0.5f, halfRoad - Mathf.Min(CrowdRadius * 0.6f, halfRoad - 0.8f));
            targetX = Mathf.Clamp(targetX, -maxOffset, maxOffset);
        }

        /// <summary>
        /// Moves crowd forward and smooths horizontal steering.
        /// </summary>
        private void UpdateMovement(float dt)
        {
            float smoothSpeed = config != null ? config.horizontalSmoothSpeed : 16.0f;
            currentX = Mathf.Lerp(currentX, targetX, dt * smoothSpeed);

            Vector3 pos = transform.position;
            pos.x = currentX;
            pos.z += currentRunSpeed * dt;
            transform.position = pos;
        }

        /// <summary>
        /// Updates formation offsets and procedural unit animations.
        /// </summary>
        private void UpdateUnits(float dt)
        {
            float spacing = config != null ? config.unitSpacing : 0.65f;
            float lerpSpeed = config != null ? config.formationLerpSpeed : 12.0f;
            float jitterMag = config != null ? config.jitterMagnitude : 0.08f;
            float jitterFreq = config != null ? config.jitterFrequency : 14.0f;

            for (int i = 0; i < _units.Count; i++)
            {
                StickmanUnit unit = _units[i];
                if (unit == null) continue;

                unit.TargetLocalPosition = FormationHelper.GetFormationOffset(i, spacing);
                unit.UpdatePosition(lerpSpeed, jitterMag, jitterFreq, dt);
                unit.UpdateAnimation(currentRunSpeed, dt);
            }
        }

        /// <summary>
        /// Adds N units to the crowd, spawning from a specified origin point or centroid.
        /// </summary>
        public void AddUnits(int count, Vector3 spawnOrigin, bool isHeavy = false)
        {
            if (count <= 0) return;
            if (PoolManager.Instance == null || PoolManager.Instance.PlayerUnits == null) return;

            int maxCap = config != null ? config.maxCrowdCapacity : 600;
            int canSpawn = Mathf.Min(count, maxCap - _units.Count);

            for (int i = 0; i < canSpawn; i++)
            {
                StickmanUnit unit = PoolManager.Instance.PlayerUnits.Get(spawnOrigin, Quaternion.identity);
                unit.transform.SetParent(transform);
                unit.formationIndex = _units.Count;

                if (isHeavy)
                {
                    unit.SetupAsHeavy(5);
                }

                _units.Add(unit);

                // Sync current weapon visual
                if (weaponSystem != null && weaponSystem.CurrentWeapon != WeaponType.None)
                {
                    unit.SetWeaponVisual(weaponSystem.CurrentWeapon);
                }
            }

            // Spawn particle burst
            if (PoolManager.Instance.SpawnEffects != null)
            {
                var fx = PoolManager.Instance.SpawnEffects.Get(spawnOrigin + Vector3.up * 0.5f, Quaternion.identity);
                fx.Play(Color.cyan);
            }

            SoundManager.Instance?.PlaySound(SoundEffectType.UnitSpawn);
            OnCountChanged?.Invoke(CurrentCount);
        }

        /// <summary>
        /// Removes N units from the crowd (from the back/outer edges) and plays death effects.
        /// </summary>
        public void RemoveUnits(int count)
        {
            if (count <= 0 || _units.Count == 0) return;

            int removed = 0;
            while (_units.Count > 0 && removed < count)
            {
                int lastIdx = _units.Count - 1;
                StickmanUnit unit = _units[lastIdx];
                _units.RemoveAt(lastIdx);

                removed += unit.unitValue;
                unit.Die();
            }

            SoundManager.Instance?.PlaySound(SoundEffectType.UnitDeath);
            OnCountChanged?.Invoke(CurrentCount);

            if (_units.Count == 0)
            {
                OnCrowdEliminated?.Invoke();
            }
        }

        /// <summary>
        /// Clears all units back to pool.
        /// </summary>
        public void ClearCrowd()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i] != null)
                {
                    PoolManager.Instance?.PlayerUnits.Return(_units[i]);
                }
            }
            _units.Clear();
            OnCountChanged?.Invoke(0);
        }

        /// <summary>
        /// Applies gate mathematical operation (+, -, *, /) with cooldown guard.
        /// </summary>
        public bool ApplyGateOperation(GateType type, int value, Vector3 gatePos)
        {
            if (Time.time - _lastGateTime < (config != null ? config.gateCooldownTime : 0.4f))
            {
                return false;
            }
            _lastGateTime = Time.time;

            int oldCount = CurrentCount;
            int newCount = oldCount;

            switch (type)
            {
                case GateType.Add:
                    newCount = oldCount + value;
                    break;
                case GateType.Multiply:
                    newCount = oldCount * value;
                    break;
                case GateType.Subtract:
                    newCount = Mathf.Max(0, oldCount - value);
                    break;
                case GateType.Divide:
                    newCount = value > 0 ? Mathf.FloorToInt((float)oldCount / value) : 0;
                    break;
            }

            int delta = newCount - oldCount;
            if (delta > 0)
            {
                AddUnits(delta, gatePos);
                FloatingTextManager.Instance?.SpawnFloatingText($"+{delta}", gatePos + Vector3.up * 1.5f, Color.green);
                SoundManager.Instance?.PlaySound(SoundEffectType.GatePositive);
            }
            else if (delta < 0)
            {
                RemoveUnits(Mathf.Abs(delta));
                FloatingTextManager.Instance?.SpawnFloatingText($"{delta}", gatePos + Vector3.up * 1.5f, Color.red);
                SoundManager.Instance?.PlaySound(SoundEffectType.GateNegative);
            }

            return true;
        }

        /// <summary>
        /// Updates weapon model on active units when weapon tier changes.
        /// </summary>
        public void SyncWeaponVisuals(WeaponType weaponType)
        {
            // Up to front 60 units visually hold weapons to optimize render calls
            int maxEquip = Mathf.Min(_units.Count, 60);
            for (int i = 0; i < _units.Count; i++)
            {
                if (i < maxEquip)
                    _units[i].SetWeaponVisual(weaponType);
                else
                    _units[i].SetWeaponVisual(WeaponType.None);
            }
        }
    }
}
