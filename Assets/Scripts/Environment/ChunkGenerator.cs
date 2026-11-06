using System;
using System.Collections.Generic;
using UnityEngine;
using EndlessCrowdRunner.Difficulty;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Gates;
using EndlessCrowdRunner.Enemies;
using EndlessCrowdRunner.Pickups;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.Environment.Obstacles;
using EndlessCrowdRunner.Pooling;
using EndlessCrowdRunner.Visuals;

namespace EndlessCrowdRunner.Environment
{
    /// <summary>
    /// Procedurally generates and recycles road segments ahead of the player.
    /// Uses weighted random generation, seeded RNG for reproducibility, and strict fairness rules.
    /// </summary>
    public class ChunkGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfig config;
        [SerializeField] private CrowdController playerCrowd;
        [SerializeField] private DifficultyScaler difficultyScaler;

        private System.Random _rng;
        private readonly List<RoadChunk> _activeChunks = new List<RoadChunk>(16);
        private float _nextSpawnZ = 0f;
        private int _chunkCounter = 0;

        public int CurrentSeed { get; private set; }

        public void Initialize(GameConfig gameConfig, CrowdController crowd, DifficultyScaler scaler)
        {
            config = gameConfig;
            playerCrowd = crowd;
            difficultyScaler = scaler;

            // Initialize seeded randomness
            CurrentSeed = config.generationSeed != 0 ? config.generationSeed : UnityEngine.Random.Range(10000, 99999);
            _rng = new System.Random(CurrentSeed);

            ClearAllChunks();

            // Spawn initial safe runway chunks and first challenges
            _nextSpawnZ = 0f;
            _chunkCounter = 0;

            for (int i = 0; i < config.activeChunkCount; i++)
            {
                ChunkType type = (i == 0) ? ChunkType.Runway : (i == 1 ? ChunkType.GateRow : PickNextChunkType());
                SpawnChunk(type);
            }
        }

        private void Update()
        {
            if (playerCrowd == null || _activeChunks.Count == 0) return;

            float playerZ = playerCrowd.CentroidPosition.z;
            RoadChunk firstChunk = _activeChunks[0];

            // Recycle chunks that have fallen behind the camera view
            if (playerZ > firstChunk.EndZ + 15.0f)
            {
                RecycleChunk(firstChunk);
            }
        }

        private ChunkType PickNextChunkType()
        {
            // Check if boss encounter is due
            if (difficultyScaler != null && difficultyScaler.CheckAndConsumeBossEncounter())
            {
                return ChunkType.BossEncounter;
            }

            int totalWeight = config.weightGateChunk + config.weightEnemyChunk + config.weightMixedChunk + config.weightObstacleChunk + config.weightRewardChunk;
            int roll = _rng.Next(0, totalWeight);

            if (roll < config.weightGateChunk) return ChunkType.GateRow;
            roll -= config.weightGateChunk;

            if (roll < config.weightEnemyChunk) return ChunkType.EnemyEncounter;
            roll -= config.weightEnemyChunk;

            if (roll < config.weightMixedChunk) return ChunkType.MixedEncounter;
            roll -= config.weightMixedChunk;

            if (roll < config.weightObstacleChunk) return ChunkType.ObstacleHazard;

            return ChunkType.Reward;
        }

        private void SpawnChunk(ChunkType type)
        {
            float length = config.chunkLength;
            RoadChunk chunk;

            if (PoolManager.Instance != null && PoolManager.Instance.RoadChunks != null)
            {
                chunk = PoolManager.Instance.RoadChunks.Get(new Vector3(0f, 0f, _nextSpawnZ), Quaternion.identity);
            }
            else
            {
                chunk = VisualAssetFactory.CreateRoadChunkPrefab(transform);
            }

            chunk.Setup(_nextSpawnZ, type, length);
            PopulateChunk(chunk, type);

            _activeChunks.Add(chunk);
            _nextSpawnZ += length;
            _chunkCounter++;
        }

        private void RecycleChunk(RoadChunk chunk)
        {
            _activeChunks.RemoveAt(0);

            ChunkType nextType = PickNextChunkType();
            chunk.Setup(_nextSpawnZ, nextType, config.chunkLength);
            PopulateChunk(chunk, nextType);

            _activeChunks.Add(chunk);
            _nextSpawnZ += config.chunkLength;
            _chunkCounter++;
        }

