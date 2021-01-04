using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    public abstract class Module : ScriptableObject, IState
    {
        #region --- [INSPECTOR] ---
        
        [Tooltip("Enabled modules will execute their update and inspection functions. Comparable to MonoBehaviours")]
        [SerializeField] private bool isEnabled = true;
        
        [Tooltip("Active modules are visible")]
        [SerializeField] private bool isActive = true;
        
        
        
        [Header("Settings")]
        [Tooltip("Set a custom style for the module. False will use the default style instead")]
        [SerializeField] protected bool useCustomStyle = default;
        
        [Tooltip("If enabled, use this style. Null will use the default style instead")]
        [SerializeField] protected Configurable customStyle = null;

        
        
        [Space]
        [Tooltip("How should the value be displayed")]
        [SerializeField] protected ValueInterpretationOption previewValueAs = ValueInterpretationOption.CurrentValue;
        
        [Tooltip("Set custom prefix text")]
        [SerializeField] protected string prefixText = default;
        
        [Tooltip("Set custom suffix text")]
        [SerializeField] protected string suffixText = default;
        
        [Tooltip("Insert a break after the prefix")]
        [SerializeField] protected bool prefixBreak = false;
        
        [Tooltip("Insert a break before the suffix")]
        [SerializeField] protected bool suffixBreak = false;
        
        
        
        [Space]
        [Tooltip("What does the module do. Optional field for clarity only. Can be accessed during runtime by console commands if enabled")]
        [SerializeField] private string description = default;
        
        
        
        [Space]
        [Tooltip("If enabled OnInspection will be called periodically. Use to validate the values integrity")]
        [SerializeField] private bool enableAutoInspection;
        
        [Tooltip("How much time should pass between inspections")]
        [SerializeField][Range(.1f, 60f)] private float secondsBetweenInspections = 1f;

        #endregion
        
        #region --- [FIELDS] ---

        protected Configurable previousConfiguration = null;
        protected string compiledPrefix = string.Empty;         
        protected string compiledSuffix = string.Empty;
        private readonly int id = default;
        private bool wasEnabled = true;
        private bool wasActive = true;

        #endregion
        
        #region --- [PROPERTIES] ---

        /// <summary>
        /// What does the module do. Optional field for clarity only. Can be accessed during runtime by console commands if enabled
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
        public Configurable Configuration
        {
            get
            {
                try
                {
                    if (!useCustomStyle) return MonitorBehaviour.Instance.GlobalConfiguration;
                    return customStyle ? customStyle : MonitorBehaviour.Instance.GlobalConfiguration;
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
        

        /// <summary>
        /// Enable / Disable every module.
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
        /// Activate / Deactivate every module
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
        /// Enable & Activate / Disable & Deactivate every module
        /// </summary>
        /// <param name="activeAndEnable"></param>
        public static void ActivateAndEnableAll(bool activeAndEnable)
        {
            foreach (var module in ModuleDictionary)
            {
                module.Value.SetActiveAndEnabled(activeAndEnable);
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
        protected void SetAutoInspection(bool? value = null)
        {
            CancelInspection();
            enableAutoInspection = value ?? enableAutoInspection;
            if(Application.isPlaying && enableAutoInspection) StartInspection();
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
            while (enableAutoInspection)
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
        /// Event is invoked if the modules active and / or enabled state has changed
        /// </summary>
        public event ActiveAndEnabledDelegate OnActiveAndEnabledStateChanged;
        
        /// <summary>
        /// Event is invoked if the modules active state has changed
        /// </summary>
        public event ActiveDelegate OnActiveStateChanged;
        
        /// <summary>
        /// Event is invoked if the modules enabled state has changed
        /// </summary>
        public event EnabledDelegate OnEnabledStateChanged;
        
        
        public bool IsEnabled
        {
            get => isEnabled;
            private set
            {
                if (!value)
                {
                    SetActiveAndEnabled(false);
                }
                    
                else
                {
                    // We subscribe Tick to Update if we enable it
                    if (!wasEnabled) {
                        UnityEventCallbacks.AddEventListener(
                            listener: Tick, 
                            removePreviousListener: true,
                            ApplicationState.PlayMode,
                            UnityEventType.Update);
                        
                        OnModuleEnabled();
                    }
                        
                    
                    isEnabled = true;
                    wasEnabled = isEnabled;
            
                    OnActiveAndEnabledStateChanged?.Invoke(IsActive, IsEnabled);
                    OnEnabledStateChanged?.Invoke(true);
                }
            }
        }
        
        /// <summary>
        /// Enable / Disable the module
        /// </summary>
        /// <param name="enabled"></param>
        public void SetEnabled(bool enabled)
            => IsEnabled = enabled;
        
        /// <summary>
        /// Enable / Disable the module
        /// </summary>
        /// <param name="enabled"></param>
        public void SetEnabled(bool? enabled = null)
            => IsEnabled = enabled ?? isEnabled;

        
        public bool IsActive
        {
            get => isActive;
            private set
            {
                if(!isEnabled) return;
                
                if (!wasActive && value)
                    OnModuleActivated();
                if (wasActive && !value)
                    OnModuleDeactivated();
                
                isActive = value;
                wasActive = isActive;
                
                OnActiveAndEnabledStateChanged?.Invoke(IsActive, IsEnabled);
                OnActiveStateChanged?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Activate / Deactivate the module
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
            => IsActive = isEnabled? active : isActive;
        
        /// <summary>
        /// Activate / Deactivate the module
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool? active = null)
            => IsActive = isEnabled? active ?? isActive : isActive;
        

        public bool IsActiveAndEnabled
        {
            get => IsEnabled && IsActive;
            private set
            {
                if (!wasEnabled && value) {
                    UnityEventCallbacks.AddEventListener(Tick, true, ApplicationState.PlayMode, UnityEventType.Update);
                    OnModuleEnabled();
                }
                if (wasEnabled && !value) {
                    UnityEventCallbacks.RemoveEventListener(Tick, ApplicationState.PlayMode, UnityEventType.Update);
                    OnModuleDisabled();
                }
                if (!wasActive && value)
                    OnModuleActivated();
                if (wasActive && !value)
                    OnModuleDeactivated();
                
                
                isEnabled = value;
                isActive = value;
                wasEnabled = isEnabled;
                wasActive = isActive;

                OnActiveAndEnabledStateChanged?.Invoke(IsActive, IsEnabled);
                OnEnabledStateChanged?.Invoke(value);
                OnActiveStateChanged?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Set the active and enabled state of the module
        /// </summary>
        /// <param name="value"></param>
        public void SetActiveAndEnabled(bool value)
            => IsActiveAndEnabled = value;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VIRTUAL] ---

        /// <summary>
        /// Method is called repeatedly if auto inspection and the module are enabled.
        /// Delay between calls can be determined in the inspector
        /// </summary>
        protected virtual void OnInspection() {}
        

        /// <summary>
        /// Tick is called every Unity-Update if the module is enabled.
        /// </summary>
        protected virtual void Tick() {}
        
        
        /// <summary>
        /// OnModuleEnabled is called when the module is enabled
        /// </summary>
        protected virtual void OnModuleEnabled(){}
        
        /// <summary>
        /// OnModuleDisabled is called when the module is disabled
        /// </summary>
        protected virtual void OnModuleDisabled(){}
        
        /// <summary>
        /// OnModuleActivated is called when the module is activated
        /// </summary>
        protected virtual void OnModuleActivated(){}
        
        /// <summary>
        /// OnModuleDeactivated is called when the module is deactivated
        /// </summary>
        protected virtual void OnModuleDeactivated(){}

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
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [ABSTRACT] ---
        
        /// <summary>
        /// Get the type modules type as string.
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeOfTAsString();

        /// <summary>
        /// Get the type of the modules value.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetTypeOfT();

        /// <summary>
        /// Get the value of the modules value T
        /// </summary>
        /// <returns></returns>
        public abstract object GetValueOfT();
        
        
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
        /// Add a listener to the modules Repaint event
        /// </summary>
        /// <param name="listener"></param>
        public abstract void AddOnRepaintChangedListener(Action<Configuration.Configurable, string, InvokeOrigin> listener);
       
        /// <summary>
        /// Remove a listener form the modules Repaint event
        /// </summary>
        /// <param name="listener"></param>
        public abstract void RemoveOnRepaintChangedListener(Action<Configuration.Configurable, string, InvokeOrigin> listener);
        
        
        
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
        public abstract void SetActiveDelegate(ref Action<bool> invoker, bool invert = false);
        
        /// <summary>
        /// Set delegate that will automatically enable/disable the module
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public abstract void SetEnableDelegate(ref Action<bool> invoker, bool invert = false);

        #endregion
    }
}