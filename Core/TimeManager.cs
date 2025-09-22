using Enums;
using UnityEngine;

namespace Core
{
    public class TimeManager : MonoSingleton<TimeManager>
    {
        public float dayDurationInSeconds = 60f;
        public float nightDurationInSeconds = 30f;
        
        public DayNightState CurrentDayNightState { get; private set; }
        private float currentTimer;
        private float currentDuration;
        private int currentTimeScaleIndex = 0;

        private float previousTimeScale;

        private void OnEnable()
        {
            EventManager.OnGameStart += ResetTimeState;
            EventManager.OnGamePause += OnGamePause;
            EventManager.OnGameResume += OnGameResume;
            EventManager.OnGameEnd += OnGameEnd;
        }

        private void OnDisable()
        {
            EventManager.OnGameStart -= ResetTimeState;
            EventManager.OnGamePause -= OnGamePause;
            EventManager.OnGameResume -= OnGameResume;
            EventManager.OnGameEnd -= OnGameEnd;
        }
        
        void Start()
        {
            ResetTimeState();
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            
            UpdateDayNightCycle();
        }

        private void ResetTimeState()
        {
            CurrentDayNightState = DayNightState.Day;
            currentDuration = dayDurationInSeconds;
            currentTimer = 0f;
            currentTimeScaleIndex = 0;
            
            SetTimeScale(1);
        }
        
        private void UpdateDayNightCycle()
        {
            currentTimer += Time.deltaTime;

            float timeProgress = currentTimer / currentDuration;
            
            if (CurrentDayNightState == DayNightState.Day)
                EventManager.OnDayTick?.Invoke(timeProgress);
            else
                EventManager.OnNightTick?.Invoke(timeProgress);

            if (currentTimer >= currentDuration)
            {
                SwitchDayNight();
            }
        }
        
        private void SwitchDayNight()
        {
            currentTimer = 0f;
        
            if (CurrentDayNightState == DayNightState.Day)
            {
                CurrentDayNightState = DayNightState.Night;
                currentDuration = nightDurationInSeconds;
                GameManager.Instance?.StartNight();
            }
            else
            {
                CurrentDayNightState = DayNightState.Day;
                currentDuration = dayDurationInSeconds;
                GameManager.Instance?.StartNewDay();
            }
        }
        
        public void SetTimeScale(float newTimeScale)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = newTimeScale;
            EventManager.OnTimeScaleChanged?.Invoke(newTimeScale);
        }
    
        private void OnGamePause()
        {
            SetTimeScale(0);
        }
    
        private void OnGameResume()
        {
            SetTimeScale(previousTimeScale);
        }

        void OnGameEnd()
        {
            SetTimeScale(0);
        }
    
        public bool IsDay => CurrentDayNightState == DayNightState.Day;
        public bool IsNight => CurrentDayNightState == DayNightState.Night;
    }
}