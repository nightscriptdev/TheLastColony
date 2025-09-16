using System.Text;
using UnityEngine;
using Enums;

namespace Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Game Data/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("基本信息")]
        public string skillName;
        public string description;
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

        public string GetTooltip()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<b>{skillName}</b>");
    
            if (!string.IsNullOrEmpty(description))
                sb.AppendLine(description);

            sb.AppendLine();

            sb.AppendLine($"消耗学识: {knowledgeCost}");

            sb.AppendLine($"冷却时间: {cooldownTime:0.0} 秒");

            if (IsInstantDamage)
                sb.AppendLine($"伤害: {baseDamage}");

            if (IsAOE)
                sb.AppendLine($"范围: {effectRadius:0.0}");

            if (skillType == SkillType.VoidVortex)
                sb.AppendLine($"持续时间: {duration:0.0} 秒");

            sb.AppendLine($"快捷键: {hotkey}");

            return sb.ToString();
        }

    }
}