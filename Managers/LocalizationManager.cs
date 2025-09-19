using System.Collections;
using Core;
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
                // 使用保存的语言设置
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
            
            Debug.LogWarning($"找不到本地化字符串: {tableReference}/{entryReference}");
            return entryReference;
        }
        
        public string GetLocalizedString(string tableReference, string entryReference, params object[] args)
        {
            string localizedString = GetLocalizedString(tableReference, entryReference);
            try
            {
                return string.Format(localizedString, args);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"格式化本地化字符串失败: {entryReference}, 错误: {e.Message}");
                return localizedString;
            }
        }
        
        public string GetGameText(string key) => GetLocalizedString("GameTexts", key);
        public string GetGameText(string key, params object[] args) => GetLocalizedString("GameTexts", key, args);
    }
}