using System.Text;
using UnityEngine;
using Enums;
using Managers;

namespace Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Game Data/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("基本信息")]
        //public string skillName;
        //public string description;
        public Sprite icon;
        public SkillType skillType;
        
        [Header("技能属性")]
        public int baseDamage;              // 基础伤害
        public int knowledgeCost;           // 学识消耗
        public float cooldownTime;          // 冷却时间
        public float effectRadius;          // 技能效果半径
        public float duration;              // 持续时间（对虚空漩涡有效）
        
        [Header("视觉效果")]
        public GameObject effectPrefab;     // 特效预制体
        public AudioClip castSound;         // 施放音效
        
        [Header("快捷键")]
        public KeyCode hotkey;              // 快捷键
        
        public bool IsInstantDamage => skillType != SkillType.VoidVortex;
        public bool IsAOE => skillType != SkillType.Lightning;

        public string GetLocalizedDescription()
        {
            return LocalizationManager.Instance.GetGameText("skill." + skillType + ".desc");
        }
        
        public string GetLocalizedName()
        {
            return LocalizationManager.Instance.GetGameText("skill."+skillType);
        }
        
        public string GetTooltip()
        {
            StringBuilder sb = new StringBuilder();
            var loc = LocalizationManager.Instance;

            sb.AppendLine($"<b>{GetLocalizedName()}</b>");
            
            sb.AppendLine(GetLocalizedDescription());

            sb.AppendLine();

            sb.AppendLine(loc.GetGameText("tooltip.skill.knowledge", knowledgeCost));

            sb.AppendLine(loc.GetGameText("tooltip.skill.cooldown", cooldownTime));

            if (IsInstantDamage)
                sb.AppendLine(loc.GetGameText("combat.damage") + baseDamage);

            if (IsAOE)
                sb.AppendLine(loc.GetGameText("tooltip.skill.area", effectRadius*2));

            if (skillType == SkillType.VoidVortex)
                sb.AppendLine(loc.GetGameText("tooltip.skill.duration", duration));

            sb.AppendLine(loc.GetGameText("tooltip.skill.hotkey", hotkey));

            return sb.ToString();
        }
    }
}