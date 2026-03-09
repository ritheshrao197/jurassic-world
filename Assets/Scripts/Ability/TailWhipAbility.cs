using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Combat;

namespace DinosBattle
{
    // AOE attack: hits all enemies at 60% power.
    public class TailWhipAbility : BaseAbility
    {
        public override string        Name          => "Tail Whip";
        public override int           CooldownTurns => 3;
        public override AbilityTarget Targeting     => AbilityTarget.AllEnemies;

        public override IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets)
        {
            var resolver = ServiceLocator.Get<CombatResolver>();
            foreach (var t in targets.Where(t => t.IsAlive))
            {
                resolver.ResolveAttackOn(user, t, 0.6f);
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log($"[Ability] {user.Name} used {Name} on all enemies, dealing damage.");
        }
    }
}
