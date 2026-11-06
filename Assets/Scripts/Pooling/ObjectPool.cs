using System.Collections.Generic;
using UnityEngine;

namespace EndlessCrowdRunner.Pooling
{
    /// <summary>
    /// Generic GameObject pool with zero GC per-frame allocations.
    /// Supports pre-warming, fast retrieval, and automatic growth.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _available;
        private readonly List<T> _activeList;

        public int AvailableCount => _available.Count;
        public int ActiveCount => _activeList.Count;
        public IReadOnlyList<T> ActiveItems => _activeList;

        public ObjectPool(T prefab, int initialCapacity, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            _available = new Stack<T>(initialCapacity);
            _activeList = new List<T>(initialCapacity);

            Prewarm(initialCapacity);
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = Object.Instantiate(_prefab, _parent);
                item.gameObject.SetActive(false);
                _available.Push(item);
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T item;
            if (_available.Count > 0)
            {
                item = _available.Pop();
            }
            else
            {
                item = Object.Instantiate(_prefab, _parent);
            }

            Transform t = item.transform;
            t.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            _activeList.Add(item);

            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;

            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            item.gameObject.SetActive(false);
            if (_parent != null)
            {
                item.transform.SetParent(_parent);
            }
            _activeList.Remove(item);
            _available.Push(item);
        }

        public void ReturnAll()
        {
            while (_activeList.Count > 0)
            {
                Return(_activeList[_activeList.Count - 1]);
            }
        }
    }
}
