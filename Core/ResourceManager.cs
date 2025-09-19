using Components.Buildings;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 资源管理器 - 管理所有游戏资源（人口、食物、金币、学识）
    /// 单例模式，提供统一的资源访问接口
    /// </summary>
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        [Header("初始资源设置")]
        [SerializeField] private int initialPopulation = 0;
        [SerializeField] private int initialFood = 20;
        [SerializeField] private int initialGold = 100;
        [SerializeField] private int initialKnowledge = 25;
    
        // 当前资源数量
        private int _currentPopulation;
        public int CurrentPopulation 
        {
            get => _currentPopulation;
            set 
            {
                if (_currentPopulation != value)
                {
                    _currentPopulation = value;
                    EventManager.OnPopulationChanged?.Invoke(value);
                }
            }
        }
        private int currentFood;
        private int currentGold;
        private int currentKnowledge;
    
        // 最大人口容量（由房屋提供）
        private int maxPopulation;
    
        void OnEnable()
        {
            // 订阅事件
            EventManager.OnDayStart += OnDayStart;
        }
    
        void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnDayStart -= OnDayStart;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AddFood(10);
                AddGold(10);
                AddKnowledge(10);
            }
        }

        /// <summary>
        /// 初始化资源到开局状态
        /// </summary>
        public void InitializeResources()
        {
            CurrentPopulation = initialPopulation;
            currentFood = initialFood;
            currentGold = initialGold;
            currentKnowledge = initialKnowledge;
            maxPopulation = initialPopulation; // 开局房屋提供的人口
        
            // 广播资源变化事件
            EventManager.OnFoodChanged?.Invoke(currentFood);
            EventManager.OnGoldChanged?.Invoke(currentGold);
            EventManager.OnKnowledgeChanged?.Invoke(currentKnowledge);
        }
        
        public void IncreaseMaxPopulation(int amount)
        {
            maxPopulation += amount;
        }

        public void DecreaseMaxPopulation(int amount)
        {
            maxPopulation = Mathf.Max(0, maxPopulation - amount);
            if(maxPopulation < CurrentPopulation) CurrentPopulation = maxPopulation;
        }

        /// <summary>
        /// 每天开始时的资源处理
        /// </summary>
        private void OnDayStart(int day)
        {
            ConsumeDailyFood();
            IncreasePopulation();
        }

        private void IncreasePopulation()
        {
            if(CurrentPopulation < maxPopulation && currentFood > CurrentPopulation) CurrentPopulation++;
        }
        
        /// <summary>
        /// 每日食物消耗
        /// </summary>
        private void ConsumeDailyFood()
        {
            int foodNeeded = CurrentPopulation; // 每人每天消耗1单位食物
        
            if (currentFood >= foodNeeded)
            {
                // 食物充足
                currentFood -= foodNeeded;
                EventManager.OnFoodChanged?.Invoke(currentFood);
                Debug.Log($"消耗食物 {foodNeeded}，剩余食物 {currentFood}");
            }
            else
            {
                // 食物不足，人口死亡
                int populationLoss = foodNeeded - currentFood;
                currentFood = 0;
            
                CurrentPopulation = Mathf.Max(0, CurrentPopulation - populationLoss);
            
                EventManager.OnFoodChanged?.Invoke(currentFood);
                
            
                Debug.Log($"食物不足！{populationLoss} 人死亡，当前人口: {CurrentPopulation}");
            }
        }
    
    
        // 资源操作方法
        public void AddFood(int amount)
        {
            currentFood += amount;
            EventManager.OnFoodChanged?.Invoke(currentFood);
        }
    
        public void AddGold(int amount)
        {
            currentGold += amount;
            EventManager.OnGoldChanged?.Invoke(currentGold);
        }
    
        public void AddKnowledge(int amount)
        {
            currentKnowledge += amount;
            EventManager.OnKnowledgeChanged?.Invoke(currentKnowledge);
        }
    
        public bool SpendGold(int amount)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                EventManager.OnGoldChanged?.Invoke(currentGold);
                return true;
            }
            return false;
        }
    
        public bool SpendKnowledge(int amount)
        {
            if (currentKnowledge >= amount)
            {
                currentKnowledge -= amount;
                EventManager.OnKnowledgeChanged?.Invoke(currentKnowledge);
                return true;
            }
            return false;
        }
    
        // 资源检查方法
        public bool HasEnoughGold(int amount) => currentGold >= amount;
        public bool HasEnoughKnowledge(int amount) => currentKnowledge >= amount;
    
        // 获取器
        public int Population => CurrentPopulation;
        public int Food => currentFood;
        public int Gold => currentGold;
        public int Knowledge => currentKnowledge;
        public int MaxPopulation => maxPopulation;
    
        /// <summary>
        /// 直接设置资源（用于存档加载）
        /// </summary>
        public void SetResources(int population, int food, int gold, int knowledge, int maxPop)
        {
            CurrentPopulation = population;
            currentFood = food;
            currentGold = gold;
            currentKnowledge = knowledge;
            maxPopulation = maxPop;
        
            // 广播资源变化事件
            EventManager.OnFoodChanged?.Invoke(currentFood);
            EventManager.OnGoldChanged?.Invoke(currentGold);
            EventManager.OnKnowledgeChanged?.Invoke(currentKnowledge);
        }
        
        public int CalculateFinalProduction(int baseAmount)
        {
            // 人口影响公式
            return Mathf.RoundToInt(baseAmount * (1 + CurrentPopulation * 0.02f));
        }
    }
}