using System;
using Core;
using Data.Buildings;
using UI.Tooltip;
using UnityEngine;
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

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClick);
            Initialize(buildingData);
        }
        
        public void Initialize(BuildingData data)
        {
            this.buildingData = data;
            var levelData = buildingData.LevelDatas[0];
            
            System.Text.StringBuilder infoBuilder = new System.Text.StringBuilder();
            // 标题
            infoBuilder.AppendLine($"<b><size=120%>{buildingData.BuildingName}</size></b>");
            // 描述
            infoBuilder.AppendLine($"<i>{buildingData.Description}</i>\n");
            // 属性
            infoBuilder.AppendLine($"建造成本: {data.BuildCost} 金币");
            infoBuilder.AppendLine($"生命值: {levelData.MaxHP}");
            
            if (buildingData.IsHousing)
                infoBuilder.AppendLine($"人口上限: +{levelData.PopulationCapacity}");
            
            if (buildingData.IsProduction)
                infoBuilder.AppendLine($"产出: {levelData.BaseProduction} {buildingData.ResourceType} / 30秒");
            
            if (buildingData.IsTower)
            {
                infoBuilder.AppendLine($"伤害: {levelData.MinDamage}-{levelData.MaxDamage}");
                infoBuilder.AppendLine($"射程: {levelData.AttackRange} 格");
                infoBuilder.AppendLine($"攻速: {levelData.AttackInterval} 秒/次");
            }

            staticTooltip = infoBuilder.ToString();
        }

        private void OnEnable()
        {
            EventManager.OnGoldChanged += OnGoldChanged;
            EventManager.OnDayStart += OnDayStart;
            EventManager.OnNightStart += OnNightStart;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.OnGoldChanged -= OnGoldChanged;
            EventManager.OnDayStart -= OnDayStart;
            EventManager.OnNightStart -= OnNightStart;
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
                tooltip = $"<color=red>只能在白天建造</color>";
            }
            else if (!isAffordable)
            {
                tooltip = $"<color=red>金币不足</color>";
            }
            
            return tooltip + staticTooltip;
        }
    }
}