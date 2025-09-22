using UnityEngine;
using UnityEngine.EventSystems;
using UI.Buildings;

namespace UI.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea(3, 10)]
        public string customTooltip = "";
        
        private BuildingButtonUI buildingButtonUI;

        public void RefreshTooltip()
        {
            if (!string.IsNullOrEmpty(customTooltip))
                TooltipManager.Instance.Show(customTooltip);
            else
                TooltipManager.Instance.Show(GetTooltip());
        }
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            RefreshTooltip();
        }
        
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }

        protected virtual void OnDisable()
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
        }

        public virtual string GetTooltip()
        {
            return null;
        }
    }
}