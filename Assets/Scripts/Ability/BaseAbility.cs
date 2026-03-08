using System.Collections;
using System.Collections.Generic;

namespace DinosBattle
{
    // Base ability — subclasses only override what they need.
    public abstract class BaseAbility : IAbility
    {
        public abstract string        Name          { get; }
        public abstract int           CooldownTurns { get; }
        public abstract AbilityTarget Targeting     { get; }

        public virtual bool CanUse(CombatUnit user) =>
            user.IsAlive && !user.IsOnCooldown(Name);

        public abstract IEnumerator Execute(CombatUnit user, IReadOnlyList<CombatUnit> targets);
    }
}
