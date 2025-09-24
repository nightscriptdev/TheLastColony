using System.Collections;
using System.Collections.Generic;
using Components.Enemies;
using Core;
using Data;
using Managers;
using UnityEngine;

namespace Game
{
    public class EnemySpawner : MonoSingleton<EnemySpawner>
    {
        [SerializeField] private List<EnemyData> normalEnemyDataList = new List<EnemyData>();
        [SerializeField] private List<EnemyData> eliteEnemyDataList = new List<EnemyData>();
        
        [Header("生成设置")]
        [SerializeField] private int maxEnemyCount = 150;
        [SerializeField] private float baseSpawnInterval = 2f;
        [SerializeField] private float minSpawnInterval = 0.5f;
        [SerializeField] private float intervalDecreasePerDay = 0.02f;
        
        [Header("精英敌人设置")]
        [SerializeField] private float baseEliteChance = 0.05f;
        [SerializeField] private float eliteChanceIncreasePerDay = 0.01f;
        [SerializeField] private float maxEliteChance = 0.75f;
        
        [Header("生成区域设置")]
        [SerializeField] private SpawnZone[] spawnZones;
        [System.Serializable] 
        private class SpawnZone
        {
            public Vector2 min;
            public Vector2 max;
        }
        
        private List<EnemyData> currentNormalEnemies = new List<EnemyData>();
        private HashSet<EnemyData> usedNormalEnemies = new HashSet<EnemyData>();
        private EnemyData currentEliteEnemy;
        private HashSet<EnemyData> usedEliteEnemies = new HashSet<EnemyData>();
        
        private Coroutine spawnCoroutine;
        private int currentDay = 1;
        private float currentSpawnInterval;
        private float currentEliteChance;
        
        private List<SpawnZone> activeSpawnZones = new List<SpawnZone>();
        
        private void OnEnable()
        {
            EventManager.OnNightStart += OnNightStart;
            EventManager.OnDayStart += OnDayStart;
        }
        
        private void OnDisable()
        {
            EventManager.OnNightStart -= OnNightStart;
            EventManager.OnDayStart -= OnDayStart;
        }
        
        private void OnNightStart(int day)
        {
            currentDay = day;
            UpdateSpawnParameters();
            SelectEnemiesForTonight();
            GenerateSpawnZones();
            
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }
        
        private void OnDayStart(int day)
        {
            // 停止生成
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
        
        private void UpdateSpawnParameters()
        {
            currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentDay - 1) * intervalDecreasePerDay);
            
            // 更新精英概率：初始5%，每天+1%，上限75%
            if (currentDay >= 3) // 第3天才开始有精英
            {
                currentEliteChance = Mathf.Min(maxEliteChance, baseEliteChance + (currentDay - 3) * eliteChanceIncreasePerDay);
            }
            else
            {
                currentEliteChance = 0f;
            }
        }
        
        private void SelectEnemiesForTonight()
        {
            currentNormalEnemies.Clear();
            
            // 前两天：2种普通敌人
            if (currentDay <= 2)
            {
                SelectRandomNormalEnemies(2);
                currentEliteEnemy = null;
            }
            // 第3天起：2种普通敌人 + 1种精英敌人
            else
            {
                SelectRandomNormalEnemies(2);
                
                // 每天换一个普通敌人
                if (currentDay > 3)
                {
                    ReplaceOneNormalEnemy();
                }
                
                // 每两天换一次精英敌人
                if ((currentDay - 3) % 2 == 0 || currentEliteEnemy == null)
                {
                    SelectRandomEliteEnemy();
                }
            }
        }
        
