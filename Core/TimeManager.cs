using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    /// <summary>
    /// 时间管理器 - 改进版
    /// 处理昼夜循环、游戏速度控制和简单天气系统
    /// </summary>
    public class TimeManager : MonoSingleton<TimeManager>
    {
        [Header("时间设置 (秒)")]
        public float dayDurationInSeconds = 60f;
        public float nightDurationInSeconds = 30f;

        [Header("天气设置")]
        [SerializeField] private float weatherCheckInterval = 10f;
        [SerializeField] [Range(0f, 1f)] private float rainStartChance = 0.02f;
        [SerializeField] [Range(0f, 1f)] private float rainStopChance = 0.1f;
        
        // 时间状态
        public DayNightState CurrentDayNightState { get; private set; }
        private float currentTimer;
        private float currentDuration;
        private int currentTimeScaleIndex = 0;
        
        // 天气状态
        private float weatherCheckTimer = 0f;
        private bool isRaining = false;

        private float previousTimeScale;
        
        protected override void Awake()
        {
            base.Awake();
            // 初始化为白天状态
            CurrentDayNightState = DayNightState.Day;
            currentDuration = dayDurationInSeconds;
        }

        private void OnEnable()
        {
            EventManager.OnGameStart += OnGameStart;
            EventManager.OnGamePause += OnGamePause;
            EventManager.OnGameResume += OnGameResume;
        }

        private void OnDisable()
        {
            EventManager.OnGameStart -= OnGameStart;
            EventManager.OnGamePause -= OnGamePause;
            EventManager.OnGameResume -= OnGameResume;
        }

        private void Update()
        {
            if (!IsGamePlaying())
                return;
            
            UpdateDayNightCycle();
            UpdateWeatherSystem();
        }

        /// <summary>
        /// 检查游戏是否在进行中
        /// </summary>
        private bool IsGamePlaying()
        {
            return GameManager.Instance?.CurrentState == GameState.Playing;
        }

        /// <summary>
        /// 更新昼夜循环
        /// </summary>
        private void UpdateDayNightCycle()
        {
            currentTimer += Time.deltaTime;

            // 广播时间进度事件
            float timeProgress = currentTimer / currentDuration;
            if (CurrentDayNightState == DayNightState.Day)
            {
                EventManager.OnDayTick?.Invoke(timeProgress);
            }
            else
            {
                EventManager.OnNightTick?.Invoke(timeProgress);
            }

            // 检查是否需要切换昼夜
            if (currentTimer >= currentDuration)
            {
                SwitchDayNight();
            }
        }
        
        /// <summary>
        /// 切换昼夜状态
        /// </summary>
        private void SwitchDayNight()
        {
            currentTimer = 0f;
        
            if (CurrentDayNightState == DayNightState.Day)
            {
                // 白天 → 夜晚
                CurrentDayNightState = DayNightState.Night;
                currentDuration = nightDurationInSeconds;
                GameManager.Instance?.StartNight();
            }
            else
            {
                // 夜晚 → 白天
                CurrentDayNightState = DayNightState.Day;
                currentDuration = dayDurationInSeconds;
                GameManager.Instance?.StartNewDay();
            }
        
            Debug.Log($"时间切换至: {CurrentDayNightState}");
        }
        
        /// <summary>
        /// 更新天气系统
        /// </summary>
        private void UpdateWeatherSystem()
        {
            weatherCheckTimer += Time.deltaTime;
        
            if (weatherCheckTimer >= weatherCheckInterval)
            {
                weatherCheckTimer = 0f;
                CheckWeatherChange();
            }
        }
        
        /// <summary>
        /// 检测天气变化
        /// </summary>
        private void CheckWeatherChange()
        {
            float randomValue = Random.Range(0f, 1f);
            
            if (!isRaining && randomValue < rainStartChance)
            {
                StartRain();
            }
            else if (isRaining && randomValue < rainStopChance)
            {
                StopRain();
            }
        }
    
        private void StartRain()
        {
            isRaining = true;
            EventManager.OnRainStart?.Invoke();
            Debug.Log("🌧️ 开始下雨");
        }
    
        private void StopRain()
        {
            isRaining = false;
            EventManager.OnRainStop?.Invoke();
            Debug.Log("☀️ 停止下雨");
        }
        
        /// <summary>
        /// 设置游戏速度
        /// </summary>
        public void SetTimeScale(float newTimeScale)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = newTimeScale;
            EventManager.OnTimeScaleChanged?.Invoke(newTimeScale);
            Debug.Log($"游戏速度: {newTimeScale}x");
        }
    
        private void OnGameStart()
        {
            // 重置所有时间相关状态
            CurrentDayNightState = DayNightState.Day;
            currentDuration = dayDurationInSeconds;
            currentTimer = 0f;
            currentTimeScaleIndex = 0;
            
            // 重置天气
            isRaining = false;
            weatherCheckTimer = 0f;
            
            // 设置初始时间倍速
            SetTimeScale(1);
            EventManager.OnTimeScaleChanged?.Invoke(Time.timeScale);
        }
    
        private void OnGamePause()
        {
            SetTimeScale(0);
        }
    
        private void OnGameResume()
        {
            SetTimeScale(previousTimeScale);
        }
    
        // 公开访问器
        public bool IsDay => CurrentDayNightState == DayNightState.Day;
        public bool IsNight => CurrentDayNightState == DayNightState.Night;
    }
}