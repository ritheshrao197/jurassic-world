using UnityEngine;

namespace DinosBattle
{
    public abstract class BaseStatusEffect : IStatusEffect
    {
        public abstract StatusEffectType Type { get; }
        public int  RemainingTurns { get; protected set; }
        public bool IsExpired      => RemainingTurns <= 0;

        protected BaseStatusEffect(int turns) => RemainingTurns = turns;

        public virtual void OnApply(CombatUnit owner)     => Debug.Log($"[Status] {owner.Name} got {Type}");
        public virtual void OnTurnStart(CombatUnit owner) { }
        public virtual void OnTurnEnd(CombatUnit owner)   => RemainingTurns--;
        public virtual void OnRemove(CombatUnit owner)    => Debug.Log($"[Status] {Type} expired on {owner.Name}");

        public virtual void Stack(IStatusEffect incoming) =>
            RemainingTurns = Mathf.Max(RemainingTurns, incoming.RemainingTurns);
    }

    // Deals 8% max HP each turn end.
    public class PoisonEffect : BaseStatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Poison;
        private readonly float _percent;

        public PoisonEffect(int turns, float percent = 0.08f) : base(turns) => _percent = percent;

        public override void OnTurnEnd(CombatUnit owner)
        {
            int dmg = Mathf.Max(1, Mathf.RoundToInt(owner.Stats.MaxHealth * _percent));
            owner.TakeDamage(dmg);
            if (!owner.IsAlive) ServiceLocator.Get<EventBus>().Publish(new UnitDefeatedEvent(owner));
            ServiceLocator.Get<EventBus>().Publish(new HealthChangedEvent(owner));
            Debug.Log($"[Poison] {owner.Name} took {dmg} dmg.");
            base.OnTurnEnd(owner);
        }
    }

    // Unit loses its action for N turns.
    public class StunEffect : BaseStatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Stun;
        public StunEffect(int turns) : base(turns) { }
    }

    // Restores 15 HP each turn start.
    public class RegenEffect : BaseStatusEffect
    {
        public override StatusEffectType Type => StatusEffectType.Regen;
        private readonly int _heal;

        public RegenEffect(int turns, int heal = 15) : base(turns) => _heal = heal;

        public override void OnTurnStart(CombatUnit owner)
        {
            owner.Heal(_heal);
            ServiceLocator.Get<EventBus>().Publish(new HealthChangedEvent(owner));
            Debug.Log($"[Regen] {owner.Name} recovered {_heal} HP.");
        }
    }
}
