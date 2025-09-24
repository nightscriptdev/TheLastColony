using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public int baseHP = 30;
        public int attackDamage = 5;
        [Range(0.75f, 1.5f)]
        public float baseSpeed = 1.0f;
        
        public GameObject deathEffectPrefab;
        public GameObject enemyPrefab;
        
        /// <summary>
        /// 获取指定天数的实际HP
        /// </summary>
        public int GetHPForDay(int day)
        {
            // HP = base × (1 + 0.10 × (day-1))
            return Mathf.RoundToInt(baseHP * (1f + 0.1f * (day - 1)));
        }
    }
}