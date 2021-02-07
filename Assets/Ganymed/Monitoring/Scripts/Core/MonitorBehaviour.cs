using System;
using System.Collections.Generic;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.Optimization;
using Ganymed.Utils.Singleton;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ganymed.Monitoring.Core
{
    [ExecuteAlways]
    [AddComponentMenu("Monitoring/Monitor")]
    public sealed class MonitorBehaviour : MonoSingleton<MonitorBehaviour>
    {
        #region --- [CONFIG] ---
#pragma warning disable 649

        [HideInInspector][SerializeField] public MonitoringConfiguration config = null;
        [FormerlySerializedAs("ModulesUpperLeft")]
        [Space]
        [SerializeField] public List<Module> modulesUpperLeft = new List<Module>();
        [SerializeField] public List<Module> modulesUpperRight = new List<Module>();
        [SerializeField] public List<Module> modulesLowerLeft = new List<Module>();
        [SerializeField] public List<Module> modulesLowerRight = new List<Module>();
        
        
        
        [Header("Prefabs")]
        [HideInInspector][SerializeField] public GameObject GUIElementPrefab;
        [HideInInspector][SerializeField] public GameObject GUIObjectPrefab;
        [HideInInspector][SerializeField] public SetRootOnLoad SetRootObject;
        
        [Tooltip("Expose references.")]
        [HideInInspector] [SerializeField] public bool showReferences = false;
        
        [Tooltip("Show/Hide the SetRootOnLoad component of this gameObject.")]
        [HideInInspector] [SerializeField] public bool showRootComponent = true;
        
        #endregion

        #region --- [PROPERTIES] ---
       
        public MonitoringConfiguration Configuration => (config != null)? config : MonitoringConfiguration.Instance;

        public MonitoringCanvasBehaviour CanvasBehaviour { get; private set; }
        
        #endregion

        #region --- [FIELDS] ---
        
        private StyleBase lastStyleBase;
        private bool lastPlaceObjects;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [TOGGLE] ---

        private void Update()
        {
            if (!Input.GetKeyDown(config.toggleKey)) return;
            
            CanvasBehaviour.SetVisible(!CanvasBehaviour.IsVisible);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONSTRUCTOR] ---
#if UNITY_EDITOR

        private MonitorBehaviour()
        {
            UnityEventCallbacks.AddEventListener(
                () => MonitoringCanvasBehaviour.SetHideFlags(Configuration.hideCanvasGameObject ? HideFlags.HideInHierarchy : HideFlags.None), 
                UnityEventType.Recompile,
                UnityEventType.TransitionEditPlayMode);
        }

        private void OnEnable() =>
            MonitoringCanvasBehaviour.SetHideFlags(Configuration.hideCanvasGameObject ? HideFlags.HideInHierarchy : HideFlags.None);

        static MonitorBehaviour()
        {
            UnityEventCallbacks.AddEventListener(OnScriptsReloaded, true, UnityEventType.Recompile);
        }
        private static void OnScriptsReloaded()
        {
            if (TryGetInstance(out var guiController))
                guiController.Initialize(InvokeOrigin.Recompile);
        }
#endif
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---
        
        protected override void Awake()
        {
            base.Awake();
            Initialize(InvokeOrigin.UnityMessage);
        }

        /// <summary>
        /// Validate the integrity of the canvas instance.
        /// </summary>
        /// <param name="origin"></param>
        public void ValidateCanvas(InvokeOrigin origin)
        {
            if(config.logValidationEvents)
                Debug.Log("Validating Canvas.");
            ValidateCanvasInstance(origin);
        }

        private void OnDestroy()
        {
            try
            {
                if(Application.isPlaying)
                    Destroy(CanvasBehaviour.gameObject);
                else
                {
                    DestroyImmediate(CanvasBehaviour.gameObject);
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Force every canvas elements to reload.
        /// </summary>
        public void Repaint()
        {
            try
            {
                InstantiateModules(InvokeOrigin.GUI);
            }
            catch (Exception exception)
            {
                if(config.logValidationEvents)
                    Debug.Log(exception);
            }
        }

        private void Initialize(InvokeOrigin source)
        {
            ValidateCanvasInstance(source);

            UnityEventCallbacks.ValidateUnityEventCallbacks();
            
            InstantiateModules(source);

            if (!CanvasBehaviour || !config.automateCanvasState) return;
            
            
            if (Application.isPlaying)
            {
                if(!CanvasBehaviour.IsVisible && config.openCanvasOnEnterPlay)
                    CanvasBehaviour.SetVisible(true);
            }
            else if(Application.isEditor && CanvasBehaviour.IsVisible)
            {
                CanvasBehaviour.SetVisible(!config.closeCanvasOnEdit);
            }
        }
        
        
        private void InstantiateModules(InvokeOrigin source)
        {
            if(CanvasBehaviour == null) return;
            CanvasBehaviour.ClearAllChildren(source);
            
            foreach (var module in modulesUpperLeft)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(GUIElementPrefab, CanvasBehaviour.UpperLeft), module);
            }
            foreach (var module in modulesUpperRight)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(GUIElementPrefab, CanvasBehaviour.UpperRight), module);
            }
            foreach (var module in modulesLowerLeft)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(GUIElementPrefab, CanvasBehaviour.LowerLeft), module);
            }
            foreach (var module in modulesLowerRight)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(GUIElementPrefab, CanvasBehaviour.LowerRight), module);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATE] ---
        
        /// <summary>
        /// Check the presence of a MonitoringCanvasBehaviour instance 
        /// </summary>
        /// <param name="origin"></param>
        private void ValidateCanvasInstance(InvokeOrigin origin)
        {
            if (CanvasBehaviour == null && origin != InvokeOrigin.Recompile)
            {
                if (MonitoringCanvasBehaviour.TryGetInstance(out var i))
                {
                    CanvasBehaviour = i;
#if UNITY_EDITOR
                    if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(i.gameObject))
                    {
                        UnityEditor.PrefabUtility.UnpackPrefabInstance(
                            i.gameObject,
                            UnityEditor.PrefabUnpackMode.Completely,
                            UnityEditor.InteractionMode.AutomatedAction);    
                    }
#endif
                    
                }
                else
                {
                    Instantiate(GUIObjectPrefab);
                    if(config.logValidationEvents)
                        Debug.Log("Canvas instance was not valid! New instance instantiated!" +
                                  "(You can toggle this message in the monitoring configuration)");
                    CanvasBehaviour = MonitoringCanvasBehaviour.Instance;
                }
            }
            else
            {
                if(config.logValidationEvents)
                    Debug.Log("Canvas instance is valid! (You can toggle this message in the monitoring configuration)");
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [GAMEOBJECT] ---

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Ganymed/Monitoring",false, 11)]
        private static void CreateGameObjectInstance()
        {
            try
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
                
                foreach (var guid in guids)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);

                    if (prefab.name != "MonitorBehaviour") continue;

                    UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("Instantiated Monitoring Prefab");
                    break;
                }
            }
            catch
            {
                Debug.LogWarning("Failed to instantiate Monitoring Prefab!Make sure that the corresponding prefab" +
                                 "[MonitorBehaviour] can be found within the project.");
            }
        }
#endif   
        #endregion
    }
}
