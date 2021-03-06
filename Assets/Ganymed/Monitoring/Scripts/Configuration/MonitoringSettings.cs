﻿using System;
using System.Collections.Generic;
using System.IO;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.Helper;
using Ganymed.Utils.Singleton;
using UnityEngine;

namespace Ganymed.Monitoring.Configuration
{
    public sealed class MonitoringSettings : Settings<MonitoringSettings>
    {
        #region --- [SETTINGS] ---
        
        [Tooltip("Enable / Disable all canvas elements")]
        [HideInInspector] [SerializeField] public bool active = false;
        private bool wasActive = false;
        
        [Tooltip("When enabled the canvas is allowed to toggle automatically (on / off) when entering edit / play mode")]
        [HideInInspector] [SerializeField] public bool automateCanvasState = false;
        [Tooltip("When enabled the canvas will be enabled when entering playmode")]
        [HideInInspector] [SerializeField] public bool openCanvasOnEnterPlay = true;
        [Tooltip("When enabled the canvas will be disabled when exiting playmode")]
        [HideInInspector] [SerializeField] public bool closeCanvasOnEdit = true;
        [Tooltip("The key to enable / disable the canvas")]
        [HideInInspector] [SerializeField] public KeyCode toggleKey = KeyCode.F3;
        [Tooltip("The canvas soring order")]
        [HideInInspector] [SerializeField] public int sortingOrder = 10000;
        
        
         
        [Tooltip("Show/Hide the gameObject containing the canvas elements.")]
        [HideInInspector] [SerializeField] public bool hideCanvasGameObject = false;
        
        [Tooltip("When enabled, Modules will be instantiated and updated in Edit-Mode.")]
        [HideInInspector] [SerializeField] public bool enableLifePreview = false;
        
        
        [Tooltip("When enabled logs, warnings can be logged")]
        [HideInInspector] [SerializeField] public bool enableWarnings = false; 

        
        [HideInInspector] [SerializeField] public float canvasPadding = 0;
        [HideInInspector] [SerializeField] public float canvasMargin = 10;
        [HideInInspector] [SerializeField] public float elementSpacing = 0;
        [HideInInspector] [SerializeField] public float areaSpacing = 10;
        
        
        [HideInInspector] [SerializeField] public bool showBackground = true;
        [HideInInspector] [SerializeField] public Color colorCanvasBackground = new Color(0.71f, 1f, 0.68f);
        [HideInInspector] [SerializeField] public bool showAreaBackground = true;
        [HideInInspector] [SerializeField] public Color colorTopLeft = new Color(0.43f, 0.99f, 1f);
        [HideInInspector] [SerializeField] public Color colorTopRight = new Color(1f, 0.57f, 0.87f);
        [HideInInspector] [SerializeField] public Color colorBottomLeft = new Color(0.54f, 0.39f, 1f);
        [HideInInspector] [SerializeField] public Color colorBottomRight = new Color(0.69f, 0.64f, 1f);
        
        
        
        [HideInInspector] [SerializeField] public List<Module> modulesUpperLeft = new List<Module>();
        [HideInInspector] [SerializeField] public List<Module> modulesUpperRight = new List<Module>();
        [HideInInspector] [SerializeField] public List<Module> modulesLowerLeft = new List<Module>();
        [HideInInspector] [SerializeField] public List<Module> modulesLowerRight = new List<Module>();
        
        [HideInInspector] [SerializeField] public bool showReferences = false;

        public GameObject CanvasElementPrefab
        {
            get
            {
                if (canvasElementPrefab != null) return canvasElementPrefab;
                canvasElementPrefab = Resources.Load($"Prefabs/{CanvasElementPrefabName}") as GameObject;
                if (canvasElementPrefab != null) return canvasElementPrefab;
                Debug.LogWarning($"{CanvasElementPrefabName} can not be loaded!");
                return null;
            }
            set => canvasElementPrefab = value;
        }

        [HideInInspector] [SerializeField] private GameObject canvasElementPrefab = null;
        
        
        public GameObject CanvasPrefab
        {
            get
            {
                if (canvasPrefab != null) return canvasPrefab;
                canvasPrefab = Resources.Load($"Prefabs/{CanvasPrefabName}") as GameObject;
                if (canvasPrefab != null) return canvasPrefab;
                Debug.LogWarning($"{CanvasPrefabName} can not be loaded!");
                return null;
            }
            set => canvasPrefab = value;
        }

        [HideInInspector] [SerializeField] private GameObject canvasPrefab = null;
        
        
        /// <summary>
        /// Default style that is used by every module without an individual style.
        /// </summary>
        public Style Style
        {
            get
            {
                if (style != null)
                {
                    OnSettingsUpdated?.Invoke(this);
                    return style;
                }
                foreach (var styleName in DefaultStyleNames)
                {
                    style = Resources.Load(styleName) as Style;
                    if (style == null) continue;
                    OnSettingsUpdated?.Invoke(this);
                    return style;
                }
                
#if UNITY_EDITOR
                if(AssetImportHelper.IsImporting) return null;
#endif
                
                style = CreateInstance<Style>();
                Debug.LogWarning($"Default Style could not be found! Creating new default asset! Make sure that the default " +
                                 $"style asset is located in a resources folder and is named: {DefaultStyleNames[0]}" +
                                 $" or select a default style manually!");

#if UNITY_EDITOR
                if (!Directory.Exists($"{Instance.FilePath()}/Resources"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName($"{Instance.FilePath()}/Resources") ??
                                              "Assets/Resources");
                    UnityEditor.AssetDatabase.CreateFolder(Instance.FilePath(), "Resources");
                }

                UnityEditor.AssetDatabase.CreateAsset(style,
                    $"{Instance.FilePath()}/Resources/{DefaultStyleNames[0]}.asset");
#endif
                return style;
            }
            set
            {
                OnSettingsUpdated?.Invoke(this);
                
                style = value;
                Module.RepaintAll(true);
            }
        }

        [SerializeField] [HideInInspector] private Style style = null;
        
        #endregion

        #region --- [RESOURCE NAMES] ---

        private static readonly string[] DefaultStyleNames = {"Style_Default", "Style", "DefaultStyle", "Default"};
        private const string CanvasPrefabName = "MonitorCanvas";
        private const string CanvasElementPrefabName = "MonitorElement";

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTS] ---

        public static event Action<MonitoringSettings> OnSettingsUpdated;
        public static event Action<bool> OnActiveStateChanged;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        public void OnValidate()
        {
            if(active != wasActive)
                OnActiveStateChanged?.Invoke(active);
            wasActive = active;
            
            OnSettingsUpdated?.Invoke(this);
        }


        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [OVERRIDE GENERIC BASE] ---

        public override string FilePath() => "Assets/Ganymed/Monitoring";
        
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Ganymed/Edit Monitoring Settings", priority = 20)]
        public static void EditSettings()
        {
            SelectObject(Instance);
        }
#endif
        #endregion

    }
}
