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
        public Image icon;
        public Image completedImage;
        public TMP_Text nameText;
        public TMP_Text costText;
        public Button button;
    
        public ResearchData research;
        private ResearchUI researchUI;
    
        public void Setup(ResearchUI ui)
        {
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
                completedImage.enabled = true;
                if(button)
                    Destroy(button.gameObject);
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