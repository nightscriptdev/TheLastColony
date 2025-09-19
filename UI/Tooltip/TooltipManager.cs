using TMPro;
using UnityEngine;
using System.Collections;
using Core;
using UnityEngine.UI;

namespace UI
{
    public class TooltipManager : MonoSingleton<TooltipManager>
    {
        [Header("UI组件")]
        [SerializeField] private GameObject tooltip;
        [SerializeField] private TextMeshProUGUI tooltipText;
        
        [Header("设置")]
        [Tooltip("鼠标悬停多久后显示（秒）")]
        [SerializeField] private float showDelay = 0.5f;
        [Tooltip("提示框与鼠标光标的间距")]
        [SerializeField] private Vector2 padding = new Vector2(10f, 10f);

        private RectTransform tooltipRectTransform;
        private Coroutine showCoroutine;
        
        protected override void Awake()
        {
            base.Awake();
            tooltipRectTransform = tooltip.GetComponent<RectTransform>();
            tooltip.SetActive(false);
        }

        private void Update()
        {
            if (tooltip != null && tooltip.activeSelf)
            {
                PositionTooltip();
            }
        }
        
        public void Show(string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
            }
            if(showDelay >0)
                showCoroutine = StartCoroutine(ShowTooltipAfterDelay(content, showDelay));
            else
                ShowTooltip(content);
        }

        public void Hide()
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
            if (tooltip != null && tooltip.activeSelf)
            {
                tooltip.SetActive(false);
            }
        }
        
        private IEnumerator ShowTooltipAfterDelay(string content, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ShowTooltip(content);
            showCoroutine = null;
        }

        private void ShowTooltip(string content)
        {
            if (tooltipText != null)
            {
                tooltipText.text = content;
                tooltip.SetActive(true);

                // 在显示的第一帧就强制更新布局和位置，避免闪烁
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
                PositionTooltip();
            }
        }
        
        private void PositionTooltip()
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 tooltipSize = tooltipRectTransform.sizeDelta;

            Vector2 pivot = new Vector2(0f, 1f); // 默认左上角

            // 如果鼠标在屏幕右侧，右对齐
            if (mousePosition.x + tooltipSize.x + padding.x > Screen.width)
                pivot.x = 1f;
    
            // 如果鼠标在屏幕下方，底对齐
            if (mousePosition.y - tooltipSize.y - padding.y < 0)
                pivot.y = 0f;

            tooltipRectTransform.pivot = pivot;

            // 计算偏移
            Vector2 offset = new Vector2(pivot.x == 1 ? -padding.x : padding.x,
                pivot.y == 0 ? padding.y : -padding.y);

            tooltipRectTransform.position = mousePosition + offset;
        }
    }
}