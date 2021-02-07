using Ganymed.Monitoring;
using Ganymed.Monitoring.Core;
using UnityEngine;

namespace Ganymed.Examples.Modules
{
    /// <summary>
    /// Example module that (if enabled) will will log the calls of its methods. 
    /// </summary>
    [CreateAssetMenu(fileName = "Module_ModuleExampleMethods", menuName = "Example/Modules/ModuleExampleMethods")]
    public class ModuleExample_Methods : Module<int>
    {
        [SerializeField] private bool LogExamples = true;
        private event ModuleUpdateDelegate update;
        
        /// <summary>
        /// OnInitialize must be implemented in each module. It can be used to setup initial values and must be used to
        /// initialize the value of the module as well as the event that will update the module. 
        /// </summary>
        protected override void OnInitialize()
        {
            // You can pass a value or not. If no value is given either the cached value or the default value of the type is used.
            // Note that event reference types can have a default value if their type implements a default constructor.
            InitializeValue(0);
            InitializeValue();     
            
            // We pass the update event. Note that the event is of type ModuleUpdateDelegate. use this delegate type for
            // your update events.
            InitializeUpdateEvent(ref update);
            
            // If we didnt call these initialization methods we would get warnings after OnInitialize is called. These
            // warnings can be deactivated in the inspector if a module does not require a default value or an update event.
            
            if(LogExamples) Debug.Log("OnInitialize");
        }

        
#if UNITY_EDITOR
        
        /// <summary>
        /// The ParseToString Method is called every time the value of the module is displayed or updated. It is the
        /// equivalent of a ToString method and can be used for custom formatting depending of the value.
        /// E.g. The FPS module is using this method to color the value in red, yellow or green depending on the
        /// current framerate. 
        /// </summary>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        protected override string ParseToString(int currentValue)
        {
            if(LogExamples) Debug.Log("ParseToString");
            return base.ParseToString(currentValue);
        }

        

        /// <summary>
        /// OnBeforeUpdate is called at the beginning of an update and before the value is processed.
        /// </summary>
        protected override void OnBeforeUpdate(int currentValue)
        {
            if(LogExamples) Debug.Log("OnBeforeUpdate");
        }

        /// <summary>
        /// OnAfterUpdate is called after an update. 
        /// </summary>
        /// <param name="data"></param>
        protected override void OnAfterUpdate(ModuleData<int> data)
        {
            if(LogExamples) Debug.Log("OnBeforeUpdate");
        }

        

        /// <summary>
        /// Tick is called every (MonoBehaviour) Update call if the module is active.
        /// </summary>
        protected override void Tick()
        {
            if(LogExamples) Debug.Log("Tick");
        }

        /// <summary>
        /// Method is called repeatedly if auto inspection is enabled and the module is active.
        /// Delay between calls can be set in the inspector
        /// </summary>
        protected override void OnInspection()
        {
            if(LogExamples) Debug.Log("OnInspection");
        }

        

        /// <summary>
        /// ModuleEnabled is called when the module is enabled
        /// </summary>
        protected override void ModuleEnabled() 
        {
            if(LogExamples) Debug.Log("Module Enabled");
        }

        /// <summary>
        /// ModuleDisabled is called when the module is disabled
        /// </summary>
        protected override void ModuleDisabled()
        {
            if(LogExamples) Debug.Log("Module Disabled");
        }

        

        /// <summary>
        /// ModuleActivated is called when the module is activated
        /// </summary>
        protected override void ModuleActivated()
        {
            if(LogExamples) Debug.Log("Module Activated");
        }

        /// <summary>
        /// ModuleDeactivated is called when the module is deactivated
        /// </summary>
        protected override void ModuleDeactivated()
        {
            if(LogExamples) Debug.Log("Module Deactivated");
        }

        
        /// <summary>
        /// ModuleVisible is called when the module is set visible
        /// </summary>
        protected override void ModuleVisible()
        {
            if(LogExamples) Debug.Log("Module Visible");
        }

        /// <summary>
        /// ModuleInvisible is called when the module is set invisible
        /// </summary>
        protected override void ModuleInvisible()
        {
            if(LogExamples) Debug.Log("Module Invisible");
        }

#endif
    }
}