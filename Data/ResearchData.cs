using Enums;
using Managers;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ResearchData", menuName = "Game Data/Research Data")]
    public class ResearchData : ScriptableObject
    {
        [Header("基本信息")]
        public Sprite icon;
        public int knowledgeCost;
        public ResearchType type;
    
        [Header("解锁内容")]
        public BuildingType unlockBuildingType;
        public int unlockBuildingLevel = 1;
        public SkillType unlockSkill;
    
        [Header("前置条件")]
        public ResearchData prerequisite;
        
        public string GetLocalizedResearchName()
        {
            switch (type)
            {
                case ResearchType.Building:
                    return $"{LocalizationManager.Instance.GetGameText("tooltip.building.level", unlockBuildingLevel)}{LocalizationManager.Instance.GetLocalizedBuildingName(unlockBuildingType)}";
                case ResearchType.Skill:
                    return LocalizationManager.Instance.GetLocalizedSkillName(unlockSkill);
                default:
                    return "";
            }
        }
    }
}