using System.Collections.Generic;
using Components.Enemies;
using Core.Grid;
using Core.Pathfinding;
using Data;
using UnityEngine;

namespace Test
{
    public class TestPathfinding : MonoBehaviour
    {
        public EnemyData EnemyData;
        
        [Header("移动设置")]
        public float moveSpeed = 5f; // 单位秒移动速度
        
        private List<Vector3> currentPath = new List<Vector3>();
        private int pathIndex = 0;

        void Update()
        {
            HandleMouseInput();
            FollowPath();
        }

        /// <summary>
        /// 处理鼠标点击输入
        /// </summary>
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0; // 保持在2D平面

                PathfindingNode endNode;
                // 使用GridManager寻路
                currentPath = GridManager.Instance.FindPath(transform.position, mouseWorldPos, out endNode);
                pathIndex = 0;
            }
            else if (Input.GetMouseButtonUp(2))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0; // 保持在2D平面
                
                var enemy = Instantiate(EnemyData.enemyPrefab, mouseWorldPos, Quaternion.identity).GetComponent<EnemyComponent>();
                enemy.Initialize(EnemyData, 1);
                
            }
        }

        /// <summary>
        /// 按路径移动
        /// </summary>
        private void FollowPath()
        {
            if (currentPath == null || pathIndex >= currentPath.Count) return;

            if (pathIndex == 0)
            {
                if (GridManager.Instance.WorldToGrid(transform.position) == GridManager.Instance.WorldToGrid(currentPath[0]))
                {
                    pathIndex++;
                    if (pathIndex >= currentPath.Count) return;
                }
            }
            
            Vector3 targetPos = currentPath[pathIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                pathIndex++;
            }
        }

        /// <summary>
        /// 可选：在场景中可视化路径
        /// </summary>
        private void OnDrawGizmos()
        {
            if (currentPath == null || currentPath.Count == 0) return;

            Gizmos.color = Color.green;
            for (int i = pathIndex; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }
}