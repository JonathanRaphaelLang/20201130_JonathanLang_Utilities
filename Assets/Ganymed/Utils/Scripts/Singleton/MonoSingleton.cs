using System;
using Ganymed.Utils.Helper;
using UnityEngine;

namespace Ganymed.Utils.Singleton
{
    /// <summary>
    /// Abstract class for making scene persistent MonoBehaviour singletons.
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region --- [ACCESS] ---

        /// <summary>
        /// Get the active instance of the accessed type.
        /// </summary>
        public static T Instance => instanceCache ? instanceCache : (instanceCache = FindObjectOfType<T>());

        
        
        /// <summary>
        /// Returns true if instance of type T is not null
        /// </summary>
        /// <param name="instance">null if return value is false</param>
        /// <returns></returns>
        public static bool TryGetInstance(out T instance)
        {
            if (instanceCache != null)
                instance = instanceCache;
            else
            {
                instanceCache = FindObjectOfType<T>();
                instance = instanceCache;
            }
            
            var all = FindObjectsOfType<T>();
            
            foreach (var type in all)
            {
                if(type != instanceCache)
                    DestroyImmediate(type.gameObject);
            }
        
            return instanceCache != null;
        }


        /// <summary>
        /// Set the HideFlags of the GameObject / Component of the active instance.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="target"></param>
        public static void SetHideFlags(HideFlags flags, HideFlagsTarget target)
        {
            if (!TryGetInstance(out var instance)) return;

            switch (target)
            {
                case HideFlagsTarget.GameObject:
                    instance.gameObject.hideFlags = flags;
                    return;
                
                case HideFlagsTarget.Script:
                    instance.hideFlags = flags;
                    return;
                
                case HideFlagsTarget.GameObjectAndScript:
                    instance.gameObject.hideFlags = flags;
                    instance.hideFlags = flags;
                    return;
                
                case HideFlagsTarget.None:
                    return;
                default:
                    return;
            }
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PRIVATE & PROTECTED FIELDS & PROPERTIES] ---

        private static T instanceCache;
        
        /// <summary>
        /// Override this bool and set it to false if you want to keep older instances instead of newer
        /// if multiple instances are detected. 
        /// </summary>
        protected bool KeepLastCreatedInstance => true;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        protected virtual void Awake()
        {
            if(this == null) return;
            
            // Use the DontDestroyOnLoadHandler to keep track of every singleton we dont want to destroy on load.

            if(Application.isPlaying)
                Instance.gameObject.DontDestroyOnLoad();     
            

            var otherObjectsOfType = FindObjectsOfType<T>();
            if (otherObjectsOfType.Length <= 1) return;
            
            // We destroy every object except this if we find multiple instances. 
            if (KeepLastCreatedInstance)
            {
                foreach (var obj in otherObjectsOfType)
                {
                    if (obj == this) continue;
                
                    Debug.LogWarning($"Singleton: Multiple instances of the same type {GetType()} detected! " +
                                     $"Destroyed other GameObject {obj.gameObject} instance!");
                
                    DestroyImmediate(obj.gameObject);
                }    
            }
            else
            {
                if (otherObjectsOfType.Length == 2)
                {
                    DestroyImmediate(gameObject);
                
                    Debug.LogWarning($"Singleton: Multiple instances of the same type {GetType()} detected! " +
                                     $"Destroyed other GameObject {gameObject} instance!");    
                }
                else
                {
                    for (var i = otherObjectsOfType.Length - 1; i >= 0; i--)
                    {
                        if(i > 0) DestroyImmediate(otherObjectsOfType[i].gameObject);
                    }
                    Debug.LogWarning($"Singleton: Multiple instances of the same type {GetType()} detected! " +
                                     $"Destroyed other GameObject instances!");    
                }
            }
        }        

        #endregion
    }
}