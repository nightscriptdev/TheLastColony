using System;
using System.Collections;
using System.Collections.Generic;
using Components.Buildings;
using Core;
using Core.Grid;
using Data;
using Managers;
using UnityEngine;
using Enums;
using Game;

namespace Components.Enemies
{
    /// <summary>
    /// 敌人组件 - 控制敌人的所有行为
    /// </summary>
    public class EnemyComponent : MonoBehaviour
    {
        public Transform center;
        public SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        
        [Header("AI 参数")]
        [SerializeField] private float aiUpdateFrequency = 0.2f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldownTime = 2f;
        
        [Header("分离行为参数")]
        [SerializeField] private float separationRadius = 0.5f;
        [SerializeField] private float separationForce = 1f;
        [SerializeField] [Range(0, 1)] private float separationWeight = 0.5f; // 分离力占移动方向的权重

        private EnemyData enemyData;
        private HealthComponent healthComponent;
        private float currentSpeed;
        private Dictionary<Type, StatusEffect> statusEffects = new Dictionary<Type, StatusEffect>();
        
        private BuildingComponent currentTarget;
        private List<Vector3> currentPath;
        private int currentPathIndex;
        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");
        
        private Vector3? occupiedPosition = null;
        private Collider2D[] nearbyColliders = new Collider2D[10];
        
        private enum EnemyState
        {
            Idle,
            Moving,
            Attacking,
            Dead
        }
        private EnemyState currentState = EnemyState.Idle;
        
