using Ganymed.Monitoring.Core;

namespace Ganymed.Monitoring
{
    public readonly struct ModuleData<T> : IModuleData<T>
    {
    
        #region --- PROPERTIES ---
        public string State { get; }
        public string StateRaw { get; }
        public OnValueChangedContext EventType { get; }
        public Module Sender { get; }
        public T Value { get; }
    
        #endregion

        public ModuleData(T value, string state,  string stateRaw, OnValueChangedContext eventType, Module sender)
        {
            Value = value;
            EventType = eventType;
            Sender = sender;
            State = state;
            StateRaw = stateRaw;
        }
    }
}
