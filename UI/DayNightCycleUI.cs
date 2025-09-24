using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DayNightCycleUI : MonoBehaviour
    {
        public Image dayImage;
        public Image nightImage;

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
            if (progress < 0.5f)
            {
                SetForeground(dayImage); // 白天图片在前
                // fillAmount 从 0.5 增长到 1.0
                dayImage.fillAmount = 0.5f + progress;
            }
            else
            {
                SetForeground(nightImage); // 黑夜图片在前
                // fillAmount 从 0.0 增长到 0.5
                nightImage.fillAmount = progress - 0.5f;
            }
        }
        
        private void UpdateNightProgressUI(float progress)
        {
            if (progress < 0.5f) 
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

        private void SetForeground(Image foregroundImage)
        {
            foregroundImage.transform.SetAsLastSibling();
        }
    }
}