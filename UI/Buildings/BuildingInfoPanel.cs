using Components;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Buildings;
using Core;
using Data;
using Data.Buildings;
using Managers;
using UI.Tooltip;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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
        
        private void Start()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (demolishButton != null)
                demolishButton.onClick.AddListener(OnDemolishClicked);
            
            Hide();
        }


        public void Show(BuildingComponent building)
        {
            EventManager.OnGoldChanged += OnGoldChanged;
            EventManager.OnResearchComplete += OnResearchComplete;
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

            
            currentBuilding = building;
            currentBuilding.OnBuildingCompleted += OnBuildingCompleted;
            currentHealthComponent = currentBuilding.GetComponent<HealthComponent>();
            healthBar.InitializeHealthBar(currentHealthComponent.CurrentHP, currentHealthComponent.MaxHP);
            healthText.text = $"{currentHealthComponent.CurrentHP}/{currentHealthComponent.MaxHP}";
            currentHealthComponent.OnDeath += Hide;
            currentHealthComponent.OnHealthChanged += UpdateHealBar;
            currentHealthComponent.OnHealthFull += RefreshPanel;

            RefreshPanel();

            panelRoot.SetActive(true);
        }
        public void OnDisable()
        {
            EventManager.OnGoldChanged -= OnGoldChanged;
            EventManager.OnResearchComplete -= OnResearchComplete;
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;

            if (currentBuilding)
            {
                currentBuilding.OnBuildingCompleted -= OnBuildingCompleted;
                currentBuilding = null;
            }
            
            if (currentHealthComponent)
            {
                currentHealthComponent.OnDeath -= Hide;
                currentHealthComponent.OnHealthChanged -= UpdateHealBar;
                currentHealthComponent.OnHealthFull -= UpdateButtonStates;
                currentHealthComponent = null;
            }

            healthBar.StopAllCoroutines();
        }

        void OnBuildingCompleted(BuildingComponent building)
        {
            RefreshPanel();
        }
        
        void OnSelectedLocaleChanged(Locale locale)
        {
            UpdateBuildingInfo();
        }
        
        public void Hide()
        {
            panelRoot.SetActive(false);
        }
        
        private void UpdateHealBar(int currentHealth, int maxHealth)
        {
            if (currentHealthComponent)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
                healthBar.UpdateHealthBar(currentHealth, maxHealth);
            }
        }

        private void UpdateBuildingInfo()
        {
            buildingNameText.text = LocalizationManager.Instance.GetLocalizedBuildingName(currentBuilding.Data.BuildingType);
            buildingInfoText.text = currentBuilding.GetInfoText();
        }

        private void UpdateButtonStates()
        {
            var loc = LocalizationManager.Instance;
            
            buildingLevelText.text = currentBuilding.Level.ToString();
            
            if (currentBuilding.HasNextLevel)
            {
                upgradeButton.gameObject.SetActive(true);

                if (!ResearchManager.Instance.IsBuildingUnlocked(new BuildingKey(currentBuilding.Data.BuildingType, currentBuilding.Level+1)))
                {
                    upgradeTooltipTrigger.constantTooltip = $"<color=red>{loc.GetGameText("tooltip.research_required")} [{loc.GetGameText("tooltip.building.level", currentBuilding.Level+1)}{loc.GetLocalizedBuildingName(currentBuilding.Data.BuildingType)}]</color>";
                    upgradeButton.interactable = false;
                    return;
                }
                if (!currentBuilding.IsUpgradeable)
                {
                    upgradeTooltipTrigger.constantTooltip = $"<color=red>{loc.GetGameText("building.require_full_health")}</color>";
                    upgradeButton.interactable = false;
                    return;
                }
                if (ResourceManager.Instance.Gold < currentBuilding.LevelData.UpgradeCost)
                {
                    upgradeTooltipTrigger.constantTooltip = $"<color=red>{loc.GetGameText("building.insufficient_gold")}</color>";
                    upgradeButton.interactable = false;
                    return;
                }

                upgradeTooltipTrigger.constantTooltip = "";
                upgradeButton.interactable = true;
                upgradeTooltipTrigger.Hide();
            }
            else
            {
                upgradeTooltipTrigger.Hide();
                upgradeButton.gameObject.SetActive(false);
            }
        }

        void RefreshPanel()
        {
            UpdateBuildingInfo();
            UpdateButtonStates();
        }
        
        void OnGoldChanged()
        {
            RefreshPanel();
        }

        void OnResearchComplete(ResearchData researchData)
        {
            RefreshPanel();
        }
        
        private void OnUpgradeClicked()
        {
            BuildingManager.Instance.UpgradeBuilding(currentBuilding);
        }

        private void OnDemolishClicked()
        {
            BuildingManager.Instance.DemolishBuilding(currentBuilding);
            Hide();
        }
    }
}