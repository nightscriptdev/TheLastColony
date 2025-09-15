using Components;
using Components.Enemies;
using Managers;
using UnityEngine;

namespace Game
{
    public class DarkImpactEffect : SkillEffect
    {
        public void OnHit()
        {
            var enemies = Physics2D.OverlapCircleAll(targetPosition, skillData.effectRadius, EnemyManager.Instance.EnemyLayerMask);
            foreach (var collider in enemies)
            {
                var enemy = collider.GetComponent<HealthComponent>();
                if (enemy != null)
                {
                    enemy.TakeDamage(skillData.baseDamage);
                }
            }
        }
    }
}