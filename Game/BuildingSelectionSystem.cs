using UnityEngine;
using Components.Buildings;
using Managers;
using UI.Buildings;
using UnityEngine.EventSystems;

namespace Game
{
    /// <summary>
    /// 处理建筑的点击选择和信息显示
    /// </summary>
    public class BuildingSelectionSystem : MonoBehaviour
    {
        [SerializeField] private LayerMask buildingLayerMask = -1;
        [SerializeField] private Camera gameCamera;
        
        [SerializeField] private Vector2 padding = new Vector2(10f, 10f);

        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private Color selectedColor = Color.yellow;
        
        [SerializeField] private BuildingInfoPanel buildingInfoPanel;
        [SerializeField] private RectTransform panelRectTransform;

        private BuildingComponent selectedBuilding;
        private Collider2D lastHoveredCollider2D = null; // 记录上一帧悬停的
        private SpriteRenderer selectedBuildingRenderer;
        private Color originalColor;

        private void Awake()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            Vector3 worldPos = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, buildingLayerMask);
    
            if (!BuildingManager.Instance.IsBuildingMode)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    DeselectBuilding();
                }

                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    if (hitCollider == null)
                    {
                        DeselectBuilding();
                        return;
                    }

                    var buildingComponent = hitCollider.GetComponent<BuildingComponent>();
                    if (buildingComponent != null)
                    {
                        SelectBuilding(buildingComponent);
                    }
                    else
                    {
                        DeselectBuilding();
                    }
                }
            }
   
            // 处理悬停逻辑
            HandleHoverLogic(hitCollider);
        }

        private void HandleHoverLogic(Collider2D hitCollider)
        {
            if (hitCollider != lastHoveredCollider2D)
            {
                // 处理离开事件
                if (lastHoveredCollider2D != null && lastHoveredCollider2D.CompareTag("Tower"))
                {
                    lastHoveredCollider2D.GetComponent<TowerComponent>().HideRange();
                }

                // 处理进入事件
                if (hitCollider != null && hitCollider.CompareTag("Tower"))
                {
                    hitCollider.GetComponent<TowerComponent>().ShowRange();
                }

                lastHoveredCollider2D = hitCollider;
            }
        }
        
        /// <summary>
        /// 选择建筑
        /// </summary>
        public void SelectBuilding(BuildingComponent building)
        {
            if (building == null) return;

            // 如果点击的是同一个建筑，取消选择
            if (selectedBuilding == building)
            {
                DeselectBuilding();
                return;
            }

            // 取消之前的选择
            DeselectBuilding();

            // 选择新建筑
            selectedBuilding = building;
            
            // 设置视觉效果
            SetBuildingSelectedVisual(true);
            
            // 显示建筑信息面板
            if (buildingInfoPanel != null)
            {
                PositionBuildingInfoPanel(building.transform.position);
                buildingInfoPanel.ShowBuildingInfo(building);
            }
            
            Debug.Log($"选择建筑: {building.Data.BuildingName}");
        }

        /// <summary>
        /// 取消选择建筑
        /// </summary>
        public void DeselectBuilding()
        {
            if (selectedBuilding != null)
            {
                // 恢复视觉效果
                SetBuildingSelectedVisual(false);
                
                selectedBuilding = null;
                selectedBuildingRenderer = null;
            }

            // 隐藏建筑信息面板
            if (buildingInfoPanel != null)
            {
                buildingInfoPanel.ClosePanel();
            }
        }

        /// <summary>
        /// 设置建筑选中视觉效果
        /// </summary>
        private void SetBuildingSelectedVisual(bool selected)
        {
            if (selectedBuilding == null) return;

            // 获取建筑的SpriteRenderer
            if (selectedBuildingRenderer == null)
            {
                selectedBuildingRenderer = selectedBuilding.GetComponent<SpriteRenderer>();
            }

            if (selectedBuildingRenderer != null)
            {
                if (selected)
                {
                    // 保存原始颜色
                    originalColor = selectedBuildingRenderer.color;
                    // 设置选中颜色
                    selectedBuildingRenderer.color = selectedColor;
                }
                else
                {
                    // 恢复原始颜色
                    selectedBuildingRenderer.color = originalColor;
                }
            }

            // 显示/隐藏选择指示器
            if (selectionIndicator != null)
            {
                if (selected)
                {
                    selectionIndicator.SetActive(true);
                    selectionIndicator.transform.position = selectedBuilding.transform.position;
                }
                else
                {
                    selectionIndicator.SetActive(false);
                }
            }
        }

        private void PositionBuildingInfoPanel(Vector2 worldPosition)
        {
            Vector2 pivot = new Vector2(0f, 1f); // 默认左上角
            Vector2 mousePosition = Input.mousePosition;
            
            // 如果鼠标在屏幕右侧，右对齐
            if (mousePosition.x > Screen.width * 0.5f)
                pivot.x = 1f;
    
            // 如果鼠标在屏幕下方，底对齐
            if (mousePosition.y < Screen.height * 0.5f)
                pivot.y = 0f;

            panelRectTransform.pivot = pivot;

            // 计算偏移
            Vector2 offset = new Vector2(pivot.x == 1 ? -padding.x : padding.x,
                pivot.y == 0 ? padding.y : -padding.y);

            panelRectTransform.position = worldPosition + offset;
        }
        
        /// <summary>
        /// 获取当前选中的建筑
        /// </summary>
        public BuildingComponent GetSelectedBuilding()
        {
            return selectedBuilding;
        }

        /// <summary>
        /// 检查指定建筑是否被选中
        /// </summary>
        public bool IsBuildingSelected(BuildingComponent building)
        {
            return selectedBuilding == building;
        }

        /// <summary>
        /// 强制选择指定建筑（供外部调用）
        /// </summary>
        public void ForceSelectBuilding(BuildingComponent building)
        {
            SelectBuilding(building);
        }

        /*/// <summary>
        /// 设置建筑信息面板引用
        /// </summary>
        public void SetBuildingInfoPanel(BuildingInfoPanel infoPanel)
        {
            buildingInfoPanel = infoPanel;
        }*/
    }
}