using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DinosBattle.Combat;

namespace DinosBattle
{
    // Single target: deals 120% damage and poisons the target.
    public class PoisonBiteAbility : BaseAbility
    {
        public override string        Name          => "Poison Bite";
        public override int           CooldownTurns => 2;
        public override AbilityTarget Targeting     => AbilityTarget.SingleEnemy;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            var target = targets.FirstOrDefault(t => t.IsAlive);
            if (target == null) yield break;

            ServiceLocator.Get<CombatResolver>().ResolveAttackOn(user, target, 1.2f, DamageType.Poison);
            target.AddStatus(new PoisonEffect(3));
        }
    }
}
