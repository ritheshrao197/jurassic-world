using UnityEngine;

namespace DinosBattle.Data
{
    [CreateAssetMenu(menuName = "DinosBattle/DinosaurData")]
    public class DinosaurData : ScriptableObject
    {
        [Header("Identity")]
        public string     dinoName    = "Unknown";
        public Sprite     portrait;
        public GameObject modelPrefab;

        [Header("Stats")]
        [Min(1)] public int   maxHealth   = 100;
        [Min(1)] public int   attack      = 20;
        [Min(0)] public int   defense     = 5;
        [Min(1)] public int   speed       = 10;
        [Range(0f, 1f)] public float critChance     = 0.10f;
        [Range(1f, 3f)] public float critMultiplier = 1.50f;

        public CombatStatistics ToStatBlock() =>
            new CombatStatistics(maxHealth, attack, defense, speed, critChance, critMultiplier);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(dinoName)) dinoName = name;
        }
    }
}
