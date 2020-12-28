using System;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.Singleton;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    [ExecuteInEditMode]
    public class MonitorBehaviour : MonoSingleton<MonitorBehaviour>
    {
        #region --- [CONFIG] ---
#pragma warning disable 649

        [Header("GlobalConfiguration Configuration")]
        [SerializeField] private GlobalConfiguration config = null;

        [Header("Modules")]
        [SerializeField] private Module[] ModulesUpperLeft;
        [SerializeField] private Module[] ModulesUpperRight;
        [SerializeField] private Module[] ModulesLowerLeft;
        [SerializeField] private Module[] ModulesLowerRight;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject GUIElementPrefab;
        [SerializeField] private GameObject GUIObjectPrefab;

        [Header("Position In Hierarchy")]
        [SerializeField] private PositionInHierarchy positionInHierarchy= PositionInHierarchy.BottomOfHierarchy;
        [SerializeField] private bool placeObjectAtSceneRoot = true;
       
        #endregion

        #region --- [PROPERTIES] ---
        public GlobalConfiguration GlobalConfiguration => (config != null)? config : GlobalConfiguration.Instance;

        #endregion

        #region --- [FIELDS] ---

        public MonitoringCanvasBehaviour MonitoringCanvasBehaviour => GUIcanvas;
        private MonitoringCanvasBehaviour GUIcanvas;
        private Configuration.Configurable lastConfigurable;

        private PositionInHierarchy lastPositionInHierarchy;
        private bool lastPlaceObjects;
        
        private enum PositionInHierarchy
        {
            /// <summary>
            /// Dont set the position.
            /// </summary>
            Ignore,
            /// <summary>
            /// Place the object at the top of the hierarchy.
            /// </summary>
            TopOfHierarchy,
            /// <summary>
            /// Place the object at the bottom of the hierarchy.
            /// </summary>
            BottomOfHierarchy
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [TOGGLE] ---

        private void Update()
        {
            if (!Input.GetKeyDown(config.toggleKey)) return;
            
            var last = GUIcanvas.VisibilityFlags;
            GUIcanvas.SetVisibility(
                last == Visibility.ActiveAndVisible?
                    Visibility.ActiveAndHidden :
                    Visibility.ActiveAndVisible);
        }        

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONSTRUCTOR] ---
#if UNITY_EDITOR

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
        
        private void Start()
        {
            this.gameObject.name = nameof(MonitorBehaviour);
            Initialize(InvokeOrigin.UnityMessage);
        }
        
        
        private void Initialize(InvokeOrigin source)
        {
            ValidateCanvasInstance(source);

            UnityEventCallbacks.ValidateUnityEventCallbacks();

            ValidatePositionsInHierarchy(source, positionInHierarchy,placeObjectAtSceneRoot);
            
            InstantiateModules(source);

            if (!GUIcanvas || !config.automateCanvasState) return;
            
            
            //TODO: include cases... create enum for initial flags
            if (Application.isPlaying)
            {
                if(GUIcanvas.VisibilityFlags != Visibility.ActiveAndVisible && config.openCanvasOnEnterPlay)
                    GUIcanvas.SetVisibility(Visibility.ActiveAndVisible);
            }
            else if(Application.isEditor)
            {
                GUIcanvas.SetVisibility(config.closeCanvasOnEdit? Visibility.ActiveAndHidden : Visibility.ActiveAndVisible);
            }
        }
        
        
        private void InstantiateModules(InvokeOrigin source)
        {
            if(GUIcanvas == null) return;
            GUIcanvas.ClearAllChildren(source);
            
            foreach (var module in ModulesUpperLeft)
            {
                if (module == null) continue;
                MonitoringElement.CreateComponent(Instantiate(GUIElementPrefab, GUIcanvas.UpperLeft), module);
            }
            foreach (var module in ModulesUpperRight)
            {
                if (module == null) continue;
                MonitoringElement.CreateComponent(Instantiate(GUIElementPrefab, GUIcanvas.UpperRight), module);
            }
            foreach (var module in ModulesLowerLeft)
            {
                if (module == null) continue;
                MonitoringElement.CreateComponent(Instantiate(GUIElementPrefab, GUIcanvas.LowerLeft), module);
            }
            foreach (var module in ModulesLowerRight)
            {
                if (module == null) continue;
                MonitoringElement.CreateComponent(Instantiate(GUIElementPrefab, GUIcanvas.LowerRight), module);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATE] ---
        
        private void OnValidate()
        {
            if(config == null) Debug.LogError("Monitoring configuration is null! Please select a configuration");
            
            if (positionInHierarchy == lastPositionInHierarchy && lastPlaceObjects == placeObjectAtSceneRoot) return;
            ValidatePositionsInHierarchy(InvokeOrigin.UnityMessage, positionInHierarchy, placeObjectAtSceneRoot);
            lastPositionInHierarchy = positionInHierarchy;
            lastPlaceObjects = placeObjectAtSceneRoot;
        }
        
        
        /// <summary>
        /// Check the presence of a MonitoringCanvasBehaviour instance 
        /// </summary>
        /// <param name="origin"></param>
        private void ValidateCanvasInstance(InvokeOrigin origin)
        {
            if (GUIcanvas == null && origin != InvokeOrigin.Recompile)
            {
                GUIcanvas = MonitoringCanvasBehaviour.TryGetInstance(out var i) ? i : Instantiate(GUIObjectPrefab).GetComponent<MonitoringCanvasBehaviour>();
                GUIcanvas.gameObject.name = "MonitorCanvas";
            }
        }

        
        /// <summary>
        /// Check if the placement of the object in the hierarchy is up-to-date
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="placement"></param>
        /// <param name="root">place the object at the root of the hierarchy</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ValidatePositionsInHierarchy(InvokeOrigin origin, PositionInHierarchy placement, bool root = true)
        {
            if (root)
            {
                transform.SetParent(null);
                if(GUIcanvas != null)
                    GUIcanvas.transform.SetParent(null);
            }

            switch (placement)
            {
                case PositionInHierarchy.Ignore:
                    return;
                case PositionInHierarchy.TopOfHierarchy:
                    transform.SetAsFirstSibling();
                    if(GUIcanvas != null)
                        GUIcanvas.transform.SetSiblingIndex(transform.GetSiblingIndex());
                    break;
                case PositionInHierarchy.BottomOfHierarchy:
                    transform.SetAsLastSibling();
                    if(GUIcanvas != null)
                        GUIcanvas.transform.SetSiblingIndex(transform.GetSiblingIndex());
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(placement), placement, null);
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
                var guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });
                
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadMainAssetAtPath(path);

                    if (prefab.name != "MonitorBehaviour") continue;

                    Instantiate(prefab);
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
