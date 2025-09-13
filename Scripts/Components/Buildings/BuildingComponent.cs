using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Components.Enemies;
using Data.Buildings;
using Core;
using Core.Grid;

namespace Components.Buildings
{
    /// <summary>
    /// 建筑基础组件 - 所有建筑的通用行为
    /// </summary>
    public class BuildingComponent : MonoBehaviour
    {
        [Header("建筑数据")]
        [SerializeField] private BuildingData buildingData;
        
        [Header("建造状态")]
        [SerializeField] private bool isBuilt = false;
        [Header("视觉效果")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject buildingCompleteEffect;
        [SerializeField] private GameObject destructionEffect;
        [SerializeField] private float buildingAlpha = 0.5f;
        public Action<BuildingComponent> OnBuildingCompleted;
        public Action<BuildingComponent> OnBuildingDestroyed;

        public int LevelIndex = 0;

        public Dictionary<Vector2Int, EnemyComponent> attackSlots;
        /*public class AttackSlot
        {
            public Vector2Int gridPos;
            public EnemyComponent Occupant; // 正在使用的敌人（null = 空闲）

            public AttackSlot(Vector2Int gridPos)
            {
                this.gridPos = gridPos;
            }
        }*/
        
        // 组件引用
        private HealthComponent _healthComponent;
        private HealthBarUI _healthBarUI;
        // 属性
        public BuildingData Data => buildingData;
        public bool IsBuilt => isBuilt;
        public bool CanUpgrade => isBuilt && buildingData.CanUpgradeTo(LevelIndex + 1);
        
        // Shader 属性的 ID，比使用字符串更高效
        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        
        public Vector2Int GridPosition { get; private set; }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                foreach (var keyValuePair in attackSlots)
                {
                    Debug.LogError(keyValuePair);
                }
            }
        }

        protected virtual void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
            _healthBarUI = GetComponent<HealthBarUI>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
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

            _healthComponent.SetMaxHP(data.LevelDatas[LevelIndex].MaxHP);
            
            StartBuilding();
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
                // 累积血量增长
                accumulatedHp += buildingData.HpIncreaseRate * Time.deltaTime;
            
                // 当累积值大于等于1时，增加血量
                if (accumulatedHp >= 1f)
                {
                    int hpToAdd = Mathf.FloorToInt(accumulatedHp);
                    _healthComponent.SetCurrentHP(_healthComponent.CurrentHP + hpToAdd);
                    accumulatedHp -= hpToAdd; // 减去已经添加的部分
                }
                else
                {
                    // 等到白天再继续
                    yield return new WaitUntil(() => TimeManager.Instance.IsDay);
                }
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
                Instantiate(buildingCompleteEffect, transform.position, Quaternion.identity);
            }

            OnBuildingCompleted?.Invoke(this);
            Debug.Log($"{buildingData.BuildingName} 建造完成！");
        }
        /// <summary>
        /// 升级建筑
        /// </summary>
        public virtual void UpgradeBuilding()
        {
            if (!CanUpgrade) return;
            //var newData = BuildingManager.Instance.GetBuildingData(buildingData.upgradeToType);
            //if (newData == null) return;
            isBuilt = false;

            // 更新数据
            //buildingData = newData;
            //_healthComponent.SetMaxHP(newData.maxHP);

            //spriteRenderer.sprite = newData.icon;

            // 开始升级过程
            StartCoroutine(BuildingAndHealProcess());
        }
        public virtual void DemolishBuilding()
        {
            OnBuildingDeath();
        }

        /// <summary>
        /// 建筑死亡处理
        /// </summary>
        protected virtual void OnBuildingDeath()
        {
            if (isBuilt)
            {
                OnBuildingDestroyed?.Invoke(this);

                if (destructionEffect != null)
                {
                    Destroy(Instantiate(destructionEffect, transform.position, Quaternion.identity), 0.667f); 
                }
            }

            Destroy(gameObject);
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
        public virtual string GetInfoText()
        {
            string info = $"{buildingData.BuildingName}\n";
            info += $"HP: {_healthComponent.CurrentHP}/{_healthComponent.MaxHP}\n";
            var levelData = buildingData.LevelDatas[LevelIndex];

            if (buildingData.IsHousing)
                info += $"人口上限: +{levelData.PopulationCapacity}\n";

            if (buildingData.IsProduction)
                info += $"生产: {levelData.BaseProduction} {buildingData.ResourceType}\n";

            if (CanUpgrade)
                info += $"升级花费: {levelData.UpgradeCost}金币";

            return info;
        }

        /// <summary>
        /// 检查是否可以升级（满血且有升级路径）
        /// </summary>
        public bool CanPerformUpgrade()
        {
            return CanUpgrade && _healthComponent.IsFullHealth;
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
            yield return new WaitForSeconds(0.1f);

            // 恢复正常
            spriteRenderer.material.SetFloat(FlashAmountID, 0f);
        }
        
        // <summary>
        /// 获取当前可用槽位数量
        /// </summary>
        public int GetAvailableSlotCount()
        {
            int count = 0;
            foreach (var slot in attackSlots)
            {
                if (slot.Value == null)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 打印当前槽位占用情况（用于调试）
        /// </summary>
        public void DebugSlotStatus()
        {
            Debug.Log($"建筑 {name} 槽位状态：");
            foreach (var slot in attackSlots)
            {
                string enemyName = slot.Value != null ? slot.Value.name : "空闲";
                Debug.Log($"  槽位 {slot.Key}: {enemyName}");
            }
            Debug.Log($"  总槽位: {attackSlots.Count}, 可用槽位: {GetAvailableSlotCount()}");
        }

        // 在BuildingComponent的Gizmos绘制中添加槽位可视化
        private void OnDrawGizmos()
        {
            if (attackSlots == null || attackSlots.Count == 0) return;
    
            foreach (var slot in attackSlots)
            {
                Vector3 slotWorldPos = GridManager.Instance.GridToWorldCenter(slot.Key.x, slot.Key.y);
        
                if (slot.Value == null)
                {
                    // 空闲槽位用绿色显示
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(slotWorldPos, Vector3.one * 0.8f);
                }
                else
                {
                    // 被占用的槽位用红色显示
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(slotWorldPos, Vector3.one * 0.6f);
            
                    // 绘制到占用敌人的连线
                    if (slot.Value != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(slotWorldPos, slot.Value.transform.position);
                    }
                }
        
                // 绘制从建筑到槽位的连线
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, slotWorldPos);
            }
        }
    }
}