using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle.Combat
{
/// <summary>
/// The CombatResolver is responsible for resolving attacks: selecting targets, calculating damage, applying it, and publishing relevant events.
/// This centralizes combat logic and ensures consistent handling of attacks across different abilities and AI strategies.
/// It uses an IDamageCalculator to compute damage based on attacker and defender stats, and an ITargetSelector to determine which enemy to target (e.g., lowest HP).
/// When an attack is resolved, it checks for misses, applies damage to the target, and publishes events for the attack execution, health changes, and unit defeats. This allows other systems (like
/// the UI) to react accordingly. The ResolveAttack method targets a single enemy based on the selector, while ResolveAttackOn allows specifying an exact target.
/// </summary>
    public class CombatResolver
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly ITargetSelector _targetSelector;
        private readonly EventBus _bus;

        public CombatResolver(IDamageCalculator calc, ITargetSelector selector, EventBus bus)
        {
            _damageCalculator = calc;
            _targetSelector = selector;
            _bus = bus;
        }

        public DamageResult? ResolveAttack(CombatUnit attacker, IReadOnlyList<CombatUnit> enemies,
                                            float powerMult = 1f, DamageType type = DamageType.Physical)
        {
            var target = _targetSelector.SelectOne(attacker, enemies);
            if (target == null) return null;

            var result = _damageCalculator.Calculate(attacker, target, powerMult, type);
            if (!result.IsMiss)
            {
                target.TakeDamage(result.Damage);
                if (!target.IsAlive) _bus.Publish(new UnitDefeatedEvent(target));
            }

            _bus.Publish(new AttackExecutedEvent(result));
            _bus.Publish(new HealthChangedEvent(target));
            Debug.Log($"[Attack] {result}  |  {target.Name} HP: {target.CurrentHealth}/{target.Stats.MaxHealth}");
            return result;
        }

        public DamageResult ResolveAttackOn(CombatUnit attacker, CombatUnit defender,
                                             float powerMult = 1f, DamageType type = DamageType.Physical)
        {
            var result = _damageCalculator.Calculate(attacker, defender, powerMult, type);
            if (!result.IsMiss)
            {
                defender.TakeDamage(result.Damage);
                if (!defender.IsAlive) _bus.Publish(new UnitDefeatedEvent(defender));
            }

            _bus.Publish(new AttackExecutedEvent(result));
            _bus.Publish(new HealthChangedEvent(defender));
            Debug.Log($"[Attack] {result}  |  {defender.Name} HP: {defender.CurrentHealth}/{defender.Stats.MaxHealth}");
            return result;
        }
    }
}