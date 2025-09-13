using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 游戏速度控制UI组件 - 作为HUD始终显示
    /// </summary>
    public class SpeedControlUI : MonoBehaviour
    {
        [Header("游戏速度控制按钮")]
        [SerializeField] private Button[] speedButtonArray;

        [Header("按钮视觉效果")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        private void OnEnable()
        {
            // 订阅时间倍速变化事件，同步按钮状态
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
        
        /// <summary>
        /// 更新按钮视觉状态
        /// </summary>
        private void UpdateSelectedButton(int selectedIndex)
        {
            for (var i = 0; i < speedButtonArray.Length; i++)
            {
                var colors = speedButtonArray[i].colors;
                
                if(i == selectedIndex)
                    colors.normalColor = selectedColor;
                else
                    colors.normalColor = normalColor;
                
                speedButtonArray[i].colors = colors;
            }
        }
        
        /// <summary>
        /// 游戏速度按钮回调
        /// </summary>
        private void OnSpeedButtonClicked(int speedIndex)
        {
            TimeManager.Instance?.SetTimeScale(speedIndex);
        }

        /// <summary>
        /// 时间倍速改变时的回调
        /// </summary>
        private void OnTimeScaleChanged(float newTimeScale)
        {
            UpdateSelectedButton((int)newTimeScale);
        }
    }
}