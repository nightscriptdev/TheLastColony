using Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button creditsCloseButton;
        [SerializeField] private Button settingsButton;

        [SerializeField] private TextMeshProUGUI bestRecordText;

        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
            
            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(OnQuitGameClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(()=> creditsPanel.SetActive(true));
            
            if (creditsCloseButton != null)
                creditsCloseButton.onClick.AddListener(()=> creditsPanel.SetActive(false));

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

        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }
    }
}