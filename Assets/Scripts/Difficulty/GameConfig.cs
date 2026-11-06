using System;
using UnityEngine;

namespace EndlessCrowdRunner.Difficulty
{
    /// <summary>
    /// Master ScriptableObject configuration holding all tunable gameplay, crowd,
    /// enemy, gate, weapon, and difficulty parameters. Can be tweaked in Inspector
    /// without modifying code.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "EndlessCrowdRunner/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Movement Settings")]
        [Tooltip("Starting forward running speed in m/s.")]
        public float baseRunSpeed = 11.0f;

        [Tooltip("Maximum forward running speed cap.")]
        public float maxRunSpeed = 22.0f;

        [Tooltip("Speed increase per 100 meters traveled.")]
        public float speedAccelerationPer100m = 0.5f;

        [Tooltip("Horizontal touch drag sensitivity.")]
        public float dragSensitivity = 1.35f;

        [Tooltip("Damping speed for horizontal crowd movement.")]
        public float horizontalSmoothSpeed = 16.0f;

        [Tooltip("Total usable road width (half-width clamped on both sides).")]
        public float roadWidth = 10.0f;

        [Header("Crowd Settings")]
        [Tooltip("Starting crowd count at run begin.")]
        public int startingCount = 1;

        [Tooltip("Absolute maximum units rendered on screen.")]
        public int maxCrowdCapacity = 600;

        [Tooltip("Radial spacing between units in formation.")]
        public float unitSpacing = 0.65f;

        [Tooltip("Speed units lerp towards their formation slot.")]
        public float formationLerpSpeed = 12.0f;

        [Tooltip("Simulated natural jitter amplitude while running.")]
        public float jitterMagnitude = 0.08f;

        [Tooltip("Jitter oscillation frequency.")]
        public float jitterFrequency = 14.0f;

        [Tooltip("Duration of spawn/scale bounce animation.")]
        public float unitSpawnBounceTime = 0.25f;

        [Header("Gate Settings")]
        [Tooltip("Minimum time between consecutive gate triggers to avoid double counting.")]
        public float gateCooldownTime = 0.4f;

        [Tooltip("Possible addition values for gates.")]
        public int[] additionPool = { 5, 10, 15, 25, 50, 100 };

        [Tooltip("Possible multiplier values for gates.")]
        public int[] multiplierPool = { 2, 3, 5 };

        [Tooltip("Possible subtraction values for gates.")]
        public int[] subtractionPool = { 5, 10, 20, 30, 50 };

        [Tooltip("Possible divisor values for gates.")]
        public int[] divisionPool = { 2, 3, 4 };

        [Header("Enemy & Combat Settings")]
        [Tooltip("Base count for first enemy group.")]
        public int baseEnemyGroupCount = 8;

        [Tooltip("Enemy count growth per 100 meters.")]
        public float enemyGrowthPer100m = 3.0f;

        [Tooltip("Cap on regular enemy group size.")]
        public int maxEnemyGroupCount = 200;

        [Tooltip("Fire interval for enemy gunners (seconds).")]
        public float enemyShooterInterval = 1.4f;

        [Tooltip("Detection distance for enemy gunners to engage.")]
        public float enemyShooterRange = 36.0f;

        [Tooltip("Velocity of enemy bullets.")]
        public float enemyBulletSpeed = 22.0f;

        [Header("Boss Settings")]
        [Tooltip("Time interval in seconds between boss encounters.")]
        public float bossIntervalSeconds = 60.0f;

        [Tooltip("Base HP of the first boss.")]
        public int bossBaseHp = 250;

        [Tooltip("HP added to boss per subsequent encounter.")]
        public int bossHpGrowthPerTier = 160;

        [Tooltip("Interval between boss special slam attacks.")]
        public float bossAttackInterval = 3.2f;

        [Tooltip("Stickman bonus rewarded upon defeating a boss.")]
        public int bossRewardStickmen = 35;

        [Tooltip("Score bonus for defeating a boss.")]
        public int bossRewardScore = 1500;

        [Header("Weapons & Upgrades")]
        [Tooltip("Damage dealt per unit collision in melee clash.")]
        public int meleeDamagePerUnit = 1;

        [Tooltip("Bullet flight speed for player weapons.")]
        public float playerBulletSpeed = 40.0f;

        [Tooltip("Weapon firing range forward.")]
        public float weaponRange = 35.0f;

        [Header("Procedural Chunks")]
        [Tooltip("Length of each road chunk along Z in meters.")]
        public float chunkLength = 50.0f;

        [Tooltip("Number of active chunks ahead of player.")]
        public int activeChunkCount = 6;

        [Tooltip("Seed for deterministic generation. 0 uses random seed.")]
        public int generationSeed = 0;

        [Header("Difficulty Ramp Weights (normalized dynamically)")]
        [Range(0, 100)] public int weightGateChunk = 30;
        [Range(0, 100)] public int weightEnemyChunk = 25;
        [Range(0, 100)] public int weightMixedChunk = 25;
        [Range(0, 100)] public int weightObstacleChunk = 15;
        [Range(0, 100)] public int weightRewardChunk = 5;

        [Header("Juice & Feedback")]
        public float screenShakeIntensity = 0.28f;
        public float screenShakeDuration = 0.25f;

        /// <summary>
        /// Calculates current forward run speed for a given distance.
        /// </summary>
        public float GetSpeedForDistance(float distance)
        {
            float speed = baseRunSpeed + (distance / 100.0f) * speedAccelerationPer100m;
            return Mathf.Min(speed, maxRunSpeed);
        }

        /// <summary>
        /// Calculates target enemy group size for a given distance.
        /// </summary>
        public int GetEnemyCountForDistance(float distance)
        {
            float scaled = baseEnemyGroupCount + (distance / 100.0f) * enemyGrowthPer100m;
            return Mathf.Min(Mathf.RoundToInt(scaled), maxEnemyGroupCount);
        }
    }
}
