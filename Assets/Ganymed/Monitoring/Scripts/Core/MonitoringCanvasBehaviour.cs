using System;
using System.Threading.Tasks;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Attributes;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace Ganymed.Monitoring.Core
{
    /// <summary>
    /// MonitoringCanvasBehaviour for Monitoring ModuleDictionary
    /// The prefab of this gameObject will be unpacked automatically.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Monitoring/Monitoring Canvas")]
    [RequireComponent(typeof(Canvas))]
    public sealed class MonitoringCanvasBehaviour : MonoSingleton<MonitoringCanvasBehaviour>, IState
    {
        #region --- [FIELDS] ---

#pragma warning disable 649
#pragma warning disable 414

        [HideInInspector] [SerializeField] internal bool showFields = false;
        
        [Header("Parents")]
        [SerializeField] private Transform upperLeft;
        [SerializeField] private Transform upperRight;
        [SerializeField] private Transform lowerLeft;
        [SerializeField] private Transform lowerRight;
        [Space]
        [SerializeField] private RectTransform frame;
        [Space]
        [SerializeField] private HorizontalLayoutGroup mainGroup;
        [SerializeField] private VerticalLayoutGroup rightGroup;
        [SerializeField] private VerticalLayoutGroup leftGroup;
        [SerializeField] private VerticalLayoutGroup[] areaGroups;
        [Space]
        [SerializeField] private Image canvasBackground;
        [Space]
        [SerializeField] private Image upperLeftBackground;
        [SerializeField] private Image upperRightBackground;
        [SerializeField] private Image lowerLeftBackground;
        [SerializeField] private Image lowerRightBackground;
        [Space]
        [SerializeField] private Canvas canvas;

        [Space]
        [SerializeField] private Transform[] parentsToClear;

        #endregion

        #region --- [PROPERTIES] ---

        public Transform UpperLeft => upperLeft;
        public Transform UpperRight => upperRight;
        public Transform LowerLeft => lowerLeft;
        public Transform LowerRight => lowerRight;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [STATE] ---
        
        public event EnabledDelegate OnEnabledStateChanged;
        public event ActiveAndVisibleDelegate OnAnyStateChanged;
        public event VisibilityDelegate OnVisibilityStateChanged;
        public event ActiveDelegate OnActiveStateChanged;

        

        public bool IsEnabled
        {
            get => isEnabled;
            private set
            {
                isEnabled = value;
                IsVisible = isEnabled;
                IsActive = isEnabled;
                OnEnabledStateChanged?.Invoke(value);
            }
        }
        [SerializeField] [HideInInspector] private bool isEnabled;
        public void SetEnabled(bool enable)
            => IsEnabled = enable;


        public bool IsActive
        {
            get => isActive;
            private set
            {
                isActive = value;
            
                OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                OnActiveStateChanged?.Invoke(value);
                
                gameObject.SetActive(value);
                canvas.enabled = value;
            }
        }
        [SerializeField] [HideInInspector] private bool isActive = true;
        public void SetActive(bool active)
            => IsActive = active;


        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                isVisible = value;
                
                OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                OnVisibilityStateChanged?.Invoke(value);

                try
                {
                    gameObject.SetActive(value);
                    canvas.enabled = value;    
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        [SerializeField] [HideInInspector] private bool isVisible;
        public void SetVisible(bool visible)
            => IsVisible = isActive? visible : isVisible;
        

        public bool IsEnabledActiveAndVisible
        {
            get => IsEnabled && IsActive && IsVisible;
            private set
            {
                isActive = value;
                isVisible = value;
                
                OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                OnActiveStateChanged?.Invoke(value);
                OnVisibilityStateChanged?.Invoke(value);
            }
        }
        
        public void SetStates(bool value)
            => IsEnabledActiveAndVisible = value;
      


        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        protected override void Awake()
        {
            base.Awake();
            canvas = GetComponent<Canvas>();
        }
        
        
        private MonitoringCanvasBehaviour()
        {
            MonitoringSettings.OnSettingsUpdated += RepaintCanvas;
            MonitoringSettings.OnActiveStateChanged += SetVisible;
        }
        
        
        ~MonitoringCanvasBehaviour()
        {
            MonitoringSettings.OnSettingsUpdated -= RepaintCanvas;
            MonitoringSettings.OnActiveStateChanged -= SetVisible;
        }

        
        private void OnDestroy()
        {
            MonitoringSettings.OnSettingsUpdated -= RepaintCanvas;
            MonitoringSettings.OnActiveStateChanged -= SetVisible;
        }
        
        
        #endregion
        
        //---------------------------------------------------------------------------------------------------------------

        #region --- [CLEAR CHILDREN] ---

        public void ClearAllChildren(InvokeOrigin origin)
        {
            foreach (var parent in parentsToClear)
            {
                ClearChildren(origin, parent);
            }
        }
        
        private static void ClearChildren(InvokeOrigin origin, Transform parent)
        {
            var childrenNum = parent.childCount;
            for (var i = childrenNum - 1; i >= 0; i--)
            {
                var target = parent.GetChild(i).gameObject;
                if(target == null) continue;
                try
                {
                    DestroyImmediate(parent.GetChild(i).gameObject, true);
                }
                catch
                {
                    continue;
                }
            }
        }

        #endregion
        
        //---------------------------------------------------------------------------------------------------------------

        #region --- [REPAINT] ---
       
       /// <summary>
       /// Set new values for canvas elements based on the global config
       /// </summary>
        private async void RepaintCanvas(MonitoringSettings settings)
        {
            if(frame == null || rightGroup == null || leftGroup == null || !MonitoringSettings.Instance) return;

            #region [EDITOR]

#if UNITY_EDITOR
            rightGroup.runInEditMode = true;
            leftGroup.runInEditMode = true; 
#endif

            #endregion

            canvas.sortingOrder = settings.sortingOrder;
            
            frame.offsetMin = Vector2.one * settings.canvasPadding; 
            frame.offsetMax = -Vector2.one * settings.canvasPadding;

            var margin = (int)settings.canvasMargin;

            rightGroup.padding = new RectOffset(
                left: 0,
                right: margin,
                top: margin,
                bottom: margin);
            
            leftGroup.padding = new RectOffset(
                left: margin,
                right: 0,
                top: margin,
                bottom: margin);

            //set spacing for every vertical layout group
            foreach (var verticalLayoutGroup in areaGroups) {
                verticalLayoutGroup.spacing = settings.elementSpacing;
            }

            if (settings.showBackground)
            {
                canvasBackground.enabled = true;
                canvasBackground.color = settings.colorCanvasBackground;
            }
            else
            {
                canvasBackground.enabled = false;
            }


            if (settings.showAreaBackground)
            {
                upperLeftBackground.enabled = true;
                upperRightBackground.enabled = true;
                lowerLeftBackground.enabled = true;
                lowerRightBackground.enabled = true;
                
                upperLeftBackground.color = settings.colorTopLeft;
                upperRightBackground.color = settings.colorTopRight;
                lowerLeftBackground.color = settings.colorBottomLeft;
                lowerRightBackground.color = settings.colorBottomRight;
            }
            else
            {
                upperLeftBackground.enabled = false;
                upperRightBackground.enabled = false;
                lowerLeftBackground.enabled = false;
                lowerRightBackground.enabled = false;
            }

            mainGroup.spacing = settings.areaSpacing;
            leftGroup.spacing = settings.areaSpacing;
            rightGroup.spacing = settings.areaSpacing;

            await Task.CompletedTask.BreakContext();
            Canvas.ForceUpdateCanvases();
        }
        #endregion
    }
    
        
#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(MonitoringCanvasBehaviour))]
    [UnityEditor.CanEditMultipleObjects]
    internal class MonitoringCanvasBehaviourInspector : UnityEditor.Editor
    {
        private MonitoringCanvasBehaviour Target;
        private void OnEnable()
        {
            Target = (MonitoringCanvasBehaviour) target;
        }

        public override void OnInspectorGUI()
        {
            Target.showFields = UnityEditor.EditorGUILayout.ToggleLeft("Show Fields", Target.showFields);
            if (Target.showFields)
            {
                DrawDefaultInspector();
            }
        }
    }
    
#endif
}
