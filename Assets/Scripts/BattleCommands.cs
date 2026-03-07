using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;
using DinosBattle.Core.Models;
using DinosBattle.Systems.Combat;

namespace DinosBattle.Commands
{
    // ════════════════════════════════════════════════════════════════════════
    //  COMMAND HISTORY  (enables replay, undo, AI simulation)
    // ════════════════════════════════════════════════════════════════════════

    public class CommandHistory
    {
        private readonly System.Collections.Generic.Stack<IBattleCommand> _executed =
            new System.Collections.Generic.Stack<IBattleCommand>();
        private readonly List<IBattleCommand> _log = new List<IBattleCommand>();

        public IReadOnlyList<IBattleCommand> FullLog => _log;

        public void Record(IBattleCommand cmd)
        {
            _executed.Push(cmd);
            _log.Add(cmd);
        }

        public void UndoLast()
        {
            if (_executed.Count > 0)
                _executed.Pop().Undo();
        }

        public void Clear()
        {
            _executed.Clear();
            _log.Clear();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CONCRETE COMMANDS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Basic attack command: select target → resolve damage → animate → log.
    /// </summary>
    public class BasicAttackCommand : IBattleCommand
    {
        public string Description => $"{_attacker.Name} uses Basic Attack";

        private readonly CombatUnit               _attacker;
        private readonly IReadOnlyList<CombatUnit> _enemyTeam;
        private readonly CombatResolver            _resolver;
        private readonly IAnimationHandler         _animation;

        private DamageResult? _executedResult;

        public BasicAttackCommand(CombatUnit attacker, IReadOnlyList<CombatUnit> enemyTeam,
                                   CombatResolver resolver, IAnimationHandler animation)
        {
            _attacker  = attacker;
            _enemyTeam = enemyTeam;
            _resolver  = resolver;
            _animation = animation;
        }

        public IEnumerator Execute()
        {
            _executedResult = _resolver.ResolveBasicAttack(_attacker, _enemyTeam);
            if (!_executedResult.HasValue) yield break;

            var result = _executedResult.Value;

            // Attack animation
            yield return _animation.PlayAnimation(AnimationType.Attack, _attacker, result.Defender);

            // Hit reaction animation
            AnimationType hitAnim = result.Defender.IsAlive ? AnimationType.Hurt : AnimationType.Death;
            yield return _animation.PlayAnimation(hitAnim, result.Defender);
        }

        public void Undo()
        {
            if (!_executedResult.HasValue) return;
            _executedResult.Value.Defender.RestoreHealth(_executedResult.Value.FinalDamage);
            Debug.Log($"[Undo] Restored {_executedResult.Value.FinalDamage} HP to {_executedResult.Value.Defender.Name}");
        }
    }

    /// <summary>Use an IAbility: resolve targeting, execute, set cooldown.</summary>
    public class UseAbilityCommand : IBattleCommand
    {
        public string Description => $"{_user.Name} uses {_ability.AbilityName}";

        private readonly CombatUnit               _user;
        private readonly IAbility                 _ability;
        private readonly IReadOnlyList<CombatUnit> _enemyTeam;
        private readonly ITargetSelector          _selector;
        private readonly IAnimationHandler        _animation;

        public UseAbilityCommand(CombatUnit user, IAbility ability,
                                  IReadOnlyList<CombatUnit> enemyTeam,
                                  ITargetSelector selector, IAnimationHandler animation)
        {
            _user      = user;
            _ability   = ability;
            _enemyTeam = enemyTeam;
            _selector  = selector;
            _animation = animation;
        }

        public IEnumerator Execute()
        {
            if (!_ability.CanUse(_user))
            {
                Debug.LogWarning($"[UseAbilityCommand] {_user.Name} cannot use {_ability.AbilityName}");
                yield break;
            }

            var targets = _selector.SelectTargets(_user, _enemyTeam, _ability.Targeting);
            yield return _ability.Execute(_user, targets);
            _user.SetCooldown(_ability.AbilityName, _ability.CooldownTurns);
        }

        public void Undo() { /* Ability undo: store snapshot in Execute() if needed */ }
    }

    /// <summary>Tick all status effects on a unit at turn start or end.</summary>
    public class TickStatusEffectsCommand : IBattleCommand
    {
        public string Description => $"Tick status effects on {_unit.Name}";

        private readonly CombatUnit _unit;
        private readonly bool       _isTurnStart;

        public TickStatusEffectsCommand(CombatUnit unit, bool isTurnStart)
        {
            _unit        = unit;
            _isTurnStart = isTurnStart;
        }

        public IEnumerator Execute()
        {
            _unit.TickStatusEffects(_isTurnStart);
            _unit.TickCooldowns();
            // _unit.TickModifiers();
            yield break;
        }

        public void Undo() { }
    }
}
