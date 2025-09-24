using System.Text;
using UnityEngine;
using Core;
using Enums;
using Managers;

namespace UI.Tooltip
{
    public class ResourceTooltipTrigger : TooltipTrigger
    {
        [SerializeField] private ResourceType resourceType;

        public override string GetTooltip()
        {
            switch (resourceType)
            {
                case ResourceType.Population:
                    return GeneratePopulationTooltip();
                case ResourceType.Food:
                    return GenerateFoodTooltip();
                case ResourceType.Gold:
                    return GenerateGoldTooltip();
                case ResourceType.Knowledge:
                    return GenerateKnowledgeTooltip();
                default:
                    return "";
            }
        }

        private string GeneratePopulationTooltip()
        {
            var rm = ResourceManager.Instance;
            var loc = LocalizationManager.Instance;

            if (rm == null || loc == null) return "";

            var tooltip = new StringBuilder();

            // 基本信息
            tooltip.AppendLine($"<b>{loc.GetGameText("resource.population")}</b>");
            tooltip.AppendLine(loc.GetGameText("tooltip.population.current", rm.Population.ToString("N0")));
            tooltip.AppendLine(loc.GetGameText("tooltip.population.max", rm.MaxPopulation.ToString("N0")));
            tooltip.AppendLine();

            // 人口影响
            tooltip.AppendLine($"{loc.GetGameText("tooltip.population.effects")}:");
            float populationBonus = rm.Population * 0.02f * 100f;
            tooltip.AppendLine(loc.GetGameText("tooltip.population.production_bonus", populationBonus.ToString("F1")));
            tooltip.AppendLine();

            // 每日消耗
            tooltip.AppendLine($"{loc.GetGameText("tooltip.population.daily_consumption")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.population.food_per_person"));
            tooltip.AppendLine(loc.GetGameText("tooltip.population.current_consumption", rm.Population));
            tooltip.AppendLine(loc.GetGameText("tooltip.population.food_shortage_warning"));
            tooltip.AppendLine();

            // 附加说明
            tooltip.AppendLine($"{loc.GetGameText("tooltip.tips")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.population.growth_condition"));
            tooltip.Append($"<color=red>{loc.GetGameText("tooltip.population.game_over_warning")}</color>");

            return tooltip.ToString();
        }

        private string GenerateFoodTooltip()
        {
            var rm = ResourceManager.Instance;
            var loc = LocalizationManager.Instance;

            if (rm == null || loc == null) return "";

            var tooltip = new StringBuilder();

            tooltip.AppendLine($"<b>{loc.GetGameText("resource.food")}</b>");
            tooltip.AppendLine(loc.GetGameText("tooltip.food.current", rm.Food));
            tooltip.AppendLine();

            tooltip.AppendLine($"{loc.GetGameText("tooltip.usage")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.food.maintain_population"));
            tooltip.AppendLine(loc.GetGameText("tooltip.food.consumption_rate"));
            tooltip.AppendLine(loc.GetGameText("tooltip.food.today_need", rm.Population));
            tooltip.AppendLine();

            tooltip.AppendLine($"{loc.GetGameText("tooltip.sources")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.food.from_farms"));
            tooltip.AppendLine(loc.GetGameText("tooltip.common.daytime_only"));

            if (rm.Food < rm.Population)
            {
                int shortage = rm.Population - rm.Food;
                tooltip.AppendLine();
                tooltip.Append($"<color=red>{loc.GetGameText("tooltip.food.shortage_warning", shortage)}</color>");
            }

            return tooltip.ToString();
        }

        private string GenerateGoldTooltip()
        {
            var rm = ResourceManager.Instance;
            var loc = LocalizationManager.Instance;

            if (rm == null || loc == null) return "";

            var tooltip = new StringBuilder();

            tooltip.AppendLine($"<b>{loc.GetGameText("resource.gold")}</b>");
            tooltip.AppendLine(loc.GetGameText("tooltip.gold.current", rm.Gold));
            tooltip.AppendLine();

            tooltip.AppendLine($"{loc.GetGameText("tooltip.usage")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.gold.build_buildings"));
            tooltip.AppendLine(loc.GetGameText("tooltip.gold.upgrade_buildings"));
            tooltip.AppendLine();

            tooltip.AppendLine($"{loc.GetGameText("tooltip.sources")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.gold.from_mines"));
            tooltip.Append(loc.GetGameText("tooltip.common.daytime_only"));

            return tooltip.ToString();
        }

        private string GenerateKnowledgeTooltip()
        {
            var rm = ResourceManager.Instance;
            var loc = LocalizationManager.Instance;

            if (rm == null || loc == null) return "";

            var tooltip = new StringBuilder();

            tooltip.AppendLine($"<b>{loc.GetGameText("resource.knowledge")}</b>");
            tooltip.AppendLine(loc.GetGameText("tooltip.knowledge.current", rm.Knowledge));
            tooltip.AppendLine();

            tooltip.AppendLine($"{loc.GetGameText("tooltip.usage")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.knowledge.research_buildings"));
            tooltip.AppendLine(loc.GetGameText("tooltip.knowledge.research_upgrades"));
            tooltip.AppendLine(loc.GetGameText("tooltip.knowledge.research_skills"));
            tooltip.AppendLine(loc.GetGameText("tooltip.knowledge.cast_skills"));
            tooltip.AppendLine();

            tooltip.AppendLine($"{loc.GetGameText("tooltip.sources")}:");
            tooltip.AppendLine(loc.GetGameText("tooltip.knowledge.from_labs"));
            tooltip.Append(loc.GetGameText("tooltip.common.daytime_only"));

            return tooltip.ToString();
        }
    }
}