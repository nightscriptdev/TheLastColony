using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 对象池管理器 - 管理所有的对象池
    /// </summary>
    public class PoolingManager : MonoSingleton<PoolingManager>
    {
        [Header("对象池设置")]
        [SerializeField] private Transform poolParent;
        
        private Dictionary<string, object> pools = new Dictionary<string, object>();
        
        protected override void Awake()
        {
            base.Awake();
            
            // 创建对象池父对象
            if (poolParent == null)
            {
                GameObject poolObj = new GameObject("ObjectPools");
                poolObj.transform.SetParent(transform);
                poolParent = poolObj.transform;
            }
        }
        
        /// <summary>
        /// 创建或获取对象池
        /// </summary>
        public ObjectPool<T> CreatePool<T>(
            string poolName, 
            T prefab, 
            int defaultCapacity = 10, 
            int maxSize = 100) where T : Component
        {
            if (pools.ContainsKey(poolName))
            {
                return pools[poolName] as ObjectPool<T>;
            }
            
            // 为这个池创建专用父对象
            GameObject poolContainer = new GameObject($"Pool_{poolName}");
            poolContainer.transform.SetParent(poolParent);
            
            var pool = new ObjectPool<T>(
                createFunc: () => {
                    var obj = Instantiate(prefab, poolContainer.transform);
                    obj.name = $"{poolName}_Instance";
                    return obj;
                },
                onGet: (obj) => {
                    obj.gameObject.SetActive(true);
                },
                onRelease: (obj) => {
                    obj.gameObject.SetActive(false);
                    obj.transform.SetParent(poolContainer.transform);
                },
                onDestroy: null,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
            
            pools[poolName] = pool;
            return pool;
        }
        
        /// <summary>
        /// 获取已存在的对象池
        /// </summary>
        public ObjectPool<T> GetPool<T>(string poolName) where T : Component
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                return pool as ObjectPool<T>;
            }
            
            Debug.LogWarning($"对象池 {poolName} 不存在");
            return null;
        }
        
        /// <summary>
        /// 清理所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                if (pool is ObjectPool<Component> componentPool)
                {
                    componentPool.Clear();
                }
            }
            pools.Clear();
        }
        
        /// <summary>
        /// 清理指定对象池
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                if (pool is ObjectPool<Component> componentPool)
                {
                    componentPool.Clear();
                }
                pools.Remove(poolName);
            }
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}