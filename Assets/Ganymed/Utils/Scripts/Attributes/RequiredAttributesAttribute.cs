using System;
using System.Linq;

namespace Ganymed.Utils.Attributes
{
    /// <summary>
    /// RequiredAttributes states that instances of the specified (target)attribute
    /// require instances of the passed attribute/s type/s to be a valid attribute.
    /// Only viable as an attribute for attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredAttributesAttribute : Attribute
    {
        #region --- [PROPERTIES] ---
        
        /// <summary>
        /// Determines whether or not the instances of the required types can be a subclass of the types or not.
        /// </summary>
        public bool Inherited { get; set; } = false;

        /// <summary>
        /// Array containing types required by the attribute.
        /// Types only contain types that are a subclass of typeof(Attribute)
        /// </summary>
        public Type[] RequiredAttributes => requiredAttributes;

        private Type[] requiredAttributes;
        
        
        /// <summary>
        /// RequiresAny set / get a boolean value that indicates whether instances of the specified (target)attribute
        /// require instances of every specified type / attribute in order to count as valid or if the attribute is
        /// valid as soon as there is one instance of the specified types / attributes alongside.
        /// Returns true if only one instance is required. Returns false if instances of every type are required.
        /// </summary>
        public bool RequiresAny { get; set; } = false;

        #endregion

        /// <summary>
        /// RequiredAttributes states that instances of the specified (target)attribute
        /// require instances of the passed attribute/s type/s to be a valid attribute.
        /// Initialize a new instance of the RequiredAttributesAttribute class.
        /// Only viable as an attribute for attributes.
        /// </summary>
        /// <param name="requiredAttributes"></param>
        public RequiredAttributesAttribute(params Type[] requiredAttributes)
            => this.requiredAttributes = requiredAttributes.Where(type => type.IsSubclassOf(typeof(Attribute))).ToArray();
        
    }
}
