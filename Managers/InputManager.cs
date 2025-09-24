using UnityEngine;

namespace Core
{
    public class InputManager : MonoSingleton<InputManager>
    {
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private KeyCode speedUpKey = KeyCode.Space;

        private bool inputEnabled = true;


        private void Update()
        {
            if (!inputEnabled || GameManager.Instance == null)
                return;

            HandleGameplayInput();
        }

        
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
    }
}