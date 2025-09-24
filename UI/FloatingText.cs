using System.Collections;
using Core;
using UnityEngine;
using TMPro;

namespace UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [SerializeField] private float scaleSpeed = 2.5f;
        [SerializeField] private float fadeSpeed = 2.5f;
        
        private Coroutine animationCoroutine;
        
        private void Awake()
        {
            if (textComponent == null)
                textComponent = GetComponentInChildren<TextMeshProUGUI>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        public void Show(string text, Vector3 worldPosition, Color color, float moveSpeed = 0)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            textComponent.text = text;
            textComponent.color = color;
            
            transform.position = worldPosition;
            
            animationCoroutine = StartCoroutine(FloatAnimation(moveSpeed));
        }
        
        public void ShowResourceProduction(string text, Vector3 worldPosition)
        {
            Show(text, worldPosition, Color.white, 50);
        }
        
        public void ShowDamage(int damage, Vector3 worldPosition, bool isCritical = false)
        {
            string text = damage.ToString();
            Color color = isCritical ? Color.yellow : Color.red;
            
            // 添加随机偏移
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.45f, 0.45f),
                Random.Range(-0.45f, 0.45f),
                0
            );
            
            Show(text, worldPosition + randomOffset, color);
        }
        
        private IEnumerator FloatAnimation(float moveSpeed)
        {
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.zero;
            
            while (transform.localScale.x < 1)
            {
                transform.localScale += new Vector3(scaleSpeed * 3, scaleSpeed * 3, 0) * Time.deltaTime;
                yield return null;
            }

            if(moveSpeed > 0) yield return new WaitForSeconds(0.5f);
            while (transform.localScale.x > 0)
            {
                canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
                transform.localScale -= new Vector3(scaleSpeed, scaleSpeed, 0) * Time.deltaTime;
                transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0) * Time.deltaTime, Space.World);
                yield return null;
            }
            
            animationCoroutine = null;
            PoolingManager.Instance.ReleaseFloatingText(this);
        }
        
        public void StopAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
        }
        
        private void OnDisable()
        {
            StopAnimation();
        }
    }
}