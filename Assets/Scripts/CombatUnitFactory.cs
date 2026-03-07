using System.Collections.Generic;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Data;
using DinosBattle.Systems.Abilities;
using DinosBattle.Systems.Animation;

namespace DinosBattle.Systems.Spawn
{
    public class CombatUnitFactory
    {
        private readonly UnityAnimationHandler _anim;

        public CombatUnitFactory(UnityAnimationHandler anim) => _anim = anim;

        public CombatUnit Create(DinosaurData data, TeamId team, int slot, Transform[] spawnPoints = null)
        {
            var unit = new CombatUnit(data.dinoName, team, slot, data.ToStatBlock());

            // Assign abilities based on speed
            unit.AddAbility(new TailWhipAbility());
            unit.AddAbility(unit.BaseStats.Speed > 12
                ? (Systems.Abilities.BaseAbility) new PoisonBiteAbility()
                : new HealRoarAbility());

            if (data.modelPrefab != null)
            {
                var pos   = ResolveSpawn(team, slot, spawnPoints);
                var rot   = team == TeamId.Enemy ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                var model = Object.Instantiate(data.modelPrefab, pos, rot);
                model.name         = $"{team}_{data.dinoName}_{slot}";
                unit.ModelInstance = model;
                _anim?.RegisterUnit(unit);
            }

            return unit;
        }

        public IReadOnlyList<CombatUnit> CreateTeam(DinosaurData[] data, TeamId team, Transform[] spawnPoints = null)
        {
            var units = new List<CombatUnit>(data.Length);
            for (int i = 0; i < data.Length; i++)
                units.Add(Create(data[i], team, i, spawnPoints));
            return units;
        }

        private static Vector3 ResolveSpawn(TeamId team, int slot, Transform[] points)
        {
            if (points != null && slot < points.Length && points[slot] != null)
                return points[slot].position;

            return new Vector3((team == TeamId.Player ? -4f : 4f) + slot * 2.5f, 0f, 0f);
        }
    }
}
