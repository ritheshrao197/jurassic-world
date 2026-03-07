using DinosBattle.Core.Enums;

namespace DinosBattle.Core.Models
{
    public readonly struct StatBlock
    {
        public readonly int   MaxHealth;
        public readonly int   AttackPower;
        public readonly int   Defense;
        public readonly int   Speed;
        public readonly float CritChance;
        public readonly float CritMultiplier;

        public StatBlock(int maxHealth, int attackPower, int defense, int speed,
                         float critChance = 0.1f, float critMultiplier = 1.5f)
        {
            MaxHealth      = maxHealth;
            AttackPower    = attackPower;
            Defense        = defense;
            Speed          = speed;
            CritChance     = critChance;
            CritMultiplier = critMultiplier;
        }
    }

    public readonly struct DamageResult
    {
        public readonly CombatUnit Attacker;
        public readonly CombatUnit Defender;
        public readonly int        FinalDamage;
        public readonly bool       IsCritical;
        public readonly bool       IsMiss;

        public DamageResult(CombatUnit attacker, CombatUnit defender,
                            int finalDamage, bool isCrit, bool isMiss)
        {
            Attacker    = attacker;
            Defender    = defender;
            FinalDamage = finalDamage;
            IsCritical  = isCrit;
            IsMiss      = isMiss;
        }

        public static DamageResult Miss(CombatUnit attacker, CombatUnit defender) =>
            new DamageResult(attacker, defender, 0, false, true);

        public override string ToString()
        {
            if (IsMiss) return $"{Attacker?.Name} → {Defender?.Name}: MISS";
            string crit = IsCritical ? " [CRIT!]" : "";
            return $"{Attacker?.Name} → {Defender?.Name}: {FinalDamage} dmg{crit}";
        }
    }

    public readonly struct AbilityPayload
    {
        public readonly string     AbilityName;
        public readonly DamageType DamageType;
        public readonly float      PowerMultiplier;

        public AbilityPayload(string name, DamageType type = DamageType.Physical, float mult = 1f)
        {
            AbilityName     = name;
            DamageType      = type;
            PowerMultiplier = mult;
        }

        public static AbilityPayload BasicAttack =>
            new AbilityPayload("BasicAttack", DamageType.Physical, 1f);
    }
}
