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

        private GameObject beamEffect;
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
                var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.FromToRotation(firePoint.up, target.position - firePoint.position));
                var projectileScript = projectile.GetComponent<ProjectileComponent>();
                if (projectileScript != null)
                {
                    projectileScript.InitializeWithDirection(damage, levelData.HasSlowEffect);
                    //projectileScript.Initialize(target, damage, levelData.HasSlowEffect);
                }
            }
            else
            {
                // 直接造成伤害（如果没有子弹预制体）
                DamageEnemy(target, damage);
            }
        }

        private void ScatterAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            if (projectilePrefab != null)
            {
                int shotCount = levelData.MultiShotCount;
                float angleStep = 30f; // 扇形角度
                float startAngle = -(angleStep * (shotCount - 1)) / 2f;
                for (int i = 0; i < shotCount; i++)
                {
                    float angle = startAngle + angleStep * i;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * (target.position - firePoint.position).normalized;

                    var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                    var projectileScript = projectile.GetComponent<ProjectileComponent>();
                    if (projectileScript != null)
                    {
                        projectileScript.InitializeWithDirection( damage, levelData.HasSlowEffect);
                    }
                }
            }
        }

        private void PiercingAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            if (projectilePrefab != null)
            {
                var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                var projectileScript = projectile.GetComponent<ProjectileComponent>();
                if (projectileScript != null)
                {
                    projectileScript.Initialize(target, damage, levelData.HasSlowEffect, levelData.PierceCount);
                }
            }
        }

        private void BeamAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            Vector2 direction = (target.position - firePoint.position).normalized;
            float range = levelData.AttackRange;

            // 使用射线检测所有敌人
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                firePoint.position,
                direction,
                range,
                LayerMask.GetMask("Enemy")
            );
            // 对所有命中的敌人造成伤害
            foreach (var hit in hits)
            {
                DamageEnemy(hit.transform, damage);
            }

            // 显示射线特效
            StartCoroutine(ShowBeamEffect(firePoint.position, firePoint.position + (Vector3) (direction * range)));

            // 3级白色塔连续攻击
            if (levelData.ConsecutiveAttacks > 1)
            {
                StartCoroutine(ConsecutiveBeamAttack(target, damage, levelData.ConsecutiveAttacks - 1));
            }
        }

        private IEnumerator ConsecutiveBeamAttack(Transform target, int damage, int remainingAttacks)
        {
            yield return new WaitForSeconds(0.3f); // 连击间隔
            for (int i = 0; i < remainingAttacks; i++)
            {
                if (target != null) // 确保目标还存在
                {
                    BeamAttack(target, damage, buildingComponent.Data.LevelDatas[buildingComponent.LevelIndex]);
                    if (i < remainingAttacks - 1)
                        yield return new WaitForSeconds(0.3f);
                }
            }
        }

        private IEnumerator ShowBeamEffect(Vector3 start, Vector3 end)
        {
            if (beamEffect != null)
            {

                beamEffect.SetActive(true);
                yield break;
            }
        }

        private void DamageEnemy(Transform enemy, int damage)
        {
            // TODO: 这里需要接入敌人的HP系统
            // var enemyHP = enemy.GetComponent<HPComponent>();
            // if (enemyHP != null)
            // {
            //     enemyHP.TakeDamage(damage);
            // }

            Debug.Log($"对敌人 {enemy.name} 造成 {damage} 点伤害");
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