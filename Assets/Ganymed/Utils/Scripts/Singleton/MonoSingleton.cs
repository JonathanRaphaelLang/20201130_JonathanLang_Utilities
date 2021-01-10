using Ganymed.Utils.Handler;
using UnityEngine;

namespace Ganymed.Utils.Singleton
{
    /// <summary>
    /// Abstract class for making scene persistent singletons of type MonoBehaviour
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region --- [FIELDS] ---

        [SerializeField] private HideFlags gameObjectHideFlags = HideFlags.None;
        private static T _instance;        

        #endregion
  
        /// <summary>
        /// Returns true if instance of type T is not null
        /// </summary>
        /// <param name="instance">null if return value is false</param>
        /// <returns></returns>
        public static bool TryGetInstance(out T instance)
        {
            if (_instance != null)
                instance = _instance;
            else
            {
                _instance = FindObjectOfType<T>();
                instance = _instance;
            }
            
            var all = FindObjectsOfType<T>();
            foreach (var type in all)
            {
                if(type != _instance)
                    DestroyImmediate(type.gameObject);
            }
        
            return _instance != null;
        }
        
        /// <summary>
        /// Static instance of T
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<T>();
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if(this == null) return;
            gameObject.hideFlags = gameObjectHideFlags;
            if(Application.isPlaying)
                Instance.gameObject.DontDestroyOnLoad();

            var other = FindObjectsOfType<T>();
            if (other.Length <= 1) return;
            foreach (var obj in other)
            {
                if (obj == this) continue;
                
                Debug.LogWarning($"Singleton: Multiple instances of the same type {GetType()} detected! " +
                                 $"Destroyed other GameObject {obj.gameObject}");
                
                DestroyImmediate(obj.gameObject);
            }
        }
    }
}