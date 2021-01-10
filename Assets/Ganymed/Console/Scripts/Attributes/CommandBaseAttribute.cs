using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// base class for Command / Getter / Setter Attributes
    /// </summary>
    [RequiredAccess(Static = true)]
    public abstract class CommandBaseAttribute : Attribute
    {
        /// <summary>
        /// Description shown in the console and when using the "info operator "?"
        /// </summary>
        public string Description { get; set; } 

        /// <summary>
        /// Boolean value that determines if the command will be excluded from builds and is only available in Edit mode. 
        /// Note that the command will still be compiled. If the command should really be excluded from builds
        /// use preprocessor directives instead.
        /// </summary>
        public bool HideInBuild { get; set; } = false;
        
        /// <summary>
        /// Commands / Getter / Setter with a higher priority will be prefered by autocompletion and are shown higher up in listings.
        /// </summary>
        public int Priority { get; set; }
    }
}