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
            EventManager.OnFoodChanged += UpdateFoodUI;
            EventManager.OnGoldChanged += UpdateGoldUI;
            EventManager.OnKnowledgeChanged += UpdateKnowledgeUI;
        }

        private void OnDisable()
        {
            EventManager.OnPopulationChanged -= UpdatePopulationUI;
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
                UpdatePopulationUI(ResourceManager.Instance.Population);
                UpdateFoodUI(ResourceManager.Instance.Food);
                UpdateGoldUI(ResourceManager.Instance.Gold);
                UpdateKnowledgeUI(ResourceManager.Instance.Knowledge);
            }
        }

        private void UpdatePopulationUI(int population)
        {
            populationText.text = population.ToString();
            
            if (population <= 1)
            {
                populationText.color = Color.red;
            }
            else
            {
                populationText.color = Color.black;
            }
        }

        private void UpdateFoodUI(int food)
        {
            foodText.text = food.ToString();
            
            if (food < ResourceManager.Instance.Population)
            {
                foodText.color = Color.red;
            }
            else
            {
                foodText.color = Color.black;
            }
        }

        private void UpdateGoldUI(int gold)
        {
            goldText.text = gold.ToString();
        }

        private void UpdateKnowledgeUI(int knowledge)
        {
            knowledgeText.text = knowledge.ToString();
        }
    }
}