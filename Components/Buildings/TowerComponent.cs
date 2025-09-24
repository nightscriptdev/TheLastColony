using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Core;
using Data.Buildings;
using Enums;
using Interface;
using Managers;

namespace Components.Buildings
{
    public class TowerComponent : MonoBehaviour, IInfoProvider
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private ProjectileComponent projectilePrefab;
        [SerializeField] private GameObject pulseLinePrefab;
        [SerializeField] private GameObject rangeIndicator;
        [SerializeField] private GameObject impactEffect;
        [SerializeField] private AttackRangeTrigger attackRangeTrigger;
        
        private BuildingComponent buildingComponent;
        private Coroutine attackCoroutine;
        private void Awake()
        {
            buildingComponent = GetComponent<BuildingComponent>();
            if (firePoint == null)
                firePoint = transform;
            if(rangeIndicator)
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
            UpdateAttackRange();
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
                yield return null;
                while (attackRangeTrigger.enemiesInRange.Count > 0)
                {
                    AttackNearestEnemyInRange();
                    yield return new WaitForSeconds(buildingComponent.LevelData.AttackInterval);
                }
            }
        }
        
        private void AttackNearestEnemyInRange()
        {
            var nearestEnemy = attackRangeTrigger.GetNearestEnemyInRange();
            if (!nearestEnemy) return;
            
            var levelData = buildingComponent.LevelData;
            int damage = buildingComponent.Data.GetRandomDamage(buildingComponent.Level);
            switch (levelData.AttackType)
            {
                case TowerAttackType.Single:
                    SingleTargetAttack(nearestEnemy.center, damage, levelData);
                    break;
                case TowerAttackType.Scatter:
                    ScatterAttack(nearestEnemy.center, damage, levelData);
                    break;
                case TowerAttackType.Piercing:
                    PiercingAttack(nearestEnemy.center, damage, levelData);
                    break;
                case TowerAttackType.Area:
                    BeamAttack(nearestEnemy.center, damage, levelData);
                    break;
            }
        }

        private void SingleTargetAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            var projectile = PoolingManager.Instance.GetProjectile(projectilePrefab);
            projectile.transform.SetPositionAndRotation(firePoint.position, Quaternion.FromToRotation(Vector3.right, target.position - firePoint.position));
            projectile.Initialize(damage, levelData.HasSlowEffect);
        }

        private void ScatterAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            int shotCount = levelData.MultiShotCount;
            float angleStep = 15f;
            float startAngle = -(angleStep * (shotCount - 1)) / 2f;
    
            Vector3 baseDirection = (target.position - firePoint.position).normalized;
    
            for (int i = 0; i < shotCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.forward) * baseDirection;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.right, direction);
        
                var projectile = PoolingManager.Instance.GetProjectile(projectilePrefab);
                projectile.transform.SetPositionAndRotation(firePoint.position, rotation);
                projectile.Initialize(damage, levelData.HasSlowEffect);
            }
        }

        private void PiercingAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            var projectile = PoolingManager.Instance.GetProjectile(projectilePrefab);
            projectile.transform.SetPositionAndRotation(firePoint.position, Quaternion.FromToRotation(Vector3.right, target.position - firePoint.position));
            projectile.Initialize(damage, levelData.HasSlowEffect, levelData.PierceCount);
        }

        private void BeamAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            PerformSingleBeamAttack(target, damage, levelData);
    
            if (levelData.ConsecutiveAttacks > 1)
            {
                StartCoroutine(ConsecutiveBeamAttack(target, damage, levelData.ConsecutiveAttacks - 1));
            }
        }

        private void PerformSingleBeamAttack(Transform target, int damage, BuildingData.LevelData levelData)
        {
            Vector2 direction = (target.position - firePoint.position).normalized;
            float range = levelData.AttackRaidus;
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                firePoint.position,
                direction,
                range+0.03f,
                EnemyManager.Instance.EnemyLayerMask
            );
            
            foreach (var hit in hits)
            {
                hit.collider.GetComponent<HealthComponent>().TakeDamage(damage);
                
                if (impactEffect)
                {
                    PoolingManager.Instance.Get(impactEffect).transform.SetPositionAndRotation(hit.point, Quaternion.identity);
                }
            }
    
            Quaternion beamRotation = Quaternion.FromToRotation(Vector3.right, direction);
            PoolingManager.Instance.Get(pulseLinePrefab).transform.SetPositionAndRotation(firePoint.position, beamRotation);
        }

        private IEnumerator ConsecutiveBeamAttack(Transform target, int damage, int remainingAttacks)
        {
            yield return new WaitForSeconds(0.3f);
    
            for (int i = 0; i < remainingAttacks; i++)
            {
                if (!target) yield break;

                PerformSingleBeamAttack(target, damage, buildingComponent.LevelData);
                if (i < remainingAttacks - 1)
                    yield return new WaitForSeconds(0.3f);
            }
        }

        void UpdateAttackRange()
        {
            float range = buildingComponent.LevelData.AttackRaidus;
            attackRangeTrigger.collider2D.radius = range;
            range *= 2;
            rangeIndicator.transform.localScale = new Vector3(range, range, range);
        }
        
        public void ShowRange()
        {
            rangeIndicator.SetActive(true);
        }

        public void HideRange()
        {
            rangeIndicator.SetActive(false);
        }

        public string GetInfoText()
        {
            var loc = LocalizationManager.Instance;
            var levelData = buildingComponent.LevelData;
            var sb = new StringBuilder();

            sb.AppendLine($"{loc.GetGameText("combat.damage")} {levelData.MinDamage}-{levelData.MaxDamage}");
            sb.AppendLine(loc.GetGameText("combat.range", levelData.AttackRaidus));
            sb.AppendLine(loc.GetGameText("combat.attack_speed", levelData.AttackInterval));

            var effects = GetEffects(loc, levelData);
            if (effects.Count > 0)
            {
                sb.AppendLine();
                foreach (var effect in effects)
                {
                    sb.AppendLine(effect);
                }
            }

            return sb.ToString().TrimEnd('\n', '\r');
        }

        private List<string> GetEffects(LocalizationManager loc, BuildingData.LevelData levelData)
        {
            var effects = new List<string>();
    
            if (levelData.HasSlowEffect)
                effects.Add(loc.GetGameText("combat.effect.slow"));
    
            if (levelData.PierceCount > 1)
                effects.Add(loc.GetGameText("combat.effect.pierce", levelData.PierceCount));
    
            if (levelData.MultiShotCount > 1)
                effects.Add(loc.GetGameText("combat.effect.multishot", levelData.MultiShotCount));
    
            if (levelData.ConsecutiveAttacks > 1)
                effects.Add(loc.GetGameText("combat.effect.consecutive", levelData.ConsecutiveAttacks));
    
            return effects;
        }
    }
}