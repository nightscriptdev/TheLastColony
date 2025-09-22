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
        [Header("研究数据")]
        public List<ResearchData> allResearches = new List<ResearchData>();
        
        private HashSet<ResearchData> completedResearches = new HashSet<ResearchData>();
        
        private HashSet<SkillType> unlockedSkills = new HashSet<SkillType>();
        private HashSet<BuildingKey> unlockedBuildings = new HashSet<BuildingKey>();
        
        // 检查是否可以研究
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
        
        // 执行研究
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
            // 触发研究完成事件
            EventManager.OnResearchComplete?.Invoke(research);
            
            return true;
        }
        
        // 检查是否已研究
        public bool IsResearched(ResearchData research)
        {
            return completedResearches.Contains(research);
        }
        
        // 检查建筑等级是否已解锁
        public bool IsBuildingLevelUnlocked(BuildingType buildingType, int level)
        {
            foreach (var research in completedResearches)
            {
                if (research.unlockBuildingType == buildingType && research.unlockBuildingLevel >= level)
                {
                    return true;
                }
            }
            return false;
        }
        
        // 获取建筑的最高解锁等级
        public int GetMaxUnlockedLevel(BuildingType buildingType)
        {
            int maxLevel = 0;
            foreach (var research in completedResearches)
            {
                if (research.unlockBuildingType == buildingType && research.unlockBuildingLevel > maxLevel)
                {
                    maxLevel = research.unlockBuildingLevel;
                }
            }
            return maxLevel;
        }
        
        public bool IsSkillUnlocked(SkillType skillType)
        {
            return unlockedSkills.Contains(skillType);

        }
        
        public bool IsBuildingUnlocked(BuildingKey buildingKey)
        {
            return unlockedBuildings.Contains(buildingKey);
        }
        
        // 检查是否有技能专精
        public bool HasSkillSpecialty(SkillSpecialtyType specialtyType)
        {
            foreach (var research in completedResearches)
            {
                if (research.unlockSpecialty == specialtyType)
                    return true;
            }
            return false;
        }
        
        // 获取可研究的项目列表
        public List<ResearchData> GetAvailableResearches()
        {
            List<ResearchData> available = new List<ResearchData>();
            foreach (var research in allResearches)
            {
                if (CanResearch(research))
                    available.Add(research);
            }
            return available;
        }
        
        // 获取已完成的研究列表
        public List<ResearchData> GetCompletedResearches()
        {
            return new List<ResearchData>(completedResearches);
        }
        
        bool AllSkillsResearched()
        {
            int skillCount = 0;
            foreach (var research in completedResearches)
            {
                if (research.type == ResearchType.Skill)
                    skillCount++;
            }
            return skillCount >= 3; // 三个技能
        }
        
        bool HasAnySpecialty()
        {
            foreach (var research in completedResearches)
            {
                if (research.type == ResearchType.SkillSpecialty)
                    return true;
            }
            return false;
        }
        
        void OnResearchCompleted(ResearchData research)
        {
            
            // 这里可以添加研究完成的音效、UI提示等
            // AudioManager.Instance.PlaySFX("research_complete");
            // UIManager.Instance.ShowResearchCompleteNotification(research);
        }
        
        // 保存/加载
        [System.Serializable]
        public class ResearchSaveData
        {
            public List<string> completedResearchNames = new List<string>();
        }
        
        public ResearchSaveData GetSaveData()
        {
            ResearchSaveData saveData = new ResearchSaveData();
            foreach (var research in completedResearches)
            {
                saveData.completedResearchNames.Add(research.name);
            }
            return saveData;
        }
        
        public void LoadSaveData(ResearchSaveData saveData)
        {
            completedResearches.Clear();
            foreach (var researchName in saveData.completedResearchNames)
            {
                var research = allResearches.Find(r => r.name == researchName);
                if (research != null)
                    completedResearches.Add(research);
            }
        }
        
        // 获取技能冷却时间修正
        public float GetSkillCooldownMultiplier()
        {
            if (HasSkillSpecialty(SkillSpecialtyType.CooldownReduction))
                return 0.75f; // 减少25%
            return 1.0f;
        }
    
        // 获取技能学识消耗修正
        public float GetSkillKnowledgeMultiplier()
        {
            if (HasSkillSpecialty(SkillSpecialtyType.KnowledgeCostReduction))
                return 0.75f; // 减少25%
            return 1.0f;
        }
    }
}