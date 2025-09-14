using UnityEngine;
using UnityEngine.EventSystems;
using UI.Buildings;
using UnityEngine.Serialization;

namespace UI.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea(3, 10)]
        [SerializeField] private string customTooltip = "";
        
        private BuildingButtonUI buildingButtonUI;
        
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(customTooltip))
                TooltipManager.Instance.Show(customTooltip);
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