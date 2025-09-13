using UnityEngine;

public class MapGizmoDrawer : MonoBehaviour
{
    // 地图的尺寸
    public Vector2 mapSizeInTiles = new Vector2(30, 17);
    // 每个 Tile 的尺寸（例如，如果你使用 1x1 的 Tile）
    public float tileSize = 1f;
    // 线的颜色
    public Color gizmoColor = Color.green;

    // 在 Scene 视图中绘制 Gizmos
    private void OnDrawGizmos()
    {
        // 确保只在编辑器中绘制
        if (!Application.isEditor) return;

        // 设置 Gizmo 的颜色
        Gizmos.color = gizmoColor;

        // 计算地图的实际宽度和高度
        float mapWidth = mapSizeInTiles.x * tileSize;
        float mapHeight = mapSizeInTiles.y * tileSize;

        // 获取当前 Transform 的位置作为地图中心点
        Vector3 center = transform.position;

        // 计算左下角和右上角的点
        Vector3 min = new Vector3(center.x - mapWidth / 2f, center.y - mapHeight / 2f, center.z);
        Vector3 max = new Vector3(center.x + mapWidth / 2f, center.y + mapHeight / 2f, center.z);

        // 使用 Gizmos.DrawWireCube 绘制一个线框立方体
        Gizmos.DrawWireCube(center, new Vector3(mapWidth, mapHeight, 0.1f));
    }
}