        private void PopulateChunk(RoadChunk chunk, ChunkType type)
        {
            float centerZ = chunk.StartZ + (chunk.Length * 0.5f);
            float roadW = config.roadWidth;

            switch (type)
            {
                case ChunkType.Runway:
                    // Empty safe breather chunk
                    break;

                case ChunkType.GateRow:
                    SpawnGateRow(chunk, centerZ);
                    break;

                case ChunkType.EnemyEncounter:
                    SpawnEnemyMob(chunk, centerZ);
                    break;

                case ChunkType.MixedEncounter:
                    // Gate first, followed by enemy group
                    SpawnGateRow(chunk, chunk.StartZ + 15.0f);
                    SpawnEnemyMob(chunk, chunk.StartZ + 35.0f);
                    break;

                case ChunkType.ObstacleHazard:
                    SpawnObstacles(chunk, centerZ);
                    break;

                case ChunkType.Reward:
                    SpawnRewardContent(chunk, centerZ);
                    break;

                case ChunkType.BossEncounter:
                    SpawnBoss(chunk, centerZ);
                    break;
            }
        }

        private void SpawnGateRow(RoadChunk chunk, float zPos)
        {
            float halfRoad = config.roadWidth * 0.5f;
            float pNeg = difficultyScaler != null ? difficultyScaler.GetNegativeGateProbability() : 0.25f;

            // Generate a 2-gate row across the road (Left / Right)
            float leftX = -halfRoad * 0.5f;
            float rightX = halfRoad * 0.5f;
            float gateW = halfRoad * 0.9f;

            GateType leftType = PickGateType(pNeg);
            int leftVal = PickGateValue(leftType);

            GateType rightType = PickGateType(pNeg);
            int rightVal = PickGateValue(rightType);

            // Fairness guarantee: ensure at least one positive/survivable gate
            if (!IsPositiveGate(leftType) && !IsPositiveGate(rightType))
            {
                rightType = (_rng.Next(0, 2) == 0) ? GateType.Add : GateType.Multiply;
                rightVal = PickGateValue(rightType);
            }

            GameObject groupGo = new GameObject("GateGroup");
            groupGo.transform.position = new Vector3(0f, 0f, zPos);
            GateGroup group = groupGo.AddComponent<GateGroup>();

            Gate gLeft = VisualAssetFactory.CreateGate(groupGo.transform, new Vector3(leftX, 0f, 0f), gateW);
            gLeft.Setup(group, leftType, leftVal, gateW);

            Gate gRight = VisualAssetFactory.CreateGate(groupGo.transform, new Vector3(rightX, 0f, 0f), gateW);
            gRight.Setup(group, rightType, rightVal, gateW);

            var list = new List<Gate> { gLeft, gRight };
            group.Initialize(list);

            int estimatedPlayer = playerCrowd != null ? playerCrowd.CurrentCount : 10;
            group.EnforceFairness(estimatedPlayer);

            chunk.RegisterSpawnedItem(groupGo);
        }

        private void SpawnEnemyMob(RoadChunk chunk, float zPos)
        {
            int count = difficultyScaler != null ? difficultyScaler.GetCurrentEnemyCount() : 12;
            bool shooters = (_rng.Next(0, 100) < 40); // 40% chance enemy group has gunners

            float posX = (float)(_rng.NextDouble() * 4.0 - 2.0); // Slight horizontal variation
            GameObject mob = VisualAssetFactory.CreateEnemyGroup(new Vector3(posX, 0f, zPos), count, shooters);
            chunk.RegisterSpawnedItem(mob);
        }

        private void SpawnObstacles(RoadChunk chunk, float zPos)
        {
            int kind = _rng.Next(0, 3);
            if (kind == 0)
            {
                // Spinning blade sweeping road
                GameObject blade = VisualAssetFactory.CreateSpinningBlade(new Vector3(0f, 0.5f, zPos));
                chunk.RegisterSpawnedItem(blade);
            }
            else if (kind == 1)
            {
                // Wall with gap
                float gapX = (float)(_rng.NextDouble() * 4.0 - 2.0);
                GameObject wall = VisualAssetFactory.CreateWallObstacle(new Vector3(0f, 0f, zPos), gapX, 3.8f);
                chunk.RegisterSpawnedItem(wall);
            }
            else
            {
                // Pit hazard
                float pitX = (float)(_rng.NextDouble() * 3.0 - 1.5);
                GameObject pit = VisualAssetFactory.CreatePitObstacle(new Vector3(pitX, 0.05f, zPos), 4.5f, 5.0f);
                chunk.RegisterSpawnedItem(pit);
            }
        }

