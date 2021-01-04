using System;
using JetBrains.Annotations;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SetterAttribute : GetterSetterAttribute
    {
        public override string Shortcut { get; }

        public SetterAttribute() { }
        
        public SetterAttribute(string shortcut = null)
        {
            Shortcut = shortcut;
        }
    }
}