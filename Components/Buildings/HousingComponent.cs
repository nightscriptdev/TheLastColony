using Core;
using Interface;
using UnityEngine;
namespace Components.Buildings
{
    public class HousingComponent : MonoBehaviour, IInfoProvider
    {
        private BuildingComponent buildingComponent;
        private int providedMaxPopulation = 0;
        private void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();
        }
        private void OnEnable()
        {
            if (buildingComponent != null)
            {
                buildingComponent.OnBuildingCompleted += OnHousingBuilt;
                buildingComponent.OnBuildingDestroyed += OnHousingDestroyed;
                buildingComponent.OnBuildingUpgraded += OnBuildingUpgraded;
            }
        }
        private void OnDisable()
        {
            if (buildingComponent != null)
            {
                buildingComponent.OnBuildingCompleted -= OnHousingBuilt;
                buildingComponent.OnBuildingDestroyed -= OnHousingDestroyed;
                buildingComponent.OnBuildingUpgraded -= OnBuildingUpgraded;

            }
        }
        private void OnHousingBuilt(BuildingComponent building)
        {
            providedMaxPopulation = building.LevelData.PopulationCapacity;
            ResourceManager.Instance.IncreaseMaxPopulation(providedMaxPopulation);
        }
        private void OnHousingDestroyed(BuildingComponent building)
        {
            ResourceManager.Instance.DecreaseMaxPopulation(providedMaxPopulation);
        }

        void OnBuildingUpgraded()
        {
            ResourceManager.Instance.DecreaseMaxPopulation(buildingComponent.Data.LevelDatas[buildingComponent.Level-2].PopulationCapacity);
        }
        public int ProvidedPopulation => providedMaxPopulation;
        public string GetInfoText()
        {
            return buildingComponent.Data.Description + "\n+" + buildingComponent.LevelData.PopulationCapacity + "人口上限";
        }
    }
}