using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle
{
    // ── Events ────────────────────────────────────────────────────────────────

    public interface IBattleEvent { }

    public readonly struct BattleStartedEvent   : IBattleEvent { }
    public readonly struct BattleEndedEvent     : IBattleEvent
    {
        public readonly BattleOutcome Outcome;
        public BattleEndedEvent(BattleOutcome o) { Outcome = o; }
    }
    public readonly struct UnitRegisteredEvent  : IBattleEvent
    {
        public readonly CombatUnit Unit;
        public UnitRegisteredEvent(CombatUnit u) { Unit = u; }
    }
    public readonly struct TurnStartedEvent     : IBattleEvent
    {
        public readonly CombatUnit Unit; public readonly int Turn;
        public TurnStartedEvent(CombatUnit u, int t) { Unit = u; Turn = t; }
    }
    public readonly struct AttackExecutedEvent  : IBattleEvent
    {
        public readonly DamageResult Result;
        public AttackExecutedEvent(DamageResult r) { Result = r; }
    }
    public readonly struct UnitDefeatedEvent    : IBattleEvent
    {
        public readonly CombatUnit Unit;
        public UnitDefeatedEvent(CombatUnit u) { Unit = u; }
    }
    public readonly struct HealthChangedEvent   : IBattleEvent
    {
        public readonly CombatUnit Unit;
        public HealthChangedEvent(CombatUnit u) { Unit = u; }
    }

    // ── Bus ───────────────────────────────────────────────────────────────────

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
