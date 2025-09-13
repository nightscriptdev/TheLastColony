using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DayNightCycleUI : MonoBehaviour
    {
        [Header("UI 组件")] 
        [Tooltip("代表白天的图片")] public Image dayImage;
        [Tooltip("代表黑夜的图片")] public Image nightImage;

        private void OnEnable()
        {
            EventManager.OnDayTick += UpdateDayProgressUI;
            EventManager.OnNightTick += UpdateNightProgressUI;
        }

        private void OnDisable()
        {
            EventManager.OnDayTick -= UpdateDayProgressUI;
            EventManager.OnNightTick -= UpdateNightProgressUI;
        }

        private void UpdateDayProgressUI(float progress)
        {
            if (progress < 0.5f) // 上午 (0% -> 50% of day)
            {
                SetForeground(dayImage); // 白天图片在前
                // fillAmount 从 0.5 增长到 1.0
                dayImage.fillAmount = 0.5f + progress;
            }
            else // 下午 (50% -> 100% of day)
            {
                SetForeground(nightImage); // 黑夜图片在前
                // fillAmount 从 0.0 增长到 0.5
                nightImage.fillAmount = progress - 0.5f;
            }
        }
        
        private void UpdateNightProgressUI(float progress)
        {
            if (progress < 0.5f) // 前半夜 (0% -> 50% of night)
            {
                SetForeground(nightImage); // 黑夜图片在前
                // fillAmount 从 0.5 增长到 1.0
                nightImage.fillAmount = 0.5f + progress;
            }
            else // 后半夜 (50% -> 100% of night)
            {
                SetForeground(dayImage); // 白天图片在前
                // fillAmount 从 0.0 增长到 0.5
                dayImage.fillAmount = progress - 0.5f;
            }
        }

        /// <summary>
        /// 辅助函数，将指定的Image设置为前景（渲染在最上层）
        /// </summary>
        private void SetForeground(Image foregroundImage)
        {
            foregroundImage.transform.SetAsLastSibling();
        }
    }
}