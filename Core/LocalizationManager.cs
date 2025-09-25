using System.Collections;
using Core;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Managers
{
    public class LocalizationManager : MonoSingleton<LocalizationManager>
    {
        [SerializeField] private bool useSystemLanguage = true;
        [SerializeField] private int defaultLocaleIndex = 0; // 0=中文, 1=英文
        
        private const string LANGUAGE_PREF_KEY = "SelectedLanguage";
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            StartCoroutine(InitializeLocalizationCoroutine());
        }
        
        private IEnumerator InitializeLocalizationCoroutine()
        {
            // 等待本地化系统初始化
            yield return LocalizationSettings.InitializationOperation;
            
            // 获取保存的语言设置
            int savedLanguage = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY, -1);
            
            if (savedLanguage == -1)
            {
                // 首次启动，根据设置选择语言
                if (useSystemLanguage)
                {
                    SetLanguageBySystemLocale();
                }
                else
                {
                    SetLanguage(defaultLocaleIndex);
                }
            }
            else
            {
                SetLanguage(savedLanguage);
            }
        }
        
        /// <summary>
        /// 根据系统语言设置
        /// </summary>
        private void SetLanguageBySystemLocale()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    SetLanguage(0);
                    break;
                default:
                    SetLanguage(1);
                    break;
            }
        }
        
        public void SetLanguage(int localeIndex)
        {
            if (localeIndex >= LocalizationSettings.AvailableLocales.Locales.Count) return;
            LocalizationSettings.SelectedLocale =  LocalizationSettings.AvailableLocales.Locales[localeIndex];
            
            // 保存设置
            PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, localeIndex);
            PlayerPrefs.Save();
        }

        public int GetLanguageIndex()
        {
            return PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);
        }
        
        public string GetLocalizedString(string tableReference, string entryReference)
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableReference);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(entryReference);
                if (entry != null)
                {
                    return entry.GetLocalizedString();
                }
            }
            return entryReference;
        }
        
        public string GetLocalizedString(string tableReference, string entryReference, params object[] args)
        {
            string localizedString = GetLocalizedString(tableReference, entryReference);
            return string.Format(localizedString, args);
        }
        
        public string GetGameText(string key) => GetLocalizedString("GameTexts", key);
        public string GetUIText(string key) => GetLocalizedString("UITexts", key);
        public string GetGameText(string key, params object[] args) => GetLocalizedString("GameTexts", key, args);
        public string GetUIText(string key, params object[] args) => GetLocalizedString("UITexts", key, args);

        public string GetLocalizedBuildingName(BuildingType buildingType) => GetGameText("building." + buildingType);
        
        public string GetLocalizedBuildingDescription(BuildingType buildingType) => GetGameText("building." + buildingType + ".desc");
        
        public string GetLocalizedSkillName(SkillType buildingType) => GetGameText("skill." + buildingType);
        
        public string GetLocalizedSkillDescription(SkillType buildingType) => GetGameText("skill." + buildingType + ".desc");
    }
}