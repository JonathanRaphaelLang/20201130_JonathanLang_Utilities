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
    public abstract class Module : ScriptableObject, IState
    {
        #region --- [INSPECTOR] ---
       
        [Tooltip("Enabled modules will be initialized")]
        [SerializeField] private bool isEnabled = true;
        
        [Tooltip("Active modules will execute their update and inspection functions. Comparable to MonoBehaviours")]
        [SerializeField] private bool isActive = true;
        
        [Tooltip("Visible modules are visible")]
        [SerializeField] private bool isVisible = true;
        
        
        
        [Header("Settings")]
        [Tooltip("Set a custom style for the module. False will use the default style instead")]
        [SerializeField] private bool useCustomStyle = default;
        
        [Tooltip("If visible, use this style. Null will use the default style instead")]
        [SerializeField] private StyleBase customStyleBase = null;

        [Header("Warnings")]
        [Tooltip("If visible, custom warnings will (can) be logged")]
        [SerializeField] private bool enableWarnings = true;
        
        [Tooltip("If visible, custom warnings will (can) be logged")]
        [SerializeField] private bool enableInitializeUpdateEventWarnings = true;
        
        [Tooltip("If visible, custom warnings will (can) be logged")]
        [SerializeField] private bool enableInitializeValueWarnings = true;
       
        
        
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
        
        [Tooltip("Insert a break after the prefix")]
        [SerializeField] private bool prefixBreak = false;
        
        [Tooltip("Insert a break before the suffix")]
        [SerializeField] private bool suffixBreak = false;
        
        
        
        [Space]
        [Tooltip("What does the module do. Optional field for clarity only. Can be accessed during runtime by console commands if visible")]
        [SerializeField][TextArea] private string description = default;
        
        
        
        [Space]
        [Tooltip("If visible OnInspection will be called periodically. Use to validate the values integrity")]
        [SerializeField] private bool enableAutoInspection = default;
        
        [Tooltip("How much time should pass between inspections")]
        [SerializeField][Range(.1f, 60f)] private float secondsBetweenInspections = 1f;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PROTECTED PROPERTIES] ---
        
        protected bool UseCustomStyle => useCustomStyle;

        protected StyleBase CustomStyleBase => customStyleBase;
        
        protected bool EnableWarnings => enableWarnings;
        protected bool EnableInitializeValueWarnings => enableInitializeValueWarnings;
        protected bool EnableInitializeUpdateEventWarnings => enableInitializeUpdateEventWarnings;

        
        
        protected ValueInterpretationOption PreviewValueAs => previewValueAs;

        protected bool ResetValueOnQuit => resetValueOnQuit;
        
        protected string PrefixText => prefixText;
        protected string SuffixText => suffixText;
        protected bool PrefixBreak => prefixBreak;
        protected bool SuffixBreak => suffixBreak;
        
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---

        protected StyleBase previousConfiguration = null;
        protected volatile string compiledPrefix = string.Empty;         
        protected volatile string compiledSuffix = string.Empty;
        private readonly int id = default;
        private bool wasEnabled = true;
        private bool wasVisible = true;
        private bool wasActive = true;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [DELEGATES] ---

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
        public StyleBase Configuration
        {
            get
            {
                try
                {
                    if (!useCustomStyle) return MonitorBehaviour.Instance.MonitoringConfiguration;
                    return customStyleBase ? customStyleBase : MonitorBehaviour.Instance.MonitoringConfiguration;
                }
                catch
                {
                    return null;
                }
            }
        }

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
        
        
        public static void EnableAll(bool enabled)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetEnabled(enabled);
            }
        }

        public static void ActivateAll(bool active)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetActive(active);
            }
        }

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
        /// <param name="activeAndEnable"></param>
        public static void InitializeAll(bool activeAndEnable)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetStates(activeAndEnable);
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
        protected internal void InitializeAutoInspection(bool? value = null)
        {
            CancelInspection();
            runInspection = value ?? enableAutoInspection;
            if(Application.isPlaying && runInspection) StartInspection();
        }

        private bool runInspection;
        
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

        /// <summary>
        /// 
        /// </summary>
        public event ActiveAndVisibleDelegate OnAnyStateChanged;
        protected void InvokeOnAnyStateChanged() => OnAnyStateChanged?.Invoke(isEnabled, isActive, isVisible);    
        
        /// <summary>
        /// 
        /// </summary>
        public event EnabledDelegate OnEnabledStateChanged;

        /// <summary>
        /// 
        /// </summary>
        public event ActiveDelegate OnActiveStateChanged;
        
        /// <summary>
        /// 
        /// </summary>
        public event VisibilityDelegate OnVisibilityStateChanged;

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
                            listener: Tick, 
                            removePreviousListener: true,
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
                    if(!isEnabled) return;
                    if(!isActive) return;
                
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
            => IsVisible = isActive && visible;

        public void SetVisible(bool? visible = null)
            => IsVisible = isActive && (visible ?? isVisible);
        

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
        /// Tick is called every Unity-Update if the module is visible.
        /// </summary>
        protected virtual void Tick()
        {
        }
        
        /// <summary>
        /// Method is called repeatedly if auto inspection and the module are visible.
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
        /// ModuleActivated is called when the module is set visible
        /// </summary>
        protected virtual void ModuleVisible()
        {
        }

        /// <summary>
        /// ModuleDeactivated is called when the module is set invisible
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

            AfterConstruction();
        }

        private async void AfterConstruction()
        {
            await Task.Delay(1);
            if(IsEnabled) ModuleEnabled();
            if(IsActive) ModuleActivated();
            if(IsVisible) ModuleVisible();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [GENERIC OVERRIDES] ---
        
        // The abstract member in this region are overriden by the generic inheritor class. 

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
        public abstract void AddOnRepaintChangedListener(Action<Configuration.StyleBase, string, InvokeOrigin> listener);
       
        /// <summary>
        /// Remove a listener form the modules Repaint event
        /// </summary>
        /// <param name="listener"></param>
        public abstract void RemoveOnRepaintChangedListener(Action<Configuration.StyleBase, string, InvokeOrigin> listener);
        
        
        
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


        protected abstract void StartInitialization(UnityEventType context);

        #endregion
    }
}