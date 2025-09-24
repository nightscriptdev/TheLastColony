using System.Collections.Generic;
using Core;
using Data;
using Data.Buildings;
using Enums;
using UnityEngine;

namespace Managers
{
    public class ResearchManager : MonoSingleton<ResearchManager>
    {
        public List<ResearchData> allResearches = new List<ResearchData>();
        
        private HashSet<ResearchData> completedResearches = new HashSet<ResearchData>();
        
        private HashSet<SkillType> unlockedSkills = new HashSet<SkillType>();
        private HashSet<BuildingKey> unlockedBuildings = new HashSet<BuildingKey>();
        
        public bool CanResearch(ResearchData research)
        {
            // 已经研究过了
            if (IsResearched(research))
                return false;
                
            // 学识不够
            if (ResourceManager.Instance.Knowledge < research.knowledgeCost)
                return false;
                
            // 检查前置条件
            if (research.prerequisite!=null && !IsResearched(research.prerequisite))
                return false;
            
            return true;
        }
        
        public bool DoResearch(ResearchData research)
        {
            if (!CanResearch(research))
                return false;
                
            // 消耗学识
            ResourceManager.Instance.SpendKnowledge(research.knowledgeCost);
            
            // 标记为已完成
            completedResearches.Add(research);

            switch (research.type)
            {
                case ResearchType.Building:
                    unlockedBuildings.Add(new BuildingKey(research.unlockBuildingType, research.unlockBuildingLevel));
                    break;
                case ResearchType.Skill:
                    unlockedSkills.Add(research.unlockSkill);
                    break;
            }
            EventManager.OnResearchComplete?.Invoke(research);
            
            return true;
        }
        
        public bool IsResearched(ResearchData research)
        {
            return completedResearches.Contains(research);
        }
        
        public bool IsSkillUnlocked(SkillType skillType)
        {
            return unlockedSkills.Contains(skillType);

        }
        
        public bool IsBuildingUnlocked(BuildingKey buildingKey)
        {
            return unlockedBuildings.Contains(buildingKey);
        }
    }
}