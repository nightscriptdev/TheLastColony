using Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 主菜单UI控制器
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("菜单按钮")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private Button settingsButton;

        [Header("最高纪录显示")]
        [SerializeField] private TextMeshProUGUI bestRecordText;

        [Header("菜单面板")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            // 绑定按钮事件
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
            
            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(OnQuitGameClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            // 显示最高纪录
            UpdateBestRecordDisplay();
            
            // 确保主菜单面板显示
            ShowMainMenu();
        }

        private void OnStartGameClicked()
        {
            GameManager.Instance?.StartNewGame();
        }

        private void OnQuitGameClicked()
        {
            GameManager.Instance?.QuitGame();
        }

        private void OnSettingsClicked()
        {
            ShowSettings();
        }

        /// <summary>
        /// 显示主菜单
        /// </summary>
        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        /// <summary>
        /// 显示设置菜单
        /// </summary>
        public void ShowSettings()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        /// <summary>
        /// 更新最高纪录显示
        /// </summary>
        private void UpdateBestRecordDisplay()
        {
            if (bestRecordText != null)
            {
                int bestRecord = PlayerPrefs.GetInt("BestRecordDays", 0);
                bestRecordText.text = bestRecord > 0 ? 
                    $"最高纪录: {bestRecord} 天" : 
                    "还没有纪录";
            }
        }

        /// <summary>
        /// 返回主菜单按钮回调（从设置面板）
        /// </summary>
        public void OnBackToMainMenuClicked()
        {
            ShowMainMenu();
        }
    }
}