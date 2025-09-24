using System.Collections.Generic;
using Components.Enemies;
using Core;
using UnityEngine;

namespace Managers
{
    public class EnemyManager : MonoSingleton<EnemyManager>
    {
        public LayerMask EnemyLayerMask = -1;

        
        public List<EnemyComponent> Enemies = new List<EnemyComponent>();


        private void OnEnable()
        {
            EventManager.OnEnemyDeath += OnEnemyDeath;
        }
        private void OnDisable()
        {
            EventManager.OnEnemyDeath -= OnEnemyDeath;
        }

        private void OnEnemyDeath(EnemyComponent enemyComponent)
        {
            Enemies.Remove(enemyComponent);
        }
    }
}