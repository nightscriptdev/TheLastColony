using Components.Enemies;
using Enums;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "GameScene";
        
        public Texture2D cursorTexture;

        public GameState CurrentState { get; private set; }
        public int KillCount = 0;
        public int BestRecordDays = 0;
        public int CurrentDay = 1;

        [SerializeField] private TextMeshProUGUI killsText;
        
        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            DontDestroyOnLoad(gameObject);
            
            CurrentState = GameState.MainMenu;
            Physics2D.queriesStartInColliders = false;
            
            LoadBestRecord();
            
            if(cursorTexture != null) Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }

        void OnEnable()
        {
            if (Instance != this) return;

            EventManager.OnPopulationChanged += OnPopulationChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            if (Instance != this) return;

            EventManager.OnPopulationChanged -= OnPopulationChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == gameSceneName)
            {
                ResourceManager.Instance.InitializeResources();

                EventManager.OnGameStart?.Invoke();
                EventManager.OnDayStart?.Invoke(CurrentDay);
            }
        }

        public void StartNewGame()
        {
            if (CurrentState != GameState.MainMenu && CurrentState != GameState.GameOver)
                return;
            
            CurrentDay = 1;
            KillCount = 0;
            
            ChangeState(GameState.Playing);

            SceneManager.LoadScene(gameSceneName);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
                return;
            
            ChangeState(GameState.Paused);
            EventManager.OnGamePause.Invoke();
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
                return;
            
            ChangeState(GameState.Playing);
            EventManager.OnGameResume.Invoke();
        }

        public void EndGame()
        {
            if (CurrentState != GameState.Playing)
                return;
            
            ChangeState(GameState.GameOver);
        
            if (CurrentDay > BestRecordDays)
            {
                BestRecordDays = CurrentDay;
                SaveBestRecordDays();
            }
        
            EventManager.OnGameEnd.Invoke();
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void StartNewDay()
        {
            CurrentDay++;
            EventManager.OnDayStart?.Invoke(CurrentDay);
        }

        public void StartNight()
        {
            EventManager.OnNightStart?.Invoke(CurrentDay);
        }

        private void OnPopulationChanged(int newPopulation)
        {
            if (newPopulation <= 0 && CurrentState == GameState.Playing)
            {
                EndGame();
            }
        }

        private void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
        }

        private void LoadBestRecord()
        {
            BestRecordDays = PlayerPrefs.GetInt("BestRecordDays", 0);
        }

        private void SaveBestRecordDays()
        {
            PlayerPrefs.SetInt("BestRecordDays", BestRecordDays);
            PlayerPrefs.Save();
        }

        

        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsPaused => CurrentState == GameState.Paused;
    }
}