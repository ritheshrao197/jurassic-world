using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DinosBattle.Combat
{
    // Picks a random alive target; supports AOE and self-targeting.
    public class RandomTargetSelector : ITargetSelector
    {
        public CombatUnit SelectOne(CombatUnit actor, IReadOnlyList<CombatUnit> candidates)
        {
            var alive = candidates.Where(c => c.IsAlive).ToList();
            return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : null;
        }

        public IReadOnlyList<CombatUnit> SelectMany(CombatUnit actor, IReadOnlyList<CombatUnit> candidates,
                                                     AbilityTarget targeting)
        {
            var alive = candidates.Where(c => c.IsAlive).ToList();
            switch (targeting)
            {
                case AbilityTarget.AllEnemies: return alive;
                case AbilityTarget.Self:       return new[] { actor };
                default:
                    return alive.Count > 0 ? new[] { alive[Random.Range(0, alive.Count)] } : new CombatUnit[0];
            }
        }
    }
}