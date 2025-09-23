using Core;
using Enums;
using Interface;
using Managers;
using UnityEngine;

namespace Components.Buildings
{
    /// <summary>
    /// 资源生产建筑组件
    /// </summary>
    public class ProductionComponent : MonoBehaviour, IInfoProvider
    {
        public BuildingComponent buildingComponent;
        public float timer = 0f;
        public bool IsProducing;
        
        private void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();
        }

        private void OnEnable()
        {
            EventManager.OnDayStart += OnDayStart;
            EventManager.OnNightStart += OnNightStart;
        }
        private void OnDisable()
        {
            EventManager.OnDayStart -= OnDayStart;
            EventManager.OnNightStart -= OnNightStart;
        }
        
        private void Update()
        {
            if (IsProducing)
            {
                timer += Time.deltaTime;
                if (timer >= buildingComponent.Data.ProductionInterval)
                {
                    ProduceResource();
                    timer = 0f;
                }
            }
        }

        void OnDayStart(int day)
        {
            IsProducing = true;
        }
        void OnNightStart(int day)
        {
            IsProducing = false;
        }
        
        private void ProduceResource()
        {
            var data = buildingComponent.Data;

            int finalAmount = ResourceManager.Instance.CalculateFinalProduction(buildingComponent.LevelData.BaseProduction);

            // 添加资源
            switch (data.ResourceType)
            {
                case ResourceType.Food:
                    ResourceManager.Instance.AddFood(finalAmount);
                    break;
                case ResourceType.Gold:
                    ResourceManager.Instance.AddGold(finalAmount);
                    break;
                case ResourceType.Knowledge:
                    ResourceManager.Instance.AddKnowledge(finalAmount);
                    break;
            }

            // 显示飘字提示
            ShowProductionFloatingText($"+{finalAmount}");
        }
        
        private void ShowProductionFloatingText(string text)
        {
            if (FloatingTextManager.Instance == null) return;
            
            Vector3 floatingTextPosition = transform.position + new Vector3(0, 0.7f, 0);
            FloatingTextManager.Instance.ShowResourceProduction(text, floatingTextPosition);
        }
        
        private string GetLocalizedResourceName(ResourceType resourceType)
        {
            // 这里可以根据语言设置返回不同的文本
            // 暂时使用中文
            switch (resourceType)
            {
                case ResourceType.Gold:
                    return "金币";
                case ResourceType.Knowledge:
                    return "学识";
                case ResourceType.Food:
                    return "食物";
                default:
                    return resourceType.ToString();
            }
        }

        public string GetInfoText()
        {
            return LocalizationManager.Instance.GetLocalizedBuildingDescription(buildingComponent.Data.BuildingType) + "\n" + LocalizationManager.Instance.GetGameText("building.production", ResourceManager.Instance.CalculateFinalProduction(buildingComponent.LevelData.BaseProduction), buildingComponent.Data.ProductionInterval)+"\n" + LocalizationManager.Instance.GetGameText("building.daytime_only");
        }
    }
}