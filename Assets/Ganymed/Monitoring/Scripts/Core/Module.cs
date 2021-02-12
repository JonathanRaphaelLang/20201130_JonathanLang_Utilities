using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    /// <summary>
    /// Base class for Monitoring Modules
    /// </summary>
    public abstract class Module : ScriptableObject, IState
    {
        #region --- [INSPECTOR] ---
       
        [Tooltip("Enabled modules will be initialized")]
        [SerializeField] private bool isEnabled = true;
        
        [Tooltip("Active modules will execute their update and inspection functions. Comparable to MonoBehaviors")]
        [SerializeField] private bool isActive = true;
        
        [Tooltip("Visible Modules have an active canvas element.")]
        [SerializeField] private bool isVisible = true;
        
        
        [Space]
        [Tooltip("When enabled the module will only be initialized if it is part of an active scene.")]
        [SerializeField] private bool onlyInitializeWhenInScene = true;

        


        [Header("Settings")]
        [Tooltip("Set a custom style for the module. False will use the default style instead")]
        [SerializeField] private bool useCustomStyle = default;
        
        [Tooltip("If Use Custom Style is enabled, use this style. Null will use the default style instead")]
        [SerializeField] private Style customStyle = null;

     

        [Header("Warnings")]
        [Tooltip("When enabled custom warnings will (can) be logged")]
        [SerializeField] private bool enableWarnings = true;
       
        
        
        [Space]
        [Tooltip("How should the value be displayed")]
        [SerializeField] private ValueInterpretationOption previewValueAs = ValueInterpretationOption.Value;

        [Tooltip("Reset the displayed value after exiting playmode.")]
        [SerializeField] private bool resetValueOnQuit = false;

        
        
        [Space]
        [Tooltip("Set custom prefix text")]
        [SerializeField] private string prefixText = default;
        
        [Tooltip("Set custom suffix text")]
        [SerializeField] private string suffixText = default;
        
        [Tooltip("Automatically add a break after the prefix")]
        [SerializeField] private bool prefixBreak = false;
        
        [Tooltip("Automatically add a break after the suffix")]
        [SerializeField] private bool suffixBreak = false;
        
        
        
        [Space]
        [Tooltip("What does the module do. Optional field for clarity only. Can be accessed during runtime by console commands if visible")]
        [SerializeField][TextArea] private string description = default;
        
        
        
        [Space]
        [Tooltip("When enabled OnInspection will be called periodically. Use to validate the values integrity")]
        [SerializeField] private bool enableAutoInspection = default;
        
        [Tooltip("How much time should pass between inspections")]
        [SerializeField][Range(.1f, 60f)] private float secondsBetweenInspections = 1f;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PROTECTED PROPERTIES] ---

        private bool UseAndHasCustomStyle => useCustomStyle && customStyle != null;
       
        protected bool OnlyInitializeWhenInScene => onlyInitializeWhenInScene;
        
        private protected bool EnableWarnings => enableWarnings;

        
        
        protected ValueInterpretationOption PreviewValueAs => previewValueAs;

        protected bool ResetValueOnQuit => resetValueOnQuit;
        
        private protected string PrefixText => prefixText;
        private protected string SuffixText => suffixText;
        private protected bool PrefixBreak => prefixBreak;
        private protected bool SuffixBreak => suffixBreak;
        
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---

        private readonly int id = default;                           // modules internal id
        
        private protected Style previousConfiguration = null;            // The last configuration file.
        private protected volatile string compiledPrefix = string.Empty;     // suffix cache         
        private protected volatile string compiledSuffix = string.Empty;     // prefix cache
        
        private bool wasEnabled = true;     // The last enabled state of the module
        private bool wasActive = true;      // The last active state of the module
        private bool wasVisible = true;     // The last visible state of the module
        
        private bool runInspection;         // Private value that determines if an inspection task will continue.

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DELEGATES] ---

        /// <summary>
        /// Delegate type for module activation events.
        /// </summary>
        /// <param name="state"></param>
        public delegate void ModuleActivationDelegate(bool state);

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PUBLIC PROPERTIES] ---

        /// <summary>
        /// What does the module do. Optional field for clarity only. Can be accessed during runtime by console commands if visible
        /// </summary>
        public string Description => description == string.Empty ? null : description;
        
        /// <summary>
        /// Modules ID (string)
        /// </summary>
        public string UniqueName { get; } = null;
        
        /// <summary>
        /// Modules ID (integer)
        /// </summary>
        public int? UniqueId => id;
        
        /// <summary>
        /// Modules configuration. Determines objects visual style 
        /// </summary>
        public Style Style
        {
            get
            {
                try
                {
                    if (!useCustomStyle) return MonitoringSettings.Instance.Style;
                    return customStyle ? customStyle : MonitoringSettings.Instance.Style;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        /// <summary>
        /// The Modules UI element GameObject. (can be null)
        /// </summary>
        public GameObject SceneObject { get; set; }
        
        /// <summary>
        /// Returns true if the Module has a scene present (canvas element).
        /// </summary>
        /// <returns></returns>
        protected bool HasSceneObject() => SceneObject != null;

        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [STATIC] ---

        
        #region --- [STATIC FIELDS] ---

        /// <summary>
        /// Counter used for id allocation
        /// </summary>
        private static int idCounter = 0;

        #endregion
        
        
        #region --- [STATIC PROPERTIES] ---

        /// <summary>
        /// Dictionary containing every Module. Modules UniqueId is Key
        /// </summary>
        public static Dictionary<int, Module> ModuleDictionary { get; } = new Dictionary<int, Module>();

        /// <summary>
        /// Dictionary containing every Modules UniqueId. Modules UniqueName is Key
        /// </summary>
        public static Dictionary<string, int> ModuleIds { get; } = new Dictionary<string, int>();

        /// <summary>
        /// List containing every Module. Use to access all every module
        /// </summary>
        public static List<Module> Modules { get; } = new List<Module>();

        #endregion

        
        #region --- [STATIC METHODS] ---

        /// <summary>
        /// Returns the corresponding module if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Module GetModule([CanBeNull] string id)
            => ModuleIds.TryGetValue(id ?? string.Empty, out var value) ? ModuleDictionary[value] : null;
        
        /// <summary>
        /// Returns the corresponding module if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Module GetModule(int? id)
            => ModuleDictionary.TryGetValue(id ?? 0, out var value) ? value : null;
        
        /// <summary>
        /// Enable every module.
        /// </summary>
        /// <param name="enabled"></param>
        public static void EnableAll(bool enabled)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetEnabled(enabled);
            }
        }

        /// <summary>
        /// Activate every module.
        /// </summary>
        /// <param name="active"></param>
        public static void ActivateAll(bool active)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetActive(active);
            }
        }

        /// <summary>
        /// Set every module to visible. (only modules with scene object will become visible)
        /// </summary>
        /// <param name="visible"></param>
        public static void ShowAll(bool visible)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetVisible(visible);
            }
        }
        
        /// <summary>
        /// Enable & Activate / Disable & Deactivate every module
        /// </summary>
        /// <param name="state"></param>
        public static void EnableActivateAndShowAllModules(bool state)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetStates(state);
            }
        }

        public static void RepaintAll(bool excludeCustomStyles = false)
        {
            foreach (var module in ModuleDictionary)
            {
                if(excludeCustomStyles && module.Value.UseAndHasCustomStyle) continue;
                module.Value.Repaint(InvokeOrigin.GUI);
            }
        }

        
        #endregion

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INSPECTION] ---
        
        
        private CancellationTokenSource inspectionTaskCancellationSource = new CancellationTokenSource();
        private CancellationToken InspectionTaskCancellationToken => inspectionTaskCancellationSource.Token;
        private int millisecondsBetweenInspections => (int) (secondsBetweenInspections * 1000);

        
        /// <summary>
        /// Enable / Disable automatic inspection. This will invoke a asymmetric loop calling OnInspection repeatedly.
        /// </summary>
        /// <param name="value"></param>
        private protected void InitializeAutoInspection(bool? value = null)
        {
            CancelInspection();
            if(onlyInitializeWhenInScene && !HasSceneObject()) return;
            runInspection = value ?? enableAutoInspection;
            if(Application.isPlaying && runInspection) StartInspection();
        }

        
        /// <summary>
        /// Cancel Inspection Task and create a new CancellationTokenSource
        /// </summary>
        private void CancelInspection()
        {
            inspectionTaskCancellationSource.Cancel();
            inspectionTaskCancellationSource = new CancellationTokenSource();
        }
        
        /// <summary>
        /// Loop calling OnInspection repeatedly.
        /// </summary>
        private async void StartInspection()
        {
            while (runInspection && isActive)
            {
                try
                {
                    await Task.Delay(millisecondsBetweenInspections, InspectionTaskCancellationToken);
                }
                catch
                {
                    break;
                }
                // We call OnInspection after the delay, to prevent it from being called every time we change the delays
                // value in the inspector.
                OnInspection();
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [STATE] ---

        
        private protected void InvokeOnAnyStateChanged() => OnAnyStateChanged?.Invoke(isEnabled, isActive, isVisible);    
        
        /// <summary>
        /// Event is invoked when any of the modules state is altered either when it is constructed, initialized or set via inspector.
        /// </summary>
        public event ActiveAndVisibleDelegate OnAnyStateChanged;
        
        /// <summary>
        /// Event is invoked when the modules gets enabled / disabled either when it is constructed, initialized or set via inspector.
        /// Not the same as OnEnable!
        /// </summary>
        public event EnabledDelegate OnEnabledStateChanged;

        /// <summary>
        /// Event is invoked when the modules gets activated / deactivated either when it is constructed, initialized or set via inspector.
        /// </summary>
        public event ActiveDelegate OnActiveStateChanged;
        
        /// <summary>
        /// Event is invoked when the modules gets set to visible / invisible either when it is constructed, initialized or set via inspector.
        /// </summary>
        public event VisibilityDelegate OnVisibilityStateChanged;


        /// <summary>
        /// This method validates the current module states and will set properties depending on its private backfield values. 
        /// </summary>
        private protected void ValidateState()
        {
            if(isVisible != wasVisible)
                SetVisible();
            
            if(isActive != wasActive)
                SetActive();
            
            if(isEnabled != wasEnabled)
                SetEnabled();
        }
        
        
        public bool IsEnabled
        {
            get => isEnabled;
            private set
            {
                if(!value) SetStates(false);
                else
                {
                    if (wasEnabled) return;

                    StartInitialization(UnityEventType.InspectorUpdate);
                    ModuleEnabled();
                        
                    isEnabled = true;
                    wasEnabled = isEnabled;

                    OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                    OnEnabledStateChanged?.Invoke(true);
                }
            }
        }


        public void SetEnabled(bool enable)
            => IsEnabled = enable;
        
        public void SetEnabled(bool? enable = null)
            => IsEnabled = enable ?? isEnabled;
        
        
        public bool IsActive
        {
            get => isActive;
            private set
            {
                if (!value)
                {
                    if (wasActive) {
                        UnityEventCallbacks.RemoveEventListener(Tick, ApplicationState.PlayMode, UnityEventType.Update);
                        ModuleDeactivated();
                    }

                    CancelInspection();
                    IsVisible = false;
                }
                else
                {
                    // We subscribe Tick to Update if we enable it
                    if (!wasActive) {
                        UnityEventCallbacks.AddEventListener(
                            Tick, 
                            ApplicationState.PlayMode,
                            UnityEventType.Update);
                        InitializeAutoInspection();
                        ModuleActivated();
                    }
                }
                
                isActive = value;
                wasActive = isActive;
                OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                OnActiveStateChanged?.Invoke(value);
            }
        }
        
        public void SetActive(bool active)
            => IsActive = active;
        
        public void SetActive(bool? active = null)
            => IsActive = active ?? isActive;

        
        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                if (!value)
                {
                    isVisible = false;
                    if(wasVisible) ModuleInvisible();
                    wasVisible = isVisible;
                    OnAnyStateChanged?.Invoke(IsEnabled, IsActive, false);
                    OnVisibilityStateChanged?.Invoke(false);
                }
                else
                {
                    if (!isEnabled) IsEnabled = true;
                    if (!isActive)  IsActive = true;
                    
                    if (!isEnabled || !isActive) return;
                
                    if (!wasVisible)
                        ModuleVisible();
                
                    isVisible = true;
                    wasVisible = isVisible;
                
                    OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                    OnVisibilityStateChanged?.Invoke(true);
                }
            }
        }
        
        public void SetVisible(bool visible)
            => IsVisible = visible;

        public void SetVisible(bool? visible = null)
            => IsVisible = visible ?? isVisible;
        

        public bool IsEnabledActiveAndVisible
        {
            get => IsEnabled && IsActive && IsVisible;
            private set
            {
                if (!wasEnabled && value)
                {
                    StartInitialization(UnityEventType.InspectorUpdate);
                    ModuleEnabled();
                }
                if (wasEnabled && !value)
                {
                    ModuleDisabled(); 
                }
                   
                
                if (!wasActive && value) {
                    UnityEventCallbacks.AddEventListener(Tick, true, ApplicationState.PlayMode, UnityEventType.Update);
                    ModuleActivated();
                }
                if (wasActive && !value) {
                    UnityEventCallbacks.RemoveEventListener(Tick, ApplicationState.PlayMode, UnityEventType.Update);
                    ModuleDeactivated();
                }
                
                if (!wasVisible && value)
                    ModuleVisible();
                if (wasVisible && !value)
                    ModuleInvisible();


                isEnabled = value;
                isActive = value;
                isVisible = value;
                wasEnabled = isEnabled;
                wasActive = isActive;
                wasVisible = isVisible;

                OnAnyStateChanged?.Invoke(IsEnabled, IsActive, IsVisible);
                OnEnabledStateChanged?.Invoke(value);
                OnActiveStateChanged?.Invoke(value);
                OnVisibilityStateChanged?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Set the visible and visible state of the module
        /// </summary>
        /// <param name="value"></param>
        public void SetStates(bool value)
            => IsEnabledActiveAndVisible = value;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VIRTUAL] ---


        /// <summary>
        /// Tick is called every (MonoBehaviour) Update call if the module is active.
        /// </summary>
        protected virtual void Tick()
        {
        }
        
        /// <summary>
        /// Method is called repeatedly if auto inspection is enabled and the module is active.
        /// Delay between calls can be set in the inspector
        /// </summary>
        protected virtual void OnInspection()
        {
        }

        /// <summary>
        /// ModuleEnabled is called when the module is enabled
        /// </summary>
        protected virtual void ModuleEnabled()
        {
        }

        /// <summary>
        /// ModuleDisabled is called when the module is disabled
        /// </summary>
        protected virtual void ModuleDisabled()
        {
        }

        /// <summary>
        /// ModuleActivated is called when the module is activated
        /// </summary>
        protected virtual void ModuleActivated()
        {
        }

        /// <summary>
        /// ModuleDeactivated is called when the module is deactivated
        /// </summary>
        protected virtual void ModuleDeactivated()
        {
        }
        
        /// <summary>
        /// ModuleVisible is called when the module is set visible
        /// </summary>
        protected virtual void ModuleVisible()
        {
        }

        /// <summary>
        /// ModuleInvisible is called when the module is set invisible
        /// </summary>
        protected virtual void ModuleInvisible()
        {
        }

        /// <summary>
        /// OnQuit is called either when exiting playMode or when quitting the application.
        /// </summary>
        protected virtual void OnQuit()
        {
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONSTRUCTOR] ---

        protected Module()
        {
            id = idCounter++;
            UniqueName = $"{GetType().Name.Replace("Module", string.Empty)}";
            SceneObject = null;
           
            Modules.Add(this);
            ModuleDictionary.Add(id, this);

            var num = 0;
            var doAgain = false;
            do
            {
                try
                {
                    ModuleIds.Add(UniqueName, id);
                    doAgain = false;
                }
                catch
                {
                    UniqueName = $"{GetType().Name.Replace("Module", string.Empty)}{num:00}";
                    doAgain = true;
                    num++;
                }
            } while (doAgain);

            ConstructionFinished();
        }

        private async void ConstructionFinished()
        {
            await Task.CompletedTask.BreakContext();
            
            if(OnlyInitializeWhenInScene && !HasSceneObject()) return;
            
            if(IsEnabled) ModuleEnabled();
            if(IsActive) ModuleActivated();
            if(IsVisible) ModuleVisible();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [GENERIC OVERRIDES] ---
        
        // The abstract member in this region are overriden by the generic inheritor class. 

        /// <summary>
        /// Method can be used to set the value of a module dirty.
        /// Note that it is not guaranteed that the value can be converted.
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="U"></typeparam>
        public abstract void SetValueDirty<U>(U value);
        
        /// <summary>
        /// Get the type modules type as string.
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeOfTAsString();

        /// <summary>
        /// Get the type of the modules value.
        /// </summary>
        /// <returns></returns>
        public abstract Type ValueType();

        /// <summary>
        /// Get the value of the modules value T
        /// </summary>
        /// <returns></returns>
        public abstract object GetValue();
        
        
        /// <summary>
        /// Get the modules raw value as string
        /// </summary>
        /// <returns></returns>
        public abstract string GetStateRaw();
        
        /// <summary>
        /// Get the modules (formatted) value as string
        /// /// </summary>
        /// <param name="interpretationOption"></param>
        /// <returns></returns>
        public abstract string GetState(ValueInterpretationOption interpretationOption);


        /// <summary>
        /// Force the canvas elements to update.
        /// </summary>
        /// <param name="origin"></param>
        public abstract void Repaint(InvokeOrigin origin);
        
        
        /// <summary>
        /// Add a listener to the modules Repaint event
        /// </summary>
        /// <param name="listener"></param>
        public abstract void AddOnRepaintListener(Action<Style, string, InvokeOrigin> listener);
       
        /// <summary>
        /// Remove a listener form the modules Repaint event
        /// </summary>
        /// <param name="listener"></param>
        public abstract void RemoveOnRepaintChangedListener(Action<Style, string, InvokeOrigin> listener);
        
        
        
        /// <summary>
        /// Add multiple listener to the modules OnValueChanged event
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="updateListener"></param>
        public abstract void AddOnValueChangedListener(OnValueChangedContext eventType,
            params Action<IModuleData>[] updateListener);
        
        /// <summary>
        /// Remove a listener form the modules OnValueChanged event
        /// </summary>
        /// <param name="context"></param>
        public abstract void RemoveOnValueChangedListener(OnValueChangedContext context);
        
        /// <summary>
        /// Remove multiple listener form the modules OnValueChanged event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="updateListener"></param>
        public abstract void RemoveOnValueChangedListener(OnValueChangedContext context,
            params Action<IModuleData>[] updateListener);
        
        
        
        /// <summary>
        /// Set delegate that will automatically activate/deactivate the module
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public abstract void InitializeActivationEvent(ref ModuleActivationDelegate invoker, bool invert = false);
        
        /// <summary>
        /// Set delegate that will automatically enable/disable the module
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public abstract void InitializeEnableEvent(ref ModuleActivationDelegate invoker, bool invert = false);


        /// <summary>
        /// StartInitialization will begin the initialization of the module.
        /// </summary>
        /// <param name="context"></param>
        private protected abstract void StartInitialization(UnityEventType context);

        #endregion
    }
}