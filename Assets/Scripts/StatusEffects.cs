using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;

namespace DinosBattle.Systems.StatusEffects
{
    public abstract class BaseStatusEffect : IStatusEffect
    {
        public abstract string           EffectName     { get; }
        public abstract StatusEffectType Type           { get; }
        public          int              RemainingTurns { get; protected set; }
        public          bool             IsExpired      => RemainingTurns <= 0;

        protected BaseStatusEffect(int turns) => RemainingTurns = turns;

        public virtual void OnApply(CombatUnit owner)     => Debug.Log($"[Status] {owner.Name} got {EffectName}");
        public virtual void OnTurnStart(CombatUnit owner) { }
        public virtual void OnTurnEnd(CombatUnit owner)   => RemainingTurns--;
        public virtual void OnRemove(CombatUnit owner)    => Debug.Log($"[Status] {EffectName} ended on {owner.Name}");

        public virtual IStatusEffect Stack(IStatusEffect incoming)
        {
            RemainingTurns = Mathf.Max(RemainingTurns, incoming.RemainingTurns);
            return this;
        }
    }

    // Deals % max HP damage each turn
    public class PoisonEffect : BaseStatusEffect
    {
        public override string           EffectName => "Poison";
        public override StatusEffectType Type       => StatusEffectType.Poison;

        private readonly float _percent;

        public PoisonEffect(int turns, float percent = 0.08f) : base(turns) => _percent = percent;

        public override void OnTurnEnd(CombatUnit owner)
        {
            int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.BaseStats.MaxHealth * _percent));
            owner.ApplyDamage(dmg);
            Debug.Log($"[Poison] {owner.Name} took {dmg} damage.");
            base.OnTurnEnd(owner);
        }
    }

    // Skips the unit's action for N turns
    public class StunEffect : BaseStatusEffect
    {
        public override string           EffectName => "Stun";
        public override StatusEffectType Type       => StatusEffectType.Stun;

        public StunEffect(int turns) : base(turns) { }
    }

    // Restores HP each turn
    public class RegenEffect : BaseStatusEffect
    {
        public override string           EffectName => "Regen";
        public override StatusEffectType Type       => StatusEffectType.Regen;

        private readonly int _heal;

        public RegenEffect(int turns, int heal = 15) : base(turns) => _heal = heal;

        public override void OnTurnStart(CombatUnit owner)
        {
            owner.RestoreHealth(_heal);
            Debug.Log($"[Regen] {owner.Name} recovered {_heal} HP.");
        }
    }
}
