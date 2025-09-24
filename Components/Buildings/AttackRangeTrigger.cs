using System.Collections.Generic;
using Components.Enemies;
using Core;
using UnityEngine;

namespace Components.Buildings
{
    public class AttackRangeTrigger : MonoBehaviour
    {
        public List<EnemyComponent> enemiesInRange = new();
        public CircleCollider2D collider2D;
        private void OnEnable()
        {
            EventManager.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnDisable()
        {
            EventManager.OnEnemyDeath -= OnEnemyDeath;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            enemiesInRange.Add(other.GetComponent<EnemyComponent>());
        }
    
        void OnTriggerExit2D(Collider2D other)
        {
            enemiesInRange.Remove(other.GetComponent<EnemyComponent>());
        }
    
        void OnEnemyDeath(EnemyComponent enemy)
        {
            if (enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Remove(enemy);
            }
        }

        public EnemyComponent GetNearestEnemyInRange()
        {
            EnemyComponent nearestEnemy = null;
            float minDistance = float.MaxValue;

            for (var i = 0; i < enemiesInRange.Count; i++)
            {
                float distance = Vector2.Distance(transform.position, enemiesInRange[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemiesInRange[i];
                }
            }
            return nearestEnemy;
        }
    }
}