using System;
using Components;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Buildings;
using Core;
using Data;
using Data.Buildings;
using Enums;
using Managers;
using UI.Tooltip;

namespace UI.Buildings
{
    /// <summary>
    /// 显示选中建筑的详细信息和操作按钮
    /// </summary>
    public class BuildingInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [Header("建筑信息显示")]
        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI buildingInfoText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private HealthBarUI healthBar;
        
        [Header("操作按钮")]
        [SerializeField] private TextMeshProUGUI buildingLevelText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TooltipTrigger upgradeTooltipTrigger;
        [SerializeField] private Button demolishButton;
        private BuildingComponent currentBuilding;
        private HealthComponent currentHealthComponent;
        
        //private TowerComponent currentTower;
        
        private void Start()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (demolishButton != null)
                demolishButton.onClick.AddListener(OnDemolishClicked);
            
            Hide();
        }

        private void Update()
        {
            UpdateBuildingInfo();
        }

        public void Show(BuildingComponent building)
        {
            EventManager.OnGoldChanged += OnGoldChanged;
            EventManager.OnResearchComplete += OnResearchComplete;
            
            
            currentBuilding = building;
            currentHealthComponent = currentBuilding.GetComponent<HealthComponent>();
            healthBar.InitializeHealthBar(currentHealthComponent.CurrentHP, currentHealthComponent.MaxHP);
            healthText.text = $"{currentHealthComponent.CurrentHP}/{currentHealthComponent.MaxHP}";
            currentHealthComponent.OnDeath += Hide;
            currentHealthComponent.OnHealthChanged += UpdateHealBar;
            buildingNameText.text = currentBuilding.Data.BuildingName;

            UpdateBuildingInfo();
            UpdateButtonStates();
            panelRoot.SetActive(true);
        }
        public void Hide()
        {
            EventManager.OnGoldChanged -= OnGoldChanged;
            EventManager.OnResearchComplete -= OnResearchComplete;

            
            currentBuilding = null;
            if (currentHealthComponent)
            {
                currentHealthComponent.OnDeath -= Hide;
                currentHealthComponent.OnHealthChanged -= UpdateHealBar;
                currentHealthComponent = null;
            }

            healthBar.StopAllCoroutines();
            panelRoot.SetActive(false);
        }

        private void UpdateHealBar(int currentHealth, int maxHealth)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        private void UpdateBuildingInfo()
        {
            buildingInfoText.text = currentBuilding.GetInfoText();
        }

        private void UpdateButtonStates()
        {
            buildingLevelText.text = "等级\n" + (currentBuilding.Level);
            
            if (currentBuilding.HasNextLevel)
            {
                upgradeButton.gameObject.SetActive(true);

                if (!ResearchManager.Instance.IsBuildingUnlocked(new BuildingKey(currentBuilding.Data.BuildingType, currentBuilding.Level+1)))
                {
                    upgradeTooltipTrigger.customTooltip = "<color=red>尚未研究</color>";
                    upgradeButton.interactable = false;
                    return;
                }
                if (!currentBuilding.IsUpgradeable)
                {
                    upgradeTooltipTrigger.customTooltip = "<color=red>需要满血</color>";
                    upgradeButton.interactable = false;
                    return;
                }
                if (ResourceManager.Instance.Gold < currentBuilding.LevelData.UpgradeCost)
                {
                    upgradeTooltipTrigger.customTooltip = "<color=red>金币不足</color>";
                    upgradeButton.interactable = false;
                    return;
                }

                upgradeTooltipTrigger.customTooltip = "";
                upgradeButton.interactable = true;
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
            }
        }

        void OnGoldChanged(int currentGold)
        {
            UpdateButtonStates();
        }

        void OnResearchComplete(ResearchData researchData)
        {
            UpdateButtonStates();
        }
        
        /// <summary>
        /// 升级按钮回调
        /// </summary>
        private void OnUpgradeClicked()
        {
            BuildingManager.Instance.UpgradeBuilding(currentBuilding);
            UpdateButtonStates();
            upgradeTooltipTrigger.RefreshTooltip();
            currentHealthComponent = null;
        }
        /// <summary>
        /// 拆除按钮回调
        /// </summary>
        private void OnDemolishClicked()
        {
            BuildingManager.Instance.DemolishBuilding(currentBuilding);
            Hide();
        }
        
        private string GetResourceName(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Food => "食物",
                ResourceType.Gold => "金币",
                ResourceType.Knowledge => "学识",
                _ => "未知"
            };
        }
        /// <summary>
        /// 外部调用，用于处理建筑点击
        /// </summary>
        public bool IsShowingBuilding(BuildingComponent building)
        {
            return currentBuilding == building && panelRoot.activeSelf;
        }
    }
}