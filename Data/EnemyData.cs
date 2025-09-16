using Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本信息")]
        public string enemyName = "敌人";
        public EnemyType enemyType = EnemyType.Normal;
        
        [Header("属性")]
        [Tooltip("基础生命值")]
        public int baseHP = 30;
        
        [Tooltip("攻击力")]
        public int attackDamage = 5;
        
        [Tooltip("移动速度")]
        [Range(0.75f, 1.5f)]
        public float baseSpeed = 1.0f;
        
        [Header("视觉")]
        [Tooltip("待机精灵图")]
        public Sprite idleSprite;
        
        [Tooltip("移动精灵图")]
        public Sprite moveSprite;
        
        [Tooltip("死亡特效预制体")]
        public GameObject deathEffectPrefab;
        
        [Header("预制体")]
        [Tooltip("敌人预制体")]
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