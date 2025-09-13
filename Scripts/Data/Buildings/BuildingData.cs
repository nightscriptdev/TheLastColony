using UnityEngine;
using Enums;

namespace Data.Buildings
{
    /// <summary>
    /// 建筑基础数据配置 - 包含所有等级数据
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Game Data/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [System.Serializable]
        public class LevelData
        {
            [Header("等级信息")]
            public int MaxHP = 100;
            public int UpgradeCost = 75;
            public Sprite sprite;
            
            [Header("房屋功能")]
            public int PopulationCapacity = 2;
            
            [Header("资源生产功能")]
            public int BaseProduction = 10;
            
            [Header("魔法塔功能")]
            public int MinDamage = 5;
            public int MaxDamage = 10;
            public float AttackInterval = 1.5f;
            public float AttackRange = 3.0f;
            public TowerAttackType AttackType = TowerAttackType.Single;
            public bool HasSlowEffect = false;
            public int PierceCount = 1;
            public int MultiShotCount = 1;
            public int ConsecutiveAttacks = 1;
            
            [Header("特殊效果")]
            public bool ReflectDamage = false;
            public float ReflectPercent = 1.0f;
        }
        
        [Header("基本信息")]
        public BuildingType BuildingType;
        public BuildingFunction Function;
        public string BuildingName;
        [TextArea(2, 4)]
        public string Description;
        public GameObject Prefab;
        public int BuildCost = 50;
        public float HpIncreaseRate = 100f;        // 建造速度
        public bool CanBuildDirectly = true;       // 是否可以直接建造
        public ResourceType ResourceType = ResourceType.Food; // 生产的资源类型
        public float ProductionInterval = 30f;     // 生产间隔
        
        [Header("等级数据")]
        public LevelData[] LevelDatas;
        
        /// <summary>
        /// 是否可以升级到指定等级
        /// </summary>
        public bool CanUpgradeTo(int targetLevelIndex)
        {
            return targetLevelIndex > 0 && targetLevelIndex < LevelDatas.Length;
        }
        
        /// <summary>
        /// 是否是房屋建筑
        /// </summary>
        public bool IsHousing => Function == BuildingFunction.Housing;
        
        /// <summary>
        /// 是否是生产建筑
        /// </summary>
        public bool IsProduction => Function == BuildingFunction.ResourceProduction;
        
        /// <summary>
        /// 是否是防御建筑
        /// </summary>
        public bool IsDefense => Function == BuildingFunction.Attack;
        
        /// <summary>
        /// 是否是魔法塔
        /// </summary>
        public bool IsTower => BuildingType.ToString().Contains("Tower");
        
        /// <summary>
        /// 获取指定等级的随机伤害值
        /// </summary>
        public int GetRandomDamage(int levelIndex)
        {
            var data = LevelDatas[levelIndex];
            return Random.Range(data.MinDamage, data.MaxDamage + 1);
        }
    }
}