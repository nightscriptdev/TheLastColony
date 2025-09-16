using UnityEngine;
using System.Collections.Generic;
using Data.Buildings;
using Components.Buildings;
using Components.Enemies;
using Core;
using Core.Grid;
using Enums;
using UnityEngine.EventSystems;

namespace Managers
{
    /// <summary>
    /// 建筑管理器 - 管理所有建筑的建造、升级、拆除等操作
    /// 使用网格系统管理建筑位置，处理建筑与资源系统的交互
    /// </summary>
    public class BuildingManager : MonoSingleton<BuildingManager>
    {
        [Header("建筑数据")]
        [SerializeField] private BuildingDatabase buildingDatabase;
        
        [Header("建造设置")]
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        [SerializeField] private Transform buildingsParent;
        
        // 建筑列表
        private List<BuildingComponent> allBuildings = new List<BuildingComponent>();
        
        // 建造模式
        private bool isBuildingMode = false;
        private BuildingData currentBuildingData;
        private GameObject buildingPreview;
        private SpriteRenderer buildingPreviewSpriteRenderer;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            EventManager.OnGameStart += OnGameStart;
            EventManager.OnBuildingButtonClick += StartBuildingMode;
        }

        private void OnDisable()
        {
            EventManager.OnGameStart -= OnGameStart;
            EventManager.OnBuildingButtonClick -= StartBuildingMode;
        }

        private void Update()
        {
            if (isBuildingMode)
            {
                UpdateBuildingMode();
            }
        }

        /// <summary>
        /// 游戏开始时的初始化
        /// </summary>
        private void OnGameStart()
        {
            // 初始化开局建筑
            //InitializeStartingBuildings();
        }

        /// <summary>
        /// 初始化开局建筑
        /// </summary>
        private void InitializeStartingBuildings()
        {
            BuildingComponent[] existingBuildings = FindObjectsOfType<BuildingComponent>();
            foreach (var building in existingBuildings)
            {
                RegisterBuilding(building);
            }
        }

        /// <summary>
        /// 开始建造模式
        /// </summary>
        public void StartBuildingMode(BuildingType buildingType)
        {
            var buildingData = GetBuildingData(buildingType);
            if (buildingData == null)
            {
                Debug.LogError($"找不到建筑类型 {buildingType} 的数据！");
                return;
            }

            currentBuildingData = buildingData;
            isBuildingMode = true;
            CreateBuildingPreview();
        }

        /// <summary>
        /// 结束建造模式
        /// </summary>
        public void EndBuildingMode()
        {
            isBuildingMode = false;
            currentBuildingData = null;
            
            if (buildingPreview != null)
            {
                Destroy(buildingPreview);
                buildingPreview = null;
            }
        }

        /// <summary>
        /// 创建建筑预览
        /// </summary>
        private void CreateBuildingPreview()
        {
            if (currentBuildingData.Prefab != null)
            {
                buildingPreview = new GameObject("BuildingPreview");
                
                buildingPreviewSpriteRenderer = buildingPreview.AddComponent<SpriteRenderer>();
                buildingPreviewSpriteRenderer.sortingLayerName = "BuildingPreview";
                buildingPreviewSpriteRenderer.sprite = currentBuildingData.LevelDatas[0].sprite; 
            }
        }

        /// <summary>
        /// 更新建造模式
        /// </summary>
        private void UpdateBuildingMode()
        {
            var gridManager = GridManager.Instance;
            Vector2Int gridPos = gridManager.WorldToGrid(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            
            // 更新预览位置
            if (buildingPreview != null)
            {
                buildingPreview.transform.position =  GridManager.Instance.GridToWorldCenter(gridPos.x, gridPos.y);
                
                // 根据是否可建造改变颜色
                bool canBuild = gridManager.CanBuildAt(buildingPreview.transform.position) && ResourceManager.Instance.HasEnoughGold(currentBuildingData.BuildCost);
                
                var color = buildingPreviewSpriteRenderer.color;
                if (canBuild)
                {
                    color = new Color(1f, 1f, 1f, 0.5f);
                    
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) // 左键建造
                        BuildAt(buildingPreview.transform.position, gridPos.x, gridPos.y);

                }
                else
                {
                    color = new Color(1f, 0f, 0f, 0.5f); // 红色半透明
                }
                buildingPreviewSpriteRenderer.color = color;
            }
            
            if (Input.GetMouseButtonDown(1)) // 右键取消
            {
                EndBuildingMode();
            }
        }

