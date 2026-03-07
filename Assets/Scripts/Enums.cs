namespace DinosBattle.Core.Enums
{
    public enum TargetType
    {
        Self,
        Ally,
        Enemy,
        AllAllies,
        AllEnemies,
        RandomEnemy,
        // Future: Area, FrontRow, BackRow, etc.
    }
    public enum AnimationType
    {
        Attack,
        Hurt,
        Death,
        Victory,
        Idle
        // Future: Cast, Taunt, etc.
    }
    public enum AbilityTarget
    {
        Self,
        SingleEnemy,
        AllEnemies,
        AllAllies
    }
    public enum DamageType
    {
        Physical,
        Magical,
        True, Poison, Burn, Freeze, Heal, Shield, Stun,None
        
        // Future: Elemental, Status, etc.
    }
    public enum StatusEffectType
    {
        Poison,
        Shield,
        Stun,
        Regen,
        Burn, Freeze, Buff, Debuff    
        }
         public enum BattleOutcome
    {
        PlayerVictory,
        EnemyVictory,
        Draw,
    }
      public enum BattlePhase
    {
        TurnStart,
        TurnEnd,
        VictoryCheck,
        Animate,
        Idle,
        Setup,SelectAction, ExecuteAction
    }
    public enum TeamId
    {
        Player,
        Enemy,
    }
    
    
}