using System;
using Components.Buildings;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 输入管理器 - 处理游戏中的键盘和鼠标输入
    /// 统一管理输入逻辑，避免在各个脚本中分散处理
    /// </summary>
    public class InputManager : MonoSingleton<InputManager>
    {
        [Header("输入设置")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode speedUpKey = KeyCode.Space;

        // 输入状态
        private bool inputEnabled = true;


        private void Update()
        {
            if (!inputEnabled || GameManager.Instance == null)
                return;

            HandleGameplayInput();
        }

        
        /// <summary>
        /// 处理游戏进行中的输入
        /// </summary>
        private void HandleGameplayInput()
        {
            var gameManager = GameManager.Instance;
            
            // 暂停/继续游戏
            if (Input.GetKeyDown(pauseKey))
            {
                if (gameManager.IsPlaying)
                {
                    gameManager.PauseGame();
                }
                else if (gameManager.IsPaused)
                {
                    gameManager.ResumeGame();
                }
            }

            // 游戏速度控制（数字键1-3）
            if (gameManager.IsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    TimeManager.Instance?.SetTimeScale(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    TimeManager.Instance?.SetTimeScale(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    TimeManager.Instance?.SetTimeScale(2);
                }
            }
        }

        /// <summary>
        /// 启用输入处理
        /// </summary>
        public void EnableInput()
        {
            inputEnabled = true;
        }

        /// <summary>
        /// 禁用输入处理
        /// </summary>
        public void DisableInput()
        {
            inputEnabled = false;
        }

        /// <summary>
        /// 检查输入是否启用
        /// </summary>
        public bool IsInputEnabled => inputEnabled;
    }
}