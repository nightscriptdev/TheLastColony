using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuPanel;

        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [SerializeField] private Button pauseToggleButton;

        private void OnEnable()
        {
            EventManager.OnGamePause += ShowPauseMenu;
            EventManager.OnGameResume += HidePauseMenu;
        }

        private void OnDisable()
        {
            EventManager.OnGamePause -= ShowPauseMenu;
            EventManager.OnGameResume -= HidePauseMenu;
        }

        private void Start()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (pauseToggleButton != null)
                pauseToggleButton.onClick.AddListener(OnPauseToggleClicked);

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            
        }

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

        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        private void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        private void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        private void OnMainMenuClicked()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance?.QuitGame();
        }
    }
}