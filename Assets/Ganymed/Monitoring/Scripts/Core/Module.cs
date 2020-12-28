using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Enumerations;
using Ganymed.Monitoring.Interfaces;
using Ganymed.Monitoring.Structures;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    public abstract class Module : ScriptableObject, IVisible
    {
#pragma warning disable 649
        
        #region --- [INSPECTOR] ---

        [Header("Visibility")]
        [SerializeField] protected Visibility currentVisibility = Visibility.ActiveAndVisible;
        [Space]
        [SerializeField] protected Visibility visibilityOnLoad = Visibility.ActiveAndVisible;
        [SerializeField] protected Visibility visibilityOnEdit = Visibility.ActiveAndVisible;
        [Space]
        [Header("Style")]
        [SerializeField] protected bool useCustomStyle;
        [SerializeField] protected Configurable customConfigurable = null;
        [Space]
        [Header("Config")]
        [SerializeField] protected ValueInterpretationOption previewValueAs = ValueInterpretationOption.CurrentValue;
        [SerializeField] protected string prefixText;
        [SerializeField] protected string suffixText;
        [SerializeField] protected bool prefixBreak = false;
        [SerializeField] protected bool suffixBreak = false;
        
        [Tooltip("Update automatically validate the modules values every X amount of time passed")]
        [SerializeField]
        public bool autoInspect;
        
        [HideInInspector] [SerializeField] public InspectPeriods InspectOn = InspectPeriods.Yield;
        [HideInInspector] [SerializeField] public int milliseconds = 1000;
        
        #endregion
        
        #region --- [FIELDS] ---

        private Visibility cachedVisibility = Visibility.ActiveAndVisible;
        private Configurable lastConfigurable;
        protected string appendedPrefix = string.Empty;
        protected string appendedSuffix = string.Empty;

        #endregion
        
        #region --- [EVENTS] ---

        public event VisibilityDelegate OnVisibilityChanged;

        #endregion

        #region --- [PROPERTIES] ---

        public Visibility VisibilityFlags { get; private set; } = Visibility.Inactive;
       
        protected Configurable Config
        {
            get
            {
                try
                {
                    if (!useCustomStyle) return MonitorBehaviour.Instance.GlobalConfiguration;
                    return customConfigurable ? customConfigurable : MonitorBehaviour.Instance.GlobalConfiguration;
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

        #region --- [ENUM] ---

        public enum InspectPeriods
        {
            Update,
            FixedUpdate,
            Yield
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VISIBLITY] ---
        
        /// <summary>
        /// Flag the visibility of the module.
        /// </summary>
        /// <param name="visibility">the new state of visibility</param>
        public async void SetVisibility(Visibility visibility)
        {
            VisibilityChanged(
                newState: visibility,
                lastState: VisibilityFlags);

            VisibilityFlags = visibility;
            
            OnVisibilityChanged?.Invoke(
                was: visibility,
                now: VisibilityFlags);

            await Task.Delay(10);
            InvokeToggle(visibility == Visibility.ActiveAndVisible);
            OnValidate();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---
      
        protected virtual void OnValidate()
        {
            ValidateModuleGUI(InvokeOrigin.UnityMessage);
           
            
            if(currentVisibility != cachedVisibility)
                SetVisibility(currentVisibility);

            currentVisibility = VisibilityFlags;
            cachedVisibility = VisibilityFlags;
        }
       
        /// <summary>
        /// Validates the visual representation of the module. 
        /// </summary>
        /// <param name="origin">origin</param>
        /// <param name="timeout">Time out after x amount of milliseconds if the styleBase is null</param>
        public async void ValidateModuleGUI(InvokeOrigin origin = InvokeOrigin.Unknown, int timeout = 1000)
        {
            if (Config == null)
            {
                var timer = 0;
                while (Config == null)
                {
                    await Task.Delay(1);
                    if (timer++ <= timeout) continue;
                    Debug.Log("Timeout");
                    break;
                }
            }
            
            if (lastConfigurable != null)
                lastConfigurable.OnValuesChanged -= ValidateContent;

            Config.OnValuesChanged += ValidateContent;
            
            ValidateContent(Config, origin);
            lastConfigurable = Config;
        }

        private void ValidateContent(Configuration.Configurable provider, InvokeOrigin origin) 
        {
            AppendAffix();
            UpdateGUI(origin);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VIRTUAL] ---

        /// <summary>
        /// Override this Method for logic depending on the modules visibility flag.
        /// </summary>
        /// <param name="newState">The new visibility state</param>
        /// <param name="lastState">The last visibility state</param>
        protected virtual void VisibilityChanged(Visibility newState, Visibility lastState){}

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [ABSTRACT] ---
        
        protected abstract void InvokeToggle(bool show);
        
        protected abstract void UpdateGUI(InvokeOrigin origin, bool delay = false, int milliseconds = 100);

        public abstract string GetStateRaw();
        public abstract string GetState(ValueInterpretationOption interpretationOption);
        
        
        protected abstract void AppendAffix();
        
        
        public abstract void AddOnGUIChangedListener(Action<Configuration.Configurable, string, InvokeOrigin> listener);
        public abstract void RemoveOnGUIChangedListener(Action<Configuration.Configurable, string, InvokeOrigin> listener);
        public abstract void AddOnValueChangedListener(OnValueChangedContext eventType,
            params Action<IModuleUpdateData>[] updateListener);
        public abstract void RemoveOnValueChangedListener(OnValueChangedContext context);
        public abstract void RemoveOnValueChangedListener(OnValueChangedContext context,
            params Action<IModuleUpdateData>[] updateListener);
        
        public enum CoreEventType { Update, Toggle }

        /// <summary>
        /// Set event invoking UI toggle eventListener 
        /// </summary>
        /// <param name="ToggleEvent"></param>
        /// <param name="coreEventType"></param>
        public abstract void SetValueDelegate(ref Action<bool> ToggleEvent, CoreEventType coreEventType);
        
        #endregion
    }

    //------------------------------------------------------------------------------------------------------------------
    // GENERIC MODULE
    //------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Generic Module
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Module<T> : Module
    {
        #region --- [FIELDS] ---

        /// <summary>
        /// cached Module Update Data
        /// </summary>
        private ModuleUpdateData<T> ModuleUpdateData;

        /// <summary>
        /// DefaultValue event listener
        /// </summary>
        private readonly Dictionary<OnValueChangedContext, Action<IModuleUpdateData>> OnValueChanged
            = new Dictionary<OnValueChangedContext, Action<IModuleUpdateData>>();

        /// <summary>
        /// Generic event listener (T)
        /// </summary>
        private readonly Dictionary<OnValueChangedContext, Action<IModuleUpdateData<T>>> OnValueChangedT
            = new Dictionary<OnValueChangedContext, Action<IModuleUpdateData<T>>>();

        /// <summary>
        /// GUI event listener will be subscribed to this event
        /// </summary>
        private event Action<Configurable, string, InvokeOrigin> OnGUIChanged;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [DEFAULT VALUES] ---

        /// <summary>
        /// The default value of T
        /// </summary>
        protected static T defaultValue = default;
        
        
        /// <summary>
        /// The last and/or cached value of T
        /// </summary>
        protected T CurrentValue => currentValue;
        /// <summary>
        /// The current value of T
        /// </summary>
        private T currentValue = default;
        
        
        
        /// <summary>
        /// The last and/or cached value of T
        /// </summary>
        protected T CachedValue => cachedValue;
        
        /// <summary>
        /// The last and/or cached value of T
        /// </summary>
        private T cachedValue = default;
        
        
        //--- OVERRIDE DEFAULT VALUE ---
                
        /// <summary>
        /// Override the default value of T (ref) 
        /// </summary>
        /// <param name="value">new default (you can pass as ref)</param>
        protected void OverrideDefaultValue(ref T value) => defaultValue = value;
        
        /// <summary>
        /// Override the default value of T (ref) 
        /// </summary>
        /// <param name="value">new default (you can pass as ref)</param>
        /// <param name="old">old default</param>
        protected void OverrideDefaultValue(ref T value, out T old) {
            old = defaultValue;
            defaultValue = value;
        }
                
        /// <summary>
        /// Override the default value of T 
        /// </summary>
        /// <param name="value">new default</param>
        protected void OverrideDefaultValue(T value) => defaultValue = value; 
        
        /// <summary>
        /// Override the default value of T 
        /// </summary>
        /// <param name="value">new default (you can pass as ref)</param>
        /// <param name="old">old default</param>
        protected void OverrideDefaultValue(T value, out T old) {
            old = defaultValue;
            defaultValue = value;
        }

        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [ABSTRACT] ---
        
        /// <summary>
        /// OnInitialize is called during the module initialization which will happen during the Unity Start method.
        /// </summary>
        protected abstract void OnInitialize();

        #endregion

        #region --- [VIRTUAL] ---

        /// <summary>
        /// Override this method if the styleBase of the value is dynamic (eg. fps color depending on value)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual string DynamicValue(T value) => value.ToString();
        
        /// <summary>
        /// OnModuleUpdate is invoked on every module update event (GUI, UPDATE, etc.) Use for update dependent logic.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnModuleUpdate(ModuleUpdateData<T> data){}

        #endregion

        #region --- [SEALED] ---

        /// <summary>
        /// Returns the raw state ToString() of the current value without any additional pre or suffix
        /// </summary>
        /// <returns>Raw Value</returns>
        public sealed override string GetStateRaw() => currentValue != null ? currentValue.ToString() : "null";

        /// <summary>
        /// Returns a compiled and appended string representing the current state
        /// </summary>
        /// <param name="interpretationOption">How to interpret the state</param>
        /// <returns></returns>
        public sealed override string GetState(ValueInterpretationOption interpretationOption)
        {
            switch (interpretationOption)
            {
                case ValueInterpretationOption.CurrentValue:
                    return $"{appendedPrefix}{DynamicValue(currentValue)}{appendedSuffix}";
                
                case ValueInterpretationOption.LastValue:
                    return $"{appendedPrefix}{DynamicValue(cachedValue)}{appendedSuffix}";
                
                case ValueInterpretationOption.DefaultValue:
                    return $"{appendedPrefix}{DynamicValue(defaultValue)}{appendedSuffix}";

                case ValueInterpretationOption.Type:
                    return $"{appendedPrefix}{typeof(T).Name}{appendedSuffix}";
                
                default:
                    return "ERROR";
            }
        }
        
        protected sealed override void AppendAffix()
        {
            appendedPrefix =  
                $"{Config.PrefixTextStyle}" +
                $"{(Config.IndividualFontSize? Config.PrefixFontSize.ToFontSize() : Config.fontSize.ToFontSize())}" +
                $"{Config.PrefixColor.ToRichTextMarkup()}" +
                $"{prefixText}" +
                $"{((Config.AutoSpace) ? (prefixText.Length > 0)? " " : string.Empty : string.Empty)}" +
                $"{(prefixBreak ? "\n" : string.Empty)}" +
                $"{Config.InfixTextStyle}" +
                $"{(Config.IndividualFontSize? Config.InfixFontSize.ToFontSize() : Config.fontSize.ToFontSize())}" +
                $"{Config.InfixColor.ToRichTextMarkup()}" +
                $"{(Config.AutoBrackets? "[" : string.Empty)}";

            appendedSuffix =  
                $"{(suffixBreak ? "\n" : string.Empty)}" +
                $"{(Config.AutoBrackets?$"{Config.InfixColor.ToRichTextMarkup()}]": string.Empty)}" +
                $"{Config.SuffixTextStyle}" +
                $"{(Config.IndividualFontSize? Config.SuffixFontSize.ToFontSize() : Config.fontSize.ToFontSize())}" +
                $"{Config.SuffixColor.ToRichTextMarkup()}" +
                $"{((Config.AutoSpace)? " " : string.Empty)}" +
                $"{suffixText}";
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTLISTENER (SEALED)] ---

        public sealed override void AddOnGUIChangedListener(Action<Configurable, string, InvokeOrigin> listener)
        {
            OnGUIChanged -= listener;
            OnGUIChanged += listener;
        }
        
        
        public sealed override void RemoveOnGUIChangedListener(Action<Configurable, string, InvokeOrigin> listener)
        {
            OnGUIChanged -= listener;
        }
        

        public sealed override void AddOnValueChangedListener(OnValueChangedContext eventType,
            params Action<IModuleUpdateData>[] updateListener)
        {
            foreach (var listener in updateListener)
            {
                OnValueChanged[eventType] += listener;
            }
        }
        

        public void AddOnValueChangedListener(OnValueChangedContext eventType,
            params Action<IModuleUpdateData<T>>[] updateListener)
        {
            foreach (var listener in updateListener)
            {
                OnValueChangedT[eventType] += listener;
            }
        }

        //REMOVE--------------------------------------------------------------------------------------------------------
        
        public sealed override void RemoveOnValueChangedListener(OnValueChangedContext context)
        {
            OnValueChanged[context] = null;
            OnValueChanged[context] = delegate(IModuleUpdateData data) { };
        }
        
        
        public sealed override void RemoveOnValueChangedListener(OnValueChangedContext context,
            params Action<IModuleUpdateData>[] updateListener)
        {
            try
            {
                foreach (var listener in updateListener)
                {
                    // ReSharper disable once DelegateSubtraction
                    OnValueChanged[context] -= listener;
                }
            }
            finally{}
        }

        
        public void RemoveOnValueChangedListener(OnValueChangedContext context,
            params Action<IModuleUpdateData<T>>[] updateListener)
        {
            try
            {
                foreach (var listener in updateListener)
                {
                    // ReSharper disable once DelegateSubtraction
                    OnValueChangedT[context] -= listener;
                }
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
        }

        
        #endregion

        
        
        //--------------------------------------------------------------------------------------------------------------

        
        
        #region --- [CONSTRUCTOR] ---

        static Module()
        {
            if (typeof(T).IsArray)
            {
                if (typeof(T).GetArrayRank() > 1)
                    defaultValue = (T)(object)Array.CreateInstance(typeof(T).GetElementType(), new int[typeof(T).GetArrayRank()]);
                else
                    defaultValue = (T)(object)Array.CreateInstance(typeof(T).GetElementType(), 0);
                return;
            }
            
            if (typeof(T) == typeof(string))
            {
                // string is IEnumerable<char>, but don't want to treat it like a collection
                defaultValue = (T)(object)string.Empty;
                return;
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                // check if an empty array is an instance of T
                if (typeof(T).IsAssignableFrom(typeof(object[])))
                {
                    defaultValue = (T)(object)new object[0];
                    return;
                }

                if (typeof(T).IsGenericType && typeof(T).GetGenericArguments().Length == 1)
                {
                    Type elementType = typeof(T).GetGenericArguments()[0];
                    if (typeof(T).IsAssignableFrom(elementType.MakeArrayType()))
                    {
                        defaultValue = (T)(object)Array.CreateInstance(elementType, 0);
                        return;
                    }
                }

                throw new NotImplementedException("No default value is implemented for type " + typeof(T).FullName);
            }
        }
        
        
        protected Module()
        {
            foreach (OnValueChangedContext type in Enum.GetValues(typeof(OnValueChangedContext)))
            {
                OnValueChanged.Add(type, delegate(IModuleUpdateData data) { });
                OnValueChangedT.Add(type, delegate(IModuleUpdateData<T> data) { });
            }
            
            cachedValue = defaultValue;
            currentValue = defaultValue;
            
            UnityEventCallbacks.AddEventListener(Initialize, true, CallbackDuring.EditAndPlayMode,
                #if UNITY_EDITOR
                UnityEventType.Recompile,
                #endif
                UnityEventType.Awake);
            
            UnityEventCallbacks.AddEventListener(OnRecompile, true, UnityEventType.Recompile);
            
#if !UNITY_EDITOR
            UnityEventCallbacks.AddEventListener(OnRecompile, true, UnityEventType.Start);
#endif
        }

        private void OnRecompile() => ValidateModuleGUI(InvokeOrigin.Recompile, 1000);
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [AUTO INSPECTION] ---

        /// <summary>
        /// OnInspection is called in determinable intervals if auto inspection is enabled.
        /// Use this method invocation to automatically validate the state of the observed Value
        /// Note: this should only be used if the state of the observed Value can be altered
        /// without the module taking note.
        /// </summary>
        protected virtual void OnInspection() {}
        
        
        private async void InspectionTask(int ms)
        {
            while (Application.isPlaying)
            {
                OnInspection();
                await Task.Delay(ms);
            }
        }
        
        
        private void SetupInspectionLoop()
        {
            if(!autoInspect) return;
            switch (InspectOn)
            {
                case InspectPeriods.Update:
                    UnityEventCallbacks.AddEventListener(OnInspection, true, CallbackDuring.PlayMode, UnityEventType.Update);
                    break;
                case InspectPeriods.FixedUpdate:
                    UnityEventCallbacks.AddEventListener(OnInspection, true, CallbackDuring.PlayMode, UnityEventType.FixedUpdate);
                    break;
                case InspectPeriods.Yield:
                    InspectionTask(milliseconds.Min(500));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
      
        
        
        #region --- INITIALIZATION ---
        
        /// <summary>
        /// Method is invoked after Start or Recompile
        /// </summary>
        private void Initialize(UnityEventType context)
        {
            ValidateModuleGUI(context.ToOrigin());
            InvokeModuleInitialization(currentValue);
            UpdateGUI(context.ToOrigin(), true, 1);
            OnInitialize();
            SetupInspectionLoop();
            if(context != UnityEventType.Recompile)
                SetVisibility(Application.isPlaying? visibilityOnLoad : visibilityOnEdit);
        }

        #endregion

        
        
        //--------------------------------------------------------------------------------------------------------------

        
        
        #region --- [BIND SOURCE EVENTS] ---
        
        
        public delegate void DelayedUpdateDelegate(T value, bool delay = false, int milliseconds = 100);
        
        /// <summary>
        /// Set event invoking Update OnValueChanged 
        /// </summary>
        /// <param name="coreEvent"></param>
        public void SetValueDelegate(ref Action<T> coreEvent, CoreEventType eventType = CoreEventType.Update)
        {
            switch (eventType)
            {
                case CoreEventType.Update:
                    coreEvent += InvokeModuleUpdate;
                    break;
                case CoreEventType.Toggle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
        
        /// <summary>
        /// Set event invoking UI Toggle OnValueChanged 
        /// </summary>
        /// <param name="coreEvent"></param>
        public override void SetValueDelegate(ref Action<bool> coreEvent, CoreEventType eventType)
        {
            switch (eventType)
            {
                case CoreEventType.Update:
                    break;
                case CoreEventType.Toggle:
                    coreEvent += InvokeToggle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
        
        /// <summary>
        /// Set event invoking Update OnValueChanged
        /// </summary>
        /// <param name="delayedUpdateEvent"></param>
        public void AssignCoreEvents(ref DelayedUpdateDelegate delayedUpdateEvent)
        {
            delayedUpdateEvent += InvokeModuleUpdate;
        }

        #endregion

        
        
        
        //--------------------------------------------------------------------------------------------------------------

        
        
        
        #region --- INVOKATIONS ---

        /// <summary>
        /// Invoke UpdateEvents using the last known state of T if the unit now enabled
        /// </summary>
        public void InvokeModuleUpdateDirty() => FinalizeInvocation(OnValueChangedContext.Dirty);

        /// <summary>
        /// Invoke UpdateEvents using the last known state of T if the unit now enabled
        /// </summary>
        public async void InvokeModuleUpdateDirty(int delay)
        {
            await Task.Delay(delay);
            FinalizeInvocation(OnValueChangedContext.Dirty);
        }

        /// <summary>
        /// Invoke event: Module/Unit Update callbacks if the unit now enabled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="delay"></param>
        /// <param name="milliseconds"></param>
        private async void InvokeModuleUpdate(T value, bool delay = false, int milliseconds = 100)
        {
            cachedValue = currentValue;
            currentValue = value;
            
            ModuleUpdateData = new ModuleUpdateData<T>(
                value: value,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Update,
                sender: this);

            if (delay)
                await Task.Delay(milliseconds);
            
            FinalizeInvocation(OnValueChangedContext.Update);
        }

        /// <summary>
        /// Invoke event: Module/Unit Update callbacks if the unit now enabled
        /// </summary>
        /// <param name="value"></param>
        private void InvokeModuleUpdate(T value)
        {
            cachedValue = currentValue;
            currentValue = value;
            
            ModuleUpdateData = new ModuleUpdateData<T>(
                value: value,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Update,
                sender: this);
            
            FinalizeInvocation(OnValueChangedContext.Update);
        }

        /// <summary>
        /// For Inspector
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ToggleUI()
        {
            switch (VisibilityFlags)
            {
                case Visibility.ActiveAndVisible:
                    InvokeToggle(false);
                    break;
                case Visibility.ActiveAndHidden:
                    InvokeToggle(true);
                    break;
                case Visibility.Inactive:
                    break;
            }
        }

        /// <summary>
        /// Invoke event: show/hide UI elements if the unit now enabled
        /// </summary>
        /// <param name="show"></param>
        protected sealed override void InvokeToggle(bool show)
        {
            ModuleUpdateData = new ModuleUpdateData<T>(
                value: ModuleUpdateData.Value,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: show ? OnValueChangedContext.Show : OnValueChangedContext.Hide,
                sender: this);

            if (show)
            {
                FinalizeInvocation(OnValueChangedContext.Show, OnValueChangedContext.Toggle);
            }
            else
            {
                FinalizeInvocation(OnValueChangedContext.Hide, OnValueChangedContext.Toggle);
            }
        }

        /// <summary>
        /// Event now invoked during module initialization 
        /// </summary>
        /// <param name="value"></param>
        private void InvokeModuleInitialization(T value)
        {
            cachedValue = currentValue;
            currentValue = value;
            
            
            ModuleUpdateData = new ModuleUpdateData<T>(
                value: value,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Initialization,
                sender: this);

            FinalizeInvocation(OnValueChangedContext.Initialization);
        }
        
        /// <summary>
        /// Invoke an event telling GUI elements that the content and or styleBase of the modules has canged.
        /// </summary>
        /// <param name="origin">InvokeOrigin</param>
        /// <param name="delay">delay the event</param>
        /// <param name="milliseconds">time of the delay in ms</param>
        protected sealed override async void UpdateGUI(InvokeOrigin origin, bool delay = false, int milliseconds = 100)
        {
            if (defaultValue == null)
            {
                return;
            }

            ModuleUpdateData = new ModuleUpdateData<T>(
                value: defaultValue,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Editor,
                sender: this);

            if (origin == InvokeOrigin.Recompile || delay)
                await Task.Delay(milliseconds);
                
            OnGUIChanged?.Invoke(Config, GetState(previewValueAs), origin);
        }

        private void FinalizeInvocation(params OnValueChangedContext[] eventTypes)
        {
            foreach (var type in eventTypes)
            {
                OnValueChanged?[type]?.Invoke(ModuleUpdateData);
                OnValueChangedT?[type]?.Invoke(ModuleUpdateData);
            }
            OnModuleUpdate(ModuleUpdateData);
        }

        #endregion
    }
}