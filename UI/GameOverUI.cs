using Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 游戏结束UI控制器
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("游戏结束面板")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("统计信息")]
        [SerializeField] private TextMeshProUGUI survivalDaysText;
        [SerializeField] private TextMeshProUGUI finalPopulationText;
        [SerializeField] private TextMeshProUGUI bestRecordText;
        [SerializeField] private TextMeshProUGUI newRecordText; // 新纪录提示

        [Header("按钮")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void OnEnable()
        {
            // 订阅游戏结束事件
            EventManager.OnGameEnd += ShowGameOverScreen;
        }

        private void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnGameEnd -= ShowGameOverScreen;
        }

        private void Start()
        {
            // 绑定按钮事件
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // 初始时隐藏游戏结束面板
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            // 清理按钮事件
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        /// <summary>
        /// 显示游戏结束界面
        /// </summary>
        private void ShowGameOverScreen()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            // 更新统计信息
            UpdateGameOverStats();
        }

        /// <summary>
        /// 更新游戏结束时的统计信息
        /// </summary>
        private void UpdateGameOverStats()
        {
            var gameManager = GameManager.Instance;
            var resourceManager = ResourceManager.Instance;
            
            if (gameManager == null || resourceManager == null)
                return;

            // 显示生存天数
            if (survivalDaysText != null)
            {
                survivalDaysText.text = $"生存了 {gameManager.CurrentDay} 天";
            }

            // 显示最终人口
            if (finalPopulationText != null)
            {
                finalPopulationText.text = $"最终人口: {resourceManager.Population}";
            }

            // 显示最高纪录
            int bestRecord = PlayerPrefs.GetInt("BestRecordDays", 0);
            if (bestRecordText != null)
            {
                bestRecordText.text = $"最高纪录: {bestRecord} 天";
            }

            // 检查是否创造了新纪录
            bool isNewRecord = gameManager.CurrentDay > PlayerPrefs.GetInt("BestRecordDays", 0);
            if (newRecordText != null)
            {
                newRecordText.gameObject.SetActive(isNewRecord);
                if (isNewRecord)
                {
                    newRecordText.text = "🎉 新纪录！";
                    newRecordText.color = Color.yellow;
                }
            }
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        private void OnRestartClicked()
        {
            // 隐藏游戏结束面板
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            // 开始新游戏
            GameManager.Instance?.StartNewGame();
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void OnMainMenuClicked()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }

        /// <summary>
        /// 手动隐藏游戏结束面板（供外部调用）
        /// </summary>
        public void HideGameOverScreen()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }
    }
}