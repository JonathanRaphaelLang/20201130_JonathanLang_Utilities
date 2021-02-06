using Ganymed.Monitoring.Core;
using UnityEngine;

namespace Ganymed.Examples.Modules
{
    [CreateAssetMenu(fileName = "Module_ModuleExample", menuName = "Example/Modules/ModuleExample")]
    public class ModuleExample : Module<int>
    {

        public int myValue = 100;
        private event ModuleUpdateDelegate OnValueChanged;
        
        protected override void OnInitialize()
        {
            InitializeValue(myValue);
            InitializeUpdateEvent(ref OnValueChanged);
        }
        
        private void ExampleLogic()
        {
            myValue++;
            OnValueChanged?.Invoke(myValue);
        }

        protected override void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Space)) ExampleLogic();
        }
    }
}
