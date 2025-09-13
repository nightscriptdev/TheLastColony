using System.Collections.Generic;
using Core.Grid;
using UnityEngine;

namespace Core.Pathfinding
{
    /// <summary>
    /// A*寻路算法实现
    /// </summary>
    public class AStarPathfinder
    {
        /// <summary>
        /// 寻找路径
        /// </summary>
        /// <param name="startPos">起始 世界坐标</param>
        /// <param name="targetPos">目标 世界坐标</param>
        /// <returns>路径点列表 世界坐标</returns>
        public List<Vector3> FindPath(Vector2Int startGrid, Vector2Int targetGrid, out PathfindingNode endNode)
        {
            //Vector2Int startGrid = GridManager.Instance.WorldToGrid(startPos);
            //Vector2Int targetGrid = GridManager.Instance.WorldToGrid(targetPos);
            
            if (!GridManager.Instance.IsWalkable(startGrid.x, startGrid.y) || 
                !GridManager.Instance.IsWalkable(targetGrid.x, targetGrid.y))
            {
                endNode = null;
                return null;
            }
            
            List<PathfindingNode> openSet = new List<PathfindingNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, PathfindingNode> nodeMap = new Dictionary<Vector2Int, PathfindingNode>();
            
            PathfindingNode startNode = new PathfindingNode(startGrid);
            startNode.gCost = 0;
            startNode.CalculateHCost(targetGrid);
            
            openSet.Add(startNode);
            nodeMap[startGrid] = startNode;
            
            while (openSet.Count > 0)
            {
                PathfindingNode currentNode = GetLowestFCostNode(openSet);
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.gridPosition);
                
                if (currentNode.gridPosition == targetGrid)
                {
                    endNode = currentNode;
                    return RetracePath(currentNode);
                }
                
                List<Vector2Int> neighbors = GridManager.Instance.GetWalkableNeighbors(
                    currentNode.gridPosition.x, currentNode.gridPosition.y);
                
                foreach (Vector2Int neighborPos in neighbors)
                {
                    if (closedSet.Contains(neighborPos))
                        continue;

                    Vector2Int direction = neighborPos - currentNode.gridPosition;
                    int moveCost = (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1) ? 14 : 10;
                    int newGCost = currentNode.gCost + moveCost;

                    // 确保 nodeMap 中有该节点实例（不会重复new不同实例）
                    PathfindingNode neighborNode;
                    if (!nodeMap.TryGetValue(neighborPos, out neighborNode))
                    {
                        neighborNode = new PathfindingNode(neighborPos);
                        neighborNode.CalculateHCost(targetGrid);
                        nodeMap[neighborPos] = neighborNode;
                        // 不在这里加入 openSet —— 先比较 gCost，再决定是否加入
                    }

                    // 只有当找到更优路径时才更新，并在必要时加入 openSet
                    if (newGCost < neighborNode.gCost)
                    {
                        neighborNode.gCost = newGCost;
                        neighborNode.parent = currentNode;
                        if (!openSet.Contains(neighborNode))
                            openSet.Add(neighborNode);
                    }
                }
            }
            
            endNode = null;
            return null;
        }
        
        /// <summary>
        /// 获取开放列表中F成本最低的节点
        /// </summary>
        private PathfindingNode GetLowestFCostNode(List<PathfindingNode> openSet)
        {
            PathfindingNode lowestFCostNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < lowestFCostNode.FCost ||
                    (openSet[i].FCost == lowestFCostNode.FCost && openSet[i].hCost < lowestFCostNode.hCost))
                {
                    lowestFCostNode = openSet[i];
                }
            }
            return lowestFCostNode;
        }
        
        /// <summary>
        /// 回溯路径
        /// </summary>
        private List<Vector3> RetracePath(PathfindingNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            PathfindingNode currentNode = endNode;
            
            while (currentNode != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorldCenter(
                    currentNode.gridPosition.x, currentNode.gridPosition.y);
                path.Add(worldPos);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();
            return path;
        }
    }
}