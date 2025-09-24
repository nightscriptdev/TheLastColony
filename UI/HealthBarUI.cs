using UnityEngine;
using UnityEngine.UI;
using Components;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;
    [SerializeField] private Image background;
    [SerializeField] private Image mainFill; // 主血条
    [SerializeField] private Image shadowFill; // 残影血条
    [SerializeField] private HealthComponent healthComponent;
    
    [Header("残影效果设置")]
    [SerializeField] private float shadowDelayTime = 0.5f;
    [SerializeField] private float shadowSpeed = 2f;
    [SerializeField] private bool enableShadowEffect = true;
    
    private float targetFillAmount;
    private bool isShadowAnimating = false;
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
        
        mainFill.fillAmount = newFillAmount;
        
        if (enableShadowEffect)
        {
            if (newFillAmount < shadowFill.fillAmount)
            {
                if (shadowCoroutine != null)
                {
                    StopCoroutine(shadowCoroutine);
                }
                shadowCoroutine = StartCoroutine(ShadowFollowCoroutine());
            }
            else if (newFillAmount > shadowFill.fillAmount)
            {
                shadowFill.fillAmount = newFillAmount;
            }
        }
        else
        {
            shadowFill.fillAmount = newFillAmount;
        }
    }
    
    private void HandleHealthChanged(int currentHP, int maxHP)
    {
        UpdateHealthBar(currentHP, maxHP);
        
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
        ShowHealthBar();
    }
    
    private void HandleHealthFull()
    {
        mainFill.fillAmount = shadowFill.fillAmount = 1f;
        HideHealthBar();
        
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
    
    public void ShowHealthBar()
    {
        healthBar.SetActive(true);
    }
    
    public void HideHealthBar()
    {
        healthBar.SetActive(false);
    }
    
    private IEnumerator ShadowFollowCoroutine()
    {
        isShadowAnimating = true;
        
        yield return new WaitForSeconds(shadowDelayTime);
        
        while (Mathf.Abs(shadowFill.fillAmount - targetFillAmount) > 0.01f)
        {
            shadowFill.fillAmount = Mathf.MoveTowards(
                shadowFill.fillAmount, 
                targetFillAmount, 
                shadowSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        shadowFill.fillAmount = targetFillAmount;
        isShadowAnimating = false;
        shadowCoroutine = null;
    }
    
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