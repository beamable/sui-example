using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    public class GenericPoolManager : GenericSingleton<GenericPoolManager>
    {
        // Internal data about each pool registration
        private class PoolData
        {
            public MonoBehaviour Prefab;
            public bool CanExpand;
            public Queue<MonoBehaviour> ObjectQueue;
        }

        private static Dictionary<Type, PoolData> _pools = new();

        public void Init()
        {
            _pools = new Dictionary<Type, PoolData>();
        }

        /// <summary>
        /// Register a script type T with a prefab for pooling.
        /// - prefab: The MonoBehaviour script's prefab.
        /// - initialSize: how many to pre-instantiate.
        /// - canExpand: whether we can instantiate more if the pool is empty.
        /// </summary>
        public void Register<T>(T prefab, int initialSize, bool canExpand, Action callback = null) where T : MonoBehaviour
        {
            var type = typeof(T);
            if (_pools.ContainsKey(type))
            {
                Debug.LogWarning($"Pool for {type} is already registered. Ignoring.");
                return;
            }

            var data = new PoolData
            {
                Prefab = prefab,
                CanExpand = canExpand,
                ObjectQueue = new Queue<MonoBehaviour>(initialSize)
            };

            // Pre-warm the pool
            for (var i = 0; i < initialSize; i++)
            {
                var obj = Instantiate(prefab, transform);
                obj.gameObject.SetActive(false);
                data.ObjectQueue.Enqueue(obj);
            }

            _pools.Add(type, data);
            callback?.Invoke();
        }
        
        public bool IsRegistered<T>() where T : MonoBehaviour
        {
            return _pools.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Retrieve an instance of script type T from the pool.
        /// If the pool is empty and canExpand == true, we instantiate a new one.
        /// Returns null if the pool doesn't exist or can't expand.
        /// </summary>
        public T Get<T>(Vector3 position) where T : MonoBehaviour
        {
            var type = typeof(T);
            if (!_pools.TryGetValue(type, out var data))
            {
                Debug.LogError($"No pool registered for {type}. Did you forget to call Register()?");
                
                return null;
            }

            if (data.ObjectQueue.Count == 0)
            {
                if (data.CanExpand)
                {
                    // Expand by creating a new instance
                    var newObj = Instantiate(data.Prefab, position, Quaternion.identity, transform) as T;
                    newObj.transform.position = position;
                    newObj.gameObject.SetActive(true);
                    return newObj;
                }
                else
                {
                    Debug.LogWarning($"Pool for {type} is empty and can't expand.");
                    return null;
                }
            }

            // Dequeue an existing instance
            var pooledObj = data.ObjectQueue.Dequeue() as T;
            
            pooledObj.transform.position = position;
            pooledObj.gameObject.SetActive(true);
            return pooledObj;
        }
        
        // Overload that accepts a prefab
        public T Get<T>(T prefab, Vector3 position) where T : MonoBehaviour
        {
            var type = typeof(T);
            if (!_pools.TryGetValue(type, out var data))
            {
                //Debug.LogWarning($"No pool registered for {type}. Did you forget to call Register()?");
                return Instantiate(prefab, position, Quaternion.identity, transform);
            }

            if (data.ObjectQueue.Count == 0)
            {
                if (data.CanExpand)
                {
                    // Expand by creating a new instance of the passed prefab
                    var newObj = Instantiate(prefab, position, Quaternion.identity, transform);
                    newObj.transform.position = position;
                    newObj.gameObject.SetActive(true);
                    return newObj;
                }
                else
                {
                    Debug.LogWarning($"Pool for {type} is empty and can't expand.");
                    return null;
                }
            }

            // Dequeue an existing instance
            var pooledObj = data.ObjectQueue.Dequeue() as T;
            pooledObj.transform.position = position;
            pooledObj.gameObject.SetActive(true);
            return pooledObj;
        }
        

        /// <summary>
        /// Return an instance of script type T to the pool.
        /// Disables the gameobject and re-enqueues it.
        /// </summary>
        public void Return<T>(T instance) where T : MonoBehaviour
        {
            var type = typeof(T);
            if (!_pools.ContainsKey(type))
            {
                Debug.LogWarning($"No pool registered for {type}. Destroying object instead.");
                Destroy(instance.gameObject);
                return;
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(transform);
            _pools[type].ObjectQueue.Enqueue(instance);
        }
    }
}