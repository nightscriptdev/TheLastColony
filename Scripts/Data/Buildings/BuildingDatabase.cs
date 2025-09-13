using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

namespace Data.Buildings
{
    [CreateAssetMenu(fileName = "BuildingDatabase", menuName = "Game Data/Building Database")]
    public class BuildingDatabase : ScriptableObject
    {
        [Header("所有建筑数据")]
        public BuildingData[] allBuildings;
        
        private Dictionary<BuildingType, BuildingData> buildingDict;
        
        private void OnEnable()
        {
            InitializeDictionary();
        }
        
        private void InitializeDictionary()
        {
            if (allBuildings == null) return;
            
            buildingDict = new Dictionary<BuildingType, BuildingData>();
            foreach (var building in allBuildings)
            {
                if (building == null) continue;
                
                if (!buildingDict.TryAdd(building.BuildingType, building))
                {
                    Debug.LogWarning($"重复的建筑类型：{building.BuildingType}");
                }
            }
        }

        /// <summary>
        /// 获取建筑数据
        /// </summary>
        public BuildingData GetBuildingData(BuildingType buildingType)
        {
            // 确保字典已初始化
            if (buildingDict == null) InitializeDictionary();
            
            if (buildingDict.TryGetValue(buildingType, out var data))
            {
                return data;
            }

            Debug.LogError($"找不到建筑类型 {buildingType} 的数据！");
            return null;
        }
        
        /// <summary>
        /// 获取指定等级的建筑数据
        /// </summary>
        public BuildingData.LevelData GetBuildingLevelData(BuildingType buildingType, int levelIndex)
        {
            var buildingData = GetBuildingData(buildingType);
            return buildingData?.LevelDatas[levelIndex];
        }
        
        /// <summary>
        /// 获取所有可直接建造的建筑
        /// </summary>
        public BuildingData[] GetDirectlyBuildableBuildings()
        {
            if (allBuildings == null) return new BuildingData[0];
            
            return allBuildings.Where(b => b != null && b.CanBuildDirectly).ToArray();
        }
        
        /// <summary>
        /// 获取指定功能类型的建筑
        /// </summary>
        public BuildingData[] GetBuildingsByFunction(BuildingFunction function)
        {
            if (allBuildings == null) return new BuildingData[0];
            
            return allBuildings.Where(b => b != null && b.Function == function).ToArray();
        }
        
        /// <summary>
        /// 检查建筑是否可以升级
        /// </summary>
        public bool CanUpgrade(BuildingType buildingType, int currentLevel)
        {
            var buildingData = GetBuildingData(buildingType);
            return buildingData?.CanUpgradeTo(currentLevel + 1) ?? false;
        }
    }
}