using System;
using UnityEngine;

namespace Core
{
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        [SerializeField] private int initialPopulation = 0;
        [SerializeField] private int initialFood = 20;
        [SerializeField] private int initialGold = 100;
        [SerializeField] private int initialKnowledge = 25;
    
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
    
        private int maxPopulation;
    
        void OnEnable()
        {
            EventManager.OnDayStart += OnDayStart;
        }
    
        void OnDisable()
        {
            EventManager.OnDayStart -= OnDayStart;
        }

//#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AddGold(20);
                AddKnowledge(20);
            }
        }
//#endif
        
        /// <summary>
        /// 初始化资源到开局状态
        /// </summary>
        public void InitializeResources()
        {
            CurrentPopulation = initialPopulation;
            currentFood = initialFood;
            currentGold = initialGold;
            currentKnowledge = initialKnowledge;
        
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

        private void OnDayStart(int day)
        {
            ConsumeDailyFood();
            IncreasePopulation();
        }

        private void IncreasePopulation()
        {
            if(CurrentPopulation < maxPopulation && currentFood > CurrentPopulation) CurrentPopulation++;
        }
        
        private void ConsumeDailyFood()
        {
            int foodNeeded = CurrentPopulation;
        
            if (currentFood >= foodNeeded)
            {
                currentFood -= foodNeeded;
                EventManager.OnFoodChanged?.Invoke(currentFood);
            }
            else
            {
                int populationLoss = foodNeeded - currentFood;
                currentFood = 0;
            
                CurrentPopulation = Mathf.Max(0, CurrentPopulation - populationLoss);
            
                EventManager.OnFoodChanged?.Invoke(currentFood);
            }
        }
    
    
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
    
        public bool HasEnoughGold(int amount) => currentGold >= amount;
        public bool HasEnoughKnowledge(int amount) => currentKnowledge >= amount;
    
        public int Population => CurrentPopulation;
        public int Food => currentFood;
        public int Gold => currentGold;
        public int Knowledge => currentKnowledge;
        public int MaxPopulation => maxPopulation;
    
        public int CalculateFinalProduction(int baseAmount)
        {
            // 人口影响公式
            return Mathf.RoundToInt(baseAmount * (1 + CurrentPopulation * 0.02f));
        }
    }
}