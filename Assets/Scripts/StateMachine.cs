using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle.Infrastructure.StateMachine
{
    /// <summary>
    /// Generic finite state machine.
    ///
    /// Used for BattlePhase transitions (Setup → TurnStart → Execute → ...).
    /// Can also drive individual unit AI states (Idle → Deciding → Attacking).
    ///
    /// Each state is an IState — pure C#, no MonoBehaviour coupling.
    /// </summary>
    public class StateMachine<TEnum> where TEnum : Enum
    {
        private readonly Dictionary<TEnum, IState> _states      = new();
        private readonly Dictionary<(TEnum, TEnum), Func<bool>> _guards = new();

        public IState   CurrentState { get; private set; }
        public TEnum    CurrentKey   { get; private set; }
        public TEnum    PreviousKey  { get; private set; }

        public event Action<TEnum, TEnum> OnTransition;   // (from, to)

        public void Register(TEnum key, IState state)
        {
            _states[key] = state;
        }

        public void AddGuard(TEnum from, TEnum to, Func<bool> condition)
        {
            _guards[(from, to)] = condition;
        }

        public void SetInitial(TEnum key)
        {
            CurrentKey  = key;
            CurrentState = _states[key];
            CurrentState.Enter();
        }

        public bool TransitionTo(TEnum next)
        {
            if (!_states.TryGetValue(next, out var nextState))
            {
                Debug.LogWarning($"[FSM] State not registered: {next}");
                return false;
            }

            // Check guard
            var guardKey = (CurrentKey, next);
            if (_guards.TryGetValue(guardKey, out var guard) && !guard())
            {
                Debug.Log($"[FSM] Transition {CurrentKey} → {next} blocked by guard.");
                return false;
            }

            PreviousKey  = CurrentKey;
            CurrentState.Exit();
            CurrentKey   = next;
            CurrentState = nextState;
            CurrentState.Enter();

            OnTransition?.Invoke(PreviousKey, CurrentKey);
            return true;
        }

        public void Tick()   => CurrentState?.Tick();
        public void FixedTick() => CurrentState?.FixedTick();
    }

    public interface IState
    {
        void Enter();
        void Tick();
        void FixedTick();
        void Exit();
    }

    /// <summary>Convenience base — override only what you need.</summary>
    public abstract class BattleState : IState
    {
        public virtual void Enter()      { }
        public virtual void Tick()       { }
        public virtual void FixedTick()  { }
        public virtual void Exit()       { }
    }
}
