using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Buildings;
using Core;
using Enums;
using Managers;
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
        [SerializeField] private Image healthBar;
        [Header("操作按钮")]
        [SerializeField] private TextMeshProUGUI buildingLevelText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button demolishButton;
        private BuildingComponent currentBuilding;
        //private TowerComponent currentTower;
        
        private void Start()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (demolishButton != null)
                demolishButton.onClick.AddListener(OnDemolishClicked);
            // 初始时隐藏面板
            SetPanelVisible(false);
        }
        
        private void Update()
        {
            // 实时更新选中建筑的信息
            if (currentBuilding != null && panelRoot.activeSelf)
            {
                UpdateBuildingInfo();
            }
        }
        /// <summary>
        /// 显示建筑信息
        /// </summary>
        public void ShowBuildingInfo(BuildingComponent building)
        {
            if (building == null) return;
            currentBuilding = building;
            SetPanelVisible(true);
            UpdateBuildingInfo();
        }
        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel()
        {
            currentBuilding = null;
            SetPanelVisible(false);
        }
        /// <summary>
        /// 更新建筑信息显示
        /// </summary>
        private void UpdateBuildingInfo()
        {
            var data = currentBuilding.Data;
            var hpComponent = currentBuilding.GetComponent<Components.HealthComponent>();
            buildingNameText.text = data.BuildingName;
            healthText.text = $"{hpComponent.CurrentHP}/{hpComponent.MaxHP}";
            healthBar.fillAmount = hpComponent.HealthPercentage;

            switch (data.BuildingType)
            {
                case BuildingType.House:
                    buildingInfoText.text = $"+{data.LevelDatas[currentBuilding.LevelIndex].PopulationCapacity} 人口上限";
                    break;
                case BuildingType.Farm:
                case BuildingType.Mine:
                case BuildingType.ResearchLab:
                    buildingInfoText.text = $"产出{GetResourceName(data.ResourceType)}\n产量: {ResourceManager.Instance.CalculateFinalProduction(data.LevelDatas[currentBuilding.LevelIndex].BaseProduction)} / {data.ProductionInterval:F0}秒";
                    break;
                case BuildingType.PurpleCrystalTower:
                    break;
                case BuildingType.BlueCrystalTower:
                    break;
                case BuildingType.WhiteCrystalTower:
                    break;
                case BuildingType.DefenseCrystal:
                    break;
            }
            UpdateTowerInfo(data);
            // 按钮状态
            UpdateButtonStates();
        }
        /// <summary>
        /// 更新魔法塔信息
        /// </summary>
        private void UpdateTowerInfo(Data.Buildings.BuildingData data)
        {
            /*bool isTower = data.IsTower;

            if (towerInfoGroup != null)
                towerInfoGroup.SetActive(isTower);
            if (isTower)
            {
                if (damageText != null)
                    damageText.text = $"伤害: {data.MinDamage}-{data.MaxDamage}";
                if (attackRangeText != null)
                    attackRangeText.text = $"射程: {data.AttackRange:F1}";
                if (attackSpeedText != null)
                    attackSpeedText.text = $"攻速: {data.AttackInterval:F1}秒";
                if (specialEffectsText != null)
                {
                    string effects = GetTowerSpecialEffects(data);
                    specialEffectsText.text = effects;
                    specialEffectsText.gameObject.SetActive(!string.IsNullOrEmpty(effects));
                }
            }*/
        }
        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            buildingLevelText.text = "等级: " + currentBuilding.LevelIndex+1;
            
            // 升级按钮
            if (upgradeButton != null)
            {
                bool canUpgrade = currentBuilding.CanPerformUpgrade();
                bool hasEnoughGold = ResourceManager.Instance.HasEnoughGold(currentBuilding.Data.LevelDatas[currentBuilding.LevelIndex]. UpgradeCost);

                upgradeButton.interactable = canUpgrade && hasEnoughGold;
            }
        }
        /// <summary>
        /// 升级按钮回调
        /// </summary>
        private void OnUpgradeClicked()
        {
            BuildingManager.Instance.UpgradeBuilding(currentBuilding);
        }
        /// <summary>
        /// 拆除按钮回调
        /// </summary>
        private void OnDemolishClicked()
        {
            BuildingManager.Instance.DemolishBuilding(currentBuilding);
            ClosePanel();
        }
        private void SetPanelVisible(bool visible)
        {
            panelRoot.SetActive(visible);
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
        /// 获取魔法塔特殊效果描述
        /// </summary>
        private string GetTowerSpecialEffects(Data.Buildings.BuildingData data)
        {
            var effects = new System.Collections.Generic.List<string>();
            /*if (data.HasSlowEffect)
                effects.Add("减速");
            if (data.PierceCount > 1)
                effects.Add($"穿透{data.PierceCount}个敌人");
            if (data.MultiShotCount > 1)
                effects.Add($"发射{data.MultiShotCount}个子弹");
            if (data.ConsecutiveAttacks > 1)
                effects.Add($"连续攻击{data.ConsecutiveAttacks}次");
            if (data.ReflectDamage)
                effects.Add($"反射{data.ReflectPercent * 100}%伤害");*/
            return effects.Count > 0 ? string.Join(", ", effects) : "";
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