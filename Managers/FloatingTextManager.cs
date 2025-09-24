using UnityEngine;
using Core;
using UI;

namespace Managers
{
    public class FloatingTextManager : MonoSingleton<FloatingTextManager>
    {
        [SerializeField] private FloatingText floatingTextPrefab;
        [SerializeField] private Canvas worldCanvas;
        
        public void ShowResourceProduction(string text, Vector3 worldPosition)
        {
            var ft = PoolingManager.Instance.GeFloatingText(floatingTextPrefab);
            ft.transform.SetParent(worldCanvas.transform, false);
            ft.ShowResourceProduction(text, worldPosition);
        }
        
        public void ShowDamage(int damage, Vector3 worldPosition, bool isCritical = false)
        {
            var ft = PoolingManager.Instance.GeFloatingText(floatingTextPrefab);
            ft.transform.SetParent(worldCanvas.transform, false);
            ft.ShowDamage(damage, worldPosition, isCritical);
        }
    }
}