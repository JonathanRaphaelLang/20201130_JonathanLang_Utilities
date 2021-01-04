using Ganymed.Monitoring.Core;

namespace Ganymed.Monitoring
{
    public interface IModuleData
    {
        string State { get; }
        string StateRaw { get; }
        OnValueChangedContext EventType { get; }
        Module Sender { get; }
    }

    public interface IModuleData<out T> : IModuleData
    {
        T Value{ get; }
    }
}