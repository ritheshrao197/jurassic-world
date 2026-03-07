using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;
using DinosBattle.Core.Models;
using DinosBattle.Infrastructure.EventBus;

namespace DinosBattle.Infrastructure
{
    /// <summary>
    /// Concrete IBattleLogger. Subscribes to EventBus and formats log output.
    /// Swap with analytics/file logger by registering a different IBattleLogger.
    /// </summary>
    public class BattleLogger : IBattleLogger
    {
        public BattleLogger(BattleEventBus bus)
        {
            bus.Subscribe<AttackExecutedEvent>(e  => LogAttack(e.Result.Attacker, e.Result.Defender, e.Result));
            bus.Subscribe<TurnStartedEvent>(e     => LogTurnStart(e.Unit, e.TurnNumber));
            bus.Subscribe<UnitDefeatedEvent>(e    => Log($"X {e.Unit.Name} defeated!", LogChannel.Combat));
            bus.Subscribe<BattleEndedEvent>(e     => Divider($"BATTLE OVER - {e.Outcome} in {e.TotalTurns} turns"));
        }

        public void Log(string msg, LogChannel channel = LogChannel.General)
        {
            Debug.Log($"[{channel}] {msg}");
        }

        public void LogAttack(CombatUnit attacker, CombatUnit target, DamageResult result)
        {
            string crit = result.IsCritical ? " *** CRITICAL! ***" : "";
            string miss = result.IsMiss     ? " (MISS)"            : "";
            Log($"Attack: {result}{crit}{miss}", LogChannel.Combat);
        }

        public void LogStatusEffect(CombatUnit unit, IStatusEffect effect, bool applied)
        {
            string verb = applied ? "afflicted with" : "cured of";
            Log($"{unit.Name} is {verb} [{effect.EffectName}]", LogChannel.Combat);
        }

        public void LogTurnStart(CombatUnit unit, int turnNumber)
        {
            Divider($"Turn {turnNumber}: {unit.Name} [{unit.Team}]");
        }

        private static void Divider(string label = "")
        {
            Debug.Log(label.Length > 0 ? $"=== {label} ===" : new string('=', 40));
        }
    }
}
