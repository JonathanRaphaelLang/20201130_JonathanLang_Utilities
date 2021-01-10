using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [RequiredAttributes(typeof(GetterSetterAttribute), Inherited = true, RequiresAny = true)]
    public class AllowUnsafeSetterAttribute : AllowUnsafeAttribute
    {
        /// <summary>
        /// Suppress exceptions on "unsafe" Types that would otherwise be marked as invalid.
        /// </summary>
        public AllowUnsafeSetterAttribute() { }
    }
}