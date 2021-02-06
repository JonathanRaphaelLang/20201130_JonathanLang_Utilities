using System;
using Ganymed.Monitoring;
using Ganymed.Monitoring.Core;
using UnityEngine;

namespace Ganymed.Examples.Modules
{
    [CreateAssetMenu(fileName = "Module_ModuleExampleMethods", menuName = "Example/Modules/ModuleExampleMethods")]
    public class ModuleExampleMethods : Module<int>
    {
        [SerializeField] private bool LogExamples = true;
        
        
        protected override void OnInitialize()
        {
            if(LogExamples) Debug.Log("OnInitialize");
        }


        protected override string ParseToString(int currentValue)
        {
            if(LogExamples) Debug.Log("OnInitialize");
            return base.ParseToString(currentValue);
        }


        protected override void OnBeforeUpdate(int currentValue)
        {
            if(LogExamples) Debug.Log("OnBeforeUpdate");
        }

        protected override void OnAfterUpdate(ModuleData<int> data)
        {
            if(LogExamples) Debug.Log("OnBeforeUpdate");
        }

        

        protected override void Tick()
        {
            if(LogExamples) Debug.Log("Tick");
        }

        protected override void OnInspection()
        {
            if(LogExamples) Debug.Log("OnInspection");
        }

        

        protected override void ModuleEnabled() 
        {
            if(LogExamples) Debug.Log("Module Enabled");
        }

        protected override void ModuleDisabled()
        {
            if(LogExamples) Debug.Log("Module Disabled");
        }

        

        protected override void ModuleActivated()
        {
            if(LogExamples) Debug.Log("Module Activated");
        }

        protected override void ModuleDeactivated()
        {
            if(LogExamples) Debug.Log("Module Deactivated");
        }

        

        protected override void ModuleVisible()
        {
            if(LogExamples) Debug.Log("Module Visible");
        }

        protected override void ModuleInvisible()
        {
            if(LogExamples) Debug.Log("Module Invisible");
        }
    }
}