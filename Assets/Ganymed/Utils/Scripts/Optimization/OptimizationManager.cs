using System.Collections.Generic;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Utils.Optimization
{
    [CreateAssetMenu(fileName = "OptimizationConfig", menuName = "Hierarchy Optimization/Configuration")]
    public class OptimizationManager : Singleton.ScriptableSingleton<OptimizationManager>
    {
        #region --- [FIELDS (HIERARCHY)] ---
        
        [SerializeField] [HideInInspector] public bool enableUnfoldingOnLoad = false;
        [SerializeField] [HideInInspector] public bool enableSetRootOnLoad = false;
        [SerializeField] [HideInInspector] public bool enableDestroyOnLoad = false;
        
        [Tooltip("When enabled, allows warning messages send by individual components.")]
        [SerializeField] [HideInInspector] public bool enableComponentWarnings = false;
        [Tooltip("When enabled, allows notification logs.")]
        [SerializeField] [HideInInspector] public bool enableLogs = true;

        [SerializeField] [HideInInspector] public bool hideComponentsInInspector = false;
        [SerializeField] [HideInInspector] internal bool hideComponentsInInspectorCache = false;
        
        #endregion

        #region --- [PROPERTIES] ---
        
        public static bool HideComponentsInInspector => Instance != null && Instance.hideComponentsInInspector;
        
        public static bool EnableWarnings => Instance != null && Instance.enableComponentWarnings;
        
        /// <summary>
        /// Are Unfold components deactivated.  
        /// </summary>
        public static bool EnableUnfoldingOnLoad => Instance != null && Instance.enableUnfoldingOnLoad;
        
        /// <summary>
        /// Are SetParent components deactivated.  
        /// </summary>
        public static bool EnableSetRootOnLoad => Instance != null && Instance.enableSetRootOnLoad;
        
        /// <summary>
        /// Are DestroyOnLoad components deactivated.  
        /// </summary>
        public static bool EnableDestroyOnLoad => Instance != null && Instance.enableDestroyOnLoad;
        
        #endregion

        #region --- [VALIDATE] ---

        public void Validate()
        {
            if (hideComponentsInInspector != hideComponentsInInspectorCache)
            {

                var value = hideComponentsInInspector ? HideFlags.HideInInspector : HideFlags.None;
                foreach (var obj in optimizationComponents)
                {
                    obj.hideFlags = value;
                }
            }

            hideComponentsInInspectorCache = hideComponentsInInspector;
        }
        
        

        #endregion
        
        #region --- [STATIC] ---

        public static readonly List<TransformOptimizationComponent> optimizationComponents = new List<TransformOptimizationComponent>();

        public static void AddOptimizationComponent(TransformOptimizationComponent component)
        {
            optimizationComponents.AddIfNotInList(component);
        }
        
        public static void RemoveOptimizationComponent(TransformOptimizationComponent component)
        {
            try
            {
                optimizationComponents.Remove(component);
            }
            catch
            {
                // ignored
            }
        }
        
        #endregion
    }
}