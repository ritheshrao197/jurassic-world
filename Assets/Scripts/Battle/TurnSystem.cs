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
        public CombatUnit CurrentCombatant { get; private set; }

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

      /// <summary>
      /// Advances to the next alive unit in the turn order. If the end of the turn queue is reached, it automatically rebuilds the round by recalculating the turn order based on the current state of the battle. This method ensures that only alive units are included in the turn sequence and that the turn order is updated dynamically as units are defeated or their speed changes. The Current property is updated to reflect the unit whose turn it is after advancing, allowing other systems to react accordingly. This approach maintains a fluid and responsive turn system that adapts to the evolving conditions of the battle, providing a strategic layer for players to consider when planning their actions.
      /// The method first increments the index to move to the next unit in the turn order. It then checks if the current unit is alive; if not, it continues to advance until it finds an alive unit or reaches the end of the queue. If the end of the queue is reached without finding an alive unit, it calls the Rebuild method to start a new round with an updated turn order based on the remaining alive units. Finally, it returns the Current unit, which is now set to the next active combatant in the battle.
      /// This method is crucial for maintaining the flow of combat, ensuring that turns are only given to units that can act and that the turn order reflects the current state of the battle. By automatically rebuilding the round when necessary, it allows for a seamless transition between rounds and keeps players engaged in the strategic aspects of managing their units' actions and anticipating their opponents' moves.
      /// Overall, the Advance method is a key component of the TurnSystem, responsible for progressing the turn order while accounting for unit deaths and ensuring that the battle flow remains dynamic and responsive to the changing conditions of the fight. It provides a robust mechanism for managing turns in a way that enhances gameplay and strategic depth.
      /// The method is designed to handle any number of players and enemies, making it scalable for larger battles without requiring changes to the underlying logic. It effectively manages the turn order while accounting for unit deaths and speed variations, providing a robust and adaptable system for determining the sequence of actions in the battle.
      /// The Advance method is called by the BattleCoordinator at the end of each unit's turn to move to the next unit in the sequence. It ensures that the turn system remains accurate and responsive to the current state of the battle, allowing for a dynamic and engaging combat experience. By automatically skipping dead units and rebuilding the round when necessary, it maintains a smooth flow of combat and keeps players focused on their strategic decisions rather than manual turn management.
      /// In summary, the Advance method is essential for progressing the turn order in the battle system, ensuring that only alive units are given turns and that the turn order is dynamically updated as the battle evolves. It provides a seamless and responsive mechanism for managing turns, enhancing both gameplay and strategic depth in the combat system.
      /// The method is designed to be called repeatedly throughout the battle, allowing for a continuous flow of turns as units take actions and are defeated. It ensures that the turn system remains accurate and responsive to the current state of the battle, providing a dynamic and engaging combat experience for players. By automatically skipping dead units and rebuilding the round when necessary, it maintains a smooth flow of combat and keeps players focused on their strategic decisions rather than manual turn management.
      /// </summary>
      /// <returns></returns>
        public CombatUnit Advance()
        {
            _index++;

            // Skip dead units mid-round
            while (_index < _order.Count && !_order[_index].IsAlive)
                _index++;

            if (_index >= _order.Count)
                Rebuild();
            else
                CurrentCombatant = _order[_index];

            return CurrentCombatant;
        }

        // Weave player and enemy lists into alternating order, both sorted by Speed.
        // If one team has more members, the extra units are appended at the end.
        /// <summary>
        /// Rebuilds the turn order at the end of each round, weaving alive players and enemies sorted by Speed.
        /// If one team has more alive members, the extra units are appended at the end of the order.
        /// This ensures a dynamic turn order that adapts to the current state of the battle, with faster units acting first and dead units automatically skipped.
        /// The order is logged for debugging purposes each time it is rebuilt, showing the sequence of units that will act in the next round. This method is called automatically when the turn queue is exhausted, starting a new round with an updated order based on the remaining alive units.
        /// The method first filters out dead units from both teams, sorts the alive units by their Speed stat in descending order, and then weaves them together into a single turn order list. The current index is reset to the beginning of the new order, and the Current unit is updated accordingly. This approach allows for a flexible and responsive turn system that reflects the changing dynamics of the battle.
        /// The turn order is determined at the start of each round, ensuring that any changes in unit status (such as deaths or speed alterations) are accounted for in the sequence of actions. This method is crucial for maintaining a fair and engaging battle flow, as it dynamically adjusts to the evolving conditions of the fight. The logging of the new round order provides valuable insight into the turn sequence, aiding in debugging and enhancing player understanding of the battle mechanics.
        /// The Rebuild method is a key component of the TurnSystem, responsible for recalculating the turn order based on the current state of the battle. It ensures that only alive units are included in the turn sequence and that they are ordered by their Speed stat, allowing for a dynamic and strategic battle experience. By weaving players and enemies together, it creates an alternating turn order that keeps the battle engaging and unpredictable.
        /// The method is designed to handle any number of players and enemies, making it scalable for larger battles without requiring changes to the underlying logic. It effectively manages the turn order while accounting for unit deaths and speed variations, providing a robust and adaptable system for determining the sequence of actions in the battle.
        /// Overall, the Rebuild method is essential for maintaining an accurate and responsive turn order in the battle system, ensuring that the flow of combat remains dynamic and reflective of the current state of the fight. It allows for a seamless transition between rounds while keeping players informed of the upcoming turn sequence through logging, enhancing both gameplay and debugging processes.
        /// The method is called automatically at the end of each round when the turn queue is exhausted, ensuring that the turn order is always up-to-date with the current status of the units in the battle. This dynamic rebuilding of the turn order adds depth and strategy to the combat system, as players must consider not only their own unit's speed but also the potential actions of their opponents when planning their moves.
        /// </summary>
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
            CurrentCombatant = _order.Count > 0 ? _order[0] : null;

            Debug.Log("[Turn] New round order: " + string.Join(" → ", _order.Select(u => $"{u.Name}({u.Team})")));
        }
    }
}