using System.Collections.Generic;
using System.Linq;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Infrastructure.EventBus;

namespace DinosBattle.Systems
{
    public class BattleTeam
    {
        public TeamId                    Id         { get; }
        public IReadOnlyList<CombatUnit> Members    => _members;
        public bool                      IsWiped    => _members.All(u => !u.IsAlive);

        private readonly List<CombatUnit> _members = new List<CombatUnit>();
        private readonly BattleEventBus   _bus;

        public BattleTeam(TeamId id, BattleEventBus bus)
        {
            Id   = id;
            _bus = bus;
        }

        public void AddMember(CombatUnit unit)
        {
            _members.Add(unit);
        }
    }
}
