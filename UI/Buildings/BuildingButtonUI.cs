using System;
using Core;
using Data.Buildings;
using Managers;
using UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI.Buildings
{
    public class BuildingButtonUI : TooltipTrigger
    {
        [Header("UI组件")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        
        [SerializeField] private BuildingData buildingData;
        public BuildingData BuildingData => buildingData;
        private bool isAffordable;
        private string tooltip;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            UpdateTooltip(null);
            LocalizationSettings.SelectedLocaleChanged += UpdateTooltip;
            
            EventManager.OnGoldChanged += OnGoldChanged;
            EventManager.OnDayStart += OnDayStart;
            EventManager.OnNightStart += OnNightStart;
        }

        private void OnDestroy()
        {
            EventManager.OnGoldChanged -= OnGoldChanged;
            EventManager.OnDayStart -= OnDayStart;
            EventManager.OnNightStart -= OnNightStart;
            LocalizationSettings.SelectedLocaleChanged -= UpdateTooltip;
        }

        public void UpdateTooltip(Locale locale)
        {
            var levelData = buildingData.LevelDatas[0];
            var loc = LocalizationManager.Instance;
            
            System.Text.StringBuilder infoBuilder = new System.Text.StringBuilder();
            
            infoBuilder.AppendLine($"<b><size=120%>{loc.GetLocalizedBuildingName(buildingData.BuildingType)}</size></b>");
            
            // 描述 - 根据建筑类型获取本地化描述
            infoBuilder.AppendLine($"<i>{loc.GetLocalizedBuildingDescription(buildingData.BuildingType)}</i>\n");
            
            // 属性
            infoBuilder.AppendLine(loc.GetGameText("building.cost", buildingData.BuildCost));
            infoBuilder.AppendLine(loc.GetGameText("building.health", levelData.MaxHP));

            tooltip = infoBuilder.ToString();
        }
        
        private void OnButtonClick()
        {
            EventManager.OnBuildingButtonClick?.Invoke(buildingData.BuildingType);
        }
        
        private void OnGoldChanged(int newAmount)
        {
            if (buildingData == null) return;
            
            isAffordable = newAmount >= buildingData.BuildCost;
            UpdateInteractableState();
        }
        
        private void OnDayStart(int day)
        {
            UpdateInteractableState();
        }

        private void OnNightStart(int day)
        {
            UpdateInteractableState();
        }
        
        private void UpdateInteractableState()
        {
            // 必须是白天并且金币足够才能交互
            bool isDayTime = TimeManager.Instance.IsDay;
            button.interactable = isDayTime && isAffordable;
        }
        
        public override string GetTooltip()
        {
            string tooltip = String.Empty;
            if (!TimeManager.Instance.IsDay)
            {
                tooltip = $"<color=red>{LocalizationManager.Instance.GetGameText("building.daytime_build_only")}\n</color>";
            }
            else if (!isAffordable)
            {
                tooltip = $"<color=red>{LocalizationManager.Instance.GetGameText("building.insufficient_gold")}\n</color>";
            }
            
            return tooltip + this.tooltip;
        }
    }
}