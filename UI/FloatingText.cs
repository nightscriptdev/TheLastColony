using System.Collections;
using Core;
using UnityEngine;
using TMPro;

namespace UI
{
    /// <summary>
    /// 飘字效果组件
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("动画设置")]
        [SerializeField] private float scaleSpeed = 2.5f;
        [SerializeField] private float fadeSpeed = 2.5f;
        //[SerializeField] private Vector2 randomOffset = new Vector2(0.5f, 0.5f);
        
        private Coroutine animationCoroutine;
        
        private void Awake()
        {
            if (textComponent == null)
                textComponent = GetComponentInChildren<TextMeshProUGUI>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// 显示飘字
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <param name="color">文字颜色</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="onCompleteCallback">完成回调</param>
        public void Show(string text, Vector3 worldPosition, Color color, float moveSpeed = 0)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            // 设置文本
            textComponent.text = text;
            textComponent.color = color;
            
            // 设置位置
            transform.position = worldPosition;
            
            // 开始动画
            animationCoroutine = StartCoroutine(FloatAnimation(moveSpeed));
        }
        
        /// <summary>
        /// 显示资源产出飘字
        /// </summary>
        public void ShowResourceProduction(string text, Vector3 worldPosition)
        {
            Show(text, worldPosition, Color.white, 50);
        }
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamage(int damage, Vector3 worldPosition, bool isCritical = false)
        {
            string text = damage.ToString();
            Color color = isCritical ? Color.yellow : Color.red;
            //float fontSize = isCritical ? 32f : 24f;
            
            // 添加随机偏移
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.45f, 0.45f),
                Random.Range(-0.45f, 0.45f),
                0
            );
            
            Show(text, worldPosition + randomOffset, color);
        }
        
        /// <summary>
        /// 飘字动画协程
        /// </summary>
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
            EventManager.OnFloatingTextComplete?.Invoke(this);
        }
        
        /// <summary>
        /// 停止动画
        /// </summary>
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