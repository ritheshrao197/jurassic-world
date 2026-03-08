using System.Collections.Generic;
using System.Linq;

namespace DinosBattle.Combat
{
    // Always targets the lowest-HP enemy (smarter AI strategy).
    public class LowestHpTargetSelector : ITargetSelector
    {
        public CombatUnit SelectOne(CombatUnit actor, IReadOnlyList<CombatUnit> candidates) =>
            candidates.Where(c => c.IsAlive).OrderBy(c => c.CurrentHealth).FirstOrDefault();

        public IReadOnlyList<CombatUnit> SelectMany(CombatUnit actor, IReadOnlyList<CombatUnit> candidates,
                                                     AbilityTarget targeting)
        {
            var alive = candidates.Where(c => c.IsAlive).OrderBy(c => c.CurrentHealth).ToList();
            return targeting == AbilityTarget.AllEnemies ? alive
                 : alive.Count > 0 ? new[] { alive[0] } : new CombatUnit[0];
        }
    }
}