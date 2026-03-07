using UnityEngine;
using DinosBattle.Core.Models;

namespace DinosBattle.Data
{
    [CreateAssetMenu(fileName = "NewDinosaur", menuName = "DinosBattle/DinosaurData")]
    public class DinosaurData : ScriptableObject
    {
        [Header("Identity")]
        public string     dinoName    = "Unknown";
        public Sprite     portrait;
        public GameObject modelPrefab;

        [Header("Stats")]
        [Min(1)] public int   maxHealth   = 100;
        [Min(1)] public int   attackPower = 20;
        [Min(0)] public int   defense     = 5;
        [Min(1)] public int   speed       = 10;
        [Range(0f, 1f)] public float critChance     = 0.1f;
        [Range(1f, 3f)] public float critMultiplier = 1.5f;

        public StatBlock ToStatBlock() =>
            new StatBlock(maxHealth, attackPower, defense, speed, critChance, critMultiplier);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(dinoName)) dinoName = name;
        }
    }
}
