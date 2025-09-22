using UnityEngine;

namespace Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if(_instance == null)
                _instance = this as T;
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}