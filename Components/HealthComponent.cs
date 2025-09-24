using UnityEngine;
using System;

namespace Components
{
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField] private int maxHP = 100;
        private int currentHP;

        public event Action<int, int> OnHealthChanged; // (currentHP, maxHP)
        public event Action<int> OnTakeDamage;       // (damageAmount)
        public event Action OnDeath;
        public event Action OnHealthFull;

        public bool IsAlive => currentHP > 0;
        public bool IsFullHealth => currentHP >= maxHP;
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public float HealthPercentage => maxHP > 0 ? (float)currentHP / maxHP : 0f;

        private bool isDead = false;

        private void Awake()
        {
            currentHP = maxHP;
        }

        public void SetMaxHP(int newMaxHP, bool healToFull = false)
        {
            maxHP = newMaxHP;
            if (healToFull)
            {
                currentHP = maxHP;
            }
            else
            {
                currentHP = Mathf.Min(currentHP, maxHP);
            }
            
            OnHealthChanged?.Invoke(currentHP, maxHP);
            if (IsFullHealth)
            {
                OnHealthFull?.Invoke();
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead || damage <= 0) return;

            currentHP = Mathf.Max(0, currentHP - damage);
            
            OnTakeDamage?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHP, maxHP);
            
            if (currentHP <= 0)
            {
                isDead = true;
                OnDeath?.Invoke();
            }
        }

        public void Heal(int healAmount)
        {
            if (isDead || healAmount <= 0 || IsFullHealth) return;

            bool wasFullHealth = IsFullHealth;
            currentHP = Mathf.Min(maxHP, currentHP + healAmount);
            
            OnHealthChanged?.Invoke(currentHP, maxHP);
            
            if (IsFullHealth && !wasFullHealth)
            {
                OnHealthFull?.Invoke();
            }
        }

        public void HealToFull()
        {
            Heal(maxHP - currentHP);
        }

        public void SetCurrentHP(int newCurrentHP)
        {
            currentHP = Mathf.Clamp(newCurrentHP, 0, maxHP);
            OnHealthChanged?.Invoke(currentHP, maxHP);

            if (IsFullHealth) {
                OnHealthFull?.Invoke();
            }
        }

        public void Kill()
        {
            TakeDamage(currentHP);
        }
    }
}