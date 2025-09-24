using System;
using UnityEngine;
using System.Collections;
using Data.Buildings;
using Core;
using Interface;
using Managers;

namespace Components.Buildings
{
    public class BuildingComponent : MonoBehaviour
    {
        [SerializeField] private BuildingData buildingData;
        
        [SerializeField] private bool isBuilt = false;
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject buildingCompleteEffect;
        [SerializeField] private GameObject destructionEffect;
        [SerializeField] private float buildingAlpha = 0.5f;
        public Action<BuildingComponent> OnBuildingCompleted;
        public Action<BuildingComponent> OnBuildingDestroyed;
        public Action OnBuildingUpgraded;

        private IInfoProvider _infoProvider;

        private HealthComponent _healthComponent;
        private HealthBarUI _healthBarUI;
        
        public BuildingData Data => buildingData;
        public BuildingData.LevelData LevelData => buildingData.LevelDatas[Level-1];
        public bool HasNextLevel => Level < Data.LevelDatas.Length;
        public bool IsUpgradeable => isBuilt && _healthComponent.IsFullHealth;
        
        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        public Vector2Int GridPosition { get; private set; }
        public int Level { get; private set; } = 1;

        protected virtual void Awake()
        {
            if (_healthComponent == null)
                _healthComponent = GetComponent<HealthComponent>();
            if (_healthBarUI == null)
                _healthBarUI = GetComponent<HealthBarUI>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            _infoProvider = GetComponent<IInfoProvider>();
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
                _healthComponent.OnTakeDamage -= OnTakeDamage;
                _healthComponent.OnDeath -= OnBuildingDeath;
            }
            EventManager.OnDayStart -= OnDayStart;
        }

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
        
        private void StartBuilding()
        {
            if (isBuilt) return;

            _healthComponent.SetCurrentHP(0);

            StartCoroutine(BuildingAndHealProcess());
        }
        
        private IEnumerator BuildingAndHealProcess()
        {
            if(!isBuilt)
                SetBuildingAlpha(buildingAlpha);
            
            float accumulatedHp = 0f;

            while (_healthComponent.CurrentHP < _healthComponent.MaxHP)
            {
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
                yield return null;
            }
            
            if (!isBuilt)
                CompleteBuilding();
        }

        protected virtual void CompleteBuilding()
        {
            isBuilt = true;
            
            SetBuildingAlpha(1f);

            if (buildingCompleteEffect)
            {
                PoolingManager.Instance.Get(buildingCompleteEffect).transform.SetPositionAndRotation(transform.position+ new Vector3(0, 0.45f,0), Quaternion.identity);
            }

            OnBuildingCompleted?.Invoke(this);
        }
        
        public virtual void UpgradeBuilding()
        {
            var newLevelData = buildingData.LevelDatas[Level++];

            spriteRenderer.sprite = newLevelData.sprite;

            _healthComponent.SetMaxHP(newLevelData.MaxHP, true);

            CompleteBuilding();
            OnBuildingUpgraded?.Invoke();
        }

        public virtual void DemolishBuilding()
        {
            _healthComponent.Kill();
        }

        protected virtual void OnBuildingDeath()
        {
            if (isBuilt)
                OnBuildingDestroyed?.Invoke(this);

            if (destructionEffect)
            {
                PoolingManager.Instance.Get(destructionEffect).transform.SetPositionAndRotation(transform.position + new Vector3(0, 0.45f, 0), Quaternion.identity);
            }
            Destroy(gameObject);
            
            EventManager.OnGridRelease?.Invoke(transform.position);
        }
        
        protected virtual void OnDayStart(int day)
        {
            if (isBuilt && !_healthComponent.IsFullHealth)
            {
                StartCoroutine(BuildingAndHealProcess());
            }
        }
        
        private void SetBuildingAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        
        public string GetInfoText()
        {
            if (_infoProvider != null)
                return _infoProvider.GetInfoText();
            return LocalizationManager.Instance.GetLocalizedBuildingDescription(Data.BuildingType);
        }

        private void OnTakeDamage(int amount)
        {
            StartCoroutine(DamageFlash());
        }

        IEnumerator DamageFlash()
        {
            spriteRenderer. material.SetFloat(FlashAmountID, 1f);
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.material.SetFloat(FlashAmountID, 0f);
        }
    }
}