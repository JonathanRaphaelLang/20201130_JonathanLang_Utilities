using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Marks command as required by native command code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    [RequiredAttributes(typeof(GetterSetterAttribute), typeof(CommandAttribute), Inherited = true, RequiresAny = true)]
    internal sealed class NativeCommandAttribute : Attribute
    {

    }
}
