using Ganymed.Monitoring.Core;
using UnityEngine;

namespace Ganymed.Examples.Modules
{
    /// <summary>
    /// Example module that will show the time since startup in seconds. Use only for demonstration.
    /// </summary>
    [CreateAssetMenu(fileName = "Module_ModuleExample", menuName = "Monitoring/Modules/ModuleExample",  order = 100)]
    public class ModuleExample_Timer : Module<float>
    {
        // This is a simple module to demonstrate how to set up a new module.
        // If you need some more complex examples, feel free to check out the other modules but be advised that they
        // might not offer the same level of comments as this.
        
        // This modules will show the playtime in seconds since initialization.
        // Some settings must be set in the inspector. 

        
        private float timer = 0; // This will be the main focus of out module.
        private event ModuleUpdateDelegate OnValueChanged; // This event will tell the module and its systems that
                                                           // it needs to update and provide a new value.
        
        
        // OnInitialize must be implemented in each module. It can be used to setup initial values and must be used to
        // initialize the value of the module as well as the event that will update the module. Note that we have to
        // reset the timer every time we initialize the module because the values of our modules member will be serialized.
        // The InitializeValue method does not require a value to be passed. If used like InitializeValue(); A default 
        // value will be used.
        protected override void OnInitialize()
        {
            // Reset the timer to 0 and initialize the value.
            InitializeValue(timer = 0);
            // Bind the module Update to the OnValueChanged event.
            InitializeUpdateEvent(ref OnValueChanged);
        }

        // Tick is the equivalent of the Update function of a MonoBehaviour. It is called every frame and can be used
        // for custom logic. In this case we just add deltaTime to our timer to increase the value by one for each
        // second passed.
        protected override void Tick()
        {
            timer += Time.deltaTime;
        }

        
        // We use OnInspection in this case to invoke the OnValueChanged method and therefore update the module.
        // Although the intended use of this function is to validate values that are outside the reachable scope,
        // the function can of course also be used in situations like this, where it makes sense to leave certain
        // intervals between the updates.
        // This method must be enabled in the inspector in order to be called. We can also set the intervals in which the 
        // Method will be called to once every .1 to have a smooth counter.
        protected override void OnInspection() => OnValueChanged?.Invoke(timer);
        
        
        // The ParseToString method can be seen as another layer on top of the ToString method.
        // It offers a way to add custom formatting to the parsed value. We use it in this case to apply a custom format
        // to our float value. 
        protected override string ParseToString(float currentValue)
        {
            return currentValue.ToString("00.0");
        }
    }
}
