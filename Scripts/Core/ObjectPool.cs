using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> pool = new Queue<T>();
        private readonly HashSet<T> pooledObjects = new HashSet<T>(); // 用于快速检查
        private readonly Func<T> createFunc;
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;
        private readonly Action<T> onDestroy;
        private readonly int maxSize;
        private readonly bool collectionCheck;
        
        private int createdCount = 0; // 记录创建的总数

        /// <summary>
        /// 构造函数
        /// </summary>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));
            
            this.createFunc = createFunc;
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.onDestroy = onDestroy;
            this.collectionCheck = collectionCheck;
            this.maxSize = Math.Max(1, maxSize);

            // 可选：延迟初始化而不是预填充
            // PreFill(defaultCapacity);
        }

        /// <summary>
        /// 预填充对象池
        /// </summary>
        public void PreFill(int count)
        {
            for (int i = 0; i < count && pool.Count < maxSize; i++)
            {
                var obj = createFunc();
                if (obj != null)
                {
                    onRelease?.Invoke(obj);
                    pool.Enqueue(obj);
                    pooledObjects.Add(obj);
                    createdCount++;
                }
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Get()
        {
            T obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
                pooledObjects.Remove(obj);
            }
            else
            {
                obj = createFunc();
                if (obj == null)
                {
                    Debug.LogError("创建对象失败");
                    return null;
                }
                createdCount++;
            }

            try
            {
                onGet?.Invoke(obj);
            }
            catch (Exception e)
            {
                Debug.LogError($"onGet回调执行失败: {e.Message}");
            }
            
            return obj;
        }

        /// <summary>
        /// 释放对象回池
        /// </summary>
        public void Release(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("尝试释放空对象到对象池");
                return;
            }

            // 使用HashSet进行快速检查
            if (collectionCheck && pooledObjects.Contains(obj))
            {
                Debug.LogWarning($"对象 {obj.name} 已经在池中，避免重复释放");
                return;
            }

            try
            {
                onRelease?.Invoke(obj);
            }
            catch (Exception e)
            {
                Debug.LogError($"onRelease回调执行失败: {e.Message}");
            }
            
            if (pool.Count < maxSize)
            {
                pool.Enqueue(obj);
                pooledObjects.Add(obj);
            }
            else
            {
                // 池已满，销毁对象
                DestroyObject(obj);
            }
        }

        /// <summary>
        /// 强制释放对象（即使池满也尝试放入）
        /// </summary>
        public void ForceRelease(T obj)
        {
            if (obj == null) return;

            if (collectionCheck && pooledObjects.Contains(obj))
            {
                Debug.LogWarning($"对象 {obj.name} 已经在池中");
                return;
            }

            try
            {
                onRelease?.Invoke(obj);
                pool.Enqueue(obj);
                pooledObjects.Add(obj);
            }
            catch (Exception e)
            {
                Debug.LogError($"强制释放失败: {e.Message}");
                DestroyObject(obj);
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                DestroyObject(obj);
            }
            pooledObjects.Clear();
            createdCount = 0;
        }

        /// <summary>
        /// 清理无效对象
        /// </summary>
        public void CleanUp()
        {
            var validObjects = new Queue<T>();
            var validSet = new HashSet<T>();

            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null && obj.gameObject != null)
                {
                    validObjects.Enqueue(obj);
                    validSet.Add(obj);
                }
                else
                {
                    createdCount--;
                }
            }

            pool.Clear();
            pooledObjects.Clear();
            
            while (validObjects.Count > 0)
            {
                var obj = validObjects.Dequeue();
                pool.Enqueue(obj);
                pooledObjects.Add(obj);
            }
        }

        /// <summary>
        /// 销毁指定数量的池中对象以释放内存
        /// </summary>
        public void Trim(int targetCount)
        {
            targetCount = Math.Max(0, targetCount);
            
            while (pool.Count > targetCount)
            {
                var obj = pool.Dequeue();
                pooledObjects.Remove(obj);
                DestroyObject(obj);
            }
        }

        private void DestroyObject(T obj)
        {
            try
            {
                onDestroy?.Invoke(obj);
                if (obj != null && obj.gameObject != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
                createdCount--;
            }
            catch (Exception e)
            {
                Debug.LogError($"销毁对象失败: {e.Message}");
            }
        }

        /// <summary>
        /// 获取池中缓存的对象数量
        /// </summary>
        public int CountInactive => pool.Count;

        /// <summary>
        /// 获取活跃的对象数量
        /// </summary>
        public int CountActive => createdCount - pool.Count;

        /// <summary>
        /// 获取创建的总对象数量
        /// </summary>
        public int CountAll => createdCount;

        /// <summary>
        /// 获取池的最大容量
        /// </summary>
        public int MaxSize => maxSize;

        /// <summary>
        /// 检查对象是否在池中
        /// </summary>
        public bool Contains(T obj)
        {
            if (obj == null) return false;
            return pooledObjects.Contains(obj);
        }

        /// <summary>
        /// 获取池的使用率 (0-1)
        /// </summary>
        public float UtilizationRate => maxSize > 0 ? (float)pool.Count / maxSize : 0f;

        /// <summary>
        /// 检查池是否为空
        /// </summary>
        public bool IsEmpty => pool.Count == 0;

        /// <summary>
        /// 检查池是否已满
        /// </summary>
        public bool IsFull => pool.Count >= maxSize;
    }
}