        private void SelectRandomNormalEnemies(int count)
        {
            // 移除已使用的敌人
            List<EnemyData> availableEnemies = new List<EnemyData>();
            foreach (var enemyData in normalEnemyDataList)
            {
                if(usedNormalEnemies.Contains(enemyData)) continue;
                availableEnemies.Add(enemyData);
            }
            
            // 如果可用敌人不足，重置使用列表
            if (availableEnemies.Count < count)
            {
                usedNormalEnemies.Clear();
                availableEnemies = new List<EnemyData>(normalEnemyDataList);
            }
            
            // 随机选择
            for (int i = 0; i < count && availableEnemies.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableEnemies.Count);
                EnemyData selected = availableEnemies[randomIndex];
                currentNormalEnemies.Add(selected);
                usedNormalEnemies.Add(selected);
                availableEnemies.RemoveAt(randomIndex);
            }
        }
        
        private void ReplaceOneNormalEnemy()
        {
            if (currentNormalEnemies.Count == 0) return;
            
            int replaceIndex = Random.Range(0, currentNormalEnemies.Count);
            
            List<EnemyData> availableEnemies = new List<EnemyData>();
            foreach (var enemyData in normalEnemyDataList)
            {
                if( currentNormalEnemies.Contains(enemyData) ) continue;
                availableEnemies.Add(enemyData);
            }
            
            if (availableEnemies.Count > 0)
            {
                int randomIndex = Random.Range(0, availableEnemies.Count);
                currentNormalEnemies[replaceIndex] = availableEnemies[randomIndex];
                usedNormalEnemies.Add(availableEnemies[randomIndex]);
            }
        }
        
        private void SelectRandomEliteEnemy()
        {
            List<EnemyData> availableEnemies = new List<EnemyData>();
            
            foreach (var enemyData in eliteEnemyDataList)
            {
                if (usedEliteEnemies.Contains(enemyData)) continue;
                availableEnemies.Add(enemyData);
            }
            
            // 如果可用敌人不足，重置使用列表
            if (availableEnemies.Count == 0)
            {
                usedEliteEnemies.Clear();
                availableEnemies = new List<EnemyData>(eliteEnemyDataList);
            }
            
            if (availableEnemies.Count > 0)
            {
                currentEliteEnemy = availableEnemies[ Random.Range(0, availableEnemies.Count)];
                usedEliteEnemies.Add(currentEliteEnemy);
            }
        }
        
        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                if (EnemyManager.Instance.Enemies.Count < maxEnemyCount)
                {
                    SpawnEnemy();
                }
                yield return new WaitForSeconds(currentSpawnInterval);
            }
        }
        
        private void SpawnEnemy()
        {
            bool spawnElite = Random.value < currentEliteChance && currentEliteEnemy != null;
            
            EnemyData enemyToSpawn;
            if (spawnElite)
            {
                enemyToSpawn = currentEliteEnemy;
            }
            else if (currentNormalEnemies.Count > 0)
            {
                enemyToSpawn = currentNormalEnemies[Random.Range(0, currentNormalEnemies.Count)];
            }
            else
            {
                return;
            }
            
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            if (enemyToSpawn.enemyPrefab != null)
            {
                GameObject enemyGO = Instantiate(enemyToSpawn.enemyPrefab, spawnPosition, Quaternion.identity, transform);
                EnemyComponent enemy = enemyGO.GetComponent<EnemyComponent>();
                enemy.Initialize(enemyToSpawn, currentDay);
                EnemyManager.Instance.Enemies.Add(enemy);
            }
        }
        
        private void GenerateSpawnZones()
        {
            activeSpawnZones.Clear();
            
            // 随机选择生成区域数量
            int zoneCount = Random.Range(2, spawnZones.Length + 1);
            
            // 随机选择区域
            for (int i = 0; i < zoneCount; i++)
            {
                int randomIndex = Random.Range(0, spawnZones.Length);
                activeSpawnZones.Add(spawnZones[randomIndex]);
            }
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            SpawnZone zone = activeSpawnZones[Random.Range(0, activeSpawnZones.Count)];
            Vector3 spawnPos = new Vector3(Random.Range(zone.min.x, zone.max.x), Random.Range(zone.min.y, zone.max.y), 0);
            return spawnPos;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (spawnZones == null) return;
            Gizmos.color = Color.red;
            foreach (var zone in spawnZones)
            {
                Gizmos.DrawLine(new Vector3(zone.min.x, zone.min.y, 0), new Vector3(zone.max.x, zone.min.y, 0));
                Gizmos.DrawLine(new Vector3(zone.min.x, zone.max.y, 0), new Vector3(zone.max.x, zone.max.y, 0));
                Gizmos.DrawLine(new Vector3(zone.min.x, zone.min.y, 0), new Vector3(zone.min.x, zone.max.y, 0));
                Gizmos.DrawLine(new Vector3(zone.max.x, zone.min.y, 0), new Vector3(zone.max.x, zone.max.y, 0));
            }
        }
    }
}