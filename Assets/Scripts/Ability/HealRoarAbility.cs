using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinosBattle
{
    // Self-heal: restores 30% of max HP.
    public class HealRoarAbility : BaseAbility
    {
        public override string        Name          => "Heal Roar";
        public override int           CooldownTurns => 4;
        public override AbilityTarget Targeting     => AbilityTarget.Self;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            int amount = Mathf.RoundToInt(user.Stats.MaxHealth * 0.3f);
            user.Heal(amount);

            var bus = ServiceLocator.Get<EventBus>();
            bus.Publish(new HealthChangedEvent(user));
            Debug.Log($"[Ability] {user.Name} healed {amount} HP.");
            yield break;
        }
    }
}
