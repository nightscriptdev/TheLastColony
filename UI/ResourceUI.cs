using Core;
using UnityEngine;
using TMPro;

namespace UI
{
    public class ResourceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI knowledgeText;

        private void OnEnable()
        {
            EventManager.OnPopulationChanged += UpdatePopulationUI;
            EventManager.OnPopulationChanged += UpdateFoodUI;
            EventManager.OnFoodChanged += UpdateFoodUI;
            EventManager.OnGoldChanged += UpdateGoldUI;
            EventManager.OnKnowledgeChanged += UpdateKnowledgeUI;
        }

        private void OnDisable()
        {
            EventManager.OnPopulationChanged -= UpdatePopulationUI;
            EventManager.OnPopulationChanged -= UpdateFoodUI;
            EventManager.OnFoodChanged -= UpdateFoodUI;
            EventManager.OnGoldChanged -= UpdateGoldUI;
            EventManager.OnKnowledgeChanged -= UpdateKnowledgeUI;
        }

        private void Start()
        {
            RefreshAllUI();
        }

        public void RefreshAllUI()
        {
            if (ResourceManager.Instance != null)
            {
                UpdatePopulationUI();
                UpdateFoodUI();
                UpdateGoldUI();
                UpdateKnowledgeUI();
            }
        }

        private void UpdatePopulationUI()
        {
            populationText.text = ResourceManager.Instance.Population.ToString();
            
            if (ResourceManager.Instance.Population <= 1)
            {
                populationText.color = Color.red;
            }
            else
            {
                populationText.color = Color.black;
            }
        }

        private void UpdateFoodUI()
        {
            foodText.text = ResourceManager.Instance.Food.ToString();
            
            if (ResourceManager.Instance.Food < ResourceManager.Instance.Population)
            {
                foodText.color = Color.red;
            }
            else
            {
                foodText.color = Color.black;
            }
        }

        private void UpdateGoldUI()
        {
            goldText.text = ResourceManager.Instance.Gold.ToString();
        }

        private void UpdateKnowledgeUI()
        {
            knowledgeText.text = ResourceManager.Instance.Knowledge.ToString();
        }
    }
}