using System;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;

namespace DinosBattle.Input
{
    public enum PlayerActionType { BasicAttack, UseAbility }

    public readonly struct PlayerAction
    {
        public readonly PlayerActionType Type;
        public readonly int              AbilityIndex;

        public static PlayerAction Attack           => new PlayerAction(PlayerActionType.BasicAttack, -1);
        public static PlayerAction Ability(int idx)  => new PlayerAction(PlayerActionType.UseAbility, idx);

        private PlayerAction(PlayerActionType type, int index) { Type = type; AbilityIndex = index; }
    }

    public class PlayerInputHandler : MonoBehaviour
    {
        public bool       IsWaiting  { get; private set; }
        public CombatUnit ActiveUnit { get; private set; }

        public event Action<PlayerAction> OnActionSubmitted;
        public event Action<CombatUnit>   OnInputWindowOpened;
        public event Action               OnInputWindowClosed;

        public void BeginWaitingForInput(CombatUnit unit)
        {
            ActiveUnit = unit;
            IsWaiting  = true;
            OnInputWindowOpened?.Invoke(unit);
        }

        public void CancelInput()
        {
            IsWaiting  = false;
            ActiveUnit = null;
            OnInputWindowClosed?.Invoke();
        }

        public void SubmitAction(PlayerAction action)
        {
            if (!IsWaiting || ActiveUnit == null) return;

            if (action.Type == PlayerActionType.UseAbility)
            {
                if (action.AbilityIndex < 0 || action.AbilityIndex >= ActiveUnit.Abilities.Count) return;
                if (ActiveUnit.IsOnCooldown(ActiveUnit.Abilities[action.AbilityIndex].AbilityName)) return;
            }

            IsWaiting  = false;
            ActiveUnit = null;
            OnInputWindowClosed?.Invoke();
            OnActionSubmitted?.Invoke(action);
        }

        public void SubmitAttack()           => SubmitAction(PlayerAction.Attack);
        public void SubmitAbility(int index) => SubmitAction(PlayerAction.Ability(index));
    }
}
