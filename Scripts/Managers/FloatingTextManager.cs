using UnityEngine;
using Core;
using UI;

namespace Managers
{
    /// <summary>
    /// 飘字管理器 - 管理所有飘字效果
    /// </summary>
    public class FloatingTextManager : MonoSingleton<FloatingTextManager>
    {
        [Header("预制体")]
        [SerializeField] private FloatingText floatingTextPrefab;
        
        [Header("画布设置")]
        [SerializeField] private Canvas worldCanvas;
        
        [Header("对象池设置")]
        [SerializeField] private int poolSize = 20;
        [SerializeField] private int maxPoolSize = 50;
        
        [Header("伤害数字设置")]
        [SerializeField] private bool showDamageNumbers = true;
        
        private ObjectPool<FloatingText> textPool;
        private const string POOL_NAME = "FloatingText";
        
        protected override void Awake()
        {
            base.Awake();
            InitializeCanvas();
            InitializePool();
        }
        
        private void OnEnable()
        {
            EventManager.OnFloatingTextComplete += OnFloatingTextComplete;
             //EventManager.OnDamageNumbersSettingChanged += OnDamageNumbersSettingChanged;
        }
        
        private void OnDisable()
        {
            EventManager.OnFloatingTextComplete -= OnFloatingTextComplete;
            // EventManager.OnDamageNumbersSettingChanged -= OnDamageNumbersSettingChanged;
        }
        
        /// <summary>
        /// 初始化画布
        /// </summary>
        private void InitializeCanvas()
        {
            if (worldCanvas == null)
            {
                // 创建世界空间画布
                GameObject canvasObj = new GameObject("FloatingTextCanvas");
                canvasObj.transform.SetParent(transform);
                
                worldCanvas = canvasObj.AddComponent<Canvas>();
                worldCanvas.renderMode = RenderMode.WorldSpace;
                worldCanvas.sortingLayerName = "UI";
                worldCanvas.sortingOrder = 100;
                
                // 设置画布大小
                RectTransform rt = canvasObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100, 100);
                rt.localScale = Vector3.one * 0.01f; // 缩放以适应世界空间
            }
        }
        
        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitializePool()
        {
            if (floatingTextPrefab == null)
            {
                Debug.LogError("飘字预制体未设置！");
                return;
            }
            
            // 使用PoolingManager创建对象池
            if (PoolingManager.Instance != null)
            {
                textPool = PoolingManager.Instance.CreatePool(
                    POOL_NAME, 
                    floatingTextPrefab,
                    poolSize,
                    maxPoolSize
                );
            }
            else
            {
                // 如果PoolingManager不存在，创建本地对象池
                CreateLocalPool();
            }
        }
        
        /// <summary>
        /// 创建本地对象池（备用方案）
        /// </summary>
        private void CreateLocalPool()
        {
            GameObject poolContainer = new GameObject("FloatingTextPool");
            poolContainer.transform.SetParent(transform);
            
            textPool = new ObjectPool<FloatingText>(
                createFunc: () => {
                    var obj = Instantiate(floatingTextPrefab, worldCanvas.transform);
                    return obj;
                },
                onGet: (text) => {
                    text.gameObject.SetActive(true);
                },
                onRelease: (text) => {
                    text.gameObject.SetActive(false);
                    text.transform.SetParent(worldCanvas.transform);
                },
                defaultCapacity: poolSize,
                maxSize: maxPoolSize
            );
        }
        
        /// <summary>
        /// 显示资源产出飘字
        /// </summary>
        public void ShowResourceProduction(string text, Vector3 worldPosition)
        {
            if (textPool == null) return;
            
            var floatingText = textPool.Get();
            floatingText.ShowResourceProduction(text, worldPosition);
        }
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamage(int damage, Vector3 worldPosition, bool isCritical = false)
        {
            if (!showDamageNumbers || textPool == null) return;
            
            var floatingText = textPool.Get();
            floatingText.ShowDamage(damage, worldPosition, isCritical);
        }
        
        /// <summary>
        /// 显示治疗数字
        /// </summary>
        public void ShowHealing(int amount, Vector3 worldPosition)
        {
            if (textPool == null) return;
            
            var floatingText = textPool.Get();
            string text = $"+{amount}";
            Color color = Color.green;
            floatingText.Show(text, worldPosition, color);
        }
        
        /// <summary>
        /// 显示自定义文本
        /// </summary>
        public void ShowCustomText(string text, Vector3 worldPosition, Color color, float fontSize = 24f)
        {
            if (textPool == null) return;
            
            var floatingText = textPool.Get();
            floatingText.Show(text, worldPosition, color);
        }
        
        /// <summary>
        /// 飘字动画完成回调
        /// </summary>
        private void OnFloatingTextComplete(FloatingText floatingText)
        {
            if (textPool != null && floatingText != null)
            {
                textPool.Release(floatingText);
            }
        }
        
        /// <summary>
        /// 设置是否显示伤害数字
        /// </summary>
        public void SetShowDamageNumbers(bool show)
        {
            showDamageNumbers = show;
            PlayerPrefs.SetInt("ShowDamageNumbers", show ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 加载伤害数字显示设置
        /// </summary>
        public void LoadDamageNumbersSetting()
        {
            showDamageNumbers = PlayerPrefs.GetInt("ShowDamageNumbers", 1) == 1;
        }
        
        /// <summary>
        /// 清理对象池
        /// </summary>
        public void ClearPool()
        {
            textPool?.Clear();
        }
        
        private void OnDestroy()
        {
            ClearPool();
        }
        
        // 调试用
        #if UNITY_EDITOR
        [ContextMenu("测试伤害飘字")]
        private void TestDamageText()
        {
            ShowDamage(25, transform.position + Vector3.up, false);
        }
        #endif
    }
}