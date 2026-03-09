namespace DinosBattle
{
    // Immutable stat snapshot — passed by value, safe to cache.
    public readonly struct CombatStatistics
    {
        public readonly int   MaxHealth;
        public readonly int   Attack;
        public readonly int   Defense;
        public readonly int   Speed;
        public readonly float CriticalChance;
        public readonly float CriticalMultiplier;

        public CombatStatistics(int hp, int atk, int def, int spd,
                         float critical = 0.1f, float criticalMult = 1.5f)
        {
            MaxHealth      = hp;
            Attack         = atk;
            Defense        = def;
            Speed          = spd;
            CriticalChance     = critical;
            CriticalMultiplier = criticalMult;
        }
    }
}
