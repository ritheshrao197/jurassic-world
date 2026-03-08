using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DinosBattle.Combat
{
    // Formula: (ATK - DEF*0.5) × power ± 10% variance, with crit.
    public class StandardDamageCalculator : IDamageCalculator
    {
        public DamageResult Calculate(CombatUnit attacker, CombatUnit defender, float powerMult, DamageType type)
        {
            float baseDmg = (attacker.Stats.Attack - defender.Stats.Defense * 0.5f) * powerMult;
            int   raw     = Mathf.Max(1, Mathf.RoundToInt(baseDmg * Random.Range(0.9f, 1.1f)));
            bool  isCrit  = Random.value < attacker.Stats.CritChance;
            int   final   = isCrit ? Mathf.RoundToInt(raw * attacker.Stats.CritMultiplier) : raw;
            return new DamageResult(attacker, defender, final, isCrit, false);
        }
    }

    // Picks a random alive target; supports AOE and self-targeting.
    public class RandomTargetSelector : ITargetSelector
    {
        public CombatUnit SelectOne(CombatUnit actor, IReadOnlyList<CombatUnit> candidates)
        {
            var alive = candidates.Where(c => c.IsAlive).ToList();
            return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : null;
        }

        public IReadOnlyList<CombatUnit> SelectMany(CombatUnit actor, IReadOnlyList<CombatUnit> candidates,
                                                     AbilityTarget targeting)
        {
            var alive = candidates.Where(c => c.IsAlive).ToList();
            switch (targeting)
            {
                case AbilityTarget.AllEnemies: return alive;
                case AbilityTarget.Self:       return new[] { actor };
                default:
                    return alive.Count > 0 ? new[] { alive[Random.Range(0, alive.Count)] } : new CombatUnit[0];
            }
        }
    }

    // Always targets the lowest-HP enemy (smarter AI strategy).
    public class LowestHpTargetSelector : ITargetSelector
    {
        public CombatUnit SelectOne(CombatUnit actor, IReadOnlyList<CombatUnit> candidates) =>
            candidates.Where(c => c.IsAlive).OrderBy(c => c.CurrentHealth).FirstOrDefault();

        public IReadOnlyList<CombatUnit> SelectMany(CombatUnit actor, IReadOnlyList<CombatUnit> candidates,
                                                     AbilityTarget targeting)
        {
            var alive = candidates.Where(c => c.IsAlive).OrderBy(c => c.CurrentHealth).ToList();
            return targeting == AbilityTarget.AllEnemies ? alive
                 : alive.Count > 0 ? new[] { alive[0] } : new CombatUnit[0];
        }
    }

    // Resolves an attack: select target → calculate → apply damage → publish event.
    public class CombatResolver
    {
        private readonly IDamageCalculator _calc;
        private readonly ITargetSelector   _selector;
        private readonly EventBus          _bus;

        public CombatResolver(IDamageCalculator calc, ITargetSelector selector, EventBus bus)
        {
            _calc     = calc;
            _selector = selector;
            _bus      = bus;
        }

        public DamageResult? ResolveAttack(CombatUnit attacker, IReadOnlyList<CombatUnit> enemies,
                                            float powerMult = 1f, DamageType type = DamageType.Physical)
        {
            var target = _selector.SelectOne(attacker, enemies);
            if (target == null) return null;

            var result = _calc.Calculate(attacker, target, powerMult, type);
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
            var result = _calc.Calculate(attacker, defender, powerMult, type);
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