using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePreview : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;  // 移动速度
    
    private Camera mainCamera;
    private float screenRightEdge;   // 屏幕右边界
    private float screenLeftEdge;    // 屏幕左边界
    private float objectWidth;       // 对象宽度的一半
    
    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // 计算屏幕边界（世界坐标）
        CalculateScreenBounds();
        
        // 获取对象宽度
        CalculateObjectWidth();
    }
    
    void Update()
    {
        // 向右移动
        MoveRight();
        
        // 检查是否越过右边界，如果是则重置到左边
        CheckAndWrapPosition();
    }
    
    /// <summary>
    /// 向右移动对象（世界坐标系）
    /// </summary>
    void MoveRight()
    {
        // 使用世界坐标系的右方向移动
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime, Space.World);
    }
    
    /// <summary>
    /// 检查位置并在必要时重置
    /// </summary>
    void CheckAndWrapPosition()
    {
        // 获取对象当前位置
        float objectRightEdge = transform.position.x + objectWidth;
        
        // 调试信息
        Debug.Log($"对象位置: {transform.position.x}, 对象右边缘: {objectRightEdge}, 屏幕右边界: {screenRightEdge}");
        
        // 如果对象完全越过屏幕右边界
        if (objectRightEdge > screenRightEdge)
        {
            // 重置到屏幕左边界外
            Vector3 newPosition = transform.position;
            newPosition.x = screenLeftEdge - objectWidth;
            transform.position = newPosition;
            
            Debug.Log($"对象重置到位置: {newPosition.x}");
        }
    }
    
    /// <summary>
    /// 计算屏幕边界
    /// </summary>
    void CalculateScreenBounds()
    {
        if (mainCamera != null)
        {
            // 对于2D游戏，使用摄像机的orthographicSize来计算边界
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            
            screenRightEdge = mainCamera.transform.position.x + cameraWidth / 2f;
            screenLeftEdge = mainCamera.transform.position.x - cameraWidth / 2f;
            
            // 调试信息
            Debug.Log($"屏幕边界 - 左: {screenLeftEdge}, 右: {screenRightEdge}");
        }
    }
    
    /// <summary>
    /// 计算对象宽度
    /// </summary>
    void CalculateObjectWidth()
    {
        // 尝试获取Renderer组件来计算宽度
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectWidth = objectRenderer.bounds.size.x / 2f;
        }
        else
        {
            // 如果没有Renderer，尝试获取Collider2D
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                objectWidth = collider.bounds.size.x / 2f;
            }
            else
            {
                // 默认宽度
                objectWidth = 0.5f;
            }
        }
        
        // 调试信息
        Debug.Log($"对象宽度(半宽): {objectWidth}");
    }
    
    /// <summary>
    /// 在屏幕尺寸改变时重新计算边界
    /// </summary>
    void OnValidate()
    {
        if (Application.isPlaying && mainCamera != null)
        {
            CalculateScreenBounds();
        }
    }
}
