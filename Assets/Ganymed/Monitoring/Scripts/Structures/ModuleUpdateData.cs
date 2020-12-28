using Ganymed.Monitoring.Core;
using Ganymed.Monitoring.Enumerations;
using Ganymed.Monitoring.Interfaces;

namespace Ganymed.Monitoring.Structures
{
    public readonly struct ModuleUpdateData<T> : IModuleUpdateData<T>
    {
    
        #region --- PROPERTIES ---
        public string State { get; }
        public string StateRaw { get; }
        public OnValueChangedContext EventType { get; }
        public Module Sender { get; }
        public T Value { get; }
    
        #endregion

        public ModuleUpdateData(T value, string state,  string stateRaw, OnValueChangedContext eventType, Module sender)
        {
            Value = value;
            EventType = eventType;
            Sender = sender;
            State = state;
            StateRaw = stateRaw;
        }
    }
}
