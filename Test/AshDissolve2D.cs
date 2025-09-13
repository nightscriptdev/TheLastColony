using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class AshDissolve2D : MonoBehaviour
{
    public GameObject obj;
    
    public static AshDissolve2D Instance;
    
    [Header("2D灰烬效果设置")]
    public int ashParticleCount = 15;
    public float dissolveTime = 2f;
    public Color ashColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    public float ashSizeMin = 0.05f;
    public float ashSizeMax = 0.15f;
    public int sortingOrder = 100; // 确保在前景显示
    
    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Q))
            CreateSimple2DAsh(obj.transform.position);
        if (Input.GetKey(KeyCode.W))
            CreateAshDissolveEffect2D(obj);
    }

    // 主要的2D灰烬消散效果
    public void CreateAshDissolveEffect2D(GameObject target)
    {
        if (target == null) return;
        
        StartCoroutine(DissolveToAsh2D(target));
    }
    
    IEnumerator DissolveToAsh2D(GameObject target)
    {
        // 第一阶段：目标物体变暗
        SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        
        if (targetRenderer != null)
        {
            originalColor = targetRenderer.color;
            yield return StartCoroutine(FadeToGray2D(targetRenderer, originalColor));
        }
        
        // 第二阶段：生成2D圆形灰烬粒子
        Vector3 targetPos = target.transform.position;
        Bounds targetBounds = GetSpriteBounds(target);
        
        Create2DAshParticles(targetPos, targetBounds);
        
        // 第三阶段：隐藏原物体
        if (target != null)
        {
            target.SetActive(false);
        }
    }
    
    // 获取2D精灵的边界
    Bounds GetSpriteBounds(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            return sr.bounds;
        }
        
        // 默认大小
        return new Bounds(target.transform.position, Vector3.one);
    }
    
    // 变成灰色效果
    IEnumerator FadeToGray2D(SpriteRenderer spriteRenderer, Color originalColor)
    {
        float fadeTime = dissolveTime * 0.3f;
        float elapsed = 0f;
        Color grayColor = new Color(0.3f, 0.3f, 0.3f, originalColor.a);
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeTime;
            
            spriteRenderer.color = Color.Lerp(originalColor, grayColor, progress);
            yield return null;
        }
    }
    
    // 创建2D圆形灰烬粒子
    void Create2DAshParticles(Vector3 centerPos, Bounds bounds)
    {
        for (int i = 0; i < ashParticleCount; i++)
        {
            Create2DAshParticle(centerPos, bounds);
        }
    }
    
    void Create2DAshParticle(Vector3 centerPos, Bounds bounds)
    {
        // 创建空的游戏对象
        GameObject ashParticle = new GameObject("AshParticle2D");
        
        // 添加SpriteRenderer组件
        SpriteRenderer spriteRenderer = ashParticle.AddComponent<SpriteRenderer>();
        
        // 创建圆形精灵
        spriteRenderer.sprite = CreateCircleSprite();
        
        // 设置2D渲染属性
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.color = GetRandomAshColor();
        
        // 设置初始位置（在目标边界内随机分布）
        Vector2 randomOffset = Random.insideUnitCircle * bounds.size.magnitude * 0.4f;
        ashParticle.transform.position = centerPos + new Vector3(randomOffset.x, randomOffset.y, 0);
        
        // 设置随机大小
        float size = Random.Range(ashSizeMin, ashSizeMax);
        ashParticle.transform.localScale = Vector3.one * size;
        
        // 开始粒子动画
        StartCoroutine(Animate2DAshParticle(ashParticle));
    }
    
    // 创建圆形精灵（程序生成）
    Sprite CreateCircleSprite()
    {
        int textureSize = 32;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Vector2 center = Vector2.one * textureSize * 0.5f;
        float radius = textureSize * 0.4f;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    // 圆形内部，根据距离设置透明度（边缘软化）
                    float alpha = 1f - (distance / radius);
                    alpha = Mathf.Clamp01(alpha * 2f); // 让边缘更软
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    // 圆形外部，完全透明
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // 创建精灵
        Sprite sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f), // 锚点在中心
            100f // 像素单位
        );
        
        return sprite;
    }
    
    // 获取随机灰烬颜色
    Color GetRandomAshColor()
    {
        Color color = ashColor;
        
        // 添加一些随机变化
        color.r += Random.Range(-0.1f, 0.1f);
        color.g += Random.Range(-0.1f, 0.1f);
        color.b += Random.Range(-0.1f, 0.1f);
        color.a = Random.Range(0.6f, 1f);
        
        // 确保在合理范围内
        color.r = Mathf.Clamp01(color.r);
        color.g = Mathf.Clamp01(color.g);
        color.b = Mathf.Clamp01(color.b);
        
        return color;
    }
    
    // 2D灰烬粒子动画
    IEnumerator Animate2DAshParticle(GameObject particle)
    {
        SpriteRenderer spriteRenderer = particle.GetComponent<SpriteRenderer>();
        Vector3 startPos = particle.transform.position;
        Vector3 startScale = particle.transform.localScale;
        Color startColor = spriteRenderer.color;
        
        // 随机运动参数
        Vector2 velocity2D = Random.insideUnitCircle * Random.Range(0.5f, 1.5f);
        velocity2D.y = Mathf.Abs(velocity2D.y) + 0.5f; // 确保向上飘
        
        float rotationSpeed = Random.Range(-90f, 90f);
        float lifetime = Random.Range(dissolveTime * 0.8f, dissolveTime * 1.2f);
        float elapsed = 0f;
        
        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            // 位置变化：2D飘散
            velocity2D.x *= 0.98f; // 水平阻力
            velocity2D.y += Random.Range(-0.05f, 0.05f); // 轻微随机飘动
            
            Vector3 velocity3D = new Vector3(velocity2D.x, velocity2D.y, 0);
            particle.transform.position += velocity3D * Time.deltaTime;
            
            // 2D旋转（只绕Z轴）
            particle.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // 缩放变化：逐渐变小
            float scaleMultiplier = Mathf.Lerp(1f, 0.1f, progress * progress);
            particle.transform.localScale = startScale * scaleMultiplier;
            
            // 颜色和透明度变化
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(startColor.a, 0f, progress);
            
            // 添加闪烁效果
            if (Random.value < 0.05f)
            {
                currentColor.a *= Random.Range(0.3f, 1f);
            }
            
            spriteRenderer.color = currentColor;
            
            yield return null;
        }
        
        // 销毁粒子
        if (particle != null)
            Destroy(particle);
    }
    
    // 简化版本 - 如果不想要复杂效果
    public void CreateSimple2DAsh(Vector3 position)
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject ash = new GameObject("SimpleAsh2D");
            
            // 添加SpriteRenderer
            SpriteRenderer sr = ash.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleCircleSprite();
            sr.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            sr.sortingOrder = sortingOrder;
            
            // 位置
            Vector2 randomOffset = Random.insideUnitCircle * 0.8f;
            ash.transform.position = position + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            // 大小
            ash.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
            
            // 添加简单动画组件
            ash.AddComponent<Simple2DAshParticle>();
        }
    }
    
    // 创建简单圆形精灵
    Sprite CreateSimpleCircleSprite()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = Vector2.one * size * 0.5f;
        float radius = size * 0.4f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                Color color = distance <= radius ? Color.white : Color.clear;
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
    }
}

// 简单的2D灰烬粒子行为
public class Simple2DAshParticle : MonoBehaviour
{
    private Vector2 velocity;
    private float lifetime = 2f;
    private SpriteRenderer spriteRenderer;
    private Color startColor;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        
        // 随机向上的速度
        velocity = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0.8f, 1.5f));
        
        // 自动销毁
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // 移动
        Vector3 movement = new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
        transform.position += movement;
        
        // 阻力
        velocity.x *= 0.98f;
        
        // 2D旋转
        transform.Rotate(0, 0, 45f * Time.deltaTime);
        
        // 透明度变化
        Color color = startColor;
        color.a = startColor.a * (1f - (Time.time - Time.fixedTime) / lifetime);
        spriteRenderer.color = color;
        
        // 缩放变化
        transform.localScale *= 0.998f;
    }
}

// 使用示例
public class Monster2D : MonoBehaviour
{
    public int health = 100;
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // 使用2D灰烬效果
        AshDissolve2D.Instance?.CreateAshDissolveEffect2D(gameObject);
        
        // 或者使用简单版本
        // AshDissolve2D.Instance?.CreateSimple2DAsh(transform.position);
        
        // 延迟销毁
        Destroy(gameObject, 3f);
    }
}