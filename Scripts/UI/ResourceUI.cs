using Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 资源显示UI - 实时显示玩家的各种资源数量
    /// </summary>
    public class ResourceUI : MonoBehaviour
    {
        [Header("资源显示文本")]
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI knowledgeText;
        [SerializeField] private TextMeshProUGUI dayText;

        [Header("资源图标（可选）")]
        [SerializeField] private Image populationIcon;
        [SerializeField] private Image foodIcon;
        [SerializeField] private Image goldIcon;
        [SerializeField] private Image knowledgeIcon;

        private void OnEnable()
        {
            // 订阅资源变化事件
            EventManager.OnPopulationChanged += UpdatePopulationUI;
            EventManager.OnFoodChanged += UpdateFoodUI;
            EventManager.OnGoldChanged += UpdateGoldUI;
            EventManager.OnKnowledgeChanged += UpdateKnowledgeUI;
            EventManager.OnDayStart += UpdateDayUI;
        }

        private void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnPopulationChanged -= UpdatePopulationUI;
            EventManager.OnFoodChanged -= UpdateFoodUI;
            EventManager.OnGoldChanged -= UpdateGoldUI;
            EventManager.OnKnowledgeChanged -= UpdateKnowledgeUI;
            EventManager.OnDayStart -= UpdateDayUI;
        }

        private void Start()
        {
            // 初始化显示
            RefreshAllUI();
        }

        /// <summary>
        /// 刷新所有UI显示
        /// </summary>
        public void RefreshAllUI()
        {
            if (ResourceManager.Instance != null)
            {
                UpdatePopulationUI(ResourceManager.Instance.Population);
                UpdateFoodUI(ResourceManager.Instance.Food);
                UpdateGoldUI(ResourceManager.Instance.Gold);
                UpdateKnowledgeUI(ResourceManager.Instance.Knowledge);
            }

            if (GameManager.Instance != null)
            {
                UpdateDayUI(GameManager.Instance.CurrentDay);
            }
        }

        private void UpdatePopulationUI(int population)
        {
            if (populationText != null)
            {
                populationText.text = population.ToString();
                
                // 人口不足时显示红色警告
                if (population <= 1)
                {
                    populationText.color = Color.red;
                }
                else if (population <= 3)
                {
                    populationText.color = Color.yellow;
                }
                else
                {
                    populationText.color = Color.white;
                }
            }
        }

        private void UpdateFoodUI(int food)
        {
            if (foodText != null)
            {
                foodText.text = food.ToString();
                
                // 食物不足时显示警告颜色
                int dailyConsumption = ResourceManager.Instance?.Population ?? 0;
                if (food < dailyConsumption)
                {
                    foodText.color = Color.red;
                }
                else if (food < dailyConsumption * 2)
                {
                    foodText.color = Color.yellow;
                }
                else
                {
                    foodText.color = Color.white;
                }
            }
        }

        private void UpdateGoldUI(int gold)
        {
            if (goldText != null)
            {
                goldText.text = gold.ToString();
            }
        }

        private void UpdateKnowledgeUI(int knowledge)
        {
            if (knowledgeText != null)
            {
                knowledgeText.text = knowledge.ToString();
            }
        }

        private void UpdateDayUI(int day)
        {
            if (dayText != null)
            {
                dayText.text = $"第 {day} 天";
            }
        }
    }
}