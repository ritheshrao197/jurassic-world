using System.Collections;
using System.Collections.Generic;
using DinosBattle.Combat;

namespace DinosBattle
{
    // Basic attack: resolve damage → play attacker animation → play defender reaction.
    public class AttackCommand : IBattleCommand
    {
        private readonly CombatUnit               _attacker;
        private readonly IReadOnlyList<CombatUnit> _enemies;
        private readonly CombatResolver            _resolver;

        public AttackCommand(CombatUnit attacker, IReadOnlyList<CombatUnit> enemies, CombatResolver resolver)
        { _attacker = attacker; _enemies = enemies; _resolver = resolver; }

        public IEnumerator Execute()
        {
            var result = _resolver.ResolveAttack(_attacker, _enemies);
            if (!result.HasValue) yield break;

            var defender = result.Value.Defender;

            if (_attacker.Animator != null)
                yield return _attacker.Animator.Play(AnimationType.Attack,
                    defender.Model != null ? defender.Model.transform : null);

            if (defender.Animator != null)
                yield return defender.Animator.Play(defender.IsAlive ? AnimationType.Hurt : AnimationType.Death);
        }
    }
}