        private float attackCooldown = 0f;
        private bool isAttacking = false;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            if (healthComponent == null)
                healthComponent = gameObject.AddComponent<HealthComponent>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null)
                animator = GetComponent<Animator>();
            if(center == null)
                center = transform.GetChild(0);
        }
        
        private void OnEnable()
        {
            healthComponent.OnDeath += OnEnemyDeath;
            healthComponent.OnTakeDamage += OnTakeDamage;
            EventManager.OnDayStart += OnDayStart;
            EventManager.OnCellBecomeObstacle += OnCellBecameObstacle;
        }
        
        private void OnDisable()
        {
            healthComponent.OnDeath -= OnEnemyDeath;
            healthComponent.OnTakeDamage -= OnTakeDamage;
            EventManager.OnDayStart -= OnDayStart;
            EventManager.OnCellBecomeObstacle -= OnCellBecameObstacle;
        }
        
        public void Initialize(EnemyData data, int currentDay)
        {
            enemyData = data;
            
            int hp = enemyData.GetHPForDay(currentDay);
            healthComponent.SetMaxHP(hp, true);
            
            currentSpeed = enemyData.baseSpeed;

            isAttacking = false;
            attackCooldown = 0f;
            currentPath = null;
            currentTarget = null;
            
            statusEffects.Clear();
            RecalculateStats();
            
            StopAllCoroutines();
            StartCoroutine(AILoop());

            currentState = EnemyState.Idle;
        }
        
        private IEnumerator AILoop()
        {
            while (currentState != EnemyState.Dead)
            {
                yield return new WaitForSeconds(aiUpdateFrequency);
                
                UpdateTarget();
                
                if (currentTarget)
                {
                    if (Vector2.Distance(transform.position, currentTarget.transform.position) <= attackRange)
                    {
                        currentState = EnemyState.Attacking;
                        continue;
                    }
                }
                currentState = EnemyState.Moving;
            }
        }
        
        private void Update()
        {
            if (currentState == EnemyState.Dead) return;
            
            UpdateStatusEffects(Time.deltaTime);
            
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }
            
            switch (currentState)
            {
                case EnemyState.Moving:
                    MoveAlongPath();
                    break;
                case EnemyState.Attacking:
                    TryAttack();
                    break;
            }
            
        }
        
        public void ApplyStatusEffect(StatusEffect newEffect)
        {
            Type effectType = newEffect.GetType();

            if (statusEffects.TryGetValue(effectType, out StatusEffect existingEffect))
            {
                // 效果已存在，刷新持续时间
                // 也可以根据游戏设计决定是取效果强的，还是叠加等
                existingEffect.remainingTime = newEffect.duration;
            
                // 如果新效果的参数不同（例如减速倍率），则替换掉旧的
                if (newEffect is SlowEffect newSlow && existingEffect is SlowEffect oldSlow)
                {
                    if (newSlow.slowMultiplier < oldSlow.slowMultiplier) // 假设乘数越小效果越强
                    {
                        statusEffects[effectType] = newEffect;
                    }
                }
            }
            else
            {
                // 效果不存在，添加并应用
                statusEffects.Add(effectType, newEffect);
                newEffect.Apply(this);
            }
        }
        
        private void UpdateStatusEffects(float deltaTime)
        {
            if (statusEffects.Count == 0) return;

            List<Type> effectsToRemove = new List<Type>();
            foreach (var kvp in statusEffects)
            {
                kvp.Value.Update(deltaTime);
                if (kvp.Value.remainingTime <= 0)
                {
                    effectsToRemove.Add(kvp.Key);
                }
            }
        
            // 统一移除过期的效果
            foreach (var type in effectsToRemove)
            {
                if (statusEffects.TryGetValue(type, out StatusEffect effect))
                {
                    effect.Remove(this);
                    statusEffects.Remove(type);
                }
            }
        }
        
        public void RecalculateStats()
        {
            // --- 数值计算 ---
            float speedMultiplier = 1f; // 用于跟踪总的速度倍率
            currentSpeed = enemyData.baseSpeed;
    
            bool isSlowed = false;

            // 遍历所有当前激活的效果
            foreach (var effect in statusEffects.Values)
            {
                if (effect is SlowEffect slow)
                {
                    float effectMultiplier = slow.slowMultiplier;
                    currentSpeed *= effectMultiplier;
                    speedMultiplier *= effectMultiplier; // 累积速度倍率
                    isSlowed = true;
                }
            }
    
            spriteRenderer.color = isSlowed ? new Color(0.3f, 0.3f, 1f) : Color.white;
    
            if (animator != null)
            {
                animator.speed = speedMultiplier;
            }
        }

        private void UpdateTarget()
        {
            if (!currentTarget || !currentTarget.gameObject.activeSelf)
            {
                currentPath = GridManager.Instance.GetNearestPathToTarget(this, out currentTarget);
                if (currentPath != null)
                {
                    currentPathIndex = 0;
                    ReleaseGrid();
                }
            }
        }

        void ReleaseGrid()
        {
            if (occupiedPosition.HasValue)
            {
                EventManager.OnGridRelease?.Invoke(occupiedPosition.Value);
                occupiedPosition = null;
            }
        }
        
        private void MoveAlongPath()
        {
            if (currentPath == null || currentPathIndex >= currentPath.Count)
            {
                return;
            }
            
            SetMovingAnimation(true);
            
            // 判断是否和路径的第一个点在同一个格子
            if (currentPathIndex == 0)
            {
                if (GridManager.Instance.WorldToGrid(transform.position) == GridManager.Instance.WorldToGrid(currentPath[0]))
                {
                    currentPathIndex++;
                    if (currentPathIndex >= currentPath.Count) return;
                }
            }
            
            Vector3 targetPos = currentPath[currentPathIndex];

            if (occupiedPosition == null && currentPathIndex == currentPath.Count - 1)
            {
                occupiedPosition = targetPos;
                EventManager.OnEnemyStayed?.Invoke(targetPos);
            }
            
            Vector3 moveDirection = (targetPos - transform.position).normalized;
            Vector3 separation = CalculateSeparationForce();
            moveDirection = (moveDirection + separation * separationWeight).normalized;
                
            
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            
            if(currentTarget)
                spriteRenderer.flipX = transform.position.x > currentTarget.transform.position.x;
            
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                currentPathIndex++;
            }
        }
        private Vector3 CalculateSeparationForce()
        {
            Vector3 force = Vector3.zero;
            
            int colliderCount = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, nearbyColliders, EnemyManager.Instance.EnemyLayerMask);
            int count = 0;
            
            for (int i = 0; i < colliderCount; i++)
            {
                var collider = nearbyColliders[i];
                
                if (collider.transform == transform) continue;
                Vector3 diff = transform.position - collider.transform.position;
                if (diff.magnitude > 0)
                {
                    force += diff.normalized / diff.magnitude;
                    count++;
                }
            }
            
            if (count > 0)
            {
                force /= count;
                force *= separationForce; 
            }
            return force;
        }
        private void TryAttack()
        {
            if (attackCooldown > 0 || isAttacking || currentTarget == null)
            {
                return;
            }
            
            SetMovingAnimation(false);
            
            if (Vector2.Distance(transform.position, currentTarget.transform.position) > attackRange)
            {
                currentState = EnemyState.Moving;
                return;
            }
            isAttacking = true;
            StartCoroutine(AttackCoroutine());
        }
        private IEnumerator AttackCoroutine()
        {
            attackCooldown = attackCooldownTime;
            
            Vector3 originalPos = transform.position;
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 attackPos = originalPos + (targetPos - originalPos) * 0.8f;
            //Vector3 attackPos = targetPos;
            
            float attackAnimTime = 0.1f;
            float timer = 0;
            while (timer < attackAnimTime)
            {
                transform.position = Vector3.Lerp(originalPos, attackPos, timer / attackAnimTime);
                timer += Time.deltaTime;
                yield return null;
            }
            
            if (currentTarget != null)
            {
                var targetHealth = currentTarget.GetComponent<HealthComponent>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(enemyData.attackDamage);
                    
                    if (currentTarget.Data.BuildingType == BuildingType.DefenseCrystal)
                    {
                        healthComponent.TakeDamage(enemyData.attackDamage);
                    }
                }
            }
            
            timer = 0;
            while (timer < attackAnimTime)
            {
                transform.position = Vector3.Lerp(attackPos, originalPos, timer / attackAnimTime);
                timer += Time.deltaTime;
                yield return null;
            }
            
            transform.position = originalPos;
            isAttacking = false;
        }
        
        private void OnTakeDamage(int damage)
        {
            FloatingTextManager.Instance.ShowDamage(damage, center.position, false);
            StartCoroutine(DamageFlash());
        }
        
        IEnumerator DamageFlash()
        {
            spriteRenderer. material.SetColor(FlashColorID, Color.red);
            spriteRenderer. material.SetFloat(FlashAmountID, 1f);
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.material.SetFloat(FlashAmountID, 0f);
        }
        
        private void OnEnemyDeath()
        {
            if (currentState == EnemyState.Dead) return; // 防止重复调用
            currentState = EnemyState.Dead;
            
            if (enemyData.deathEffectPrefab != null)
            {
                Destroy(Instantiate(enemyData.deathEffectPrefab, transform.position, Quaternion.identity), 0.5f);
            }
            StopAllCoroutines();
            ReleaseGrid();
            EventManager.OnEnemyDeath?.Invoke(this);
            Destroy(gameObject);
            
        }
        
        private void OnDayStart(int day)
        {
            OnEnemyDeath(); // 内部防止重复调用
        }
        
        private void SetMovingAnimation(bool isMoving)
        {
            animator.SetBool("IsMoving", isMoving);
        }
        
        public void InstantKill()
        {
            OnEnemyDeath();
        }

        void OnCellBecameObstacle(Vector3 pos)
        {
            if(currentPath == null || occupiedPosition!=null) return;
            
            if (currentPath.Contains(pos))
            {
                currentTarget = null;
                UpdateTarget();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, separationRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            Gizmos.color = Color.green;
            if (currentPath != null)
            {
                for (var i = 0; i < currentPath.Count; i++)
                {
                    Gizmos.DrawSphere(currentPath[i], 0.25f);
                }
            }
        }
    }
}