        /// <summary>
        /// 尝试在指定位置建造
        /// </summary>
        public void BuildAt(Vector2 worldPos, int gridX, int gridY)
        {
            // 创建建筑
            var buildingObj = Instantiate(currentBuildingData.Prefab, worldPos, Quaternion.identity, buildingsParent);
            var buildingComponent = buildingObj.GetComponent<BuildingComponent>();
            
            if (buildingComponent != null)
            {
                // 初始化建筑
                buildingComponent.Initialize(currentBuildingData, new Vector2Int(gridX, gridY));
                
                // 注册建筑
                RegisterBuilding(buildingComponent);
                Debug.Log($"建造 {currentBuildingData.BuildingName} 在位置 ({gridX}, {gridY})");
            }
        }

        /// <summary>
        /// 注册建筑到管理系统
        /// </summary>
        public void RegisterBuilding(BuildingComponent building)
        {
            if (building == null) return;

            allBuildings.Add(building);
            
            // 订阅事件
            building.OnBuildingCompleted += OnBuildingComplete;
            building.OnBuildingDestroyed += OnBuildingDestroyed;

            EventManager. OnBuildingPlaced?.Invoke(building);

            var neighbours = GridManager.Instance.GetWalkableNeighbors(building.GridPosition.x, building.GridPosition.y);
            building.attackSlots = new Dictionary<Vector2Int, EnemyComponent>(neighbours.Count);
            for (var i = 0; i < neighbours.Count; i++)
            {
                building.attackSlots.Add(neighbours[i], null);
            }
        }

        /// <summary>
        /// 建筑完成回调
        /// </summary>
        private void OnBuildingComplete(BuildingComponent building)
        {
        }

        /// <summary>
        /// 建筑被摧毁回调
        /// </summary>
        private void OnBuildingDestroyed(BuildingComponent building)
        {
            foreach (var item in allBuildings)
            {
                if(GridManager.IsNeighbor(item.GridPosition, building.GridPosition))
                    item.attackSlots.TryAdd(building.GridPosition, null);
            }

            // 从列表移除
            allBuildings.Remove(building);
        }

        /// <summary>
        /// 升级建筑
        /// </summary>
        public bool UpgradeBuilding(BuildingComponent building)
        {
            if (!building.HasNextLevel || !building.IsUpgradeable)
            {
                Debug.Log("无法升级：建筑未满血或已是最高等级。");
                return false;
            }

            // 从建筑当前等级的数据中获取升级到下一级所需的费用
            int upgradeCost = building.Data.LevelDatas[building.LevelIndex].UpgradeCost;

            if (!ResourceManager.Instance.SpendGold(upgradeCost))
            {
                Debug.Log("金币不足，无法升级。");
                return false;
            }

            building.UpgradeBuilding();
            return true;
        }

        /// <summary>
        /// 拆除建筑
        /// </summary>
        public void DemolishBuilding(BuildingComponent building)
        {
            building.DemolishBuilding();
        }

        /// <summary>
        /// 获取建筑数据
        /// </summary>
        public BuildingData GetBuildingData(BuildingType buildingType)
        {
            return buildingDatabase?.GetBuildingData(buildingType);
        }

        public bool IsBuildingMode => isBuildingMode;
        public List<BuildingComponent> AllBuildings => allBuildings;
    }
}