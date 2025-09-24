using TMPro;
using UnityEngine;
using Core;
using UnityEngine.UI;

namespace UI
{
    public class TooltipManager : MonoSingleton<TooltipManager>
    {
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
            if (string.IsNullOrEmpty(content))
            {
                Hide();
                return;
            }
            ShowTooltip(content);
        }

        public void Hide()
        {
            if (tooltip != null && tooltip.activeSelf)
            {
                tooltip.SetActive(false);
            }
        }
        

        private void ShowTooltip(string content)
        {
            if (tooltipText != null)
            {
                tooltipText.text = content;
                tooltip.SetActive(true);

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

            Vector2 offset = new Vector2(pivot.x == 1 ? -padding.x : padding.x,
                pivot.y == 0 ? padding.y : -padding.y);

            tooltipRectTransform.position = mousePosition + offset;
        }
    }
}