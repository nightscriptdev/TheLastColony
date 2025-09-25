using UnityEngine;

namespace Core
{
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        [SerializeField] private int initialPopulation = 0;
        [SerializeField] private int initialFood = 20;
        [SerializeField] private int initialGold = 100;
        [SerializeField] private int initialKnowledge = 25;
    
        private int _population;
        public int Population 
        {
            get => _population;
            set
            {
                if (_population == value) return;
                _population = value;
                EventManager.OnPopulationChanged?.Invoke();
            }
        }
        private int _food;
        public int Food 
        {
            get => _food;
            set
            {
                if (_food == value) return;
                _food = value;
                EventManager.OnFoodChanged?.Invoke();
            }
        }
        private int _gold;
        public int Gold 
        {
            get => _gold;
            set
            {
                if (_gold == value) return;
                _gold = value;
                EventManager.OnGoldChanged?.Invoke();
            }
        }
        private int _knowledge;
        public int Knowledge 
        {
            get => _knowledge;
            set
            {
                if (_knowledge == value) return;
                _knowledge = value;
                EventManager.OnKnowledgeChanged?.Invoke();
            }
        }
    
        private int _maxPopulation;
        public int MaxPopulation 
        {
            get => _maxPopulation;
            set
            {
                if (_maxPopulation == value) return;
                _maxPopulation = value;
                EventManager.OnMaxPopulationChanged?.Invoke();
            }
        }
    
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
                Population += 10;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Food += 10;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Gold += 1000;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                Knowledge += 100;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Population -= 10;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                Food -= 10;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                Gold -= 1000;
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                Knowledge -= 100;
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.Instance.CurrentDay = 21;
                GameManager.Instance.KillCount = 701;
            }
        }
//#endif
        
        /// <summary>
        /// 初始化资源到开局状态
        /// </summary>
        public void InitializeResources()
        {
            Population = initialPopulation;
            Food = initialFood;
            Gold = initialGold;
            Knowledge = initialKnowledge;
        }
        
        public void IncreaseMaxPopulation(int amount)
        {
            MaxPopulation += amount;
        }

        public void DecreaseMaxPopulation(int amount)
        {
            MaxPopulation = Mathf.Max(0, _maxPopulation - amount);
            if(_maxPopulation < Population) Population = _maxPopulation;
        }

        private void OnDayStart(int day)
        {
            ConsumeDailyFood();
            IncreasePopulation();
        }

        private void IncreasePopulation()
        {
            if(Population < _maxPopulation && _food > Population) Population++;
        }
        
        private void ConsumeDailyFood()
        {
            int foodNeeded = Population;
        
            if (Food >= foodNeeded)
            {
                Food -= foodNeeded;
            }
            else
            {
                int populationLoss = foodNeeded - Food;
                Food = 0;
                Population = Mathf.Max(0, Population - populationLoss);
            }
        }
    
        public bool SpendGold(int amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                return true;
            }
            return false;
        }
    
        public bool SpendKnowledge(int amount)
        {
            if (Knowledge >= amount)
            {
                Knowledge -= amount;
                return true;
            }
            return false;
        }
    
        public bool HasEnoughGold(int amount) => _gold >= amount;
        public bool HasEnoughKnowledge(int amount) => _knowledge >= amount;
    
        public int CalculateFinalProduction(int baseAmount)
        {
            // 人口影响公式
            return Mathf.RoundToInt(baseAmount * (1 + Population * 0.02f));
        }
    }
}