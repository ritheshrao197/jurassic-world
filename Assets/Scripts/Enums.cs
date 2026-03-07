namespace DinosBattle
{
    public enum TeamId           { Player, Enemy }
    public enum DamageType       { Physical, Poison }
    public enum StatusEffectType { Poison, Stun, Regen }
    public enum AbilityTarget    { SingleEnemy, AllEnemies, Self }
    public enum AnimationType    { Attack, Hurt, Death, Victory, Ability }
    public enum BattleOutcome    { PlayerVictory, EnemyVictory }
    public enum GameState        { MainMenu, Loading, Battle, Paused, Victory, Defeat }
}
