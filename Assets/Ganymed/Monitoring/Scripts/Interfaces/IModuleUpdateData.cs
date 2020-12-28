using Ganymed.Monitoring.Core;
using Ganymed.Monitoring.Enumerations;

namespace Ganymed.Monitoring.Interfaces
{
    public interface IModuleUpdateData
    {
        string State { get; }
        string StateRaw { get; }
        OnValueChangedContext EventType { get; }
        Module Sender { get; }
    }

    public interface IModuleUpdateData<out T> : IModuleUpdateData
    {
        T Value{ get; }
    }
}