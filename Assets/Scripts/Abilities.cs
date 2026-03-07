using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;
using DinosBattle.Core.Models;
using DinosBattle.Systems.Combat;
using DinosBattle.Systems.StatusEffects;
using DinosBattle.Infrastructure.ServiceLocator;

namespace DinosBattle.Systems.Abilities
{
    public abstract class BaseAbility : IAbility
    {
        public abstract string        AbilityName   { get; }
        public abstract int           CooldownTurns { get; }
        public abstract AbilityTarget Targeting     { get; }

        public virtual bool CanUse(CombatUnit user) =>
            !user.IsOnCooldown(AbilityName) && user.IsAlive;

        public abstract IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets);

        protected CombatResolver GetResolver() => BattleServices.Get<CombatResolver>();
    }

    // AOE attack hitting all enemies for 60% power
    public class TailWhipAbility : BaseAbility
    {
        public override string        AbilityName   => "Tail Whip";
        public override int           CooldownTurns => 3;
        public override AbilityTarget Targeting     => AbilityTarget.AllEnemies;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            var payload = new AbilityPayload("Tail Whip", DamageType.Physical, 0.6f);
            foreach (var target in targets.Where(t => t.IsAlive))
            {
                GetResolver().ResolveAttack(user, target, payload);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // Single-target attack that applies Poison
    public class PoisonBiteAbility : BaseAbility
    {
        public override string        AbilityName   => "Poison Bite";
        public override int           CooldownTurns => 2;
        public override AbilityTarget Targeting     => AbilityTarget.SingleEnemy;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            var target = targets.FirstOrDefault(t => t.IsAlive);
            if (target == null) yield break;

            GetResolver().ResolveAttack(user, target, new AbilityPayload("Poison Bite", DamageType.Poison, 1.2f));
            target.AddStatusEffect(new PoisonEffect(3, 0.06f));
        }
    }

    // Heal self for 30% max HP
    public class HealRoarAbility : BaseAbility
    {
        public override string        AbilityName   => "Heal Roar";
        public override int           CooldownTurns => 4;
        public override AbilityTarget Targeting     => AbilityTarget.Self;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            int amount = Mathf.RoundToInt(user.BaseStats.MaxHealth * 0.3f);
            user.RestoreHealth(amount);
            Debug.Log($"[Ability] {user.Name} healed {amount} HP.");
            yield break;
        }
    }
}
