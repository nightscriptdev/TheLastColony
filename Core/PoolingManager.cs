using System.Collections.Generic;
using Components;
using UI;
using UnityEngine;
using UnityEngine.Pool;

namespace Core
{
    public class PoolingManager : MonoSingleton<PoolingManager>
    {
        public bool collectionCheck = false;
        
        public int poolSize = 5;
        public int poolMaxSize = 20;
        
        public int floatingTextPoolSize = 10;
        public int floatingTextPoolMaxSize = 50;
        
        public int projectilePoolSize = 10;
        public int projectilePoolMaxSize = 50;
        
        private Dictionary<GameObject, IObjectPool<GameObject>> pools = new ();
        private Dictionary<GameObject, IObjectPool<GameObject>> instancePoolMap = new ();
        
        private Dictionary<FloatingText, IObjectPool<FloatingText>> floatingTextPools = new ();
        private Dictionary<FloatingText, IObjectPool<FloatingText>> instanceFloatingTextMap = new ();
        
        private Dictionary<ProjectileComponent, IObjectPool<ProjectileComponent>> projectilePools = new ();
        private Dictionary<ProjectileComponent, IObjectPool<ProjectileComponent>> instanceProjectileMap = new ();

#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            collectionCheck = true;
        }
#endif
        
        public GameObject Get(GameObject prefab)
        {
            if (!pools.ContainsKey(prefab))
            {
                var newPool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: obj => obj.SetActive(true),
                    actionOnRelease: obj => obj.SetActive(false),
                    actionOnDestroy: obj =>
                    {
                        instancePoolMap.Remove(obj);
                        Destroy(obj);
                    },
                    collectionCheck: collectionCheck,
                    defaultCapacity: poolSize,
                    maxSize: poolMaxSize
                );
                pools[prefab] = newPool;
            }

            var instance = pools[prefab].Get();
            instancePoolMap[instance] = pools[prefab];
            
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instancePoolMap.ContainsKey(instance))
            {
                instancePoolMap[instance].Release(instance);
                instancePoolMap.Remove(instance);
            }
        }
        
        public ProjectileComponent GetProjectile(ProjectileComponent prefab)
        {
            if (!projectilePools.ContainsKey(prefab))
            {
                var newPool = new ObjectPool<ProjectileComponent>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: obj => obj.gameObject.SetActive(true),
                    actionOnRelease: obj => obj.gameObject.SetActive(false),
                    actionOnDestroy: obj =>
                    {
                        instanceProjectileMap.Remove(obj);
                        Destroy(obj.gameObject);
                    },
                    collectionCheck: collectionCheck,
                    defaultCapacity: projectilePoolSize,
                    maxSize: projectilePoolMaxSize
                );
                projectilePools[prefab] = newPool;
            }

            var instance = projectilePools[prefab].Get();
            instanceProjectileMap[instance] = projectilePools[prefab];
            
            return instance;
        }
        
        public void ReleaseProjectile(ProjectileComponent instance)
        {
            if (instanceProjectileMap.ContainsKey(instance))
            {
                instanceProjectileMap[instance].Release(instance);
                instanceProjectileMap.Remove(instance);
            }
        }
        
        public FloatingText GeFloatingText(FloatingText prefab)
        {
            if (!floatingTextPools.ContainsKey(prefab))
            {
                var newPool = new ObjectPool<FloatingText>(
                    createFunc: () => Instantiate(prefab),
                    actionOnGet: obj => obj.gameObject.SetActive(true),
                    actionOnRelease: obj => obj.gameObject.SetActive(false),
                    actionOnDestroy: obj =>
                    {
                        instanceFloatingTextMap.Remove(obj);
                        Destroy(obj.gameObject);
                    },
                    collectionCheck: collectionCheck,
                    defaultCapacity: floatingTextPoolSize,
                    maxSize: floatingTextPoolMaxSize
                );
                floatingTextPools[prefab] = newPool;
            }

            var instance = floatingTextPools[prefab].Get();
            instanceFloatingTextMap[instance] = floatingTextPools[prefab];

            return instance;
        }
        
        public void ReleaseFloatingText(FloatingText instance)
        {
            if (instanceFloatingTextMap.ContainsKey(instance))
            {
                instanceFloatingTextMap[instance].Release(instance);
                instanceFloatingTextMap.Remove(instance);
            }
        }
    }
}