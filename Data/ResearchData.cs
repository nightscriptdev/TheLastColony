using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ResearchData", menuName = "Game Data/Research Data")]
    public class ResearchData : ScriptableObject
    {
        [Header("基本信息")]
        public string researchName;
        public string description;
        public Sprite icon;
        public int knowledgeCost;
        public ResearchType type;
    
        [Header("解锁内容")]
        public BuildingType unlockBuildingType;
        public int unlockBuildingLevel = 1; // 解锁建筑的等级（1,2,3）
        public SkillType unlockSkill;
        public SkillSpecialtyType unlockSpecialty;
    
        [Header("前置条件")]
        public ResearchData prerequisite;
    }
}