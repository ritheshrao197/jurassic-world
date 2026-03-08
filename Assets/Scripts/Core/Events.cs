namespace DinosBattle
{
    // ── Events ────────────────────────────────────────────────────────────────

    public interface IBattleEvent { }

    public readonly struct BattleStartedEvent   : IBattleEvent { }
    public readonly struct BattleEndedEvent     : IBattleEvent
    {
        public readonly BattleOutcome Outcome;
        public BattleEndedEvent(BattleOutcome o) { Outcome = o; }
    }
    public readonly struct UnitRegisteredEvent  : IBattleEvent
    {
        public readonly CombatUnit Unit;
        public UnitRegisteredEvent(CombatUnit u) { Unit = u; }
    }
    public readonly struct TurnStartedEvent     : IBattleEvent
    {
        public readonly CombatUnit Unit; public readonly int Turn;
        public TurnStartedEvent(CombatUnit u, int t) { Unit = u; Turn = t; }
    }
    public readonly struct AttackExecutedEvent  : IBattleEvent
    {
        public readonly DamageResult Result;
        public AttackExecutedEvent(DamageResult r) { Result = r; }
    }
    public readonly struct UnitDefeatedEvent    : IBattleEvent
    {
        public readonly CombatUnit Unit;
        public UnitDefeatedEvent(CombatUnit u) { Unit = u; }
    }
    public readonly struct HealthChangedEvent   : IBattleEvent
    {
        public readonly CombatUnit Unit;
        public HealthChangedEvent(CombatUnit u) { Unit = u; }
    }
}
