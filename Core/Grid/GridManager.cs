using System;
using System.Collections;
using System.Collections.Generic;
using Components.Buildings;
using Components.Enemies;
using Core.Pathfinding;
using Enums;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Grid
{
    public class GridManager : MonoSingleton<GridManager>
    {
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap obstaclesTilemap;
        
        [SerializeField] private int gridWidth = 28;
        [SerializeField] private int gridHeight = 16;
        [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);
        
        const int BUILDINGS_PER_FRAME = 4; // 每帧检查的建筑数量

        private Vector3 origin;
        
        private CellState[,] gridArray;
        private AStarPathfinder pathfinder;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeGrid();
        }
        
        void OnEnable()
        {
            EventManager.OnBuildingPlaced += OnBuildingPlaced;
            EventManager.OnEnemyStayed += OnEnemyStayed;
            EventManager.OnGridRelease += OnGridRelease;
        }
        
        void OnDisable()
        {
            EventManager.OnBuildingPlaced -= OnBuildingPlaced;
            EventManager.OnEnemyStayed -= OnEnemyStayed;
            EventManager.OnGridRelease -= OnGridRelease;
        }
        
        private void InitializeGrid()
        {
            gridArray = new CellState[gridWidth, gridHeight];
            
            origin = transform.position - new Vector3(
                (gridWidth * 0.5f * cellSize.x),
                (gridHeight * 0.5f * cellSize.y),
                0f
            );
            
            pathfinder = new AStarPathfinder();
            
            LoadObstaclesFromTilemap();
        }
        
        private void LoadObstaclesFromTilemap()
        {
            if (obstaclesTilemap == null)
            {
                return;
            }
            BoundsInt area = obstaclesTilemap.cellBounds;
            for (int x = area.xMin; x < area.xMax; x++)
            {
                for (int y = area.yMin; y < area.yMax; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    TileBase tile = obstaclesTilemap.GetTile(cellPos);
                    if (tile == null) continue;
                    Vector3 tileWorldPos = obstaclesTilemap.CellToWorld(cellPos) + new Vector3(cellSize.x * 0.5f, 0, 0f);
                    Vector2Int gridPos = WorldToGrid(tileWorldPos);
                    if (IsValidGridPosition(gridPos.x, gridPos.y))
                    {
                        SetCellState(gridPos.x, gridPos.y, CellState.TerrainObstacle);
                    }
                }
            }
        }
        
        private void OnBuildingPlaced(BuildingComponent building)
        {
            SetCellState(building.GridPosition.x, building.GridPosition.y, CellState.BuildingObstacle);
        }
        
        // 保留同步版本用于向后兼容
        public List<Vector3> FindPath(Vector2Int startGrid, Vector2Int targetGrid, out PathfindingNode endNode, bool findAdjacentIfBlocked = false)
        {
            return pathfinder.FindPath(startGrid, targetGrid, out endNode, findAdjacentIfBlocked);
        }
        
        public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos, out PathfindingNode endNode)
        {
            return pathfinder.FindPath(WorldToGrid(startWorldPos), WorldToGrid(targetWorldPos), out endNode);
        }
        
        /// <summary>
        /// 协程版本的寻路
        /// </summary>
        public Coroutine FindPathAsync(Vector2Int startGrid, Vector2Int targetGrid, bool findAdjacentIfBlocked, System.Action<List<Vector3>, PathfindingNode> callback)
        {
            return StartCoroutine(pathfinder.FindPathCoroutine(startGrid, targetGrid, findAdjacentIfBlocked, callback));
        }
        
        public bool CanBuildAt(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);
            return IsWalkable(gridPos.x, gridPos.y);
        }
        
        public CellState GetCellState(int x, int y)
        {
            if (!IsValidGridPosition(x, y))
                return CellState.TerrainObstacle;
            return gridArray[x, y];
        }
        
        public void SetCellState(int x, int y, CellState state)
        {
            if (IsValidGridPosition(x, y))
            {
                gridArray[x, y] = state;
                if(state != CellState.Walkable)
                    EventManager.OnCellBecomeObstacle?.Invoke(GridToWorldBottomCenter(x, y));
            }
        }
        
        public bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }
        
        public bool IsWalkable(int x, int y)
        {
            return GetCellState(x, y) == CellState.Walkable;
        }
        
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            Vector3 gridPosition = worldPosition - origin;
            int x = Mathf.FloorToInt(gridPosition.x / cellSize.x);
            int y = Mathf.FloorToInt(gridPosition.y / cellSize.y);
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// 网格坐标转换为世界坐标（底部中心）
        /// </summary>
        public Vector3 GridToWorldBottomCenter(int x, int y)
        {
            return origin + new Vector3((x + 0.5f) * cellSize.x, y * cellSize.y, 0);
        }
        
        public List<Vector2Int> GetWalkableNeighbors(int x, int y)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int[] directions =
            {
                new Vector2Int(0, 1), // 上
                new Vector2Int(1, 1), // 右上
                new Vector2Int(1, 0), // 右
                new Vector2Int(1, -1), // 右下
                new Vector2Int(0, -1), // 下
                new Vector2Int(-1, -1), // 左下
                new Vector2Int(-1, 0), // 左
                new Vector2Int(-1, 1) // 左上
            };
            for (int i = 0; i < directions.Length; i++)
            {
                var dir = directions[i];
                int newX = x + dir.x;
                int newY = y + dir.y;
                if (IsWalkable(newX, newY))
                {
                    // 对角线移动需要检查两个相邻的直线方向都可通行
                    if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
                    {
                        if (IsWalkable(x + dir.x, y) && IsWalkable(x, y + dir.y))
                        {
                            neighbors.Add(new Vector2Int(newX, newY));
                        }
                    }
                    else // 直线方向
                    {
                        neighbors.Add(new Vector2Int(newX, newY));
                    }
                }
            }
            return neighbors;
        }
        
        /// <summary>
        /// 协程版本 - 获取到最近目标的路径
        /// </summary>
        public void GetNearestPathToTargetAsync(EnemyComponent enemy, Action<List<Vector3>, BuildingComponent> callback)
        {
            StartCoroutine(GetNearestPathCoroutine(enemy, callback));
        }
        
        private IEnumerator GetNearestPathCoroutine(EnemyComponent enemy, Action<List<Vector3>, BuildingComponent> callback)
        {
            if (BuildingManager.Instance.AllBuildings == null || BuildingManager.Instance.AllBuildings.Count == 0)
            {
                callback?.Invoke(null, null);
                yield break;
            }
            
            var allBuildings = new List<BuildingComponent>(BuildingManager.Instance.AllBuildings);
            Vector2Int startGrid = WorldToGrid(enemy.transform.position);
            bool onlyDefenseCrystalsLeft = false;
            
            List<Vector3> closestPath = null;
            BuildingComponent targetBuilding = null;
            PathfindingNode closestNode = new PathfindingNode();
            
            int buildingsChecked = 0;
            
            while (allBuildings.Count > 0)
            {
                BuildingComponent currentBuilding = null;
                Vector2Int targetGrid = Vector2Int.zero;
                
                // 查找最佳目标
                if (FindBestTargetGrid(allBuildings, startGrid, onlyDefenseCrystalsLeft, out currentBuilding, out targetGrid))
                {
                    // 使用协程寻路
                    bool pathComplete = false;
                    List<Vector3> foundPath = null;
                    PathfindingNode endNode = null;
                    
                    yield return FindPathAsync(startGrid, targetGrid, true, (path, node) =>
                    {
                        foundPath = path;
                        endNode = node;
                        pathComplete = true;
                    });
                    
                    // 等待寻路完成
                    while (!pathComplete)
                    {
                        yield return null;
                    }
                    
                    if (foundPath != null && endNode != null)
                    {
                        if (IsAdjacent(endNode.gridPosition, targetGrid))
                        {
                            // 找到了完整路径
                            callback?.Invoke(foundPath, currentBuilding);
                            yield break;
                        }
                        
                        // 记录最接近的路径
                        if (endNode.FCost < closestNode.FCost)
                        {
                            closestPath = foundPath;
                            targetBuilding = currentBuilding;
                            closestNode = endNode;
                        }
                    }
                    
                    buildingsChecked++;
                    if (buildingsChecked >= BUILDINGS_PER_FRAME)
                    {
                        buildingsChecked = 0;
                        yield return null; // 每检查几个建筑就暂停一帧
                    }
                }
                else if (!onlyDefenseCrystalsLeft)
                {
                    onlyDefenseCrystalsLeft = true;
                }
                else
                {
                    break;
                }
            }
            
            callback?.Invoke(closestPath, targetBuilding);
        }
        
        /// <summary>
        /// 同步版本
        /// </summary>
        public List<Vector3> GetNearestPathToTarget(EnemyComponent enemy, out BuildingComponent targetBuilding)
        {
            targetBuilding = null;
            
            if (BuildingManager.Instance.AllBuildings == null || BuildingManager.Instance.AllBuildings.Count == 0)
            {
                return null;
            }
            
            var allBuildings = new List<BuildingComponent>(BuildingManager.Instance.AllBuildings);
            Vector2Int targetGrid = Vector2Int.zero;
            PathfindingNode endNode = null;
            List<Vector3> path;
            List<Vector3> closestPath = null;
            
            Vector2Int startGrid = WorldToGrid(enemy.transform.position);
            bool onlyDefenseCrystalsLeft = false;
            PathfindingNode closestNode = new PathfindingNode();
            BuildingComponent currentTargetBuilding = null;
            
            while (allBuildings.Count > 0)
            {
                if (FindBestTargetGrid(allBuildings, startGrid, onlyDefenseCrystalsLeft, out currentTargetBuilding, out targetGrid))
                {
                    path = FindPath(startGrid, targetGrid, out endNode, true);
                    if (path != null && IsAdjacent(endNode.gridPosition, targetGrid))
                    {
                        targetBuilding = currentTargetBuilding;
                        return path;
                    }
                    if (endNode != null && endNode.FCost < closestNode.FCost)
                    {
                        closestPath = path;
                        targetBuilding = currentTargetBuilding;
                    }
                }
                else if (!onlyDefenseCrystalsLeft)
                {
                    onlyDefenseCrystalsLeft = true;
                }
                else
                {
                    break;
                }
            }
            
            return closestPath;
        }
        
        private bool FindBestTargetGrid(List<BuildingComponent> buildings, Vector2Int startGrid, bool onlyDefenseCrystals, out BuildingComponent targetBuilding, out Vector2Int targetGrid)
        {
            targetBuilding = null;
            targetGrid = Vector2Int.zero;
            int currentBuildingIndex = -1;
            float minHCost = float.MaxValue;
            
            for (int i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                if (!onlyDefenseCrystals && building.Data.BuildingType == BuildingType.DefenseCrystal) 
                    continue;
                
                var cost = PathfindingNode.OctileDistance(startGrid, building.GridPosition);
                if (cost < minHCost)
                {
                    targetGrid = building.GridPosition;
                    currentBuildingIndex = i;
                    minHCost = cost;
                    targetBuilding = building;
                }
            }
            
            if (currentBuildingIndex != -1)
            {
                buildings.RemoveAt(currentBuildingIndex);
                return true;
            }
            
            return false;
        }

        void OnEnemyStayed(Vector3 pos)
        {
            var grid = WorldToGrid(pos);
            SetCellState(grid.x, grid.y, CellState.EnemyObstacle);
        }
        
        void OnGridRelease(Vector3 pos)
        {
            var grid = WorldToGrid(pos);
            SetCellState(grid.x, grid.y, CellState.Walkable);
        }
        
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                origin = transform.position - new Vector3(
                    gridWidth * 0.5f * cellSize.x,
                    gridHeight * 0.5f * cellSize.y,
                    0f
                );
            }
            
            Gizmos.color = Color.white;
            
            // 垂直线
            for (int ix = 0; ix <= gridWidth; ix++)
            {
                Vector3 start = origin + new Vector3(ix * cellSize.x, 0f, 0f);
                Vector3 end = origin + new Vector3(ix * cellSize.x, gridHeight * cellSize.y, 0f);
                Gizmos.DrawLine(start, end);
            }
            // 水平线
            for (int iy = 0; iy <= gridHeight; iy++)
            {
                Vector3 start = origin + new Vector3(0f, iy * cellSize.y, 0f);
                Vector3 end = origin + new Vector3(gridWidth * cellSize.x, iy * cellSize.y, 0f);
                Gizmos.DrawLine(start, end);
            }
            if (gridArray == null) return;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    CellState state = GetCellState(x, y);
                    if (state != CellState.Walkable)
                    {
                        switch (state)
                        {
                            case CellState.TerrainObstacle:
                                Gizmos.color = Color.red;
                                break;
                            case CellState.BuildingObstacle:
                                Gizmos.color = Color.blue;
                                break;
                            case CellState.EnemyObstacle:
                                Gizmos.color = Color.yellow;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        Vector3 center = GridToWorldBottomCenter(x, y);
                        Gizmos.DrawCube(center, new Vector3(cellSize.x * 0.5f, cellSize.y * 0.5f, 0.1f));
                    }
                }
            }
        }
        
        public static bool IsAdjacent(Vector2Int self, Vector2Int other)
        {
            int dx = Mathf.Abs(other.x - self.x);
            int dy = Mathf.Abs(other.y - self.y);
            return (dx <= 1 && dy <= 1) && (dx != 0 || dy != 0);
        }
    }
}