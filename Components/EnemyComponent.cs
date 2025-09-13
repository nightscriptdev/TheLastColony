using System;
using System.Collections;
using System.Collections.Generic;
using Components.Buildings;
using Core;
using Core.Grid;
using Core.Pathfinding;
using Managers;
using UnityEngine;
using Data.Enemies;
using Enums;
namespace Components.Enemies
{
    /// <summary>
    /// 敌人组件 - 控制敌人的所有行为
    /// </summary>
    public class EnemyComponent : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        
        // REFACTOR: 将硬编码的数值暴露在Inspector中，方便调试和策划调整
        [Header("AI 参数")]
        [SerializeField] private float aiUpdateFrequency = 0.2f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float toleranceRange = 2.9f;
        [SerializeField] private float attackCooldownTime = 2f;
        [SerializeField] private float blockedTime = 1.5f;
        private float blockedTimer = 0;
        
        [Header("分离行为参数")]
        [SerializeField] private float separationRadius = 0.5f;
        [SerializeField] private float separationForce = 2f;
        [SerializeField] [Range(0, 1)] private float separationWeight = 0.3f; // 分离力占移动方向的权重
        [Header("运行时数据")]
        private EnemyData enemyData;
        private HealthComponent healthComponent;
        private BuildingComponent currentTarget;
        private List<Vector3> currentPath;
        private int currentPathIndex;
        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");
        
        private enum EnemyState
        {
            Idle,
            Moving,
            Attacking,
            Dead
        }
        private EnemyState currentState = EnemyState.Idle;
        
        private float currentSpeed;
        private float slowTimer = 0f;
        
