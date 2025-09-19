using UnityEngine;
using UnityEngine.UI;
using Components;
using System.Collections;
/// <summary>
/// 负责控制血条UI的显示和隐藏
/// 监听HPComponent的事件来更新自己
/// 包含残影效果：受伤时前景血条立即下降，背景血条延迟跟随
/// 满血时自动隐藏，血量不满时一直显示
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image background;
    [SerializeField] private Image mainFill; // 主血条
    [SerializeField] private Image shadowFill; // 残影血条
    [SerializeField] private HealthComponent healthComponent;
    
    [Header("残影效果设置")]
    [SerializeField] private float shadowDelayTime = 0.5f;
    [SerializeField] private float shadowSpeed = 2f;
    [SerializeField] private bool enableShadowEffect = true;
    
    private float targetFillAmount; // 目标血量百分比
    private bool isShadowAnimating = false; // 残影是否正在动画
    private Coroutine shadowCoroutine;

    

    private void OnEnable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged += HandleHealthChanged;
            healthComponent.OnTakeDamage += HandleTakeDamage;
            healthComponent.OnHealthFull += HandleHealthFull;
            healthComponent.OnDeath += HandleDeath;
        }
    }
    
    private void OnDisable()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= HandleHealthChanged;
            healthComponent.OnTakeDamage -= HandleTakeDamage;
            healthComponent.OnHealthFull -= HandleHealthFull;
            healthComponent.OnDeath -= HandleDeath;
        }
        
        // 停止残影协程
        if (shadowCoroutine != null)
        {
            StopCoroutine(shadowCoroutine);
            shadowCoroutine = null;
        }
    }
    
    private void Start()
    {
        if (healthComponent != null)
            InitializeHealthBar(healthComponent.CurrentHP, healthComponent.MaxHP);
    }
    
    public void InitializeHealthBar(int currentHP, int maxHP)
    {
        mainFill.fillAmount = (float)currentHP / maxHP;
        shadowFill.fillAmount = mainFill.fillAmount;
    }

    public void UpdateHealthBar(int currentHP, int maxHP)
    {
        float newFillAmount = (float)currentHP / maxHP;
        targetFillAmount = newFillAmount;
        
        // 立即更新主血条
        mainFill.fillAmount = newFillAmount;
        
        // 如果启用残影效果
        if (enableShadowEffect)
        {
            // 如果是受伤（血量下降）
            if (newFillAmount < shadowFill.fillAmount)
            {
                // 停止之前的残影动画
                if (shadowCoroutine != null)
                {
                    StopCoroutine(shadowCoroutine);
                }
                // 开始新的残影动画
                shadowCoroutine = StartCoroutine(ShadowFollowCoroutine());
            }
            // 如果是回血
            else if (newFillAmount > shadowFill.fillAmount)
            {
                // 回血时两个血条同步更新
                shadowFill.fillAmount = newFillAmount;
            }
        }
        else
        {
            // 不启用残影效果时，两个血条同步更新
            shadowFill.fillAmount = newFillAmount;
        }
    }
    
    private void HandleHealthChanged(int currentHP, int maxHP)
    {
        UpdateHealthBar(currentHP, maxHP);
        
        // 根据血量决定血条显示状态
        if (currentHP > 0 && currentHP < maxHP)
        {
            // 血量不满时显示血条
            ShowHealthBar();
        }
        else if (currentHP >= maxHP)
        {
            // 满血时隐藏血条
            HideHealthBar();
        }
    }
    
    private void HandleTakeDamage(int damageAmount)
    {
        // 受伤时显示血条（此时肯定不是满血）
        ShowHealthBar();
    }
    
    private void HandleHealthFull()
    {
        // 满血时立即隐藏血条，并同步两个血条
        mainFill.fillAmount = shadowFill.fillAmount = 1f;
        HideHealthBar();
        
        // 停止残影动画
        if (shadowCoroutine != null)
        {
            StopCoroutine(shadowCoroutine);
            shadowCoroutine = null;
        }
    }
    
    private void HandleDeath()
    {
        HideHealthBar();
        
        if (shadowCoroutine != null)
        {
            StopCoroutine(shadowCoroutine);
            shadowCoroutine = null;
        }
    }
    
    /// <summary>
    /// 显示血条
    /// </summary>
    public void ShowHealthBar()
    {
        healthBar.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏血条
    /// </summary>
    public void HideHealthBar()
    {
        healthBar.SetActive(false);
    }
    
    /// <summary>
    /// 残影跟随协程
    /// </summary>
    private IEnumerator ShadowFollowCoroutine()
    {
        isShadowAnimating = true;
        
        // 延迟一段时间再开始跟随
        yield return new WaitForSeconds(shadowDelayTime);
        
        // 平滑跟随到目标值
        while (Mathf.Abs(shadowFill.fillAmount - targetFillAmount) > 0.01f)
        {
            shadowFill.fillAmount = Mathf.MoveTowards(
                shadowFill.fillAmount, 
                targetFillAmount, 
                shadowSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        // 确保最终值精确
        shadowFill.fillAmount = targetFillAmount;
        isShadowAnimating = false;
        shadowCoroutine = null;
    }
    
    /// <summary>
    /// 重置血条状态
    /// </summary>
    public void ResetHealthBar()
    {
        if (healthComponent != null)
        {
            float currentFill = healthComponent.HealthPercentage;
            mainFill.fillAmount = currentFill;
            shadowFill.fillAmount = currentFill;
            
            if (shadowCoroutine != null)
            {
                StopCoroutine(shadowCoroutine);
                shadowCoroutine = null;
            }
            
            // 根据当前血量决定显示状态
            if (healthComponent.IsFullHealth)
            {
                HideHealthBar();
            }
            else if (healthComponent.IsAlive)
            {
                ShowHealthBar();
            }
        }
    }
    
}