using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Marks command as required by native command code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    [RequiresAdditionalAttributes(typeof(ConsoleCommandAttribute), typeof(GetterSetterBase), Inherited = true, RequiresAny = true)]
    internal sealed class NativeCommandAttribute : Attribute
    {

    }
}
