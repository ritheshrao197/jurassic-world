namespace DinosBattle
{
    // Immutable stat snapshot — passed by value, safe to cache.
    public readonly struct StatBlock
    {
        public readonly int   MaxHealth;
        public readonly int   Attack;
        public readonly int   Defense;
        public readonly int   Speed;
        public readonly float CritChance;
        public readonly float CritMultiplier;

        public StatBlock(int hp, int atk, int def, int spd,
                         float crit = 0.1f, float critMult = 1.5f)
        {
            MaxHealth      = hp;
            Attack         = atk;
            Defense        = def;
            Speed          = spd;
            CritChance     = crit;
            CritMultiplier = critMult;
        }
    }
}
