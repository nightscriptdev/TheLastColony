using System;
using Data;
using Managers;
using TMPro;
using UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI
{
    public class ResearchItemUI : TooltipTrigger
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text costText;
        public Button button;
    
        public ResearchData research;
        private ResearchUI researchUI;
        
        string knowledgeInfo = String.Empty;


        private void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        void OnSelectedLocaleChanged(Locale locale)
        {
            nameText.text = research.GetLocalizedResearchName();
            if (ResearchManager.Instance.IsResearched(research))
            {
                costText.text = LocalizationManager.Instance.GetGameText("tooltip.researched");
            }
        }
        
        public void Setup(ResearchUI ui)
        {
            researchUI = ui;
        
            // 设置基本信息
            icon.sprite = research.icon;
            nameText.text = research.GetLocalizedResearchName();
            costText.text = research.knowledgeCost.ToString();
        
            button.onClick.AddListener(() => researchUI.OnResearchButtonClicked(research));
        
        }
    
        public void RefreshState(int nowKnowledge)
        {
            if (ResearchManager.Instance.IsResearched(research))
            {
                if(button)
                    Destroy(button.gameObject);
                costText.color = Color.black;
                costText.text = LocalizationManager.Instance.GetGameText("tooltip.researched");
            }
            else
            {
                button.interactable = ResearchManager.Instance.CanResearch(research);

                if (nowKnowledge >= research.knowledgeCost)
                {
                    costText.color = Color.black;
                    knowledgeInfo = String.Empty;
                }
                else
                {
                    knowledgeInfo = $"<color=red>{LocalizationManager.Instance.GetGameText("tooltip.knowledge.insufficient")}</color>";
                    costText.color = Color.red;    
                }
            }
        }

        public override string GetTooltip()
        {
            string tooltip = String.Empty;
            
            if (research.prerequisite!=null && !ResearchManager.Instance.IsResearched(research.prerequisite))
                tooltip += $"<color=red>{LocalizationManager.Instance.GetGameText("tooltip.prerequisite", research.prerequisite.GetLocalizedResearchName())}</color>\n";
            //tooltip += knowledgeInfo;
            
            return tooltip;
        }
    }
}