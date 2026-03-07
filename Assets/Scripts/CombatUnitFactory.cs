// using System.Collections.Generic;
// using UnityEngine;
// using DinosBattle.Animation;
// using DinosBattle.Data;
// using Unity.VisualScripting;

// namespace DinosBattle.Systems.Spawn
// {
//     public class CombatUnitFactory
//     {
//         public CombatUnit Create(DinosaurData data, TeamId team, int slot, Transform[] spawnPoints = null)
//         {
//             var unit = new CombatUnit(data.dinoName, team,  data.ToStatBlock());

//             unit.AddAbility(new TailWhipAbility());
//             unit.AddAbility(data.speed > 12
//                 ? (BaseAbility) new PoisonBiteAbility()
//                 : new HealRoarAbility());

//             if (data.modelPrefab != null)
//             {
//                 var pos   = ResolveSpawn(team, slot, spawnPoints);
//                 var rot   = team == TeamId.Enemy ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
//                 Debug.Log($"[Factory] Spawning '{data.dinoName}' for {team} at {pos} with rotation {rot.eulerAngles}");
//             if(data.dinoName=="TRex")
//                 {
//                    rot =Quaternion.Euler(0, 180, 0);
//                 }
//                 var model = Object.Instantiate(data.modelPrefab, pos, rot);
//                 model.name         = $"{team}_{data.dinoName}_{slot}";
//                 unit.Model = model;
//                 unit.Animator      = model.GetComponentInChildren<DinoAnimator>();

//                 if (unit.Animator == null)
//                     Debug.LogWarning($"[Factory] '{data.modelPrefab.name}' has no DinoAnimator — add it to the prefab root.");
//             }

//             return unit;
//         }

//         public IReadOnlyList<CombatUnit> CreateTeam(DinosaurData[] data, TeamId team, Transform[] spawnPoints = null)
//         {
//             var list = new List<CombatUnit>(data.Length);
//             for (int i = 0; i < data.Length; i++)
//                 list.Add(Create(data[i], team, i, spawnPoints));
//             return list;
//         }

//         private static Vector3 ResolveSpawn(TeamId team, int slot, Transform[] points)
//         {
//             if (points != null && slot < points.Length && points[slot] != null)
//                 return points[slot].position;
//             return new Vector3((team == TeamId.Player ? -4f : 4f) + slot * 2.5f, 0f, 0f);
//         }
//     }
// }
