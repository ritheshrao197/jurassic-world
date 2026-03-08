using System.Collections.Generic;
using UnityEngine;
using DinosBattle.Animation;
using DinosBattle.Data;

namespace DinosBattle
{
    // Creates CombatUnits from DinosaurData, instantiates their prefabs,
    // and assigns the DinoAnimator found on the prefab root.
    public class UnitFactory
    {
        public CombatUnit Create(DinosaurData data, TeamId team, int slot, Transform[] spawns = null)
        {
            var unit = new CombatUnit(data.dinoName, team, data.ToStatBlock());

            // Assign abilities based on speed stat
            unit.AddAbility(new TailWhipAbility());
            unit.AddAbility(unit.Stats.Speed > 12
                ? (IAbility) new PoisonBiteAbility()
                : new HealRoarAbility());

            if (data.modelPrefab != null)
            {
                var pos   = ResolveSpawn(team, slot, spawns);
                var rot =  Quaternion.identity;
               Debug.Log($"[Factory] Spawning '{data.dinoName}' for {team} at {pos} with rotation {rot.eulerAngles}");
                  if(data.dinoName=="TRex")
                {
                   rot =Quaternion.Euler(0, -150, 0);
                }
                else if (data.dinoName == "Velociraptor")
                {
                   rot =Quaternion.Euler(0, 130, 0);
                }
                 else if (data.dinoName == "Triceratops")
                {
                   rot =Quaternion.Euler(0, -130, 0);
                }
                else
                {
                   rot =Quaternion.Euler(0, 80, 0);
                }
                 var model = Object.Instantiate(data.modelPrefab, pos, rot);
                model.name    = $"{team}_{data.dinoName}_{slot}";
                unit.Model    = model;
                unit.Animator = model.GetComponentInChildren<DinoAnimator>();

                if (unit.Animator == null)
                    Debug.LogWarning($"[UnitFactory] '{data.modelPrefab.name}' is missing a DinoAnimator component.");
            }

            return unit;
        }

        public List<CombatUnit> CreateTeam(DinosaurData[] data, TeamId team, Transform[] spawns = null)
        {
            var list = new List<CombatUnit>(data.Length);
            for (int i = 0; i < data.Length; i++)
                list.Add(Create(data[i], team, i, spawns));
            return list;
        }

        private static Vector3 ResolveSpawn(TeamId team, int slot, Transform[] points)
        {
            if (points != null && slot < points.Length && points[slot] != null)
                return points[slot].position;
            return new Vector3((team == TeamId.Player ? -4f : 4f) + slot * 2.5f, 0f, 0f);
        }
    }
}
