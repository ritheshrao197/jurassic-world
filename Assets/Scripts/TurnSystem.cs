using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Interfaces;

namespace DinosBattle.Systems.Turn
{
    // Turn order: highest speed goes first
    public class SpeedBasedTurnOrder : ITurnOrderStrategy
    {
        public IReadOnlyList<CombatUnit> BuildOrder(IEnumerable<CombatUnit> units) =>
            units.Where(u => u.IsAlive)
                 .OrderByDescending(u => u.BaseStats.Speed)
                 .ToList();
    }

    // Manages the turn queue. Skips dead units. Rebuilds each round.
    public class TurnSystem : ITurnSystem
    {
        public CombatUnit                Current { get; private set; }
        public IReadOnlyList<CombatUnit> Order   => _order;

        private List<CombatUnit>   _allUnits;
        private ITurnOrderStrategy _strategy;
        private List<CombatUnit>   _order = new List<CombatUnit>();
        private int                _index;

        public void Initialize(IEnumerable<CombatUnit> units, ITurnOrderStrategy strategy)
        {
            _allUnits = new List<CombatUnit>(units);
            _strategy = strategy;
            Rebuild();
        }

        public void Rebuild()
        {
            _order   = new List<CombatUnit>(_strategy.BuildOrder(_allUnits));
            _index   = 0;
            Current  = _order.Count > 0 ? _order[0] : null;
            Debug.Log("[Turn] Order: " + string.Join(" → ", _order.Select(u => u.Name)));
        }

        public CombatUnit Advance()
        {
            _index++;
            while (_index < _order.Count && !_order[_index].IsAlive)
                _index++;

            if (_index >= _order.Count)
                Rebuild();
            else
                Current = _order[_index];

            return Current;
        }
    }
}
