using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LanguageDropdown : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        private void Start()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener((index) => LocalizationManager.Instance.SetLanguage(index));
        }
    }
}