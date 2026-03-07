using System.Collections;
using System.Collections.Generic;

namespace DinosBattle
{
    public interface IAbility
    {
        string        Name          { get; }
        int           CooldownTurns { get; }
        AbilityTarget Targeting     { get; }
        bool          CanUse(CombatUnit user);
        IEnumerator   Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets);
    }

    public interface IStatusEffect
    {
        StatusEffectType Type           { get; }
        int              RemainingTurns { get; }
        bool             IsExpired      { get; }
        void             OnApply(CombatUnit owner);
        void             OnTurnStart(CombatUnit owner);
        void             OnTurnEnd(CombatUnit owner);
        void             OnRemove(CombatUnit owner);
        void             Stack(IStatusEffect incoming);
    }

    public interface IDamageCalculator
    {
        DamageResult Calculate(CombatUnit attacker, CombatUnit defender, float powerMult, DamageType type);
    }

    public interface ITargetSelector
    {
        CombatUnit               SelectOne(CombatUnit actor, IReadOnlyList<CombatUnit> candidates);
        IReadOnlyList<CombatUnit> SelectMany(CombatUnit actor, IReadOnlyList<CombatUnit> candidates, AbilityTarget targeting);
    }

    // Result of one damage calculation — immutable value object.
    public readonly struct DamageResult
    {
        public readonly CombatUnit Attacker;
        public readonly CombatUnit Defender;
        public readonly int        Damage;
        public readonly bool       IsCrit;
        public readonly bool       IsMiss;

        public DamageResult(CombatUnit attacker, CombatUnit defender, int damage, bool isCrit, bool isMiss)
        {
            Attacker = attacker; Defender = defender;
            Damage   = damage;   IsCrit   = isCrit; IsMiss = isMiss;
        }

        public static DamageResult Miss(CombatUnit a, CombatUnit d) =>
            new DamageResult(a, d, 0, false, true);

        public override string ToString() =>
            IsMiss ? $"{Attacker?.Name} → {Defender?.Name}: MISS"
                   : $"{Attacker?.Name} → {Defender?.Name}: {Damage}{(IsCrit ? " CRIT!" : "")}";
    }
}
