using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpeedControlUI : MonoBehaviour
    {
        [SerializeField] private Button[] speedButtonArray;

        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        private void OnEnable()
        {
            EventManager.OnTimeScaleChanged += OnTimeScaleChanged;
        }

        private void OnDisable()
        {
            EventManager.OnTimeScaleChanged -= OnTimeScaleChanged;
        }

        
        private void Start()
        {
            for (var i = 0; i < speedButtonArray.Length; i++)
            {
                var index = i;
                speedButtonArray[i].onClick.AddListener(() => OnSpeedButtonClicked(index));
            }
        }
        
        private void UpdateSelectedButton(int selectedIndex)
        {
            for (var i = 0; i < speedButtonArray.Length; i++)
            {
                var colors = speedButtonArray[i].colors;

                if (i == selectedIndex)
                {
                    colors.normalColor = selectedColor;
                    colors.highlightedColor = selectedColor;
                }
                else
                {
                    colors.normalColor = normalColor;
                    colors.highlightedColor = normalColor;
                }
                
                speedButtonArray[i].colors = colors;
            }
        }
        
        private void OnSpeedButtonClicked(int speedIndex)
        {
            TimeManager.Instance?.SetTimeScale(speedIndex+1);
        }

        private void OnTimeScaleChanged(float newTimeScale)
        {
            UpdateSelectedButton((int)newTimeScale-1);
        }
    }
}