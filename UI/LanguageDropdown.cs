using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LanguageDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        private void Start()
        {
            dropdown.onValueChanged.AddListener((index) => LocalizationManager.Instance.SetLanguage(index));
            
            dropdown.value = LocalizationManager.Instance.GetLanguageIndex();
        }
    }
}