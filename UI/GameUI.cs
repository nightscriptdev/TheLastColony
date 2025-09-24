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
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameplayUI;
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private GameObject gameOverUI;
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
            EventManager.OnGameStart += OnGameStart;
            EventManager.OnDayStart += UpdateDayUI;
            EventManager.OnNightStart += UpdateNightTimeUI;
            EventManager.OnEnemyDeath += OnEnemyDeath;
            
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        private void OnDisable()
        {
            EventManager.OnGameStart -= OnGameStart;
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
        
        private void OnGameStart()
        {
            if (resourceUI != null)
                resourceUI.RefreshAllUI();
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
    }
}