        private void SpawnRewardContent(RoadChunk chunk, float zPos)
        {
            int rewardKind = _rng.Next(0, 3);
            if (rewardKind == 0)
            {
                // Super multiplier gate
                GameObject groupGo = new GameObject("RewardGateGroup");
                groupGo.transform.position = new Vector3(0f, 0f, zPos);
                GateGroup group = groupGo.AddComponent<GateGroup>();

                float gateW = config.roadWidth * 0.45f;
                Gate g = VisualAssetFactory.CreateGate(groupGo.transform, Vector3.zero, gateW * 1.5f);
                g.Setup(group, GateType.Multiply, 3, gateW * 1.5f);

                group.Initialize(new List<Gate> { g });
                chunk.RegisterSpawnedItem(groupGo);
            }
            else if (rewardKind == 1)
            {
                // Squad +10 or Heavy Gunner pickup
                PickupType pType = (_rng.Next(0, 2) == 0) ? PickupType.Squad10 : PickupType.HeavyGunner;
                GameObject pickup = VisualAssetFactory.CreateSquadPickup(new Vector3(0f, 0.8f, zPos), pType);
                chunk.RegisterSpawnedItem(pickup);
            }
            else
            {
                // Weapon upgrade pickup
                WeaponType nextW = GetNextWeaponToOffer();
                GameObject pickup = VisualAssetFactory.CreateWeaponPickup(new Vector3(0f, 0.8f, zPos), nextW);
                chunk.RegisterSpawnedItem(pickup);
            }
        }

        private void SpawnBoss(RoadChunk chunk, float zPos)
        {
            int hp = difficultyScaler != null ? difficultyScaler.GetCurrentBossHp() : 250;
            GameObject bossGo = VisualAssetFactory.CreateBoss(new Vector3(0f, 0f, zPos), hp);
            chunk.RegisterSpawnedItem(bossGo);
        }

        private WeaponType GetNextWeaponToOffer()
        {
            WeaponSystem ws = playerCrowd != null ? playerCrowd.GetComponent<WeaponSystem>() : null;
            WeaponType current = ws != null ? ws.CurrentWeapon : WeaponType.None;

            if (current == WeaponType.None) return WeaponType.Pistol;
            if (current == WeaponType.Pistol) return WeaponType.SMG;
            if (current == WeaponType.SMG) return WeaponType.Shotgun;
            if (current == WeaponType.Shotgun) return WeaponType.AssaultRifle;
            return WeaponType.Minigun;
        }

        private GateType PickGateType(float pNeg)
        {
            if (_rng.NextDouble() < pNeg)
            {
                return (_rng.Next(0, 2) == 0) ? GateType.Subtract : GateType.Divide;
            }
            return (_rng.Next(0, 2) == 0) ? GateType.Add : GateType.Multiply;
        }

        private int PickGateValue(GateType type)
        {
            switch (type)
            {
                case GateType.Add:
                    return config.additionPool[_rng.Next(0, config.additionPool.Length)];
                case GateType.Multiply:
                    return config.multiplierPool[_rng.Next(0, config.multiplierPool.Length)];
                case GateType.Subtract:
                    return config.subtractionPool[_rng.Next(0, config.subtractionPool.Length)];
                case GateType.Divide:
                    return config.divisionPool[_rng.Next(0, config.divisionPool.Length)];
                default:
                    return 5;
            }
        }

        private bool IsPositiveGate(GateType type)
        {
            return type == GateType.Add || type == GateType.Multiply;
        }

        public void ClearAllChunks()
        {
            for (int i = 0; i < _activeChunks.Count; i++)
            {
                if (_activeChunks[i] != null)
                {
                    _activeChunks[i].ClearSpawnedItems();
                    PoolManager.Instance?.RoadChunks.Return(_activeChunks[i]);
                }
            }
            _activeChunks.Clear();
        }
    }
}
