using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using Core;
using Enums;

namespace UI.Tooltip
{
    /// <summary>
    /// 资源工具提示触发器 - 专门用于资源UI的工具提示
    /// 会根据资源类型动态生成相应的说明信息
    /// </summary>
    public class ResourceTooltipTrigger : TooltipTrigger
    {
        [Header("资源设置")]
        [SerializeField] private ResourceType resourceType;
        
        /// <summary>
        /// 根据资源类型生成工具提示内容
        /// </summary>
        public override string GetTooltip()
        {
            string content = "";
            
            switch (resourceType)
            {
                case ResourceType.Population:
                    content = GeneratePopulationTooltip();
                    break;
                case ResourceType.Food:
                    content = GenerateFoodTooltip();
                    break;
                case ResourceType.Gold:
                    content = GenerateGoldTooltip();
                    break;
                case ResourceType.Knowledge:
                    content = GenerateKnowledgeTooltip();
                    break;
            }
            return content;
        }
        
        /// <summary>
        /// 生成人口工具提示
        /// </summary>
        private string GeneratePopulationTooltip()
        {
            var rm = ResourceManager.Instance;
            var tooltip = new StringBuilder();
            // 基本信息
            tooltip.AppendLine("<b>人口</b>");
            tooltip.AppendLine($"当前人口: {rm.Population:N0}");
            tooltip.AppendLine($"人口上限: {rm.MaxPopulation:N0}");
            tooltip.AppendLine();
            // 人口影响
            tooltip.AppendLine("<color=lightblue>人口影响:</color>");
            float populationBonus = rm.Population * 0.02f * 100f;
            tooltip.AppendLine($"• 资源产出加成: +{populationBonus:F1}%");
            tooltip.AppendLine();
            // 每日消耗
            tooltip.AppendLine("<color=lightblue>每日消耗:</color>");
            tooltip.AppendLine("• 每人每天消耗 1 食物");
            tooltip.AppendLine($"• 当前食物消耗: {rm.Population}");
            tooltip.AppendLine("• 食物不足时人口会减少");
            tooltip.AppendLine();
            // 附加说明
            tooltip.AppendLine("<color=orange>提示:</color>");
            tooltip.AppendLine("• 人口在未达上限且食物充足时增长");
            tooltip.Append("• <color=red>人口归0时游戏结束</color>");
    
            return tooltip.ToString();
        }
        
        /// <summary>
        /// 生成食物工具提示
        /// </summary>
        private string GenerateFoodTooltip()
        {
            var rm = ResourceManager.Instance;
            string tooltip = $"<b>食物</b>\n";
            tooltip += $"当前食物: {rm.Food}\n\n";
            tooltip += $"<color=yellow>用途:</color>\n";
            tooltip += $"• 维持人口生存\n";
            tooltip += $"• 每人每天消耗 1 单位\n";
            tooltip += $"• 今日需要: {rm.Population} 食物\n\n";
            tooltip += $"<color=green>获取方式:</color>\n";
            tooltip += $"• 建造农田生产\n";
            tooltip += $"• 只有白天会产出\n";
            
            if (rm.Food < rm.Population)
            {
                tooltip += $"\n<color=red>警告: 食物不足！明天将有 {rm.Population - rm.Food} 人死亡</color>";
            }
            
            return tooltip;
        }
        
        /// <summary>
        /// 生成金币工具提示
        /// </summary>
        private string GenerateGoldTooltip()
        {
            var rm = ResourceManager.Instance;
            string tooltip = $"<b>金币</b>\n";
            tooltip += $"当前金币: {rm.Gold}\n\n";
            tooltip += $"<color=yellow>用途:</color>\n";
            tooltip += $"• 建造建筑\n";
            tooltip += $"• 升级建筑\n\n";
            tooltip += $"<color=green>获取方式:</color>\n";
            tooltip += $"• 建造矿场生产\n";
            tooltip += $"• 只有白天会产出";
            
            return tooltip;
        }
        
        /// <summary>
        /// 生成学识工具提示
        /// </summary>
        private string GenerateKnowledgeTooltip()
        {
            var rm = ResourceManager.Instance;
            string tooltip = $"<b>学识</b>\n";
            tooltip += $"当前学识: {rm.Knowledge}\n\n";
            tooltip += $"<color=yellow>用途:</color>\n";
            tooltip += $"• 研究新建筑\n";
            tooltip += $"• 研究建筑升级\n";
            tooltip += $"• 研究技能\n";
            tooltip += $"• 释放技能\n\n";
            tooltip += $"<color=green>获取方式:</color>\n";
            tooltip += $"• 建造研究所生产\n";
            tooltip += $"• 只有白天会产出";
            
            return tooltip;
        }
        
        /// <summary>
        /// 设置资源类型
        /// </summary>
        public void SetResourceType(ResourceType type)
        {
            resourceType = type;
        }
        
        /// <summary>
        /// 启用/禁用工具提示
        /// </summary>
        public void SetEnabled(bool isEnabled)
        {
            enabled = isEnabled;
            
            if (!enabled && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }
        
        void OnDestroy()
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }
        
        void OnDisable()
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }
    }
}