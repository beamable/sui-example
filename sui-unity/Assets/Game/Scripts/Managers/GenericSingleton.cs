using UnityEngine;

namespace MoeBeam.Game.Scripts.Managers
{
    public class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _shuttingDown = false;
        private static object _lock = new();

        public static T Instance
        {
            get
            {

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();

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
    }
}