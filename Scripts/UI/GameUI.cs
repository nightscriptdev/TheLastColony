using Core;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 游戏UI管理器 - 控制游戏进行时的UI显示状态
    /// 根据游戏状态自动显示/隐藏相应的UI面板
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("UI面板引用")]
        [SerializeField] private GameObject gameplayUI;    // 游戏进行时的UI
        [SerializeField] private GameObject pauseMenuUI;   // 暂停菜单
        [SerializeField] private GameObject gameOverUI;    // 游戏结束UI

        [Header("UI组件引用")]
        [SerializeField] private ResourceUI resourceUI;
        [SerializeField] private DayNightCycleUI dayNightUI;

        private void OnEnable()
        {
            // 订阅游戏状态变化事件
            EventManager.OnGameStart += OnGameStart;
            EventManager.OnGameEnd += OnGameEnd;
            EventManager.OnGamePause += OnGamePause;
            EventManager.OnGameResume += OnGameResume;
        }

        private void OnDisable()
        {
            // 取消事件订阅
            EventManager.OnGameStart -= OnGameStart;
            EventManager.OnGameEnd -= OnGameEnd;
            EventManager.OnGamePause -= OnGamePause;
            EventManager.OnGameResume -= OnGameResume;
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

        /// <summary>
        /// 获取当前是否在游戏中
        /// </summary>
        public bool IsInGame => GameManager.Instance?.IsPlaying == true;
    }
}