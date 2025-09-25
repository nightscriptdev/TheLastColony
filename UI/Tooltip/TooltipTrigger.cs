using UnityEngine;
using UnityEngine.EventSystems;
using UI.Buildings;
using UnityEngine.Serialization;

namespace UI.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea(3, 10)]
        public string constantTooltip = "";
        public bool isShowingTooltip = false;        
        private BuildingButtonUI buildingButtonUI;


        public void RefreshTooltipIfVisible()
        {
            if (isShowingTooltip) RefreshTooltip();
        }
        
        public void RefreshTooltip()
        {
            var str = GetTooltip();
            if (!string.IsNullOrEmpty(str))
            {
                TooltipManager.Instance.ShowTooltip(str);
                isShowingTooltip = true;
                return;
            }
            if (!string.IsNullOrEmpty(constantTooltip))
            {
                TooltipManager.Instance.ShowTooltip(constantTooltip);
                isShowingTooltip = true;
                return;
            }
            isShowingTooltip = false;
            Hide();
        }
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            RefreshTooltip();
        }
        
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Hide();
        }

        public void Hide()
        {
            TooltipManager.Instance.Hide();
            isShowingTooltip = false;
        }
        
        protected virtual void OnDisable()
        {
            Hide();
        }

        public virtual string GetTooltip()
        {
            return null;
        }
    }
}