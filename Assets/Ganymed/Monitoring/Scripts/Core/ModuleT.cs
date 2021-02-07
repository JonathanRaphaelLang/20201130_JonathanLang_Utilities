using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    /// <summary>
    /// Generic Module 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Module<T> : Module
    {
        #region --- [FIELDS] ---

        // cached Module Update Data
        private ModuleData<T> moduleData;

        // default event listener
        private readonly Dictionary<OnValueChangedContext, Action<IModuleData>> OnValueChanged
            = new Dictionary<OnValueChangedContext, Action<IModuleData>>();

        // generic event listener
        private readonly Dictionary<OnValueChangedContext, Action<IModuleData<T>>> OnValueChangedT
            = new Dictionary<OnValueChangedContext, Action<IModuleData<T>>>();

        // event listener will be invoked if the GUI element is repainted to this event
        private event Action<StyleBase, string, InvokeOrigin> OnRepaint;
        
        private bool updateEventInitialized = false;
        private bool valueInitialized = false;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [DELEGATES] ---

        public delegate void ModuleUpdateDelegate(T func);

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [DEFAULT VALUES AND TYPES] ---
       
        
        public sealed override string GetTypeOfTAsString() => $"{(typeof(T).IsEnum? "(Enum) " : string.Empty)}{typeof(T).Name}";

        public sealed override Type ValueType() => typeof(T);

        public sealed override object GetValue() => value;


        /// <summary>
        /// The default value of the modules type
        /// </summary>
        public static T Default { get; private set; }

        /// <summary>
        /// The last and/or cached newValue of T
        /// </summary>
        [SerializeField] [HideInInspector] protected T Value => value;
        
        /// <summary>
        /// The current newValue of T
        /// </summary>
        private T value = default;
        
        
        
        /// <summary>
        /// The last and/or cached newValue of T
        /// </summary>
        protected T CachedValue => cachedValue;
        
        /// <summary>
        /// The last and/or cached newValue of T
        /// </summary>
        private T cachedValue = default;
        
        
        
                
        /// <summary>
        /// Set the value dirty.
        /// </summary>
        /// <param name="dirtyValue"></param>
        /// <typeparam name="U"></typeparam>
        public sealed override void SetValueDirty<U>(U dirtyValue) => value = (T)(object)dirtyValue;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        protected virtual async void OnValidate() => await ValidateAsync();

        
        /// <summary>
        /// Task validating modules integrity
        /// </summary>
        /// <returns></returns>
        private async Task ValidateAsync()
        {
            do
            {
                await Task.CompletedTask.BreakContext();
            } while (Configuration == null);
            
            if (previousConfiguration != null)
                previousConfiguration.OnValuesChanged -= Repaint;

            Configuration.OnValuesChanged += Repaint;
            previousConfiguration = Configuration;
            
            Repaint(InvokeOrigin.GUI);

            ValidateState();
            
            InitializeAutoInspection();

            if (OnlyInitializeWhenInScene && !HasSceneObject())
            {
                UnityEventCallbacks.RemoveEventListener(Tick, ApplicationState.PlayMode, UnityEventType.Update);
            }
            else if(IsActive)
            {
                UnityEventCallbacks.AddEventListener(
                    listener: Tick,
                    removePreviousListener: true,
                    callbackDuring: ApplicationState.PlayMode,
                    callbackTypes: UnityEventType.Update);
            }
        }
       
        
        /// <summary>
        /// Compile RichText 
        /// </summary>
        /// <returns></returns>
        private Task CompileContent()
        {
            try
            {
                compiledPrefix =
                    $"{Configuration.PrefixTextStyle}" +
                    $"{(Configuration.IndividualFontSize ? Configuration.PrefixFontSize.ToFontSize() : Configuration.fontSize.ToFontSize())}" +
                    $"{Configuration.PrefixColor.AsRichText()}" +
                    $"{PrefixText}" +
                    $"{(Configuration.AutoSpace ? (PrefixText.Length > 0) ? " " : string.Empty : string.Empty)}" +
                    $"{(PrefixBreak ? "\n" : string.Empty)}" +
                    $"{Configuration.InfixTextStyle}" +
                    $"{(Configuration.IndividualFontSize ? Configuration.InfixFontSize.ToFontSize() : Configuration.fontSize.ToFontSize())}" +
                    $"{Configuration.InfixColor.AsRichText()}" +
                    $"{(Configuration.AutoBrackets ? "[" : string.Empty)}";

                compiledSuffix =
                    $"{(SuffixBreak ? "\n" : string.Empty)}" +
                    $"{(Configuration.AutoBrackets ? $"{Configuration.InfixColor.AsRichText()}]" : string.Empty)}" +
                    $"{Configuration.SuffixTextStyle}" +
                    $"{(Configuration.IndividualFontSize ? Configuration.SuffixFontSize.ToFontSize() : Configuration.fontSize.ToFontSize())}" +
                    $"{Configuration.SuffixColor.AsRichText()}" +
                    $"{(Configuration.AutoSpace ? " " : string.Empty)}" +
                    $"{SuffixText}";

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [ABSTRACT & VIRTUAL METHODS] ---
        
        /// <summary>
        /// OnInitialize is called during the modules initialization.
        /// This method must be used to set up initial events and values.
        /// </summary>
        protected abstract void OnInitialize();
        
        
        /// <summary>
        /// Override this method if the style of the value is dynamic (e.g. fps color depending on the value)
        /// </summary>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        protected virtual string ParseToString(T currentValue)
        {
            if (currentValue != null) return currentValue.ToString();
            else return string.Empty;
        }


        /// <summary>
        /// OnBeforeUpdate is invoked at the beginning of a module update and before the value is processed.
        /// </summary>
        /// <param name="currentValue"></param>
        protected virtual void OnBeforeUpdate(T currentValue) {}
        
        
        /// <summary>
        /// OnAfterUpdate is invoked on every module update event (GUI, UPDATE, etc.) Use for update dependent logic.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnAfterUpdate(ModuleData<T> data){}

        #endregion

        #region --- [SEALED] ---

        /// <summary>
        /// Returns the raw state ToString() of the current newValue without any additional pre or suffix
        /// </summary>
        /// <returns>Raw Value</returns>
        public sealed override string GetStateRaw()
            => value != null ? value.ToString() : "null";

        
        /// <summary>
        /// Returns a compiled and appended string representing the current state
        /// </summary>
        /// <param name="interpretationOption">How to interpret the state</param>
        /// <returns></returns>
        public sealed override string GetState(ValueInterpretationOption interpretationOption)
        {
            switch (interpretationOption)
            {
                case ValueInterpretationOption.Value:
                    return Compiled(ParseToString(value));
                
                case ValueInterpretationOption.Cached:
                    return Compiled(ParseToString(cachedValue));
                
                case ValueInterpretationOption.Default:
                    return Compiled(ParseToString(Default));

                case ValueInterpretationOption.Type:
                    return Compiled(typeof(T).Name);
                
                default:
                    return "ERROR";
            }
        }

        private string Compiled(string target)
        {
            return $"{compiledPrefix}{target}{compiledSuffix}";
        }
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [EVENTLISTENER] ---

        #region --- [REPAINT EVENT] ---

        /// <summary>
        /// Add a listener to the moduleDictionary repaint event
        /// </summary>
        /// <param name="listener"></param>
        public sealed override void AddOnRepaintListener(Action<StyleBase, string, InvokeOrigin> listener)
        {
            OnRepaint -= listener;
            OnRepaint += listener;
        }
        
        
        /// <summary>
        /// Remove a listener from the moduleDictionary repaint event
        /// </summary>
        /// <param name="listener"></param>
        public sealed override void RemoveOnRepaintChangedListener(Action<StyleBase, string, InvokeOrigin> listener)
        {
            OnRepaint -= listener;
        }

        #endregion

        #region --- [ADD EVENTLISTENER] ---

        /// <summary>
        /// Add a listener to the moduleDictionary OnValueChanged event
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="updateListener"></param>
        public sealed override void AddOnValueChangedListener(OnValueChangedContext eventType,
            params Action<IModuleData>[] updateListener)
        {
            foreach (var listener in updateListener)
            {
                OnValueChanged[eventType] += listener;
            }
        }
        

        /// <summary>
        /// Add a generic listener to the moduleDictionary OnValueChanged event
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="updateListener"></param>
        public void AddOnValueChangedListener(OnValueChangedContext eventType,
            params Action<IModuleData<T>>[] updateListener)
        {
            foreach (var listener in updateListener)
            {
                OnValueChangedT[eventType] += listener;
            }
        }

        #endregion

        #region --- [REMOVE EVENTLISTENER] ---

        
        /// <summary>
        /// Remove every listener of a certain context from the moduleDictionary OnValueChanged event 
        /// </summary>
        /// <param name="context"></param>
        public sealed override void RemoveOnValueChangedListener(OnValueChangedContext context)
        {
            OnValueChanged[context] = null;
            OnValueChanged[context] = delegate(IModuleData data) { };
        }
        
        
        /// <summary>
        /// Remove a listener from the moduleDictionary default OnValueChanged event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="updateListener"></param>
        public sealed override void RemoveOnValueChangedListener(OnValueChangedContext context,
            params Action<IModuleData>[] updateListener)
        {
            try
            {
                foreach (var listener in updateListener)
                {
                    // ReSharper disable once DelegateSubtraction
                    OnValueChanged[context] -= listener;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception);
            }
        }

        
        /// <summary>
        /// Remove a listener from the moduleDictionary generic OnValueChanged event
        /// </summary>
        /// <param name="context"></param>
        /// <param name="updateListener"></param>
        public void RemoveOnValueChangedListener(OnValueChangedContext context,
            params Action<IModuleData<T>>[] updateListener)
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
                Debug.LogWarning(exception);
            }
        }
        

        #endregion
        
        #endregion
       
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONSTRUCTOR] ---

        
        static Module()
        {
            Default = (T) typeof(T).TryGetDefaultInstance();
        }
        
        
        protected Module()
        {
            foreach (OnValueChangedContext type in Enum.GetValues(typeof(OnValueChangedContext)))
            {
                OnValueChanged.Add(type, delegate(IModuleData data) { });
                OnValueChangedT.Add(type, delegate(IModuleData<T> data) { });
            }
            
            cachedValue = Default;
            value = Default;
            
            UnityEventCallbacks.AddEventListener(
                () =>
                {
                    if (ResetValueOnQuit) UpdateModule(Default);
                    OnQuit();
                },
                UnityEventType.ApplicationQuit
                );
            
            UnityEventCallbacks.AddEventListener(
                StartInitialization,
                true,
                
                ApplicationState.EditAndPlayMode,
#if !UNITY_EDITOR
                UnityEventType.Start
#else
                UnityEventType.EditorApplicationStart,
                UnityEventType.EnteredEditMode
#endif
                );
            
            UnityEventCallbacks.AddEventListener(
                OnValidate,
                true,
                ApplicationState.EditAndPlayMode,
                UnityEventType.InspectorUpdate);

        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INITIALIZATION] ---
        
        private protected sealed override async void StartInitialization(UnityEventType context)
        {
            if(!IsEnabled) return;
            if(OnlyInitializeWhenInScene && !HasSceneObject()) return;
            
            await ValidateAsync();

            PrepareInitialization();
            OnInitialize();
            ValidateInitialization();
            
            Repaint(InvokeOrigin.Initialization);
            
            moduleData = new ModuleData<T>(
                value: value,
                state: GetState(PreviewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Editor,
                sender: this);
            
            Finalize(OnValueChangedContext.Initialization);
        }

        private void PrepareInitialization()
        {
            updateEventInitialized = false;
            valueInitialized = false;
        }

        private void ValidateInitialization()
        {
            if (updateEventInitialized == false && EnableWarnings && EnableInitializeUpdateEventWarnings)
            {
                Debug.LogWarning(
                    $"Warning: Use the {RichText.Orange}{nameof(InitializeUpdateEvent)}{RichText.ClearColor} " +
                    $"method during {RichText.Orange}{nameof(OnInitialize)}{RichText.ClearColor} " +
                    $"in {RichText.LightGray}{GetType().Namespace}{RichText.ClearColor}." +
                    $"{RichText.Blue}{name}{RichText.ClearColor} " +
                    $"to set up an update {RichText.Violet}EVENT{RichText.ClearColor} for the module!\n");
            }
            if (valueInitialized == false && EnableWarnings && EnableInitializeValueWarnings)
            {
                Debug.LogWarning(
                    $"Warning: Use the {RichText.Orange}{nameof(InitializeValue)}{RichText.ClearColor} " +
                    $"method during {RichText.Orange}{nameof(OnInitialize)}{RichText.ClearColor} " +
                    $"in {RichText.LightGray}{GetType().Namespace}{RichText.ClearColor}." +
                    $"{RichText.Blue}{name}{RichText.ClearColor} " +
                    $"to set up a default {RichText.Violet}VALUE{RichText.ClearColor} for the module!\n");
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INITIALIZATION] ---

        // --- UPDATE ---
        
        /// <summary>
        /// Set ModuleUpdateDelegates that will update the module.
        /// </summary>
        /// <param name="func"></param>
        protected void InitializeUpdateEvent(ref ModuleUpdateDelegate func)
        {
            func += UpdateModule;
            updateEventInitialized = true;
        }
        
        
                
        
        // --- VALUE ---
                
        /// <summary>
        /// Override the default newValue of T (ref) 
        /// </summary>
        /// <param name="newValue">new default (you can pass as ref)</param>
        protected void InitializeValue(ref T newValue)
        {
            value = newValue;
            Default = (T)(Default as object ?? value);
            
            if (value == null)
            {
                Debug.LogException(new Exception($"Warning. No Default Instance of {typeof(T)} could be instantiated!\n " +
                                                 $"Only nullable types with an accessible default constructor are supported!"));
            }
            
            valueInitialized = true;
        }
                
        /// <summary>
        /// Override the default newValue of T 
        /// </summary>
        /// <param name="newValue">new default</param>
        protected void InitializeValue(T newValue)
        {
            value = (T)(newValue as object ?? Default);
            Default = (T)(Default as object ?? value);
            
            if (value == null) {
                Debug.LogException(new Exception($"Warning. No Default Instance of {typeof(T)} could be instantiated!\n " +
                                                 $"Only nullable types with an accessible default constructor are supported!"));
            }
            
            valueInitialized = true;
        }
        
        /// <summary>
        /// Override the default newValue of T 
        /// </summary>
        protected void InitializeValue()
        {
            value = (T)(value as object ?? Default);
            Default = (T)(Default as object ?? value);
            
            if (value == null) {
                Debug.LogException(new Exception($"Warning. No Default Instance of {typeof(T)} could be instantiated!\n " +
                                                 $"Only nullable types with an accessible default constructor are supported!"));
            }
            
            valueInitialized = true;
        }
      
        // --- ACTIVATION ---
        
        /// <summary>
        /// Bind delegate/event invoking activation/deactivation of the module.
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public override void InitializeActivationEvent(ref ModuleActivationDelegate invoker, bool invert = false)
        {
            if(!invert)
                invoker += SetVisible;
            else
                invoker += InvertedSetActive;
        }

        private void InvertedSetActive(bool origin)
            => SetVisible(!origin);
        
        
        
        /// <summary>
        /// Bind delegate/event invoking enable/disable of the module.
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public override void InitializeEnableEvent(ref ModuleActivationDelegate invoker, bool invert = false)
        {
            if(!invert)
                invoker += SetActive;
            else
                invoker += InvertedSetEnabled;
        }
        private void InvertedSetEnabled(bool origin)
            => SetActive(!origin);
        
        #endregion
       
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE] ---
        
        /// <summary>
        /// Invoke event: Module/Unit Update callbacks if the unit now Enabled
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateModule(T newValue)
        {
            OnBeforeUpdate(newValue);

            cachedValue = value;
            value = newValue;
            
            moduleData = new ModuleData<T>(
                value: newValue,
                state: GetState(PreviewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Update,
                sender: this);
            
            Finalize(OnValueChangedContext.Update);
        }


        /// <summary>
        ///     Invoke event: Module/Unit Update callbacks if the unit now Enabled
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="delay"></param>
        /// <param name="ms"></param>
        private async void UpdateModule(T newValue, bool delay, int ms = 100)
        {
            OnBeforeUpdate(newValue);

            cachedValue = value;
            value = newValue;

            moduleData = new ModuleData<T>(
                value: newValue,
                state: GetState(PreviewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Update,
                sender: this);

            if (delay)
                await Task.Delay(ms);

            Finalize(OnValueChangedContext.Update);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INSPECTOR & DIRTY] ---

        /// <summary>
        /// Update the module without providing a new value
        /// </summary>
        public void UpdateModuleDirty() => Finalize(OnValueChangedContext.Dirty); 

        /// <summary>
        /// Invoke UpdateEvents using the last known state of T if the unit now Enabled
        /// </summary>
        public async void UpdateModuleDirty(int delay)
        {
            await Task.Delay(delay);
            Finalize(OnValueChangedContext.Dirty);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [REPAINT] ---
        
        /// <summary>
        /// Repaint the modules GUI
        /// </summary>
        /// <param name="origin"></param>
        public override async void Repaint(InvokeOrigin origin) 
        {
            await CompileContent();

            moduleData = new ModuleData<T>(
                value: value,
                state: GetState(PreviewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Editor,
                sender: this);
                
            OnRepaint?.Invoke(Configuration, moduleData.State, origin);
        }
        
        #endregion
        
                
        //--------------------------------------------------------------------------------------------------------------
        
        
        #region --- [FINALIZATION] ---

        /// <summary>
        /// Method will invoke OnValueChanged
        /// </summary>
        /// <param name="eventTypes"></param>
        private void Finalize(params OnValueChangedContext[] eventTypes)
        {
            foreach (var type in eventTypes)
            {
                OnValueChanged?[type]?.Invoke(moduleData);
                OnValueChangedT?[type]?.Invoke(moduleData);
            }
            OnAfterUpdate(moduleData);
        }

        #endregion
    }
}
