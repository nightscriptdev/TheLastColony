using System;
using Components.Buildings;
using Components.Enemies;
using Data;
using Enums;
using UI;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 全局事件管理器，用于模块间解耦通信。
    /// </summary>
    public static class EventManager
    {
        // 游戏流程相关事件
        public static Action OnGameStart;
        public static Action OnGameEnd;
        public static Action OnGamePause;
        public static Action OnGameResume;
    
        // 时间相关事件
        public static Action<int> OnDayStart;    // 参数：第几天
        public static Action<int> OnNightStart;  // 参数：第几天
        
        public static Action<float> OnDayTick;
        public static Action<float> OnNightTick;
        
        public static Action<float> OnTimeScaleChanged; // 参数：新的时间倍速
    
        // 资源相关事件
        public static Action<int> OnPopulationChanged;  // 参数：新的人口数
        public static Action<int> OnFoodChanged;        // 参数：新的食物数
        public static Action<int> OnGoldChanged;        // 参数：新的金币数
        public static Action<int> OnKnowledgeChanged;   // 参数：新的学识数
        
        // 天气相关事件
        public static Action OnRainStart;
        public static Action OnRainStop;
        
        // 建造
        public static Action<BuildingComponent> OnBuildingPlaced;
        public static Action<BuildingType> OnBuildingButtonClick;

        public static Action<FloatingText> OnFloatingTextComplete;

        public static Action<EnemyComponent> OnEnemyDeath;
        
        public static Action<ResearchData> OnResearchComplete;

        public static Action<Vector3> OnEnemyStayed;
        public static Action<Vector3> OnGridRelease;

        public static Action<Vector3> OnCellBecomeObstacle;
    }
}