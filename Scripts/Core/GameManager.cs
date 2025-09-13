using Components.Enemies;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// 游戏流程控制器 - 管理游戏的整体状态和流程
    /// 使用状态机模式来管理游戏的不同阶段
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        [Header("游戏设置")]
        [SerializeField] private int bestRecordDays = 0;

        [Header("场景名称")]
        [Tooltip("主菜单场景的名称")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [Tooltip("主游戏场景的名称")]
        [SerializeField] private string gameSceneName = "GameScene";

        // 当前游戏状态
        public GameState CurrentState { get; private set; }
        public int killCount = 0;
        
        // 游戏数据
        private int currentDay = 1;

        protected override void Awake()
        {
            base.Awake();
            // 确保GameManager在场景切换时不被销毁
            DontDestroyOnLoad(gameObject);
            
            // 初始化游戏状态
            CurrentState = GameState.MainMenu;
            
            Physics2D.queriesStartInColliders = false;
        }

        void OnEnable()
        {
            // 订阅事件
            EventManager.OnPopulationChanged += OnPopulationChanged;
            EventManager.OnEnemyDeath += OnEnemyDeath;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnPopulationChanged -= OnPopulationChanged;
            EventManager.OnEnemyDeath -= OnEnemyDeath;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            // 加载最高纪录
            LoadBestRecord();
        }
        
        /// <summary>
        /// 当一个新场景加载完成时调用
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 检查加载的是否是游戏主场景
            if (scene.name == gameSceneName)
            {
                // 只有在GameScene加载完成后，才进行游戏相关的初始化
                // 这样可以确保GameScene中的对象（如ResourceManager）已经准备就绪
                
                // 初始化资源
                ResourceManager.Instance?.InitializeResources();

                // 广播游戏开始和第一天开始的事件
                EventManager.OnGameStart?.Invoke();
                EventManager.OnDayStart?.Invoke(currentDay);

                Debug.Log("游戏开始 - 第一天");
            }
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            // 只有在主菜单或游戏结束状态下才能开始新游戏
            if (CurrentState != GameState.MainMenu && CurrentState != GameState.GameOver)
                return;
            
            // 重置游戏数据
            currentDay = 1;
            killCount = 0;
            
            ChangeState(GameState.Playing);

            // 加载游戏场景，后续的初始化逻辑会由OnSceneLoaded事件处理
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
                return;
            
            ChangeState(GameState.Paused);
            // Time.timeScale的控制交给了TimeManager，这里只广播事件
            EventManager.OnGamePause?.Invoke();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
                return;
            
            ChangeState(GameState.Playing);
            // 恢复时间倍速由TimeManager通过监听此事件来处理
            EventManager.OnGameResume?.Invoke();
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public void EndGame()
        {
            if (CurrentState != GameState.Playing)
                return;
            
            ChangeState(GameState.GameOver);
        
            // 更新最高纪录
            if (currentDay > bestRecordDays)
            {
                bestRecordDays = currentDay;
                SaveBestRecordDays();
            }
        
            EventManager.OnGameEnd?.Invoke();
            Debug.Log($"游戏结束 - 坚持了 {currentDay} 天，击杀 {killCount} 个敌人");
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            Time.timeScale = 1f; // 确保时间恢复正常
            // 清理事件，为返回主菜单并可能重新开始做准备
            // 注意：这会清除所有静态事件的订阅者，请确保这是期望的行为
            EventManager.ClearAllEvents();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 新的一天开始
        /// </summary>
        public void StartNewDay()
        {
            currentDay++;
            EventManager.OnDayStart?.Invoke(currentDay);
        
            // 白天开始时自动存档
            //SaveManager.Instance.AutoSave();
        
            Debug.Log($"第 {currentDay} 天开始");
        }

        /// <summary>
        /// 夜晚开始
        /// </summary>
        public void StartNight()
        {
            EventManager.OnNightStart?.Invoke(currentDay);
            Debug.Log($"第 {currentDay} 天夜晚开始");
        }

        private void OnPopulationChanged(int newPopulation)
        {
            // 人口归零，游戏结束
            if (newPopulation <= 0 && CurrentState == GameState.Playing)
            {
                EndGame();
            }
        }

        /// <summary>
        /// 改变游戏状态
        /// </summary>
        private void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState oldState = CurrentState;
            CurrentState = newState;
        
            Debug.Log($"游戏状态变化: {oldState} -> {newState}");
        }

        /// <summary>
        /// 加载最高纪录
        /// </summary>
        private void LoadBestRecord()
        {
            bestRecordDays = PlayerPrefs.GetInt("BestRecordDays", 0);
        }

        /// <summary>
        /// 保存最高纪录
        /// </summary>
        private void SaveBestRecordDays()
        {
            PlayerPrefs.SetInt("BestRecordDays", bestRecordDays);
            PlayerPrefs.Save();
        }

        private void OnEnemyDeath(EnemyComponent enemyComponent)
        {
            ++killCount;
        }

        // 公开访问器
        public int CurrentDay => currentDay;
        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsPaused => CurrentState == GameState.Paused;
    }
}
