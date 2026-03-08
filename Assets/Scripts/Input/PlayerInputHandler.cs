using System;
using UnityEngine;

namespace DinosBattle.Input
{
    public enum PlayerActionType { Attack, Ability }

    public readonly struct PlayerAction
    {
        public readonly PlayerActionType Type;
        public readonly int              Index;   // ability index, -1 for attack

        public static PlayerAction Attack        => new PlayerAction(PlayerActionType.Attack, -1);
        public static PlayerAction Ability(int i) => new PlayerAction(PlayerActionType.Ability, i);

        private PlayerAction(PlayerActionType t, int i) { Type = t; Index = i; }
    }

    // Bridge between UI buttons and the battle loop.
    // UI calls SubmitAttack() / SubmitAbility(i) → fires OnActionSubmitted → coordinator reacts.
    public class PlayerInputHandler : MonoBehaviour
    {
        public bool       IsWaiting  { get; private set; }
        public CombatUnit ActiveUnit { get; private set; }

        public event Action<PlayerAction> OnActionSubmitted;
        public event Action<CombatUnit>   OnWindowOpened;
        public event Action               OnWindowClosed;

        public void BeginWaitingForInput(CombatUnit unit)
        {
            ActiveUnit = unit;
            IsWaiting  = true;
            OnWindowOpened?.Invoke(unit);
        }

        public void CancelInput()
        {
            IsWaiting  = false;
            ActiveUnit = null;
            OnWindowClosed?.Invoke();
        }

        public void SubmitAttack()        => Submit(PlayerAction.Attack);
        public void SubmitAbility(int i)  => Submit(PlayerAction.Ability(i));

        private void Submit(PlayerAction action)
        {
            if (!IsWaiting || ActiveUnit == null) return;

            if (action.Type == PlayerActionType.Ability)
            {
                if (action.Index < 0 || action.Index >= ActiveUnit.Abilities.Count) return;
                if (ActiveUnit.IsOnCooldown(ActiveUnit.Abilities[action.Index].Name)) return;
            }

            IsWaiting  = false;
            ActiveUnit = null;
            OnWindowClosed?.Invoke();
            OnActionSubmitted?.Invoke(action);
        }
    }
}
