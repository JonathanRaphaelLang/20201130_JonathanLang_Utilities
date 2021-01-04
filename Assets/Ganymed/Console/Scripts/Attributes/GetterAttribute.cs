using System;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class GetterAttribute : GetterSetterAttribute
    {
        public override string Shortcut { get; }

        public GetterAttribute() { }
        
        public GetterAttribute(string shortcut = null)
        {
            Shortcut = shortcut;
        }
    }
}
