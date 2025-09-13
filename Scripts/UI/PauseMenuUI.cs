using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 暂停菜单UI控制器 - 移除了速度控制按钮
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("暂停菜单面板")]
        [SerializeField] private GameObject pauseMenuPanel;

        [Header("暂停菜单按钮")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("暂停触发按钮")]
        [SerializeField] private Button pauseToggleButton; // 游戏界面上的暂停按钮

        private void OnEnable()
        {
            // 订阅暂停相关事件
            EventManager.OnGamePause += ShowPauseMenu;
            EventManager.OnGameResume += HidePauseMenu;
        }

        private void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnGamePause -= ShowPauseMenu;
            EventManager.OnGameResume -= HidePauseMenu;
        }

        private void Start()
        {
            // 绑定按钮事件
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (pauseToggleButton != null)
                pauseToggleButton.onClick.AddListener(OnPauseToggleClicked);

            // 初始时隐藏暂停菜单
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        /// <summary>
        /// 暂停/继续切换
        /// </summary>
        private void OnPauseToggleClicked()
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

        /// <summary>
        /// 显示暂停菜单
        /// </summary>
        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        /// <summary>
        /// 隐藏暂停菜单
        /// </summary>
        private void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        /// <summary>
        /// 继续游戏按钮回调
        /// </summary>
        private void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        /// <summary>
        /// 返回主菜单按钮回调
        /// </summary>
        private void OnMainMenuClicked()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }

        /// <summary>
        /// 退出游戏按钮回调
        /// </summary>
        private void OnQuitClicked()
        {
            GameManager.Instance?.QuitGame();
        }
    }
}