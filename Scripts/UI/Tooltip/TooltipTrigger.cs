using UnityEngine;
using UnityEngine.EventSystems;
using UI.Buildings;

namespace UI.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea(3, 10)]
        [SerializeField] protected string staticTooltip = "";
        
        private BuildingButtonUI buildingButtonUI;
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(staticTooltip))
                TooltipManager.Instance.Show(staticTooltip);
            else
                TooltipManager.Instance.Show(GetTooltip());
        }
        
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.Hide();
        }

        protected virtual void OnDisable()
        {
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.Hide();
        }

        public virtual string GetTooltip()
        {
            return null;
        }
    }
}