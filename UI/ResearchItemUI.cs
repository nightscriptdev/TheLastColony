using System;
using Core;
using Data;
using Managers;
using TMPro;
using UI.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResearchItemUI : TooltipTrigger
    {
        [Header("UI组件")]
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text costText;
        public Button button;
    
        private ResearchData research;
        private ResearchUI researchUI;
    
        public void Setup(ResearchData researchData, ResearchUI ui)
        {
            research = researchData;
            researchUI = ui;
        
            // 设置基本信息
            icon.sprite = research.icon;
            nameText.text = research.researchName;
            costText.text = research.knowledgeCost.ToString();
        
            button.onClick.AddListener(() => researchUI.OnResearchButtonClicked(research));
        
            RefreshState();
        }
    
        public void RefreshState()
        {
            bool isResearched = ResearchManager.Instance.IsResearched(research);

            // 按钮交互性
            button.interactable = ResearchManager.Instance.CanResearch(research);

            if (isResearched)
            {
                Destroy(button.gameObject); // 露出已完成icon
                costText.text = "已研究";
            }
            else if (ResourceManager.Instance.Knowledge >= research.knowledgeCost)
                costText.color = Color.white;
            else
                costText.color = Color.red;
        }
        
        public override string GetTooltip()
        {
            string tooltip = String.Empty;
            
            if (research.prerequisite!=null && !ResearchManager.Instance.IsResearched(research.prerequisite))
                tooltip += $"<color=red>前置条件: 已研究 [{research.prerequisite.researchName}]</color>\n";
            tooltip += research.description;
            
            return tooltip;
        }
    }
}