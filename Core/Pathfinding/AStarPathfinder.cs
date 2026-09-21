using System;
using System.Collections;
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
        private const int MAX_NODES_PER_FRAME = 100; // 每帧处理的最大节点数
        
        public List<Vector3> FindPath(Vector2Int startGrid, Vector2Int targetGrid, out PathfindingNode endNode, bool findAdjacentIfBlocked = false)
        {
            // 检查起点是否可通行
            /*if (!GridManager.Instance.IsWalkable(startGrid.x, startGrid.y))
            {
                endNode = null;
                return null;
            }*/
            
            MinHeap<PathfindingNode> openSet = new MinHeap<PathfindingNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, PathfindingNode> nodeMap = new Dictionary<Vector2Int, PathfindingNode>();
            
            PathfindingNode startNode = new PathfindingNode(startGrid);
            startNode.gCost = 0;
            startNode.CalculateHCost(targetGrid);
            
            openSet.Insert(startNode);
            nodeMap[startGrid] = startNode;
            
            PathfindingNode closestNode = startNode;

            while (openSet.Count > 0)
            {
                PathfindingNode currentNode = openSet.ExtractMin();
                closedSet.Add(currentNode.gridPosition);
                
                if (currentNode.gridPosition == targetGrid || (findAdjacentIfBlocked && GridManager.IsAdjacent(currentNode.gridPosition, targetGrid)))
                {
                    endNode = currentNode;
                    return RetracePath(currentNode);
                }
                
                if (currentNode.FCost < closestNode.FCost ||
                    (currentNode.FCost == closestNode.FCost && currentNode.hCost < closestNode.hCost))
                {
                    closestNode = currentNode;
                }
                
                ProcessNeighbors(currentNode, targetGrid, openSet, closedSet, nodeMap);
            }
            
            endNode = closestNode;
            return RetracePath(closestNode);
        }
        
        /// <summary>
        /// 分帧寻路
        /// </summary>
        public IEnumerator FindPathCoroutine(Vector2Int startGrid, Vector2Int targetGrid, bool findAdjacentIfBlocked, Action<List<Vector3>, PathfindingNode> callback)
        {
            MinHeap<PathfindingNode> openSet = new MinHeap<PathfindingNode>();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, PathfindingNode> nodeMap = new Dictionary<Vector2Int, PathfindingNode>();
            
            PathfindingNode startNode = new PathfindingNode(startGrid);
            startNode.gCost = 0;
            startNode.CalculateHCost(targetGrid);
            
            openSet.Insert(startNode);
            nodeMap[startGrid] = startNode;
            
            PathfindingNode closestNode = startNode;
            int nodesProcessedThisFrame = 0;

            while (openSet.Count > 0)
            {
                PathfindingNode currentNode = openSet.ExtractMin();
                closedSet.Add(currentNode.gridPosition);
                nodesProcessedThisFrame++;
                
                // 检查是否到达目标
                if (currentNode.gridPosition == targetGrid || (findAdjacentIfBlocked && GridManager.IsAdjacent(currentNode.gridPosition, targetGrid)))
                {
                    callback?.Invoke(RetracePath(currentNode), currentNode);
                    yield break;
                }
                
                // 更新最近节点
                if (currentNode.FCost < closestNode.FCost ||
                    (currentNode.FCost == closestNode.FCost && currentNode.hCost < closestNode.hCost))
                {
                    closestNode = currentNode;
                }
                
                // 处理邻居节点
                ProcessNeighbors(currentNode, targetGrid, openSet, closedSet, nodeMap);
                
                // 每处理一定数量的节点后,暂停一帧
                if (nodesProcessedThisFrame >= MAX_NODES_PER_FRAME)
                {
                    nodesProcessedThisFrame = 0;
                    yield return null;
                }
            }
            
            // 没有找到完整路径,返回最接近的路径
            callback?.Invoke(RetracePath(closestNode), closestNode);
        }
        
        /// <summary>
        /// 处理当前节点的邻居
        /// </summary>
        private void ProcessNeighbors(PathfindingNode currentNode, Vector2Int targetGrid, MinHeap<PathfindingNode> openSet, HashSet<Vector2Int> closedSet, Dictionary<Vector2Int, PathfindingNode> nodeMap)
        {
            List<Vector2Int> neighbors = GridManager.Instance.GetWalkableNeighbors(
                currentNode.gridPosition.x, currentNode.gridPosition.y);
            
            foreach (Vector2Int neighborPos in neighbors)
            {
                if (closedSet.Contains(neighborPos))
                    continue;
                    
                Vector2Int direction = neighborPos - currentNode.gridPosition;
                int moveCost = (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1) ? 14 : 10;
                int newGCost = currentNode.gCost + moveCost;
                
                PathfindingNode neighborNode;
                if (!nodeMap.TryGetValue(neighborPos, out neighborNode))
                {
                    neighborNode = new PathfindingNode(neighborPos);
                    neighborNode.CalculateHCost(targetGrid);
                    nodeMap[neighborPos] = neighborNode;
                }
                
                if (newGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.parent = currentNode;
                    if (!openSet.Contains(neighborNode))
                        openSet.Insert(neighborNode);
                }
            }
        }
        
        /*private PathfindingNode GetLowestFCostNode(List<PathfindingNode> openSet)
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
        }*/
        
        public List<Vector3> RetracePath(PathfindingNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            PathfindingNode currentNode = endNode;
            
            while (currentNode != null)
            {
                Vector3 worldPos = GridManager.Instance.GridToWorldBottomCenter(
                    currentNode.gridPosition.x, currentNode.gridPosition.y);
                path.Add(worldPos);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();
            return path;
        }
    }
}