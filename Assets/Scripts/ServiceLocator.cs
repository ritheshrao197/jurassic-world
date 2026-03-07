using System;
using System.Collections.Generic;

namespace DinosBattle
{
    // Lightweight service container scoped to one battle session.
    // Register services in BattleInstaller; fetch them anywhere via ServiceLocator.Get<T>().
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T impl)         => _services[typeof(T)] = impl;
        public static void Clear()                      => _services.Clear();

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var s)) return (T)s;
            throw new InvalidOperationException($"[ServiceLocator] {typeof(T).Name} not registered.");
        }

        public static bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var obj)) { service = (T)obj; return true; }
            service = default;
            return false;
        }
    }
}
