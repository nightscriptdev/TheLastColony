using UnityEngine;
using System.Collections.Generic;
using Enums;
using Managers;

namespace UI.Buildings
{
    public class BuildingConstructionUI : MonoBehaviour
    {
        [SerializeField] private Transform buildingButtonParent;

        private Dictionary<BuildingType, BuildingButtonUI> buildingButtonDictionary;
        
        private void Start()
        {
            InitializeBuildingButtons();
        }

        private void InitializeBuildingButtons()
        {
            if (BuildingManager.Instance == null) return;

            buildingButtonDictionary = new Dictionary<BuildingType, BuildingButtonUI>();
            foreach (Transform o in buildingButtonParent)
            {
                var button = o.GetComponent<BuildingButtonUI>();
                buildingButtonDictionary.TryAdd(button.BuildingData.BuildingType, button);
            }
        }
    }
}