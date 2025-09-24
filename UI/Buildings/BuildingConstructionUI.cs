using UnityEngine;
using System.Collections.Generic;
using Core;
using Data;
using Data.Buildings;
using Managers;

namespace UI.Buildings
{
    public class BuildingConstructionUI : MonoBehaviour
    {
        [SerializeField] private Transform buildingButtonParent;

        private Dictionary<BuildingKey, BuildingButtonUI> buildingButtonDictionary;

        private void OnEnable()
        {
            EventManager.OnResearchComplete += OnResearchComplete;
        }
        
        private void OnDisable()
        {
            EventManager.OnResearchComplete -= OnResearchComplete;
        }

        private void Start()
        {
            InitializeBuildingButtons();
        }


        void OnResearchComplete(ResearchData researchData)
        {
            var key = new BuildingKey(researchData.unlockBuildingType, researchData.unlockBuildingLevel);
            if (buildingButtonDictionary.ContainsKey(key))
            {
                buildingButtonDictionary[key].gameObject.SetActive(true);
            }
        }
        
        private void InitializeBuildingButtons()
        {
            if (BuildingManager.Instance == null) return;

            buildingButtonDictionary = new Dictionary<BuildingKey, BuildingButtonUI>();
            foreach (Transform o in buildingButtonParent)
            {
                var button = o.GetComponent<BuildingButtonUI>();
                buildingButtonDictionary.TryAdd(new BuildingKey(button.BuildingData.BuildingType, 1), button);
            }
        }
    }
}