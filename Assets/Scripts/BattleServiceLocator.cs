using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle.Infrastructure.ServiceLocator
{
    public class BattleServiceLocator
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T impl) => _services[typeof(T)] = impl;

        public T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var s)) return (T)s;
            throw new InvalidOperationException($"[Services] Not registered: {typeof(T).Name}");
        }

        public bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var obj)) { service = (T)obj; return true; }
            service = default;
            return false;
        }

        public void Clear() => _services.Clear();
    }

    public static class BattleServices
    {
        private static BattleServiceLocator _current;

        public static void SetActive(BattleServiceLocator locator) => _current = locator;
        public static void Clear()                                   => _current = null;

        public static T Get<T>()
        {
            if (_current == null) throw new InvalidOperationException("[BattleServices] Not initialised.");
            return _current.Get<T>();
        }

        public static bool TryGet<T>(out T service)
        {
            if (_current == null) { service = default; return false; }
            return _current.TryGet(out service);
        }
    }
}
