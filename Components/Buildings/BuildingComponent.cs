using System;
using UnityEngine;
using System.Collections;
using Data.Buildings;
using Core;
using Interface;
using Managers;

namespace Components.Buildings
{
    /// <summary>
    /// 建筑基础组件 - 所有建筑的通用行为
    /// </summary>
    public class BuildingComponent : MonoBehaviour
    {
        [Header("建筑数据")]
        [SerializeField] private BuildingData buildingData;
        [SerializeField] private AnimationClip animationClip;
        
        [Header("建造状态")]
        [SerializeField] private bool isBuilt = false;
        [Header("视觉效果")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject buildingCompleteEffect;
        [SerializeField] private GameObject destructionEffect;
        [SerializeField] private float buildingAlpha = 0.5f;
        public Action<BuildingComponent> OnBuildingCompleted;
        public Action<BuildingComponent> OnBuildingDestroyed;
        public Action OnBuildingUpgraded;

        public BuildingKey BuildingKey;

        public IInfoProvider InfoProvider;
        
        // 组件引用
        private HealthComponent _healthComponent;
        private HealthBarUI _healthBarUI;
        // 属性
        public BuildingData Data => buildingData;
        public BuildingData.LevelData LevelData => buildingData.LevelDatas[Level-1];
        public bool IsBuilt => isBuilt;
        public bool HasNextLevel => Level < Data.LevelDatas.Length;
        public bool IsUpgradeable => isBuilt && _healthComponent.IsFullHealth;
        
        // Shader 属性的 ID，比使用字符串更高效
        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        
        public Vector2Int GridPosition { get; private set; }
        public int Level { get; private set; } = 1;

        protected virtual void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
            _healthBarUI = GetComponent<HealthBarUI>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            BuildingKey = new BuildingKey(Data.BuildingType, Level);
            InfoProvider = GetComponent<IInfoProvider>();
        }
        protected virtual void OnEnable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnTakeDamage += OnTakeDamage;
                _healthComponent.OnDeath += OnBuildingDeath;
            }
            EventManager.OnDayStart += OnDayStart;
        }
        protected virtual void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDeath -= OnBuildingDeath;
            }
            EventManager.OnDayStart -= OnDayStart;
        }

        /// <summary>
        /// 初始化建筑（用于运行时创建建筑）
        /// </summary>
        public virtual void Initialize(BuildingData data, Vector2Int gridPos)
        {
            buildingData = data;
            GridPosition = gridPos;

            _healthComponent.SetMaxHP(data.LevelDatas[Level-1].MaxHP);
            
            StartBuilding();
        }
        public virtual void InitializePrebuilt(Vector2Int gridPos)
        {
            GridPosition = gridPos;

            _healthComponent.SetMaxHP(Data.LevelDatas[Level-1].MaxHP, true);
            
            isBuilt = true;
            OnBuildingCompleted?.Invoke(this);
        }
        /// <summary>
        /// 开始建造过程
        /// </summary>
        public virtual void StartBuilding()
        {
            if (isBuilt) return;

            // 设置初始HP为0
            _healthComponent.SetCurrentHP(0);

            // 开始建造协程
            StartCoroutine(BuildingAndHealProcess());
        }
        private IEnumerator BuildingAndHealProcess()
        {
            // 设置半透明
            if(!isBuilt)
                SetBuildingAlpha(buildingAlpha);
            float accumulatedHp = 0f;

            while (_healthComponent.CurrentHP < _healthComponent.MaxHP)
            {
                // 仅在白天建造或维修
                if (TimeManager.Instance.IsDay)
                {
                    // 累积血量增长
                    accumulatedHp += buildingData.HpIncreaseRate * Time.deltaTime;
            
                    // 当累积值大于等于1时，增加血量
                    if (accumulatedHp >= 1f)
                    {
                        int hpToAdd = Mathf.FloorToInt(accumulatedHp);
                        _healthComponent.SetCurrentHP(_healthComponent.CurrentHP + hpToAdd);
                        accumulatedHp -= hpToAdd; // 减去已经添加的部分
                    }
                }
                yield return null; // 每帧检查一次
            }
            
            if (!isBuilt)
                CompleteBuilding();
        }
        /// <summary>
        /// 完成建造
        /// </summary>
        protected virtual void CompleteBuilding()
        {
            isBuilt = true;
            // 恢复透明度
            SetBuildingAlpha(1f);

            // 播放建造完成效果
            if (buildingCompleteEffect != null)
            {
                Destroy(Instantiate(buildingCompleteEffect, transform.position + new Vector3(0, 0.45f,0), Quaternion.identity), 0.833f);
            }

            OnBuildingCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 升级建筑
        /// </summary>
        public virtual void UpgradeBuilding()
        {
            // 2. 提升等级并获取新等级的数据
            var newLevelData = buildingData.LevelDatas[Level++];

            // 3. 更新视觉和核心属性
            spriteRenderer.sprite = newLevelData.sprite;

            // 设置新的最大生命值，同时保留当前生命值
            _healthComponent.SetMaxHP(newLevelData.MaxHP, true);

            CompleteBuilding();
            OnBuildingUpgraded?.Invoke();
            //StartCoroutine(BuildingAndHealProcess());
        }

        public virtual void DemolishBuilding()
        {
            // 直接触发死亡逻辑
            _healthComponent.Kill();
        }

        /// <summary>
        /// 建筑死亡处理
        /// </summary>
        protected virtual void OnBuildingDeath()
        {
            if (isBuilt)
            {
                OnBuildingDestroyed?.Invoke(this);
            }

            if (destructionEffect != null)
            {
                Destroy(Instantiate(destructionEffect, transform.position + new Vector3(0, 0.45f, 0), Quaternion.identity), 0.444f); 
            }
            Destroy(gameObject);
            
            EventManager.OnGridRelease?.Invoke(transform.position);
        }
        /// <summary>
        /// 白天开始时的处理（建筑自动恢复）
        /// </summary>
        protected virtual void OnDayStart(int day)
        {
            if (isBuilt && !_healthComponent.IsFullHealth)
            {
                // 白天建筑自动恢复
                StartCoroutine(BuildingAndHealProcess());
            }
        }
        /// <summary>
        /// 设置建筑透明度
        /// </summary>
        private void SetBuildingAlpha(float alpha)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
        /// <summary>
        /// 获取建筑信息文本
        /// </summary>
        public string GetInfoText()
        {
            if (InfoProvider != null)
            {
                return InfoProvider.GetInfoText();
            }
            return LocalizationManager.Instance.GetLocalizedBuildingDescription(Data.BuildingType);
        }

        private void OnTakeDamage(int amount)
        {
            StartCoroutine(DamageFlash());
        }

        IEnumerator DamageFlash()
        {
            // 设置为完全闪白
            spriteRenderer. material.SetFloat(FlashAmountID, 1f);
            
            // 等待指定的持续时间
            yield return new WaitForSeconds(0.15f);

            // 恢复正常
            spriteRenderer.material.SetFloat(FlashAmountID, 0f);
        }
    }
}