using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Data.Buildings;
using Enums;
using Managers;

namespace Components.Buildings
{
    /// <summary>
    /// 魔法塔战斗组件
    /// </summary>
    public class TowerComponent : MonoBehaviour
    {
        [Header("攻击设置")] [SerializeField] private Transform firePoint; // 发射点
        [Header("子弹预制体")] [SerializeField] private GameObject projectilePrefab; // 子弹
        [Header("范围显示")] [SerializeField] private GameObject rangeIndicator; // 攻击范围指示器
        [SerializeField] private GameObject impactEffect;

        
        //[SerializeField] private GameObject beamEffect;
        private BuildingComponent buildingComponent;
        private Coroutine attackCoroutine;
        private List<Transform> enemiesInRange = new List<Transform>();

        private void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();

            // 如果没有设置发射点，使用建筑本身的位置
            if (firePoint == null)
                firePoint = transform;

            if(rangeIndicator != null)
                rangeIndicator.SetActive(false);
        }

        private void OnEnable()
        {
            if (buildingComponent != null)
            {
                buildingComponent.OnBuildingCompleted += StartAttacking;
                buildingComponent.OnBuildingDestroyed += StopAttacking;
            }
        }

        private void OnDisable()
        {
            if (buildingComponent != null)
            {
                buildingComponent.OnBuildingCompleted -= StartAttacking;
                buildingComponent.OnBuildingDestroyed -= StopAttacking;
            }
        }

        private void StartAttacking(BuildingComponent building)
        {
            if (attackCoroutine == null)
            {
                attackCoroutine = StartCoroutine(AttackLoop());
            }
        }

        private void StopAttacking(BuildingComponent building)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(buildingComponent.Data.LevelDatas[buildingComponent.LevelIndex].AttackInterval);
                AttackNearestEnemy();
            }
        }

        private void AttackNearestEnemy()
        {
            Transform target = EnemyManager.Instance.GetNearestEnemy(transform.position);
            if (target == null) return;
            var levelData = buildingComponent.Data.LevelDatas[buildingComponent.LevelIndex];
            if (Vector2.Distance(target.position, transform.position) > levelData.AttackRange) return;
            int damage = buildingComponent.Data.GetRandomDamage(buildingComponent.LevelIndex);
            switch (levelData.AttackType)
            {
                case TowerAttackType.Single:
                    SingleTargetAttack(target, damage, levelData);
                    break;
                case TowerAttackType.Scatter:
                    ScatterAttack(target, damage, levelData);
                    break;
                case TowerAttackType.Piercing:
                    PiercingAttack(target, damage, levelData);
                    break;
                case TowerAttackType.Area:
                    BeamAttack(target, damage, levelData);
                    break;
            }
        }

        private void SingleTargetAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            if (projectilePrefab != null)
            {
                // 发射单个子弹
                var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.FromToRotation(Vector3.right, target.position - firePoint.position));
                var projectileScript = projectile.GetComponent<ProjectileComponent>();
                if (projectileScript != null)
                {
                    projectileScript.Initialize(damage, levelData.HasSlowEffect);
                    //projectileScript.Initialize(target, damage, levelData.HasSlowEffect);
                }
            }
            else
            {
                // 直接造成伤害（如果没有子弹预制体）
            }
        }

        private void ScatterAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            if (projectilePrefab != null)
            {
                int shotCount = levelData.MultiShotCount;
                float angleStep = 15f;
                float startAngle = -(angleStep * (shotCount - 1)) / 2f;
        
                Vector3 baseDirection = (target.position - firePoint.position).normalized;
        
                for (int i = 0; i < shotCount; i++)
                {
                    float angle = startAngle + angleStep * i;
                    // 使用AngleAxis更清晰
                    Vector3 direction = Quaternion.AngleAxis(angle, Vector3.forward) * baseDirection;
            
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
            
                    var projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
                    var projectileScript = projectile.GetComponent<ProjectileComponent>();
                    projectileScript?.Initialize(damage, levelData.HasSlowEffect);
                }
            }
        }



        private void PiercingAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            if (projectilePrefab != null)
            {
                var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.FromToRotation(Vector3.right, target.position - firePoint.position));
                var projectileScript = projectile.GetComponent<ProjectileComponent>();
                if (projectileScript != null)
                {
                    projectileScript.Initialize(damage, levelData.HasSlowEffect, levelData.PierceCount);
                }
            }
        }

        private void BeamAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            // 执行单次射线攻击
            PerformSingleBeamAttack(target, damage, levelData);
    
            // 处理连击
            if (levelData.ConsecutiveAttacks > 1)
            {
                StartCoroutine(ConsecutiveBeamAttack(target, damage, levelData.ConsecutiveAttacks - 1));
            }
        }

        private void PerformSingleBeamAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            Vector2 direction = (target.position - firePoint.position).normalized;
            float range = levelData.AttackRange;

            RaycastHit2D[] hits = Physics2D.RaycastAll(
                firePoint.position,
                direction,
                range,
                EnemyManager.Instance.EnemyLayerMask
            );
            
            foreach (var hit in hits)
            {
                hit.collider.GetComponent<HealthComponent>().TakeDamage(damage);
                
                if (impactEffect != null)
                {
                    var obj = Instantiate(impactEffect, hit.point, Quaternion.identity);
                    Destroy(obj, 0.208f); 
                }
            }
    
            Quaternion beamRotation = Quaternion.FromToRotation(Vector3.right, direction);
            Destroy(Instantiate(projectilePrefab, firePoint.position, beamRotation), 0.333f);
        }

        private IEnumerator ConsecutiveBeamAttack(Transform target, int damage, int remainingAttacks)
        {
            yield return new WaitForSeconds(0.3f);
    
            for (int i = 0; i < remainingAttacks; i++)
            {
                if (target != null)
                {
                    // 只执行单次攻击，不触发新的连击
                    PerformSingleBeamAttack(target, damage, buildingComponent.Data.LevelDatas[buildingComponent.LevelIndex]);
                    if (i < remainingAttacks - 1)
                        yield return new WaitForSeconds(0.3f);
                }
            }
        }


        /// <summary>
        /// 显示攻击范围
        /// </summary>
        public void ShowRange()
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(true);

                // 设置范围大小
                float range = buildingComponent.Data.LevelDatas[buildingComponent.LevelIndex].AttackRange;
                rangeIndicator.transform.localScale = Vector3.one * range * 2; // 直径
            }
        }

        /// <summary>
        /// 隐藏攻击范围
        /// </summary>
        public void HideRange()
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(false);
            }
        }
    }
}