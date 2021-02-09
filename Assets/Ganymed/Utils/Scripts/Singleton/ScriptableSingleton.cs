using Ganymed.Utils.Callbacks;
using UnityEngine;

namespace Ganymed.Utils.Singleton
{
    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created on the editor, or null if there is none
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;
        
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindObjectOfType<T>();
                return instance;
            }
        }

        protected ScriptableSingleton()
        {
            UnityEventCallbacks.AddEventListener(
                Validate,
                UnityEventType.OnLoad,
                UnityEventType.Recompile
            );
        }

        protected virtual void OnEnable() => Validate();

        private void Validate()
        {
            if (instance != null && !instance.Equals(this))
            {
                Debug.LogWarning($"Warning: Multiple Instances of {typeof(T)} were found.\nDestroying additional instances!");
                DestroyImmediate(this, true);
            }
            
            instance = (T)(object)this;
        }
        
    }
}
