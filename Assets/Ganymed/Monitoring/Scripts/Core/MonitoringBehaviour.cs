﻿using System;
using System.Runtime.CompilerServices;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.Singleton;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    [ExecuteAlways]
    [AddComponentMenu("Monitoring/Monitor")]
    public sealed class MonitoringBehaviour : MonoSingleton<MonitoringBehaviour>
    {
        
        #region --- [PROPERTIES] ---
        private MonitoringCanvasBehaviour CanvasBehaviour { get; set; }
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [TOGGLE] ---

        /// <summary>
        /// Toggle the canvas element.
        /// </summary>
        public void Toggle() => CanvasBehaviour.SetVisible(!CanvasBehaviour.IsVisible);

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONSTRUCTOR] ---
#if UNITY_EDITOR

        private MonitoringBehaviour()
        {
            UnityEventCallbacks.AddEventListener(
                () => MonitoringCanvasBehaviour.SetHideFlags(MonitoringSettings.Instance.hideCanvasGameObject ? HideFlags.HideInHierarchy : HideFlags.None), 
                UnityEventType.Recompile,
                UnityEventType.TransitionEditPlayMode);
        }

        private void OnEnable() =>
            MonitoringCanvasBehaviour.SetHideFlags(MonitoringSettings.Instance.hideCanvasGameObject ? HideFlags.HideInHierarchy : HideFlags.None);

        static MonitoringBehaviour()
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
            ValidateCanvasInstance(origin);
        }

        
        private void Initialize(InvokeOrigin source)
        {
            ValidateCanvasInstance(source);

            UnityEventCallbacks.ValidateUnityEventCallbacks();
            
            InstantiateModules(source);

            if (!CanvasBehaviour || !MonitoringSettings.Instance.automateCanvasState) return;
            
            
            if (Application.isPlaying)
            {
                if(!CanvasBehaviour.IsVisible && MonitoringSettings.Instance.openCanvasOnEnterPlay)
                    CanvasBehaviour.SetVisible(true);
            }
            else if(Application.isEditor && CanvasBehaviour.IsVisible)
            {
                CanvasBehaviour.SetVisible(!MonitoringSettings.Instance.closeCanvasOnEdit);
            }
        }
        
        
        /// <summary>
        /// Create canvas elements for the monitoring modules.
        /// </summary>
        /// <param name="source"></param>
        private void InstantiateModules(InvokeOrigin source)
        {
            if(CanvasBehaviour == null) return;
            CanvasBehaviour.ClearAllChildren(source);
            
            foreach (var module in MonitoringSettings.Instance.modulesUpperLeft)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(MonitoringSettings.Instance.GUIElementPrefab, CanvasBehaviour.UpperLeft), module);
            }
            foreach (var module in MonitoringSettings.Instance.modulesUpperRight)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(MonitoringSettings.Instance.GUIElementPrefab, CanvasBehaviour.UpperRight), module);
            }
            foreach (var module in MonitoringSettings.Instance.modulesLowerLeft)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(MonitoringSettings.Instance.GUIElementPrefab, CanvasBehaviour.LowerLeft), module);
            }
            foreach (var module in MonitoringSettings.Instance.modulesLowerRight)
            {
                if (module == null) continue;
                ModuleCanvasElement.CreateComponent(Instantiate(MonitoringSettings.Instance.GUIElementPrefab, CanvasBehaviour.LowerRight), module);
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
                if(MonitoringSettings.Instance.enableWarnings)
                    Debug.Log(exception);
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [TERMINATE] ---

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
                if (MonitoringCanvasBehaviour.TryGetInstance(out var canvasInstance))
                {
                    CanvasBehaviour = canvasInstance;
#if UNITY_EDITOR
                    if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(canvasInstance.gameObject))
                    {
                        UnityEditor.PrefabUtility.UnpackPrefabInstance(
                            canvasInstance.gameObject,
                            UnityEditor.PrefabUnpackMode.Completely,
                            UnityEditor.InteractionMode.AutomatedAction);    
                    }
#endif
                }
                else
                {
                    Instantiate(MonitoringSettings.Instance.GUIObjectPrefab);
                    if(MonitoringSettings.Instance.enableWarnings)
                        Debug.Log("Canvas instance was not valid! New instance instantiated!" +
                                  "(You can toggle this message in the monitoring configuration)");
                    CanvasBehaviour = MonitoringCanvasBehaviour.Instance;
                }
            }
            else
            {
                if(MonitoringSettings.Instance.enableWarnings)
                    Debug.Log("Canvas instance is valid! (You can toggle this message in the monitoring configuration)");
            }
        }

        #endregion
    }
}
 