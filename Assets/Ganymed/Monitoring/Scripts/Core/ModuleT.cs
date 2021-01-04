using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        private event Action<Configurable, string, InvokeOrigin> OnRepaint;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [DEFAULT VALUES AND TYPES] ---
       
        
        public sealed override string GetTypeOfTAsString() => $"{(typeof(T).IsEnum? "(Enum) " : string.Empty)}{typeof(T).Name}";

        public sealed override Type GetTypeOfT() => typeof(T);

        public sealed override object GetValueOfT() => value;

        /// <summary>
        /// The default newValue of T
        /// </summary>
        protected static T Default = default;

        /// <summary>
        /// The last and/or cached newValue of T
        /// </summary>
        protected T Value => value;
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
        
        
        //--- OVERRIDE DEFAULT VALUE ---
                
        /// <summary>
        /// Override the default newValue of T (ref) 
        /// </summary>
        /// <param name="newDefault">new default (you can pass as ref)</param>
        protected void OverrideDefaultValue(ref T newDefault)
            => Default = newDefault;
        
        /// <summary>
        /// Override the default newValue of T (ref) 
        /// </summary>
        /// <param name="newDefault">new default (you can pass as ref)</param>
        /// <param name="old">old default</param>
        protected void OverrideDefaultValue(ref T newDefault, out T old) {
            old = Default;
            Default = newDefault;
        }
                
        /// <summary>
        /// Override the default newValue of T 
        /// </summary>
        /// <param name="newDefault">new default</param>
        protected void OverrideDefaultValue(T newDefault)
            => Default = newDefault; 
        
        /// <summary>
        /// Override the default newValue of T 
        /// </summary>
        /// <param name="newDefault">new default (you can pass as ref)</param>
        /// <param name="old">old default</param>
        protected void OverrideDefaultValue(T newDefault, out T old) {
            old = Default;
            Default = newDefault;
        }
        
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
                await Task.Delay(1);
            } while (Configuration == null);
            
            if (previousConfiguration != null)
                previousConfiguration.OnValuesChanged -= Repaint;

            Configuration.OnValuesChanged += Repaint;
            previousConfiguration = Configuration;
            
            Repaint(InvokeOrigin.GUI);
            
            SetEnabled();
            SetActive();
            SetAutoInspection();
        }
        
        
        /// <summary>
        /// Repaint the modules GUI
        /// </summary>
        /// <param name="origin"></param>
        private async void Repaint(InvokeOrigin origin) 
        {
            await CompileContent();
            InvokeRepaintUpdate(origin);
        }
        
        
        /// <summary>
        /// Compile RichText 
        /// </summary>
        /// <returns></returns>
        private Task CompileContent()
        {
            compiledPrefix =  
                $"{Configuration.PrefixTextStyle}" +
                $"{(Configuration.IndividualFontSize? Configuration.PrefixFontSize.ToFontSize() : Configuration.fontSize.ToFontSize())}" +
                $"{Configuration.PrefixColor.AsRichText()}" +
                $"{prefixText}" +
                $"{(Configuration.AutoSpace ? (prefixText.Length > 0)? " " : string.Empty : string.Empty)}" +
                $"{(prefixBreak ? "\n" : string.Empty)}" +
                $"{Configuration.InfixTextStyle}" +
                $"{(Configuration.IndividualFontSize? Configuration.InfixFontSize.ToFontSize() : Configuration.fontSize.ToFontSize())}" +
                $"{Configuration.InfixColor.AsRichText()}" +
                $"{(Configuration.AutoBrackets? "[" : string.Empty)}";

            compiledSuffix =  
                $"{(suffixBreak ? "\n" : string.Empty)}" +
                $"{(Configuration.AutoBrackets?$"{Configuration.InfixColor.AsRichText()}]": string.Empty)}" +
                $"{Configuration.SuffixTextStyle}" +
                $"{(Configuration.IndividualFontSize? Configuration.SuffixFontSize.ToFontSize() : Configuration.fontSize.ToFontSize())}" +
                $"{Configuration.SuffixColor.AsRichText()}" +
                $"{(Configuration.AutoSpace? " " : string.Empty)}" +
                $"{suffixText}";
            
            return Task.CompletedTask;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [ABSTRACT & VIRTUAL METHODS] ---
        
        /// <summary>
        /// OnInitialize is called during the module initialization which will happen during the Unity Start method.
        /// </summary>
        protected abstract void OnInitialize();
        
        
        /// <summary>
        /// Override this method if the style of the value is dynamic (eg. fps color depending on newValue)
        /// </summary>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        protected virtual string ValueToString(T currentValue) => currentValue.ToString();

        
        /// <summary>
        /// OnBeforeUpdate is invoked at the beginning of an update and before the value was processed.
        /// </summary>
        /// <param name="currentValue"></param>
        protected virtual void OnBeforeUpdate(T currentValue) {}
        
        
        /// <summary>
        /// OnModuleUpdate is invoked on every module update event (GUI, UPDATE, etc.) Use for update dependent logic.
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnModuleUpdateEvent(ModuleData<T> data){}

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
                case ValueInterpretationOption.CurrentValue:
                    return $"{compiledPrefix}{ValueToString(value)}{compiledSuffix}";
                
                case ValueInterpretationOption.LastValue:
                    return $"{compiledPrefix}{ValueToString(cachedValue)}{compiledSuffix}";
                
                case ValueInterpretationOption.DefaultValue:
                    return $"{compiledPrefix}{ValueToString(Default)}{compiledSuffix}";

                case ValueInterpretationOption.Type:
                    return $"{compiledPrefix}{typeof(T).Name}{compiledSuffix}";
                
                default:
                    return "ERROR";
            }
        }
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [EVENTLISTENER] ---

        #region --- [REPAINT EVENT] ---

        /// <summary>
        /// Add a listener to the moduleDictionary repaint event
        /// </summary>
        /// <param name="listener"></param>
        public sealed override void AddOnRepaintChangedListener(Action<Configurable, string, InvokeOrigin> listener)
        {
            OnRepaint -= listener;
            OnRepaint += listener;
        }
        
        
        /// <summary>
        /// Remove a listener from the moduleDictionary repaint event
        /// </summary>
        /// <param name="listener"></param>
        public sealed override void RemoveOnRepaintChangedListener(Action<Configurable, string, InvokeOrigin> listener)
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
            if (typeof(T).IsArray)
            {
                if (typeof(T).GetArrayRank() > 1)
                    Default = (T)(object)Array.CreateInstance(typeof(T).GetElementType() ?? throw new Exception(), new int[typeof(T).GetArrayRank()]);
                else
                    Default = (T)(object)Array.CreateInstance(typeof(T).GetElementType()?? throw new Exception(), 0);
                return;
            }
            
            if (typeof(T) == typeof(string))
            {
                // string is IEnumerable<char>, but don't want to treat it like a collection
                Default = (T)(object)string.Empty;
                return;
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                // check if an empty array is an instance of T
                if (typeof(T).IsAssignableFrom(typeof(object[])))
                {
                    Default = (T)(object)new object[0];
                    return;
                }

                if (typeof(T).IsGenericType && typeof(T).GetGenericArguments().Length == 1)
                {
                    Type elementType = typeof(T).GetGenericArguments()[0];
                    if (typeof(T).IsAssignableFrom(elementType.MakeArrayType()))
                    {
                        Default = (T)(object)Array.CreateInstance(elementType, 0);
                        return;
                    }
                }
            }
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
                Initialize,
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
        
        #region --- INITIALIZATION ---
        
        private async void Initialize(UnityEventType context)
        {
            await ValidateAsync();
            
            if(IsEnabled) UnityEventCallbacks.AddEventListener(
                listener: Tick,
                removePreviousListener: true,
                callbackDuring: ApplicationState.PlayMode,
                callbackTypes: UnityEventType.Update);
            
            InvokeModuleInitialization(value);
            OnInitialize();
            Repaint(InvokeOrigin.Initialization);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [BIND SOURCE EVENTS] ---
        
        
        public delegate void DelayedUpdateDelegate(T value, bool delay = false, int milliseconds = 100);
        
        
        /// <summary>
        /// Bind delegate/event invoking an update of the module.
        /// </summary>
        /// <param name="invoker"></param>
        public void SetUpdateDelegate(ref Action<T> invoker)
        {
            invoker += InvokeModuleUpdate;
        }
        
        
        /// <summary>
        /// Bind delegate/event invoking an update of the module.
        /// </summary>
        /// <param name="delayedUpdateEvent"></param>
        public void SetUpdateDelegate(ref DelayedUpdateDelegate delayedUpdateEvent)
        {
            delayedUpdateEvent += InvokeModuleUpdate;
        }
        
        
        /// <summary>
        /// Bind delegate/event invoking activation/deactivation of the module.
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public override void SetActiveDelegate(ref Action<bool> invoker, bool invert = false)
        {
            if(!invert)
                invoker += SetActive;
            else
                invoker += InvertedSetActive;
        }

        private void InvertedSetActive(bool origin)
            => SetActive(!origin);
        
        
        
        /// <summary>
        /// Bind delegate/event invoking enable/disable of the module.
        /// </summary>
        /// <param name="invoker"></param>
        /// <param name="invert"></param>
        public override void SetEnableDelegate(ref Action<bool> invoker, bool invert = false)
        {
            if(!invert)
                invoker += SetEnabled;
            else
                invoker += InvertedSetEnabled;
        }
        private void InvertedSetEnabled(bool origin)
            => SetEnabled(!origin);
        
        #endregion
       
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INVOKATIONS] ---

        #region --- [UPDATE] ---

        /// <summary>
        /// Invoke event: Module/Unit Update callbacks if the unit now Enabled
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="delay"></param>
        /// <param name="ms"></param>
        private async void InvokeModuleUpdate(T newValue, bool delay , int ms = 100)
        {
            OnBeforeUpdate(newValue);
            
            cachedValue = this.value;
            this.value = newValue;
            
            moduleData = new ModuleData<T>(
                value: newValue,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Update,
                sender: this);

            if (delay)
                await Task.Delay(ms);
            
            FinalizeInvocation(OnValueChangedContext.Update);
        }

        
        /// <summary>
        /// Invoke event: Module/Unit Update callbacks if the unit now Enabled
        /// </summary>
        /// <param name="newValue"></param>
        private void InvokeModuleUpdate(T newValue)
        {
            OnBeforeUpdate(newValue);

            cachedValue = this.value;
            this.value = newValue;
            
            moduleData = new ModuleData<T>(
                value: newValue,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Update,
                sender: this);
            
            FinalizeInvocation(OnValueChangedContext.Update);
        }

        #endregion


        #region --- [INITIALIZATION] ---

        /// <summary>
        /// Event now invoked during module initialization 
        /// </summary>
        /// <param name="newValue"></param>
        private void InvokeModuleInitialization(T newValue)
        {
            cachedValue = this.value;
            this.value = newValue;
            
            
            moduleData = new ModuleData<T>(
                value: newValue,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Initialization,
                sender: this);

            FinalizeInvocation(OnValueChangedContext.Initialization);
        }
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FINALIZATION] ---

        /// <summary>
        /// Method will invoke OnValueChanged
        /// </summary>
        /// <param name="eventTypes"></param>
        private void FinalizeInvocation(params OnValueChangedContext[] eventTypes)
        {
            foreach (var type in eventTypes)
            {
                OnValueChanged?[type]?.Invoke(moduleData);
                OnValueChangedT?[type]?.Invoke(moduleData);
            }
            OnModuleUpdateEvent(moduleData);
        }
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INSPECTOR & DIRTY] ---

        /// <summary>
        /// Invoke UpdateEvents using the last known state of T if the unit now Enabled
        /// </summary>
        public void InvokeModuleUpdateDirty() => FinalizeInvocation(OnValueChangedContext.Dirty); 

        /// <summary>
        /// Invoke UpdateEvents using the last known state of T if the unit now Enabled
        /// </summary>
        public async void InvokeModuleUpdateDirty(int delay)
        {
            await Task.Delay(delay);
            FinalizeInvocation(OnValueChangedContext.Dirty);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [REPAINT] ---

        private void InvokeRepaintUpdate(InvokeOrigin origin, bool delay = false, int ms = 100)
        {
            if (Default == null) 
                return;

            moduleData = new ModuleData<T>(
                value: Default,
                state: GetState(previewValueAs),
                stateRaw: GetStateRaw(),
                eventType: OnValueChangedContext.Editor,
                sender: this);
                
            OnRepaint?.Invoke(Configuration, moduleData.State, origin);
        }
        
        #endregion

        #endregion
    }
}
