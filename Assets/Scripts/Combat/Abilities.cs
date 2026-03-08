using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Combat;

namespace DinosBattle
{
    // Base ability — subclasses only override what they need.
    public abstract class BaseAbility : IAbility
    {
        public abstract string        Name          { get; }
        public abstract int           CooldownTurns { get; }
        public abstract AbilityTarget Targeting     { get; }

        public virtual bool CanUse(CombatUnit user) =>
            user.IsAlive && !user.IsOnCooldown(Name);

        public abstract IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets);
    }

    // AOE attack: hits all enemies at 60% power.
    public class TailWhipAbility : BaseAbility
    {
        public override string        Name          => "Tail Whip";
        public override int           CooldownTurns => 3;
        public override AbilityTarget Targeting     => AbilityTarget.AllEnemies;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            var resolver = ServiceLocator.Get<CombatResolver>();
            foreach (var t in targets.Where(t => t.IsAlive))
            {
                resolver.ResolveAttackOn(user, t, 0.6f);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // Single target: deals 120% damage and poisons the target.
    public class PoisonBiteAbility : BaseAbility
    {
        public override string        Name          => "Poison Bite";
        public override int           CooldownTurns => 2;
        public override AbilityTarget Targeting     => AbilityTarget.SingleEnemy;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            var target = targets.FirstOrDefault(t => t.IsAlive);
            if (target == null) yield break;

            ServiceLocator.Get<CombatResolver>().ResolveAttackOn(user, target, 1.2f, DamageType.Poison);
            target.AddStatus(new PoisonEffect(3));
        }
    }

    // Self-heal: restores 30% of max HP.
    public class HealRoarAbility : BaseAbility
    {
        public override string        Name          => "Heal Roar";
        public override int           CooldownTurns => 4;
        public override AbilityTarget Targeting     => AbilityTarget.Self;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            int amount = Mathf.RoundToInt(user.Stats.MaxHealth * 0.3f);
            user.Heal(amount);

            var bus = ServiceLocator.Get<EventBus>();
            bus.Publish(new HealthChangedEvent(user));
            Debug.Log($"[Ability] {user.Name} healed {amount} HP.");
            yield break;
        }
    }
}
