using System.Collections;
using System.Collections.Generic;

namespace DinosBattle
{
    // Ability: play animation → execute ability logic → apply cooldown.
    public class AbilityCommand : IBattleCommand
    {
        private readonly CombatUnit               _user;
        private readonly IAbility                 _ability;
        private readonly IReadOnlyList<CombatUnit> _enemies;
        private readonly ITargetSelector          _selector;

        public AbilityCommand(CombatUnit user, IAbility ability,
                               IReadOnlyList<CombatUnit> enemies, ITargetSelector selector)
        { _user = user; _ability = ability; _enemies = enemies; _selector = selector; }

        public IEnumerator Execute()
        {
            if (!_ability.CanUse(_user)) yield break;

            if (_user.Animator != null)
                yield return _user.Animator.Play(AnimationType.Ability);

            var targets = _selector.SelectMany(_user, _enemies, _ability.Targeting);
            yield return _ability.Execute(_user, targets);
            _user.SetCooldown(_ability.Name, _ability.CooldownTurns);
        }
    }
}