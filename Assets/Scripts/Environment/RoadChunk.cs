using System.Collections.Generic;
using UnityEngine;
using EndlessCrowdRunner.Pooling;

namespace EndlessCrowdRunner.Environment
{
    /// <summary>
    /// Represents a single procedurally spawned road segment.
    /// Manages recycling of all gates, enemies, obstacles, and pickups placed on it.
    /// </summary>
    public class RoadChunk : MonoBehaviour, IPoolable
    {
        [Header("Chunk Properties")]
        [SerializeField] private float chunkLength = 50.0f;
        [SerializeField] private Transform contentRoot;

        private float _startZ;
        private ChunkType _type;
        private readonly List<GameObject> _spawnedItems = new List<GameObject>(16);

        public float Length => chunkLength;
        public float StartZ => _startZ;
        public float EndZ => _startZ + chunkLength;
        public ChunkType Type => _type;
        public Transform ContentRoot => contentRoot != null ? contentRoot : transform;

        private void Awake()
        {
            if (contentRoot == null)
            {
                Transform found = transform.Find("Content");
                contentRoot = found != null ? found : transform;
            }
        }

        public void Setup(float startZ, ChunkType type, float length = 50.0f)
        {
            _startZ = startZ;
            _type = type;
            chunkLength = length;
            transform.position = new Vector3(0f, 0f, startZ);
            ClearSpawnedItems();
        }

        public void RegisterSpawnedItem(GameObject item)
        {
            if (item != null && !_spawnedItems.Contains(item))
            {
                _spawnedItems.Add(item);
                item.transform.SetParent(ContentRoot);
            }
        }

        public void ClearSpawnedItems()
        {
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                GameObject item = _spawnedItems[i];
                if (item != null)
                {
                    // Check if it belongs to a pool or should be destroyed
                    Destroy(item);
                }
            }
            _spawnedItems.Clear();
        }

        public void OnSpawn()
        {
            ClearSpawnedItems();
        }

        public void OnDespawn()
        {
            ClearSpawnedItems();
        }
    }
}
