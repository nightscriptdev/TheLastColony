using System.Collections.Generic;
using Core;
using Data;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResearchUI : MonoBehaviour
    {
        public GameObject researchPanel;
        public Button openButton;
        public Button closeButton;
    
        public List<ResearchItemUI> researchButtons = new List<ResearchItemUI>();
    
        void Start()
        {
            openButton.onClick.AddListener(OpenPanel);
            closeButton.onClick.AddListener(ClosePanel);
        
            SetupItems();
            ClosePanel();
        }
    
        void SetupItems()
        {
            foreach (var researchItemUI in researchButtons)
            {
                researchItemUI.Setup(this);
            }
        }
    
        public void OpenPanel()
        {
            EventManager.OnKnowledgeChanged += RefreshResearchButtons;
            
            researchPanel.SetActive(true);
            RefreshResearchButtons();
        }

        public void ClosePanel()
        {
            EventManager.OnKnowledgeChanged -= RefreshResearchButtons;
            
            researchPanel.SetActive(false);
        }
    
        void RefreshResearchButtons()
        {
            foreach (var button in researchButtons)
            {
                button.RefreshState();
            }
        }
    
        public void OnResearchButtonClicked(ResearchData research)
        {
            if (ResearchManager.Instance.DoResearch(research))
            {
                RefreshResearchButtons();
            }
        }
    }
}