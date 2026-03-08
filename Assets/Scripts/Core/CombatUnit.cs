using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DinosBattle.Animation;

namespace DinosBattle
{
    // Pure C# — no MonoBehaviour, fully testable.
    // Owns health, abilities, status effects, and cooldowns for one combatant.
    public class CombatUnit
    {
        public string      Name          { get; }
        public TeamId      Team          { get; }
        public StatBlock   Stats         { get; }
        public int         CurrentHealth { get; private set; }
        public bool        IsAlive       => CurrentHealth > 0;
        public float       HealthPercent => (float)CurrentHealth / Stats.MaxHealth;
        public GameObject  Model         { get; set; }
        public DinoAnimator Animator     { get; set; }

        private readonly List<IAbility>      _abilities     = new List<IAbility>();
        private readonly List<IStatusEffect> _statusEffects = new List<IStatusEffect>();
        private readonly Dictionary<string, int> _cooldowns = new Dictionary<string, int>();

        public IReadOnlyList<IAbility> Abilities => _abilities;

        public CombatUnit(string name, TeamId team, StatBlock stats)
        {
            Name          = name;
            Team          = team;
            Stats         = stats;
            CurrentHealth = stats.MaxHealth;
        }

        // ── Health ────────────────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Min(Stats.MaxHealth, CurrentHealth + amount);
        }

        // ── Status Effects ────────────────────────────────────────────────────

        public void AddStatus(IStatusEffect effect)
        {
            var existing = _statusEffects.FirstOrDefault(e => e.Type == effect.Type);
            if (existing != null) existing.Stack(effect);
            else { _statusEffects.Add(effect); effect.OnApply(this); }
        }

        public bool HasStatus(StatusEffectType type) =>
            _statusEffects.Any(e => e.Type == type && !e.IsExpired);

        public void TickStatusEffects(bool isTurnStart)
        {
            foreach (var e in _statusEffects)
            {
                if (isTurnStart) e.OnTurnStart(this);
                else             e.OnTurnEnd(this);
            }

            var expired = _statusEffects.Where(e => e.IsExpired).ToList();
            foreach (var e in expired) { _statusEffects.Remove(e); e.OnRemove(this); }
        }

        // ── Abilities & Cooldowns ─────────────────────────────────────────────

        public void AddAbility(IAbility ability) => _abilities.Add(ability);

        public bool IsOnCooldown(string name) =>
            _cooldowns.TryGetValue(name, out int cd) && cd > 0;

        public void SetCooldown(string name, int turns) => _cooldowns[name] = turns;

        public void TickCooldowns()
        {
            foreach (var key in new List<string>(_cooldowns.Keys))
                if (_cooldowns[key] > 0) _cooldowns[key]--;
        }

        public override string ToString() =>
            $"{Name} [{Team}] HP:{CurrentHealth}/{Stats.MaxHealth} SPD:{Stats.Speed}";
    }
}
