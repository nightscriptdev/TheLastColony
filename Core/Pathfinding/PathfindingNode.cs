using UnityEngine;

namespace Core.Pathfinding
{
    public class PathfindingNode
    {
        public Vector2Int gridPosition;
        public int gCost; // 实际代价
        public int hCost; // 启发式代价
        public int FCost => gCost + hCost; // 总代价
        public PathfindingNode parent; // 父节点

        public PathfindingNode()
        {
            
        }
        public PathfindingNode(Vector2Int gridPosition)
        {
            this.gridPosition = gridPosition;
            this.gCost = int.MaxValue; // 初始为极大值，便于比较
            this.hCost = 0;
            this.parent = null;
        }
        
        /// <summary>
        /// 使用Octile距离（八向距离）
        /// </summary>
        public void CalculateHCost(Vector2Int targetPosition)
        {
            hCost = OctileDistance(gridPosition, targetPosition);
        }

        public static int OctileDistance(Vector2Int start, Vector2Int end)
        {
            int dx = Mathf.Abs(start.x - end.x);
            int dy = Mathf.Abs(start.y - end.y);
            return 10 * Mathf.Max(dx, dy) + 4 * Mathf.Min(dx, dy);
        }
    }
}