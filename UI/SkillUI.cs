using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SkillUI : MonoBehaviour
    {
        [SerializeField] private Transform skillButtonContainer;

        public void Hide()
        {
            skillButtonContainer.gameObject.SetActive(false);
        }
    }
}