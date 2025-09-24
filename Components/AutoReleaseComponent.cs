using Core;
using UnityEngine;

namespace Components
{
    public class AutoReleaseComponent : MonoBehaviour
    {
        public float duration = 0;

        private void OnEnable()
        {
            if (duration > 0f)
                Invoke(nameof(Release), duration);
        }
        
        private void OnDisable()
        {
            CancelInvoke();
        }
        
        public void Release()
        {
            CancelInvoke();
            PoolingManager.Instance.Release(gameObject);
        }
    }
}