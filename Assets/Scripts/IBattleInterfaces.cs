using System.Collections;
using System.Collections.Generic;
using DinosBattle.Core.Models;
using DinosBattle.Core.Enums;

namespace DinosBattle.Core.Interfaces
{
    // ════════════════════════════════════════════════════════════════════════
    //  COMBAT INTERFACES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Strategy pattern: swappable damage formula.</summary>
    public interface IDamageCalculator
    {
        DamageResult Calculate(CombatUnit attacker, CombatUnit defender, AbilityPayload ability);
    }

    /// <summary>Strategy pattern: swappable target selection.</summary>
    public interface ITargetSelector
    {
        CombatUnit SelectTarget(CombatUnit actor, IReadOnlyList<CombatUnit> candidates);
        IReadOnlyList<CombatUnit> SelectTargets(CombatUnit actor, IReadOnlyList<CombatUnit> candidates, AbilityTarget targeting);
    }

    /// <summary>Strategy pattern: swappable turn ordering.</summary>
    public interface ITurnOrderStrategy
    {
        IReadOnlyList<CombatUnit> BuildOrder(System.Collections.Generic.IEnumerable<CombatUnit> allUnits);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  COMMAND PATTERN
    // ════════════════════════════════════════════════════════════════════════

    public interface IBattleCommand
    {
        string       Description { get; }
        IEnumerator  Execute();
        void         Undo();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ABILITY / STATUS EFFECT
    // ════════════════════════════════════════════════════════════════════════

    public interface IAbility
    {
        string        AbilityName   { get; }
        int           CooldownTurns { get; }
        AbilityTarget Targeting     { get; }
        bool          CanUse(CombatUnit user);
        IEnumerator   Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets);
    }

    public interface IStatusEffect
    {
        string           EffectName     { get; }
        StatusEffectType Type           { get; }
        int              RemainingTurns { get; }
        bool             IsExpired      { get; }   // NOTE: implemented as property, NOT default interface method
        void             OnApply(CombatUnit owner);
        void             OnTurnStart(CombatUnit owner);
        void             OnTurnEnd(CombatUnit owner);
        void             OnRemove(CombatUnit owner);
        IStatusEffect    Stack(IStatusEffect incoming);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ANIMATION
    // ════════════════════════════════════════════════════════════════════════

    public interface IAnimationHandler
    {
        IEnumerator PlayAnimation(AnimationType type, CombatUnit subject, CombatUnit target = null);
        bool        IsPlaying { get; }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  SERVICES
    // ════════════════════════════════════════════════════════════════════════

    public interface ITurnSystem
    {
        CombatUnit               Current { get; }
        IReadOnlyList<CombatUnit> Order  { get; }
        void                     Initialize(System.Collections.Generic.IEnumerable<CombatUnit> units, ITurnOrderStrategy strategy);
        CombatUnit               Advance();
        void                     Rebuild();
    }

    public interface IBattleLogger
    {
        void Log(string message, LogChannel channel = LogChannel.General);
        void LogAttack(CombatUnit attacker, CombatUnit target, DamageResult result);
        void LogStatusEffect(CombatUnit unit, IStatusEffect effect, bool applied);
        void LogTurnStart(CombatUnit unit, int turnNumber);
    }

    public enum LogChannel { General, Combat, Turn, System, UI }
}
