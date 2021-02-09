using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.Singleton;
using UnityEngine;

namespace Ganymed.Monitoring.Configuration
{
    public class MonitoringSettings : Settings<MonitoringSettings>
    {
#pragma warning disable 649
#pragma warning disable 414
       
        //TODO: comment and sort settings overrides
        
        public override string FilePath() => "Assets/Ganymed/Monitoring";
        
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Ganymed/Edit Monitoring Settings", priority = 20)]
        public static void EditSettings()
        {
            SelectObject(Instance);
        }
#endif
        
        
        [Space]
        [HideInInspector] [SerializeField] public List<Module> modulesUpperLeft = new List<Module>();
        [HideInInspector] [SerializeField] public List<Module> modulesUpperRight = new List<Module>();
        [HideInInspector] [SerializeField] public List<Module> modulesLowerLeft = new List<Module>();
        [HideInInspector] [SerializeField] public List<Module> modulesLowerRight = new List<Module>();
        
        [Header("Prefabs")]
        [HideInInspector] [SerializeField] public GameObject GUIElementPrefab;
        [HideInInspector] [SerializeField] public GameObject GUIObjectPrefab;
        
        [Tooltip("Expose references.")]
        [HideInInspector] [SerializeField] public bool showReferences = false;
        
                
        public Style Style 
        { 
            get => style;
            set
            {
                OnSettingsUpdated?.Invoke(this);
                
                style = value;
                Module.RepaintAll(true);
            }
        }
        
        #region --- [SETTINGS] ---

        [SerializeField] [HideInInspector] private Style style;
        
        [Tooltip("Enable / Disable all canvas elements")]
        [HideInInspector] [SerializeField] public bool active = false;
        
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
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTS] ---

        public static event Action<MonitoringSettings> OnSettingsUpdated;
        public static event Action<bool> OnActiveStateChanged;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [PRIVATE FIELDS] ---

        private bool lastActive = false;

        #endregion
        
        //

        #region --- [VALIDATION] ---

        public void OnValidate()
        {
            if(active != lastActive)
                OnActiveStateChanged?.Invoke(active);
            lastActive = active;
            
            OnSettingsUpdated?.Invoke(this);
        }


        #endregion

    }
}
