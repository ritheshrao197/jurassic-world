using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;
using DinosBattle.Core.Models;
using DinosBattle.Infrastructure.EventBus;

namespace DinosBattle.Systems.Combat
{
    // Standard damage: ATK - DEF*0.5, ±10% variance, with crit
    public class StandardDamageCalculator : IDamageCalculator
    {
        public DamageResult Calculate(CombatUnit attacker, CombatUnit defender, AbilityPayload ability)
        {
            if (attacker == null || defender == null) return DamageResult.Miss(attacker, defender);

            float base_  = (attacker.BaseStats.AttackPower - defender.BaseStats.Defense * 0.5f) * ability.PowerMultiplier;
            int   raw    = Mathf.Max(1, Mathf.RoundToInt(base_ * (1f + Random.Range(-0.1f, 0.1f))));
            bool  isCrit = Random.value < attacker.BaseStats.CritChance;
            int   final  = isCrit ? Mathf.RoundToInt(raw * attacker.BaseStats.CritMultiplier) : raw;

            return new DamageResult(attacker, defender, final, isCrit, false);
        }
    }

    // Random target selector
    public class RandomTargetSelector : ITargetSelector
    {
        public CombatUnit SelectTarget(CombatUnit actor, IReadOnlyList<CombatUnit> candidates)
        {
            var alive = candidates.Where(c => c.IsAlive).ToList();
            return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : null;
        }

        public IReadOnlyList<CombatUnit> SelectTargets(CombatUnit actor, IReadOnlyList<CombatUnit> candidates,
                                                        AbilityTarget targeting)
        {
            var alive = candidates.Where(c => c.IsAlive).ToList();
            switch (targeting)
            {
                case AbilityTarget.AllEnemies: return alive;
                case AbilityTarget.Self:       return new[] { actor };
                default:
                    return alive.Count > 0
                        ? new[] { alive[Random.Range(0, alive.Count)] }
                        : new CombatUnit[0];
            }
        }
    }

    // Lowest HP target selector
    public class LowestHpTargetSelector : ITargetSelector
    {
        public CombatUnit SelectTarget(CombatUnit actor, IReadOnlyList<CombatUnit> candidates) =>
            candidates.Where(c => c.IsAlive).OrderBy(c => c.CurrentHealth).FirstOrDefault();

        public IReadOnlyList<CombatUnit> SelectTargets(CombatUnit actor, IReadOnlyList<CombatUnit> candidates,
                                                        AbilityTarget targeting)
        {
            var alive = candidates.Where(c => c.IsAlive).OrderBy(c => c.CurrentHealth).ToList();
            return targeting == AbilityTarget.AllEnemies ? alive
                : alive.Count > 0 ? new[] { alive[0] } : new CombatUnit[0];
        }
    }

    // Resolves an attack: pick target → calculate → apply damage → publish event
    public class CombatResolver
    {
        private readonly IDamageCalculator _calculator;
        private readonly ITargetSelector   _selector;
        private readonly BattleEventBus    _bus;

        public CombatResolver(IDamageCalculator calculator, ITargetSelector selector, BattleEventBus bus)
        {
            _calculator = calculator;
            _selector   = selector;
            _bus        = bus;
        }

        public DamageResult? ResolveBasicAttack(CombatUnit actor, IReadOnlyList<CombatUnit> enemies)
        {
            var target = _selector.SelectTarget(actor, enemies);
            if (target == null) return null;
            return ResolveAttack(actor, target, AbilityPayload.BasicAttack);
        }

        public DamageResult ResolveAttack(CombatUnit attacker, CombatUnit defender, AbilityPayload payload)
        {
            var result = _calculator.Calculate(attacker, defender, payload);
            if (!result.IsMiss) defender.ApplyDamage(result.FinalDamage);
            _bus.Publish(new AttackExecutedEvent(result));
            Debug.Log($"[Combat] {result}");
            return result;
        }
    }
}
