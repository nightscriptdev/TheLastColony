using UnityEngine;
using System.Collections.Generic;
using Data.Buildings;
using Components.Buildings;
using Core;
using Core.Grid;
using UnityEngine.EventSystems;

namespace Managers
{
    public class BuildingManager : MonoSingleton<BuildingManager>
    {
        [SerializeField] private Transform buildingsParent;
        
        private List<BuildingComponent> allBuildings = new List<BuildingComponent>();
        
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
            EventManager.OnNightStart += OnNightStart;
            EventManager.OnBuildingButtonClick += StartBuildingMode;
        }

        private void OnDisable()
        {
            EventManager.OnGameStart -= OnGameStart;
            EventManager.OnNightStart += OnNightStart;
            EventManager.OnBuildingButtonClick -= StartBuildingMode;
        }

        private void Update()
        {
            if (isBuildingMode)
            {
                UpdateBuildingMode();
            }
        }

        private void OnGameStart()
        {
            // 初始化开局建筑
            InitializeStartingBuildings();
        }

        private void InitializeStartingBuildings()
        {
            foreach (Transform buildingTransform in buildingsParent)
            {
                var buildingComponent = buildingTransform.GetComponent<BuildingComponent>();
                buildingComponent.InitializePrebuilt(GridManager.Instance.WorldToGrid(buildingTransform.position));
                RegisterBuilding(buildingComponent);
            }
        }

        public void StartBuildingMode(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                return;
            }

            currentBuildingData = buildingData;
            isBuildingMode = true;
            CreateBuildingPreview();
            Cursor.visible = false;
        }

        public void EndBuildingMode()
        {
            Cursor.visible = true;
            isBuildingMode = false;
            currentBuildingData = null;
            
            if (buildingPreview != null)
            {
                Destroy(buildingPreview);
                buildingPreview = null;
            }
        }

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

        private void UpdateBuildingMode()
        {
            var gridManager = GridManager.Instance;
            Vector2Int gridPos = gridManager.WorldToGrid(mainCamera.ScreenToWorldPoint(Input.mousePosition));
            
            // 更新预览位置
            if (buildingPreview != null)
            {
                buildingPreview.transform.position =  GridManager.Instance.GridToWorldBottomCenter(gridPos.x, gridPos.y);
                
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
                    color = new Color(1f, 0f, 0f, 0.5f);
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
                buildingComponent.Initialize(currentBuildingData, new Vector2Int(gridX, gridY));
                
                RegisterBuilding(buildingComponent);

                ResourceManager.Instance.SpendGold(buildingComponent.Data.BuildCost);
            }
        }

        /// <summary>
        /// 注册建筑到管理系统
        /// </summary>
        public void RegisterBuilding(BuildingComponent building)
        {
            if (building == null) return;

            allBuildings.Add(building);
            
            building.OnBuildingDestroyed += OnBuildingDestroyed;
            
            EventManager. OnBuildingPlaced?.Invoke(building);
        }

        private void OnBuildingDestroyed(BuildingComponent building)
        {
            allBuildings.Remove(building);
        }

        public bool UpgradeBuilding(BuildingComponent building)
        {
            if (!building.HasNextLevel || !building.IsUpgradeable)
            {
                return false;
            }

            int upgradeCost = building.LevelData.UpgradeCost;

            if (!ResourceManager.Instance.SpendGold(upgradeCost))
            {
                return false;
            }

            building.UpgradeBuilding();
            return true;
        }

        public void DemolishBuilding(BuildingComponent building)
        {
            building.DemolishBuilding();
        }

        void OnNightStart(int day)
        {
            EndBuildingMode();
        }
        
        public bool IsBuildingMode => isBuildingMode;
        public List<BuildingComponent> AllBuildings => allBuildings;
    }
}