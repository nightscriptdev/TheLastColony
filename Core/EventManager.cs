using System;
using Components.Buildings;
using Components.Enemies;
using Data;
using Data.Buildings;
using UI;
using UnityEngine;

namespace Core
{
    public static class EventManager
    {
        public static Action OnGameStart;
        public static Action OnGameEnd;
        public static Action OnGamePause;
        public static Action OnGameResume;
    
        public static Action<int> OnDayStart;    // 参数：第几天
        public static Action<int> OnNightStart;  // 参数：第几天
        
        public static Action<float> OnDayTick;
        public static Action<float> OnNightTick;
        
        public static Action<float> OnTimeScaleChanged; // 参数：新的时间倍速
    
        public static Action<int> OnPopulationChanged;  // 参数：新的人口数
        public static Action<int> OnFoodChanged;        // 参数：新的食物数
        public static Action<int> OnGoldChanged;        // 参数：新的金币数
        public static Action<int> OnKnowledgeChanged;   // 参数：新的学识数
        
        public static Action<BuildingComponent> OnBuildingPlaced;
        public static Action<BuildingData> OnBuildingButtonClick;

        public static Action<EnemyComponent> OnEnemyDeath;
        
        public static Action<ResearchData> OnResearchComplete;

        public static Action<Vector3> OnEnemyStayed;
        public static Action<Vector3> OnGridRelease;

        public static Action<Vector3> OnCellBecomeObstacle;
    }
}