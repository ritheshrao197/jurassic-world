using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DinosBattle.Battle
{
    // Manages the turn queue. Orders by speed, skips dead units, rebuilds each round.
    public class TurnSystem
    {
        public CombatUnit Current { get; private set; }

        private List<CombatUnit> _allUnits = new List<CombatUnit>();
        private List<CombatUnit> _order    = new List<CombatUnit>();
        private int              _index;

        public void Initialize(IEnumerable<CombatUnit> units)
        {
            _allUnits = new List<CombatUnit>(units);
            Rebuild();
        }

        public void Rebuild()
        {
            _order   = _allUnits.Where(u => u.IsAlive).OrderByDescending(u => u.Stats.Speed).ToList();
            _index   = 0;
            Current  = _order.Count > 0 ? _order[0] : null;
            Debug.Log("[Turn] Order: " + string.Join(" → ", _order.Select(u => u.Name)));
        }

        public CombatUnit Advance()
        {
            _index++;
            while (_index < _order.Count && !_order[_index].IsAlive)
                _index++;

            if (_index >= _order.Count) Rebuild();
            else Current = _order[_index];

            return Current;
        }
    }
}
