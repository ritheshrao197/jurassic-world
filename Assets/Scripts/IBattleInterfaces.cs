using System.Collections;
using System.Collections.Generic;
using DinosBattle.Core.Models;

namespace DinosBattle.Core.Interfaces
{
    public interface IDamageCalculator
    {
        DamageResult Calculate(CombatUnit attacker, CombatUnit defender, AbilityPayload ability);
    }

    public interface ITargetSelector
    {
        CombatUnit SelectTarget(CombatUnit actor, IReadOnlyList<CombatUnit> candidates);
        IReadOnlyList<CombatUnit> SelectTargets(CombatUnit actor, IReadOnlyList<CombatUnit> candidates, AbilityTarget targeting);
    }

    public interface ITurnOrderStrategy
    {
        IReadOnlyList<CombatUnit> BuildOrder(System.Collections.Generic.IEnumerable<CombatUnit> allUnits);
    }

    public interface IBattleCommand
    {
        IEnumerator Execute();
    }

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
        bool             IsExpired      { get; }
        void             OnApply(CombatUnit owner);
        void             OnTurnStart(CombatUnit owner);
        void             OnTurnEnd(CombatUnit owner);
        void             OnRemove(CombatUnit owner);
        IStatusEffect    Stack(IStatusEffect incoming);
    }

    public interface ITurnSystem
    {
        CombatUnit               Current { get; }
        IReadOnlyList<CombatUnit> Order  { get; }
        void                     Initialize(System.Collections.Generic.IEnumerable<CombatUnit> units, ITurnOrderStrategy strategy);
        CombatUnit               Advance();
        void                     Rebuild();
    }
}
