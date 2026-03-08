// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using DinosBattle.Core;
// using DinosBattle.Core.Models;
// using DinosBattle.Core.Interfaces;

// namespace DinosBattle.Infrastructure.EventBus
// {
//     public interface IBattleEvent { }

//     public readonly struct BattleStartedEvent  : IBattleEvent { }

//     public readonly struct UnitRegisteredEvent : IBattleEvent
//     {
//         public readonly CombatUnit Unit;
//         public UnitRegisteredEvent(CombatUnit unit) { Unit = unit; }
//     }

//     public readonly struct BattleEndedEvent : IBattleEvent
//     {
//         public readonly BattleOutcome Outcome;
//         public readonly int           TotalTurns;

//         public BattleEndedEvent(BattleOutcome outcome, int turns) { Outcome = outcome; TotalTurns = turns; }    
//     }

//     public readonly struct TurnStartedEvent : IBattleEvent
//     {
//         public readonly CombatUnit Unit;
//         public readonly int        TurnNumber;
//         public TurnStartedEvent(CombatUnit unit, int turn) { Unit = unit; TurnNumber = turn; }
//     }

//     public readonly struct AttackExecutedEvent : IBattleEvent
//     {
//         public readonly DamageResult Result;
//         public AttackExecutedEvent(DamageResult result) { Result = result; }
//     }

//     public readonly struct UnitDefeatedEvent : IBattleEvent
//     {
//         public readonly CombatUnit Unit;
//         public UnitDefeatedEvent(CombatUnit unit) { Unit = unit; }
//     }

//     public readonly struct HealthChangedEvent : IBattleEvent
//     {
//         public readonly CombatUnit Unit;
//         public readonly int        NewHealth;
//         public HealthChangedEvent(CombatUnit unit, int newHealth) { Unit = unit; NewHealth = newHealth; }
//     }

//     public readonly struct PlayerInputRequiredEvent : IBattleEvent
//     {
//         public readonly CombatUnit ActiveUnit;
//         public PlayerInputRequiredEvent(CombatUnit unit) { ActiveUnit = unit; }
//     }

//     public readonly struct PlayerInputConsumedEvent : IBattleEvent { }

//     public class BattleEventBus
//     {
//         private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

//         public void Subscribe<T>(Action<T> handler) where T : IBattleEvent
//         {
//             if (!_handlers.ContainsKey(typeof(T)))
//                 _handlers[typeof(T)] = new List<Delegate>();
//             _handlers[typeof(T)].Add(handler);
//         }

//         public void Unsubscribe<T>(Action<T> handler) where T : IBattleEvent
//         {
//             if (_handlers.TryGetValue(typeof(T), out var list))
//                 list.Remove(handler);
//         }

//         public void Publish<T>(T e) where T : IBattleEvent
//         {
//             if (!_handlers.TryGetValue(typeof(T), out var list)) return;
//             foreach (var h in new List<Delegate>(list))
//             {
//                 try { ((Action<T>)h)(e); }
//                 catch (Exception ex) { Debug.LogError($"[EventBus] {typeof(T).Name}: {ex.Message}"); }
//             }
//         }

//         public void Clear() => _handlers.Clear();
//     }
// }
