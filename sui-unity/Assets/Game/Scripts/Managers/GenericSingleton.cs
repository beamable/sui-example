using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    /// <summary>
    /// Generic singleton for any MonoBehaviour-based class.
    /// Ensures only one instance of T exists in the scene.
    /// Inherit from this to make a singleton.
    /// </summary>
    public class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _shuttingDown = false;
        private static object _lock = new();

        /// <summary>
        /// The public accessor for the singleton instance.
        /// If no instance exists yet, it tries to find one in the scene;
        /// if none is found, it creates a new GameObject with T attached.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_shuttingDown)
                {
                    Debug.LogWarning($"[Singleton] Instance {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Search for existing instance in the scene
                        _instance = FindFirstObjectByType<T>();

                        // If still null, create a new GameObject
                        if (_instance == null)
                        {
                            var singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                            _instance = singletonObject.AddComponent<T>();
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            // If there's an existing instance and it's not this, destroy this.
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} exists! Destroying {gameObject.name}.");
                Destroy(gameObject);
                return;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _shuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this) _shuttingDown = true;
        }
    }
}