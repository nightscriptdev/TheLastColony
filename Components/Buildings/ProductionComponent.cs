using Core;
using Enums;
using Interface;
using Managers;
using UnityEngine;

namespace Components.Buildings
{
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

            switch (data.ResourceType)
            {
                case ResourceType.Food:
                    ResourceManager.Instance.Food += finalAmount;
                    break;
                case ResourceType.Gold:
                    ResourceManager.Instance.Gold+=finalAmount;
                    break;
                case ResourceType.Knowledge:
                    ResourceManager.Instance.Knowledge+=finalAmount;
                    break;
            }

            ShowProductionFloatingText($"+{finalAmount}");
        }
        
        private void ShowProductionFloatingText(string text)
        {
            if (FloatingTextManager.Instance == null) return;
            FloatingTextManager.Instance.ShowResourceProduction(text, transform.position + new Vector3(0, 0.7f, 0));
        }
        
        public string GetInfoText()
        {
            return LocalizationManager.Instance.GetLocalizedBuildingDescription(buildingComponent.Data.BuildingType) + "\n" + LocalizationManager.Instance.GetGameText("building.production", ResourceManager.Instance.CalculateFinalProduction(buildingComponent.LevelData.BaseProduction), buildingComponent.Data.ProductionInterval)+"\n" + LocalizationManager.Instance.GetGameText("building.daytime_only");
        }
    }
}