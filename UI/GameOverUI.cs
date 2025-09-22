using Components.Enemies;
using Core;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;

        [SerializeField] private TextMeshProUGUI survivalDaysText;
        [SerializeField] private TextMeshProUGUI bestRecordText;
        [SerializeField] private TextMeshProUGUI newRecordText;
        [SerializeField] private TextMeshProUGUI killsText;
        
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void OnEnable()
        {
            EventManager.OnGameEnd += ShowGameOverScreen;
        }

        private void OnDisable()
        {
            EventManager.OnGameEnd -= ShowGameOverScreen;
        }

        private void Start()
        {
            restartButton.onClick.AddListener(OnRestartClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            gameOverPanel.SetActive(false);
        }

        private void ShowGameOverScreen()
        {
            gameOverPanel.SetActive(true);

            UpdateGameOverStats();
        }

        private void UpdateGameOverStats()
        {
            var gameManager = GameManager.Instance;
            
            survivalDaysText.text = LocalizationManager.Instance.GetUIText("ui.survival_days", gameManager.CurrentDay);

            bestRecordText.text = LocalizationManager.Instance.GetUIText("ui.best_record", gameManager.BestRecordDays);

            newRecordText.gameObject.SetActive(gameManager.CurrentDay > gameManager.BestRecordDays);
            
            killsText.text = LocalizationManager.Instance.GetUIText("ui.kills", gameManager.KillCount);
        }

        private void OnRestartClicked()
        {
            gameOverPanel.SetActive(false);

            GameManager.Instance.StartNewGame();
        }

        private void OnMainMenuClicked()
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}