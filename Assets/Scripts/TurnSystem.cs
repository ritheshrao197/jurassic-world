using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DinosBattle.Battle
{
    // Builds an alternating turn order: fastest Player, fastest Enemy, next Player, next Enemy…
    // Within each team, units are sorted by Speed descending so the fastest acts first.
    // Dead units are skipped on Advance(); the queue rebuilds each round.
    //
    // Example with 2v2 (speeds P1=20, P2=8, E1=15, E2=6):
    //   Round order: P1 → E1 → P2 → E2 → (rebuild) → …
    //
    // Scales to any NvN without changes — just pass more units to Initialize().
    public class TurnSystem
    {
        public CombatUnit Current { get; private set; }

        private List<CombatUnit> _players  = new List<CombatUnit>();
        private List<CombatUnit> _enemies  = new List<CombatUnit>();
        private List<CombatUnit> _order    = new List<CombatUnit>();
        private int              _index;

        public void Initialize(IEnumerable<CombatUnit> players, IEnumerable<CombatUnit> enemies)
        {
            _players = new List<CombatUnit>(players);
            _enemies = new List<CombatUnit>(enemies);
            Rebuild();
        }

        // Advances to the next alive unit. Rebuilds the round when the queue is exhausted.
        public CombatUnit Advance()
        {
            _index++;

            // Skip dead units mid-round
            while (_index < _order.Count && !_order[_index].IsAlive)
                _index++;

            if (_index >= _order.Count)
                Rebuild();
            else
                Current = _order[_index];

            return Current;
        }

        // Weave player and enemy lists into alternating order, both sorted by Speed.
        // If one team has more members, the extra units are appended at the end.
        private void Rebuild()
        {
            var pAlive = _players.Where(u => u.IsAlive).OrderByDescending(u => u.Stats.Speed).ToList();
            var eAlive = _enemies.Where(u => u.IsAlive).OrderByDescending(u => u.Stats.Speed).ToList();

            _order.Clear();
            int count = Mathf.Max(pAlive.Count, eAlive.Count);
            for (int i = 0; i < count; i++)
            {
                if (i < pAlive.Count) _order.Add(pAlive[i]);
                if (i < eAlive.Count) _order.Add(eAlive[i]);
            }

            _index  = 0;
            Current = _order.Count > 0 ? _order[0] : null;

            Debug.Log("[Turn] New round order: " + string.Join(" → ", _order.Select(u => $"{u.Name}({u.Team})")));
        }
    }
}