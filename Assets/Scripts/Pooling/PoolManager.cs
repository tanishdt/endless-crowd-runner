using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Enemies;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.Visuals;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Environment;

namespace EndlessCrowdRunner.Pooling
{
    /// <summary>
    /// Singleton manager coordinating all pre-warmed object pools.
    /// Guarantees zero runtime instantiation spikes on mobile devices.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private StickmanUnit playerUnitPrefab;
        [SerializeField] private EnemyUnit enemyUnitPrefab;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private ParticleBurst deathFxPrefab;
        [SerializeField] private ParticleBurst spawnFxPrefab;
        [SerializeField] private ParticleBurst hitFxPrefab;
        [SerializeField] private FloatingText floatingTextPrefab;
        [SerializeField] private RoadChunk roadChunkPrefab;

        [Header("Capacities")]
        [SerializeField] private int playerPoolCapacity = 400;
        [SerializeField] private int enemyPoolCapacity = 350;
        [SerializeField] private int bulletPoolCapacity = 250;
        [SerializeField] private int fxPoolCapacity = 60;
        [SerializeField] private int textPoolCapacity = 40;
        [SerializeField] private int chunkPoolCapacity = 8;

        public ObjectPool<StickmanUnit> PlayerUnits { get; private set; }
        public ObjectPool<EnemyUnit> EnemyUnits { get; private set; }
        public ObjectPool<Bullet> Bullets { get; private set; }
        public ObjectPool<ParticleBurst> DeathEffects { get; private set; }
        public ObjectPool<ParticleBurst> SpawnEffects { get; private set; }
        public ObjectPool<ParticleBurst> HitEffects { get; private set; }
        public ObjectPool<FloatingText> FloatingTexts { get; private set; }
        public ObjectPool<RoadChunk> RoadChunks { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        public void InitializePools()
        {
            // If prefabs not assigned in inspector, generate procedural fallbacks
            if (playerUnitPrefab == null) playerUnitPrefab = VisualAssetFactory.CreatePlayerStickmanPrefab(transform);
            if (enemyUnitPrefab == null) enemyUnitPrefab = VisualAssetFactory.CreateEnemyStickmanPrefab(transform);
            if (bulletPrefab == null) bulletPrefab = VisualAssetFactory.CreateBulletPrefab(transform);
            if (deathFxPrefab == null) deathFxPrefab = VisualAssetFactory.CreateDeathFxPrefab(transform);
            if (spawnFxPrefab == null) spawnFxPrefab = VisualAssetFactory.CreateSpawnFxPrefab(transform);
            if (hitFxPrefab == null) hitFxPrefab = VisualAssetFactory.CreateHitFxPrefab(transform);
            if (floatingTextPrefab == null) floatingTextPrefab = VisualAssetFactory.CreateFloatingTextPrefab(transform);
            if (roadChunkPrefab == null) roadChunkPrefab = VisualAssetFactory.CreateRoadChunkPrefab(transform);

            Transform pParent = new GameObject("[Pool_PlayerUnits]").transform;
            pParent.SetParent(transform);
            PlayerUnits = new ObjectPool<StickmanUnit>(playerUnitPrefab, playerPoolCapacity, pParent);

            Transform eParent = new GameObject("[Pool_EnemyUnits]").transform;
            eParent.SetParent(transform);
            EnemyUnits = new ObjectPool<EnemyUnit>(enemyUnitPrefab, enemyPoolCapacity, eParent);

            Transform bParent = new GameObject("[Pool_Bullets]").transform;
            bParent.SetParent(transform);
            Bullets = new ObjectPool<Bullet>(bulletPrefab, bulletPoolCapacity, bParent);

            Transform dFxParent = new GameObject("[Pool_DeathFx]").transform;
            dFxParent.SetParent(transform);
            DeathEffects = new ObjectPool<ParticleBurst>(deathFxPrefab, fxPoolCapacity, dFxParent);

            Transform sFxParent = new GameObject("[Pool_SpawnFx]").transform;
            sFxParent.SetParent(transform);
            SpawnEffects = new ObjectPool<ParticleBurst>(spawnFxPrefab, fxPoolCapacity, sFxParent);

            Transform hFxParent = new GameObject("[Pool_HitFx]").transform;
            hFxParent.SetParent(transform);
            HitEffects = new ObjectPool<ParticleBurst>(hitFxPrefab, fxPoolCapacity, hFxParent);

            Transform tParent = new GameObject("[Pool_FloatingTexts]").transform;
            tParent.SetParent(transform);
            FloatingTexts = new ObjectPool<FloatingText>(floatingTextPrefab, textPoolCapacity, tParent);

            Transform cParent = new GameObject("[Pool_RoadChunks]").transform;
            cParent.SetParent(transform);
            RoadChunks = new ObjectPool<RoadChunk>(roadChunkPrefab, chunkPoolCapacity, cParent);
        }
    }
}
