using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    /// <summary>
    /// A single manager that pools multiple MonoBehaviour script types.
    /// You register each script type with a specific prefab.
    /// Then call Get<T>() and Return<T>() without any string keys.
    /// 
    /// Limitations:
    ///  - Only one prefab per script type T.
    ///  - If you need multiple prefabs for the same script, you'll need a more advanced scheme.
    /// </summary>
    public class GenericPoolManager : GenericSingleton<GenericPoolManager>
    {
        // Internal data about each pool registration
        private class PoolData
        {
            public MonoBehaviour Prefab;
            public bool CanExpand;
            public Queue<MonoBehaviour> ObjectQueue;
        }

        // We store each registered pool in this dictionary, keyed by Type of the script.
        private static Dictionary<Type, PoolData> _pools = new();


        /// <summary>
        /// Register a script type T with a prefab for pooling.
        /// - prefab: The MonoBehaviour script's prefab.
        /// - initialSize: how many to pre-instantiate.
        /// - canExpand: whether we can instantiate more if the pool is empty.
        /// </summary>
        public void Register<T>(T prefab, int initialSize, bool canExpand) where T : MonoBehaviour
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
        }

        /// <summary>
        /// Retrieve an instance of script type T from the pool.
        /// If the pool is empty and canExpand == true, we instantiate a new one.
        /// Returns null if the pool doesn't exist or can't expand.
        /// </summary>
        public T Get<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            if (!_pools.TryGetValue(type, out var data))
            {
                Debug.LogWarning($"No pool registered for {type}. Did you forget to call Register()?");
                return null;
            }

            if (data.ObjectQueue.Count == 0)
            {
                if (data.CanExpand)
                {
                    // Expand by creating a new instance
                    var newObj = Instantiate(data.Prefab, transform) as T;
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