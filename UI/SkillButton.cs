using System;
using Core;
using Data;
using Enums;
using Managers;
using TMPro;
using UI.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SkillButton : TooltipTrigger, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;
        
        [SerializeField] private SkillData skillData;

        public void Update()
        {
            if (ResearchManager.Instance.IsSkillUnlocked(skillData.skillType))
            {
                bool isOnCooldown = SkillManager.Instance.IsSkillOnCooldown(skillData.skillType);
                bool hasEnoughKnowledge = HasEnoughKnowledge();
                bool canUse = !isOnCooldown && hasEnoughKnowledge;
            
                // 更新冷却显示
                UpdateCooldownDisplay();
            
                // 更新按钮颜色（可选）
                UpdateButtonColor(canUse, hasEnoughKnowledge);
            }
        }

        private void UpdateCooldownDisplay()
        {
            float remainingCooldown = SkillManager.Instance.GetSkillCooldown(skillData.skillType);
            bool isOnCooldown = remainingCooldown > 0;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(isOnCooldown);
                
                if (isOnCooldown)
                {
                    float maxCooldown =  SkillManager.Instance.GetSkillData(skillData.skillType).cooldownTime;
                    if (maxCooldown > 0)
                    {
                        cooldownOverlay.fillAmount = remainingCooldown / maxCooldown;
                    }
                }
            }
            
            if (cooldownText != null)
            {
                if (isOnCooldown)
                {
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = remainingCooldown.ToString("F1");
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateButtonColor(bool canUse, bool hasEnoughKnowledge)
        {
            Color targetColor = Color.white;
            
            if (!canUse)
            {
                if (!hasEnoughKnowledge)
                {
                    targetColor = new Color(1f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    targetColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                }
            }
            
            iconImage.color = targetColor;
        }

        private bool HasEnoughKnowledge()
        {
            if (ResearchManager.Instance.IsSkillUnlocked(skillData.skillType))
            {
                return ResourceManager.Instance.HasEnoughKnowledge(SkillManager.Instance.GetSkillData(skillData.skillType).knowledgeCost);
            }
            
            return false;
        }
        
        public override string GetTooltip()
        {
            string tooltip = String.Empty;

            if (ResearchManager.Instance.IsSkillUnlocked(skillData.skillType))
                tooltip += skillData.GetTooltip();
            else
                tooltip += $"<color=red>需要研究[{skillData.skillName}]</color>";
            
            return tooltip;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            SkillManager.Instance.TrySelectSkill(skillData.skillType);
        }
    }
}