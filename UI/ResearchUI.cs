using System.Collections.Generic;
using Data;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResearchUI : MonoBehaviour
    {
        public GameObject researchPanel;
        public Transform researchContent;
        public Button openButton;
        public Button closeButton;
    
        public List<ResearchItemUI> researchButtons = new List<ResearchItemUI>();
    
        void Start()
        {
            openButton.onClick.AddListener(()=> researchPanel.SetActive(true));
            closeButton.onClick.AddListener(()=> researchPanel.SetActive(false));
        
            SetupItems();
            researchPanel.SetActive(false);
        }
    
        void SetupItems()
        {
            foreach (var researchItemUI in researchButtons)
            {
                researchItemUI.Setup(this);
            }
        }
    
        public void OpenResearchPanel()
        {
            researchPanel.SetActive(true);
            RefreshResearchButtons();
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