using Components.Enemies;
using Core;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 游戏UI管理器 - 控制游戏进行时的UI显示状态
    /// 根据游戏状态自动显示/隐藏相应的UI面板
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameplayUI;    // 游戏进行时的UI
        [SerializeField] private GameObject pauseMenuUI;   // 暂停菜单
        [SerializeField] private GameObject gameOverUI;    // 游戏结束UI
        [SerializeField] private GameObject buildingConstructionUI;
        [SerializeField] private GameObject skillUI;

        [SerializeField] private ResourceUI resourceUI;
        [SerializeField] private DayNightCycleUI dayNightUI;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI dayNightText;
        [SerializeField] private TextMeshProUGUI killsText;
        
        
        [SerializeField] private Button btnShowBuildingConstructionUI;
        [SerializeField] private Button btnShowSkillUI;
        
        [SerializeField] private Color activeTabColor = Color.yellow;
        [SerializeField] private Color inactiveTabColor = Color.white;
        private void OnEnable()
        {
            // 订阅游戏状态变化事件
            EventManager.OnGameStart += OnGameStart;
            EventManager.OnGameEnd += OnGameEnd;
            EventManager.OnGamePause += OnGamePause;
            EventManager.OnGameResume += OnGameResume;
            EventManager.OnDayStart += UpdateDayUI;
            EventManager.OnNightStart += UpdateNightTimeUI;
            EventManager.OnEnemyDeath += OnEnemyDeath;
            
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        private void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnGameStart -= OnGameStart;
            EventManager.OnGameEnd -= OnGameEnd;
            EventManager.OnGamePause -= OnGamePause;
            EventManager.OnGameResume -= OnGameResume;
            EventManager.OnDayStart -= UpdateDayUI;
            EventManager.OnNightStart -= UpdateNightTimeUI;
            EventManager.OnEnemyDeath -= OnEnemyDeath;

            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        private void Start()
        {
            btnShowBuildingConstructionUI.onClick.AddListener(SwitchToBuildingConstructionUI);
            btnShowSkillUI.onClick.AddListener(SwitchToSkillUI);
            SwitchToBuildingConstructionUI();
        }

        private void SwitchToBuildingConstructionUI()
        {
            skillUI.SetActive(false);
            buildingConstructionUI.SetActive(true);
            btnShowBuildingConstructionUI.GetComponent<Image>().color = activeTabColor;
            btnShowSkillUI.GetComponent<Image>().color = inactiveTabColor;
        }

        private void SwitchToSkillUI()
        {
            buildingConstructionUI.SetActive(false);
            skillUI.SetActive(true);
            btnShowSkillUI.GetComponent<Image>().color = activeTabColor;
            btnShowBuildingConstructionUI.GetComponent<Image>().color = inactiveTabColor;
        }
        
        /// <summary>
        /// 游戏开始时的UI处理
        /// </summary>
        private void OnGameStart()
        {
            // 刷新资源UI显示
            if (resourceUI != null)
                resourceUI.RefreshAllUI();
        }

        /// <summary>
        /// 游戏结束时的UI处理
        /// </summary>
        private void OnGameEnd()
        {
            // 保持游戏UI显示，让玩家看到最终状态
            // GameOverUI会自动显示
        }

        /// <summary>
        /// 游戏暂停时的UI处理
        /// </summary>
        private void OnGamePause()
        {
            // PauseMenuUI会自动显示暂停面板
            // 这里可以添加额外的暂停时UI逻辑
        }

        /// <summary>
        /// 游戏继续时的UI处理
        /// </summary>
        private void OnGameResume()
        {
            // PauseMenuUI会自动隐藏暂停面板
            // 这里可以添加额外的继续时UI逻辑
        }

        /// <summary>
        /// 设置游戏UI的显示状态
        /// </summary>
        private void SetGameplayUIActive(bool active)
        {
            if (gameplayUI != null)
                gameplayUI.SetActive(active);
        }

        /// <summary>
        /// 手动切换暂停状态（供按钮调用）
        /// </summary>
        public void TogglePause()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return;

            if (gameManager.IsPlaying)
            {
                gameManager.PauseGame();
            }
            else if (gameManager.IsPaused)
            {
                gameManager.ResumeGame();
            }
        }
        
        private void UpdateDayUI(int day)
        {
            dayText.text = LocalizationManager.Instance.GetGameText("resource.day", day);
            UpdateDayTimeUI();
        }
        private void UpdateDayTimeUI()
        {
            dayNightText.text = LocalizationManager.Instance.GetUIText("ui.daytime");
        }
        private void UpdateNightTimeUI(int day = 1)
        {
            dayNightText.text = LocalizationManager.Instance.GetUIText("ui.nighttime");
        }
        private void OnSelectedLocaleChanged(Locale locale)
        {
            dayText.text = LocalizationManager.Instance.GetGameText("resource.day", GameManager.Instance.CurrentDay);
            if(TimeManager.Instance.IsDay)
                UpdateDayTimeUI();
            else
                UpdateNightTimeUI();
        }
        
        private void OnEnemyDeath(EnemyComponent enemyComponent)
        {
            killsText.text = ++GameManager.Instance.KillCount +"";
        }

        /// <summary>
        /// 获取当前是否在游戏中
        /// </summary>
        public bool IsInGame => GameManager.Instance?.IsPlaying == true;
    }
}