        private float attackCooldown = 0f;
        private bool isAttacking = false;
        
        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                healthComponent = gameObject.AddComponent<HealthComponent>();
            }
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null)
                animator = GetComponent<Animator>();
        }
        
        private void OnEnable()
        {
            healthComponent.OnDeath += OnEnemyDeath;
            healthComponent.OnTakeDamage += OnTakeDamage;
            EventManager.OnDayStart += OnDayStart;
        }
        
        private void OnDisable()
        {
            healthComponent.OnDeath -= OnEnemyDeath;
            healthComponent.OnTakeDamage -= OnTakeDamage;
            EventManager.OnDayStart -= OnDayStart;
        }
        
        public void Initialize(EnemyData data, int currentDay)
        {
            enemyData = data;
            
            if (enemyData.idleSprite != null)
                spriteRenderer.sprite = enemyData.idleSprite;
            
            int hp = enemyData.GetHPForDay(currentDay);
            healthComponent.SetMaxHP(hp, true);
            
            currentSpeed = enemyData.moveSpeed;
            
            attackCooldown = 0f;
            isAttacking = false;
            slowTimer = 0f;
            currentPath = null;
            currentTarget = null;
            
            StopAllCoroutines();
            StartCoroutine(AILoop());
            SetState(EnemyState.Idle);
        }
        
        private int CalculateHP(int day)
        {
            return Mathf.RoundToInt(enemyData.baseHP * (1f + 0.1f * (day - 1)));
        }
        private void SetState(EnemyState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            // 可以在这里处理进入/退出状态的逻辑
        }
        private IEnumerator AILoop()
        {
            while (currentState != EnemyState.Dead)
            {
                yield return new WaitForSeconds(aiUpdateFrequency);
                
                UpdateTarget();
                
                if (currentTarget != null)
                {
                    float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
                    
                    if (distance <= attackRange)
                    {
                        SetState(EnemyState.Attacking);
                    }
                    else
                    {
                        SetState(EnemyState.Moving);
                        //UpdatePath(); // 只有在需要移动时才更新路径
                    }
                }
                else
                {
                    SetState(EnemyState.Idle);
                }
            }
        }
        
        private void Update()
        {
            if (currentState == EnemyState.Dead) return;
            
            UpdateSlowEffect();
            
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
            
            UpdateAnimation();
        }
        private void UpdateSlowEffect()
        {
            if (slowTimer > 0)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0)
                {
                    currentSpeed = enemyData.moveSpeed;
                }
            }
        }
        
        private void UpdateTarget()
        {
            if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            {
                currentPath = GridManager.Instance.GetNearestPathToTarget(this, out currentTarget);
                if (currentPath != null)
                {
                    currentPathIndex = 0; 
                }
            }
        }
        
        /// <summary>
        /// 寻找最近的目标
        /// </summary>
        /*private BuildingComponent FindNearestTarget()
        {
            var allBuildings = BuildingManager.Instance.AllBuildings;
            if (allBuildings == null || allBuildings.Count == 0) return null;
            
            BuildingComponent nearestBuilding = null;
            float shortestPathCost = float.MaxValue;
            foreach (var building in allBuildings)
            {
                if(building.Data.BuildingType == BuildingType.DefenseCrystal) continue; //先找非DefenseCrystal建筑
                PathfindingNode endNode;
                var path  = GridManager.Instance.FindPath(transform.position, building.transform.position, out endNode);
                
                if (endNode != null)
                {
                    if (endNode.gCost < shortestPathCost)
                    {
                        shortestPathCost = endNode.gCost;
                        nearestBuilding = building;
                        currentPath = path;
                    }
                }
            }
            if (nearestBuilding != null)
            {
                return nearestBuilding;
            }
            foreach (var building in allBuildings)
            {
                if (building.Data.BuildingType == BuildingType.DefenseCrystal) //只找DefenseCrystal建筑
                {
                    PathfindingNode endNode;
                    var path  = GridManager.Instance.FindPath(transform.position, building.transform.position, out endNode);
                
                    if (endNode != null)
                    {
                        if (endNode.gCost < shortestPathCost)
                        {
                            shortestPathCost = endNode.gCost;
                            nearestBuilding = building;
                            currentPath = path;
                        }
                    }
                }
            }
            return nearestBuilding;
        }*/
        
        private void UpdatePath()
        {
            if (currentTarget == null) return;
            
            PathfindingNode endNode;
            List<Vector3> path = GridManager.Instance.FindPath(transform.position, currentTarget.transform.position, out endNode);
            if(endNode != null)
            {
                currentPath = path;
                currentPathIndex = 0;
            }
            else
            {
                currentPath = null;
            }
        }
        
        private void MoveAlongPath()
        {
            if (currentPath == null || currentPathIndex >= currentPath.Count || currentTarget==null) return;
            Vector3 targetPos;
            if (Vector2.Distance(transform.position, currentTarget.transform.position) <= Single.MaxValue)
            {
                targetPos = currentTarget.transform.position;
            }
            else targetPos = currentPath[currentPathIndex];
            
            Vector3 moveDirection = (targetPos - transform.position).normalized;
            Vector3 separation = CalculateSeparationForce();
            moveDirection = (moveDirection + separation * separationWeight).normalized;
            
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            
            if (moveDirection.x != 0)
            {
                //spriteRenderer.flipX = moveDirection.x < 0;
                if (currentTarget != null)
                {
                    Vector3 overallDirection = (currentTarget.transform.position - transform.position).normalized;
                    if (overallDirection.x > 0.1f) spriteRenderer.flipX = true;
                    else if (overallDirection.x < -0.1f) spriteRenderer.flipX = false;
                }
            }
            
            
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                currentPathIndex++;
            }
        }
        private Vector3 CalculateSeparationForce()
        {
            Vector3 force = Vector3.zero;
            int count = 0;
            
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, separationRadius, EnemyManager.Instance.EnemyLayerMask);
            
            foreach (var collider in nearbyColliders)
            {
                if (collider.gameObject == gameObject) continue;
                
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
                force *= this.separationForce; 
            }
            return force;
        }
        private void TryAttack()
        {
            if (currentTarget == null || attackCooldown > 0 || isAttacking) return;
            
            if (Vector2.Distance(transform.position, currentTarget.transform.position) > attackRange)
            {
                SetState(EnemyState.Moving);
                return;
            }
            
            StartCoroutine(AttackCoroutine());
        }
        private IEnumerator AttackCoroutine()
        {
            isAttacking = true;
            attackCooldown = attackCooldownTime;
            
            Vector3 originalPos = transform.position;
            Vector3 targetPos = currentTarget.transform.position;
            //Vector3 attackPos = originalPos + (targetPos - originalPos).normalized * 0.5f;
            Vector3 attackPos = targetPos;
            
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
        
        public void ApplySlow(float duration = 2f, float slowAmount = 0.5f)
        {
            slowTimer = duration;
            currentSpeed = enemyData.moveSpeed * (1f - slowAmount);
        }
        
        private void OnTakeDamage(int damage)
        {
            FloatingTextManager.Instance.ShowDamage(damage, transform.position, false);
            StartCoroutine(DamageFlash());
        }
        
        IEnumerator DamageFlash()
        {
            /*spriteRenderer. material.SetColor(FlashColorID, Color.red);
            // 设置为完全闪白
            spriteRenderer. material.SetFloat(FlashAmountID, 1f);
            // 等待指定的持续时间
            yield return new WaitForSeconds(0.1f);
            // 恢复正常
            spriteRenderer.material.SetFloat(FlashAmountID, 0f);*/
            
            // 设置为完全闪白
            spriteRenderer.color = Color.red;
            // 等待指定的持续时间
            yield return new WaitForSeconds(0.1f);
            // 恢复正常
            spriteRenderer.color = Color.white;
        }
        
        private void OnEnemyDeath()
        {
            if (currentState == EnemyState.Dead) return; // 防止重复调用
            SetState(EnemyState.Dead);
            
            if (enemyData.deathEffectPrefab != null)
            {
                Destroy(Instantiate(enemyData.deathEffectPrefab, transform.position, Quaternion.identity), 0.5f);
            }
            
            // 延迟一帧销毁，以防其他对象在本帧还需要引用它
            Destroy(gameObject);
            EventManager.OnEnemyDeath?.Invoke(this);
        }
        
        private void OnDayStart(int day)
        {
            OnEnemyDeath(); // 内部防止重复调用
        }
        
        private void UpdateAnimation()
        {
            if (animator != null)
            {
                animator.SetBool("IsMoving", currentState == EnemyState.Moving);
                //animator.SetBool("IsAttacking", isAttacking);
            }
            else if (spriteRenderer != null) // Fallback to manual sprite swap
            {
                spriteRenderer.sprite = (currentState == EnemyState.Moving && enemyData.moveSprite != null) 
                    ? enemyData.moveSprite 
                    : enemyData.idleSprite;
            }
        }
        
        public void InstantKill()
        {
            OnEnemyDeath();
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, separationRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}