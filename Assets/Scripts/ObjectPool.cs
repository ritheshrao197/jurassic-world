using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle.Infrastructure.ObjectPool
{
    /// <summary>
    /// Generic object pool — prevents GC spikes from instantiating/destroying
    /// VFX, damage numbers, projectiles, and pooled CombatUnits.
    ///
    /// Usage:
    ///   var pool = new ObjectPool<DamagePopup>(() => Instantiate(prefab), 10);
    ///   var popup = pool.Rent();
    ///   // ... use popup ...
    ///   pool.Return(popup);
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T>  _available;
        private readonly Func<T>   _factory;
        private readonly Action<T> _onRent;
        private readonly Action<T> _onReturn;
        private readonly int       _maxSize;

        public int AvailableCount => _available.Count;

        public ObjectPool(Func<T> factory,
                          int initialSize    = 8,
                          int maxSize        = 64,
                          Action<T> onRent   = null,
                          Action<T> onReturn = null)
        {
            _factory   = factory  ?? throw new ArgumentNullException(nameof(factory));
            _onRent    = onRent;
            _onReturn  = onReturn;
            _maxSize   = maxSize;
            _available = new Stack<T>(initialSize);

            for (int i = 0; i < initialSize; i++)
                _available.Push(_factory());
        }

        public T Rent()
        {
            T item = _available.Count > 0 ? _available.Pop() : _factory();
            _onRent?.Invoke(item);
            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;
            _onReturn?.Invoke(item);

            if (_available.Count < _maxSize)
                _available.Push(item);
        }

        public void WarmUp(int count)
        {
            for (int i = 0; i < count && _available.Count < _maxSize; i++)
                _available.Push(_factory());
        }

        public void Clear()
        {
            while (_available.Count > 0)
            {
                var item = _available.Pop();
                if (item is IDisposable d) d.Dispose();
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  UNITY GAMEOBJECT POOL
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Pool specifically for Unity GameObjects.
    /// Handles SetActive on rent/return automatically.
    /// </summary>
    public class GameObjectPool
    {
        private readonly Stack<GameObject> _pool;
        private readonly GameObject        _prefab;
        private readonly Transform         _parent;
        private readonly int               _maxSize;

        public GameObjectPool(GameObject prefab, Transform parent = null,
                              int initialSize = 4, int maxSize = 32)
        {
            _prefab  = prefab;
            _parent  = parent;
            _maxSize = maxSize;
            _pool    = new Stack<GameObject>(initialSize);

            for (int i = 0; i < initialSize; i++)
                _pool.Push(CreateNew());
        }

        public GameObject Rent(Vector3 position = default, Quaternion rotation = default)
        {
            var obj = _pool.Count > 0 ? _pool.Pop() : CreateNew();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);

            if (_pool.Count < _maxSize)
                _pool.Push(obj);
            else
                UnityEngine.Object.Destroy(obj);
        }

        private GameObject CreateNew()
        {
            var obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            obj.SetActive(false);
            return obj;
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                if (obj != null) UnityEngine.Object.Destroy(obj);
            }
        }
    }
}
