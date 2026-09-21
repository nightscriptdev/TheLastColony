using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Pathfinding
{
    public class PathfindingNode : IComparable<PathfindingNode>
    {
        public Vector2Int gridPosition;
        public int gCost;
        public int hCost;
        public int FCost => gCost + hCost;
        public PathfindingNode parent;

        public PathfindingNode()
        {
            this.gCost = int.MaxValue;
            this.hCost = 0;
            this.parent = null;
        }
        public PathfindingNode(Vector2Int gridPosition)
        {
            this.gridPosition = gridPosition;
            this.gCost = int.MaxValue;
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

        public int CompareTo(PathfindingNode other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                // FCost 相同，比较 HCost
                compare = hCost.CompareTo(other.hCost);
            }
            return compare;
        }
    }
}