using System;
using UnityEngine;
using Components.Buildings;
using Core;
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
        
        [SerializeField] private BuildingInfoPanel buildingInfoPanel;
        [SerializeField] private RectTransform panelRectTransform;

        private BuildingComponent selectedBuilding;
        private Collider2D lastHoveredCollider2D = null; // 记录上一帧悬停的

        private void Awake()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;
        }

        private void OnEnable()
        {
            EventManager.OnBuildingInfoPanelHide += OnBuildingInfoPanelHide;
        }

        private void OnDisable()
        {
            EventManager.OnBuildingInfoPanelHide -= OnBuildingInfoPanelHide;
        }

        private void Update()
        {
            HandleInput();
        }

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
            if (!building) return;

            // 如果点击的是同一个建筑，取消选择
            if (selectedBuilding == building)
            {
                DeselectBuilding();
                return;
            }

            DeselectBuilding();

            selectedBuilding = building;
            
            SetBuildingSelectedVisual(true);
            
            if (buildingInfoPanel)
            {
                buildingInfoPanel.Show(building);
            }
        }

        void OnBuildingInfoPanelHide()
        {
            DeselectBuilding();
        }
        
        public void DeselectBuilding()
        {
            if (selectedBuilding != null)
            {
                SetBuildingSelectedVisual(false);
                selectedBuilding = null;
            }

            if (buildingInfoPanel != null)
            {
                buildingInfoPanel.Hide();
            }
        }

        private void SetBuildingSelectedVisual(bool selected)
        {
            if (selectedBuilding == null) return;

            if (selectionIndicator != null)
            {
                if (selected)
                {
                    selectionIndicator.SetActive(true);
                    selectionIndicator.transform.position = selectedBuilding.transform.position + Vector3.up;
                }
                else
                {
                    selectionIndicator.SetActive(false);
                }
            }
        }
    }
}