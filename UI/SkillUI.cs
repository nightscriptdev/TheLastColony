using System.Collections.Generic;
using Data;
using Enums;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SkillUI : MonoBehaviour
    {
        [Header("技能栏组件")]
        [SerializeField] private Transform skillButtonContainer;
        [SerializeField] private GameObject skillButtonPrefab;
        
        [Header("技能预览")]
        [SerializeField] private GameObject skillPreviewPanel;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillDescriptionText;
        [SerializeField] private TextMeshProUGUI skillCostText;
        [SerializeField] private TextMeshProUGUI skillCooldownText;
        [SerializeField] private Image skillPreviewIcon;
        
        private List<SkillButton> skillButtons = new List<SkillButton>();
        private SkillManager skillManager;
        private ResearchManager researchManager;

        void Start()
        {
            skillManager = SkillManager.Instance;
            researchManager = ResearchManager.Instance;
            
            InitializeSkillUI();
            
            if (skillPreviewPanel != null)
                skillPreviewPanel.SetActive(false);
        }

        void Update()
        {
            UpdateSkillButtons();
            UpdateSkillPreview();
        }

        /// <summary>
        /// 初始化技能UI
        /// </summary>
        private void InitializeSkillUI()
        {
            if (skillButtonContainer == null || skillButtonPrefab == null)
                return;

            // 创建三个技能按钮（雷电、黑暗冲击、虚空漩涡）
            CreateSkillButton(SkillType.Lightning, "Q");
            CreateSkillButton(SkillType.DarkImpact, "W");
            CreateSkillButton(SkillType.VoidVortex, "E");
        }

        /// <summary>
        /// 创建技能按钮
        /// </summary>
        private void CreateSkillButton(SkillType skillType, string hotkey)
        {
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillButton skillButton = buttonObj.GetComponent<SkillButton>();
            
            if (skillButton != null)
            {
                skillButton.Initialize(skillType, hotkey, OnSkillButtonClick);
                skillButtons.Add(skillButton);
            }
        }

        /// <summary>
        /// 技能按钮点击事件
        /// </summary>
        private void OnSkillButtonClick(SkillType skillType)
        {
            if (skillManager != null)
            {
                // 模拟按键输入来触发技能选择
                switch (skillType)
                {
                    case SkillType.Lightning:
                        // 这里可以直接调用SkillManager的选择方法，或者发送事件
                        Debug.Log("雷电技能按钮点击");
                        break;
                    case SkillType.DarkImpact:
                        Debug.Log("黑暗冲击技能按钮点击");
                        break;
                    case SkillType.VoidVortex:
                        Debug.Log("虚空漩涡技能按钮点击");
                        break;
                }
            }
        }

        /// <summary>
        /// 更新技能按钮状态
        /// </summary>
        private void UpdateSkillButtons()
        {
            foreach (var button in skillButtons)
            {
                if (button != null)
                {
                    button.UpdateButton();
                }
            }
        }

        /// <summary>
        /// 更新技能预览面板
        /// </summary>
        private void UpdateSkillPreview()
        {
            if (skillManager == null || skillPreviewPanel == null)
                return;

            var selectedSkill = skillManager.GetCurrentSelectedSkill();
            
            if (selectedSkill != null && skillManager.IsInSkillCastMode)
            {
                ShowSkillPreview(selectedSkill);
            }
            else
            {
                HideSkillPreview();
            }
        }

        /// <summary>
        /// 显示技能预览
        /// </summary>
        private void ShowSkillPreview(SkillData skillData)
        {
            if (!skillPreviewPanel.activeSelf)
                skillPreviewPanel.SetActive(true);

            if (skillNameText != null)
                skillNameText.text = skillData.skillName;
            
            if (skillDescriptionText != null)
                skillDescriptionText.text = skillData.description;
            
            if (skillCostText != null)
            {
                int cost = Mathf.RoundToInt(skillData.knowledgeCost * researchManager.GetSkillKnowledgeMultiplier());
                skillCostText.text = $"消耗学识: {cost}";
            }
            
            if (skillCooldownText != null)
            {
                float cooldown = skillData.cooldownTime * researchManager.GetSkillCooldownMultiplier();
                skillCooldownText.text = $"冷却时间: {cooldown:F1}s";
            }
            
            if (skillPreviewIcon != null && skillData.icon != null)
                skillPreviewIcon.sprite = skillData.icon;
        }

        /// <summary>
        /// 隐藏技能预览
        /// </summary>
        private void HideSkillPreview()
        {
            if (skillPreviewPanel != null && skillPreviewPanel.activeSelf)
                skillPreviewPanel.SetActive(false);
        }
    }
}