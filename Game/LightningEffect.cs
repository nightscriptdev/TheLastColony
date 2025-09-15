using Components;
using Managers;
using UnityEngine;

namespace Game
{
    public class LightningEffect : SkillEffect
    {
        public void OnHit()
        {
            var enemies = Physics2D.OverlapPointAll(targetPosition, EnemyManager.Instance.EnemyLayerMask);
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