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
}