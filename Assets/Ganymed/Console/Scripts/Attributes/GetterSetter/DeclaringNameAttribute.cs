using System;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Set a custom declaring type name for Get / Set commands declared in this class 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DeclaringNameAttribute : Attribute
    {
        /// <summary>
        /// The alternative declaring type name
        /// </summary>
        public readonly string DeclaringName;
    
        /// <summary>
        /// DeclaringNameAttribute determines a custom DeclaringName for Get / Set commands declared in the target class.
        /// </summary>
        /// <param name="declaring"></param>
        public DeclaringNameAttribute(string declaring)
        {
            this.DeclaringName = declaring;
        }
    }
}
