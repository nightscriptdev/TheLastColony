using Core;
using Enums;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SkillButton : MonoBehaviour
    {
        [Header("按钮组件")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI hotkeyText;
        [SerializeField] private GameObject lockedOverlay;
        
        private SkillType skillType;
        private System.Action<SkillType> onClickCallback;
        
        public void Initialize(SkillType type, string hotkey, System.Action<SkillType> callback)
        {
            skillType = type;
            onClickCallback = callback;
            
            // 设置快捷键文本
            if (hotkeyText != null)
                hotkeyText.text = hotkey;
            
            // 设置按钮点击事件
            if (button != null)
                button.onClick.AddListener(() => onClickCallback?.Invoke(skillType));
            
            // 初始化冷却遮罩
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
                cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
            }
        }

        public void UpdateButton()
        {
            // 检查技能是否解锁
            bool isUnlocked = ResearchManager.Instance.IsSkillUnlocked(skillType);
            
            // 更新锁定状态
            if (lockedOverlay != null)
                lockedOverlay.SetActive(!isUnlocked);
            
            if (!isUnlocked)
            {
                // 技能未解锁，禁用按钮
                if (button != null)
                    button.interactable = false;
                return;
            }

            // 技能已解锁，检查其他状态
            bool isOnCooldown = SkillManager.Instance .IsSkillOnCooldown(skillType);
            bool hasEnoughKnowledge = HasEnoughKnowledge();
            bool canUse = !isOnCooldown && hasEnoughKnowledge;
            
            // 更新按钮可交互状态
            if (button != null)
                button.interactable = canUse;
            
            // 更新冷却显示
            UpdateCooldownDisplay();
            
            // 更新按钮颜色（可选）
            UpdateButtonColor(canUse, hasEnoughKnowledge);
        }

        private void UpdateCooldownDisplay()
        {
            float remainingCooldown = SkillManager.Instance.GetSkillCooldown(skillType);
            bool isOnCooldown = remainingCooldown > 0;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(isOnCooldown);
                
                if (isOnCooldown)
                {
                    float maxCooldown =  SkillManager.Instance.GetSkillData(skillType).cooldownTime;
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
            if (iconImage == null) return;

            Color targetColor = Color.white;
            
            if (!canUse)
            {
                if (!hasEnoughKnowledge)
                {
                    // 学识不足 - 红色调
                    targetColor = new Color(1f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    // 冷却中 - 灰色调
                    targetColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                }
            }
            
            iconImage.color = targetColor;
        }

        private bool HasEnoughKnowledge()
        {
            if (ResearchManager.Instance.IsSkillUnlocked(skillType))
            {
                return ResourceManager.Instance.HasEnoughKnowledge(SkillManager.Instance.GetSkillData(skillType).knowledgeCost);
            }
            
            return false;
        }
        
    }
}