using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;
using DinosBattle.Core.Models;

namespace DinosBattle.Core
{
    // Pure C# — no MonoBehaviour, fully testable
    public class CombatUnit
    {
        public string     Name          { get; }
        public TeamId     Team          { get; }
        public int        SlotIndex     { get; }
        public string     UnitId        { get; } = Guid.NewGuid().ToString();
        public StatBlock  BaseStats     { get; }
        public int        CurrentHealth { get; private set; }
        public bool       IsAlive       => CurrentHealth > 0;
        public float      HealthPercent => (float)CurrentHealth / BaseStats.MaxHealth;
        public GameObject ModelInstance { get; set; }

        // Abilities
        private readonly List<IAbility> _abilities = new List<IAbility>();
        public IReadOnlyList<IAbility>   Abilities  => _abilities;

        // Status effects
        private readonly List<IStatusEffect> _statusEffects = new List<IStatusEffect>();

        // Cooldowns
        private readonly Dictionary<string, int> _cooldowns = new Dictionary<string, int>();

        public CombatUnit(string name, TeamId team, int slot, StatBlock stats)
        {
            Name          = name;
            Team          = team;
            SlotIndex     = slot;
            BaseStats     = stats;
            CurrentHealth = stats.MaxHealth;
        }

        public void ApplyDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        }

        public void RestoreHealth(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Min(BaseStats.MaxHealth, CurrentHealth + amount);
        }

        public void AddStatusEffect(IStatusEffect effect)
        {
            var existing = _statusEffects.FirstOrDefault(e => e.Type == effect.Type);
            if (existing != null)
            {
                existing.Stack(effect);
            }
            else
            {
                _statusEffects.Add(effect);
                effect.OnApply(this);
            }
        }

        public bool HasStatus(StatusEffectType type) =>
            _statusEffects.Any(e => e.Type == type && !e.IsExpired);

        public void TickStatusEffects(bool isTurnStart)
        {
            var expired = new List<IStatusEffect>();
            foreach (var e in _statusEffects)
            {
                if (isTurnStart) e.OnTurnStart(this);
                else             e.OnTurnEnd(this);
                if (e.IsExpired) expired.Add(e);
            }
            foreach (var e in expired)
            {
                _statusEffects.Remove(e);
                e.OnRemove(this);
            }
        }

        public void AddAbility(IAbility ability)          => _abilities.Add(ability);
        public bool IsOnCooldown(string name)              => _cooldowns.TryGetValue(name, out int cd) && cd > 0;
        public void SetCooldown(string name, int turns)    => _cooldowns[name] = turns;

        public void TickCooldowns()
        {
            var keys = new List<string>(_cooldowns.Keys);
            foreach (var k in keys)
                if (_cooldowns[k] > 0) _cooldowns[k]--;
        }

        public override string ToString() =>
            $"{Name}[{Team}] HP:{CurrentHealth}/{BaseStats.MaxHealth} SPD:{BaseStats.Speed}";
    }
}
