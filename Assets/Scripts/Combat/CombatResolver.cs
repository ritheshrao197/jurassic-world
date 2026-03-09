using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle.Combat
{

    // Resolves an attack: select target → calculate → apply damage → publish event.
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