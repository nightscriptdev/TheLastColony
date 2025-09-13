using Core;
using UnityEngine;
namespace Components.Buildings
{
    public class HousingComponent : MonoBehaviour
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
            }
        }
        private void OnDisable()
        {
            if (buildingComponent != null)
            {
                buildingComponent.OnBuildingCompleted -= OnHousingBuilt;
                buildingComponent.OnBuildingDestroyed -= OnHousingDestroyed;
            }
        }
        private void OnHousingBuilt(BuildingComponent building)
        {
            providedMaxPopulation = building.Data.LevelDatas[building.LevelIndex].PopulationCapacity;
            ResourceManager.Instance.IncreaseMaxPopulation(providedMaxPopulation);
            Debug.Log($"房屋建成，增加 {providedMaxPopulation} 最大人口");
        }
        private void OnHousingDestroyed(BuildingComponent building)
        {
            ResourceManager.Instance.DecreaseMaxPopulation(providedMaxPopulation);
            Debug.Log($"房屋被摧毁，减小 {providedMaxPopulation} 人口");
        }
        public int ProvidedPopulation => providedMaxPopulation;
    }
}