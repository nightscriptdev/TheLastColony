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
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        
        [SerializeField] private BuildingData buildingData;
        public BuildingData BuildingData => buildingData;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            GetConstantTooltip();
            
            EventManager.OnGoldChanged += OnGoldChanged;
            EventManager.OnDayStart += OnDayStart;
            EventManager.OnNightStart += OnNightStart;
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        private void OnDestroy()
        {
            EventManager.OnGoldChanged -= OnGoldChanged;
            EventManager.OnDayStart -= OnDayStart;
            EventManager.OnNightStart -= OnNightStart;
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        
        
        public void GetConstantTooltip()
        {
            var levelData = buildingData.LevelDatas[0];
            var loc = LocalizationManager.Instance;
            
            System.Text.StringBuilder infoBuilder = new System.Text.StringBuilder();
            
            infoBuilder.AppendLine($"<b><size=120%>{loc.GetLocalizedBuildingName(buildingData.BuildingType)}</size></b>");
            
            // 描述
            infoBuilder.AppendLine($"<i>{loc.GetLocalizedBuildingDescription(buildingData.BuildingType)}</i>\n");
            
            // 属性
            infoBuilder.AppendLine(loc.GetGameText("building.cost", buildingData.BuildCost));
            infoBuilder.AppendLine(loc.GetGameText("building.health", levelData.MaxHP));

            constantTooltip = infoBuilder.ToString();
        }
        
        private void OnButtonClick()
        {
            EventManager.OnBuildingButtonClick?.Invoke(buildingData);
        }
        
        void OnSelectedLocaleChanged(Locale locale)
        {
            GetConstantTooltip();
        }
        
        private void OnGoldChanged()
        {
            if (buildingData == null) return;
            
            UpdateInteractableState();
            RefreshTooltipIfVisible();
        }
        
        private void OnDayStart(int day)
        {
            UpdateInteractableState();
            RefreshTooltipIfVisible();
        }

        private void OnNightStart(int day)
        {
            UpdateInteractableState();
            RefreshTooltipIfVisible();
        }
        
        private void UpdateInteractableState()
        {
            // 必须是白天并且金币足够才能交互
            bool isDayTime = TimeManager.Instance.IsDay;
            button.interactable = isDayTime && (ResourceManager.Instance.Gold >= buildingData.BuildCost);
        }
        
        public override string GetTooltip()
        {
            string tooltip = "";

            if (!TimeManager.Instance.IsDay)
            {
                tooltip += $"<color=red>{LocalizationManager.Instance.GetGameText("building.daytime_build_only")}</color>\n\n";
            }
            else if (ResourceManager.Instance.Gold < buildingData.BuildCost)
            {
                tooltip +=$"<color=red>{LocalizationManager.Instance.GetGameText("building.insufficient_gold")}</color>\n\n";
            }
            
            return tooltip + constantTooltip;
        }
    }
}