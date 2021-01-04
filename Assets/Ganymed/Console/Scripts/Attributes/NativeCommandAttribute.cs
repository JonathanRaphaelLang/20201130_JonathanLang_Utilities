using System;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class NativeCommandAttribute : Attribute
    {

    }
}
