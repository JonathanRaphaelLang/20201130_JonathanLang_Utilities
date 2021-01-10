using System;

namespace Ganymed.Utils.Attributes
{
    /// <summary>
    /// Determines that an attribute is only valid as an attribute for other attribute.
    /// </summary>
    [AttributesOnly]
    [AttributeUsage(AttributeTargets.Class)]
    public class AttributesOnlyAttribute : Attribute
    {
        /// <summary>
        /// Determines that an attribute is only valid as an attribute for other attribute.
        /// </summary>
        public AttributesOnlyAttribute()
        {
            
        }
    }
}