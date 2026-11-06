using UnityEngine;
using EndlessCrowdRunner.Crowd;

namespace EndlessCrowdRunner.Difficulty
{
    /// <summary>
    /// Dynamically scales running speed, enemy group sizes, gate challenges,
    /// and boss intervals as the player advances further along the endless road.
    /// </summary>
    public class DifficultyScaler : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private CrowdController crowdController;

        private float _distanceTraveled = 0f;
        private float _timeSinceLastBoss = 0f;
        private int _bossEncounterCount = 0;

        public float DistanceTraveled => _distanceTraveled;
        public int BossCount => _bossEncounterCount;

        public void Initialize(GameConfig gameConfig, CrowdController crowd)
        {
            config = gameConfig;
            crowdController = crowd;
            _distanceTraveled = 0f;
            _timeSinceLastBoss = 0f;
            _bossEncounterCount = 0;
        }

        private void Update()
        {
            if (crowdController == null || config == null) return;

            // Track distance along Z
            _distanceTraveled = Mathf.Max(0f, crowdController.CentroidPosition.z);
            _timeSinceLastBoss += Time.deltaTime;

            // Update forward running speed smoothly
            float targetSpeed = config.GetSpeedForDistance(_distanceTraveled);
            crowdController.SetRunSpeed(targetSpeed);
        }

        public bool CheckAndConsumeBossEncounter()
        {
            float interval = config != null ? config.bossIntervalSeconds : 60f;
            if (_timeSinceLastBoss >= interval)
            {
                _timeSinceLastBoss = 0f;
                _bossEncounterCount++;
                return true;
            }
            return false;
        }

        public int GetCurrentEnemyCount()
        {
            return config != null ? config.GetEnemyCountForDistance(_distanceTraveled) : 10;
        }

        public int GetCurrentBossHp()
        {
            int baseHp = config != null ? config.bossBaseHp : 250;
            int growth = config != null ? config.bossHpGrowthPerTier : 160;
            return baseHp + (_bossEncounterCount * growth);
        }

        public float GetNegativeGateProbability()
        {
            // Starts at 20% negative gates, ramps up to max 50%
            float factor = Mathf.Clamp01(_distanceTraveled / 1200.0f);
            return Mathf.Lerp(0.20f, 0.50f, factor);
        }
    }
}
