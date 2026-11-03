using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Puzzle.Presentation.Pooling
{
    /// <summary>
    /// Pool đơn giản cho view sinh theo dữ liệu (ô, cạnh). Đổi level chỉ tắt/bật lại object,
    /// không Instantiate/Destroy liên tục để tránh giật GC trên mobile.
    /// </summary>
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly List<T> _items = new List<T>();
        private int _inUse;

        public ComponentPool(T prefab, Transform parent)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _parent = parent;
        }

        public T Get()
        {
            if (_inUse == _items.Count) _items.Add(Object.Instantiate(_prefab, _parent));
            T item = _items[_inUse++];
            item.gameObject.SetActive(true);
            return item;
        }

        /// <summary>Trả mọi object về pool (tắt đi), giữ lại để dùng lần sau.</summary>
        public void ReleaseAll()
        {
            for (int i = 0; i < _items.Count; i++) _items[i].gameObject.SetActive(false);
            _inUse = 0;
        }
    }
}
