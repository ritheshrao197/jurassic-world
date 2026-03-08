using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle
{

    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler) where T : IBattleEvent
        {
            if (!_handlers.ContainsKey(typeof(T)))
                _handlers[typeof(T)] = new List<Delegate>();
            _handlers[typeof(T)].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IBattleEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public void Publish<T>(T e) where T : IBattleEvent
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            foreach (var h in new List<Delegate>(list))
            {
                try { ((Action<T>)h)(e); }
                catch (Exception ex) { Debug.LogError($"[EventBus] {typeof(T).Name}: {ex.Message}"); }
            }
        }

        public void Clear() => _handlers.Clear();
    }
}
