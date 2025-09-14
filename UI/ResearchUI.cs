using System.Collections.Generic;
using Data;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResearchUI : MonoBehaviour
    {
        [Header("UI组件")]
        public GameObject researchPanel;
        public Transform researchGrid;
        public GameObject researchButtonPrefab;
        public Button closeButton;
        public Button openButton;
    
        private List<ResearchItemUI> researchButtons = new List<ResearchItemUI>();
    
        void Start()
        {
            closeButton.onClick.AddListener(CloseResearchPanel);
            openButton.onClick.AddListener(()=> researchPanel.SetActive(true));
        
            CreateResearchButtons();
            CloseResearchPanel();
        }
    
        void CreateResearchButtons()
        {
            foreach (var research in ResearchManager.Instance.allResearches)
            {
                GameObject buttonObj = Instantiate(researchButtonPrefab, researchGrid);
                ResearchItemUI researchItemUI = buttonObj.GetComponent<ResearchItemUI>();
                if (researchItemUI == null)
                    researchItemUI = buttonObj.AddComponent<ResearchItemUI>();
                
                researchItemUI.Setup(research, this);
                researchButtons.Add(researchItemUI);
            }
        }
    
        public void OpenResearchPanel()
        {
            researchPanel.SetActive(true);
            RefreshResearchButtons();
        }
    
        public void CloseResearchPanel()
        {
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