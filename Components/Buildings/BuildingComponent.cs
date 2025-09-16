using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Components.Enemies;
using Data.Buildings;
using Core;

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
        
        // 组件引用
        private HealthComponent _healthComponent;
        private HealthBarUI _healthBarUI;
        // 属性
        public BuildingData Data => buildingData;
        public bool IsBuilt => isBuilt;
        public bool HasNextLevel => (LevelIndex + 1) < Data.LevelDatas.Length;
        public bool IsUpgradeable => isBuilt && _healthComponent.IsFullHealth;
        
        // Shader 属性的 ID，比使用字符串更高效
        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        
        public Vector2Int GridPosition { get; private set; }

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
                Instantiate(buildingCompleteEffect, transform.position, Quaternion.identity);
            }

            OnBuildingCompleted?.Invoke(this);
            Debug.Log($"{buildingData.BuildingName} 建造/升级完成！");
        }
        
        /// <summary>
        /// 升级建筑
        /// </summary>
        public virtual void UpgradeBuilding()
        {
            // 1. 标记为“未建成”状态，以暂停其功能并触发OnBuildingDestroyed事件
            isBuilt = false;
            OnBuildingDestroyed?.Invoke(this); // 移除旧等级的加成（如人口）

            // 2. 提升等级并获取新等级的数据
            LevelIndex++;
            var newLevelData = buildingData.LevelDatas[LevelIndex];

            // 3. 更新视觉和核心属性
            spriteRenderer.sprite = newLevelData.sprite;

            // 设置新的最大生命值，同时保留当前生命值
            _healthComponent.SetMaxHP(newLevelData.MaxHP);

            // 4. 开始建造/恢复流程，以达到新的最大生命值
            // 这个协程会自动处理白天建造、夜晚暂停，并在完成后调用CompleteBuilding
            StartCoroutine(BuildingAndHealProcess());
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
                Destroy(Instantiate(destructionEffect, transform.position, Quaternion.identity), 0.667f); 
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
            string info = $"<b>{buildingData.BuildingName} (等级 {LevelIndex + 1})</b>\n";
            info += $"生命值: {_healthComponent.CurrentHP}/{_healthComponent.MaxHP}\n";
            var levelData = buildingData.LevelDatas[LevelIndex];

            if (buildingData.IsHousing)
                info += $"提供人口: +{levelData.PopulationCapacity}\n";

            if (buildingData.IsProduction)
                info += $"产出: {levelData.BaseProduction} {buildingData.ResourceType}\n";
            
            if (buildingData.IsTower)
            {
                info += $"伤害: {levelData.MinDamage}-{levelData.MaxDamage}\n";
                info += $"攻速: {levelData.AttackInterval} 秒/次\n";
                info += $"射程: {levelData.AttackRange} 格\n";
            }

            if (HasNextLevel)
                info += $"\n<b>升级需要: {buildingData.LevelDatas[LevelIndex].UpgradeCost} 金币</b>";
            else if (isBuilt)
                info += "\n<b>已达到最高等级</b>";

            return info;
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
        
        // ... (rest of the class remains the same) ...
        public int GetAvailableSlotCount()
        {
            int count = 0;
            if (attackSlots == null) return 0;
            foreach (var slot in attackSlots)
            {
                if (slot.Value == null)
                    count++;
            }
            return count;
        }
        public void DebugSlotStatus()
        {
            Debug.Log($"建筑 {name} 槽位状态：");
            if (attackSlots == null) return;
            foreach (var slot in attackSlots)
            {
                string enemyName = slot.Value != null ? slot.Value.name : "空闲";
                Debug.Log($"  槽位 {slot.Key}: {enemyName}");
            }
            Debug.Log($"  总槽位: {attackSlots.Count}, 可用槽位: {GetAvailableSlotCount()}");
        }
        /*private void OnDrawGizmos()
        {
            if (attackSlots == null || attackSlots.Count == 0) return;
    
            foreach (var slot in attackSlots)
            {
                Vector3 slotWorldPos = GridManager.Instance.GridToWorldCenter(slot.Key.x, slot.Key.y);
        
                if (slot.Value == null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(slotWorldPos, Vector3.one * 0.8f);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(slotWorldPos, Vector3.one * 0.6f);
            
                    if (slot.Value != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(slotWorldPos, slot.Value.transform.position);
                    }
                }
        
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, slotWorldPos);
            }
        }*/
    }
}