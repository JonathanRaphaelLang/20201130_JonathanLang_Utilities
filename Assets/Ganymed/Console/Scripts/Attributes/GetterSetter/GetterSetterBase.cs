using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Base class for attributes that will expose the property/field to be issued and/or altered via console commands
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [RequiredAccess(Static = true)]
    public abstract class GetterSetterBase : Attribute
    {
        #region --- [PROPERTIES] ---
        
        /// <summary>
        /// Description shown in the console and when using the "info operator "?"
        /// </summary>
        public string Description { get; set; } 

        /// <summary>
        /// Boolean value that determines if the getter/setter will be excluded from builds and is only available in Edit mode. 
        /// Note that the getter/setter will still be compiled. If the getter/setter should really be excluded from builds
        /// use preprocessor directives instead.
        /// </summary>
        public bool HideInBuild { get; set; } = false;
        
        /// <summary>
        /// Getter / Setter with a higher priority will be prefered by autocompletion and are shown higher up in listings.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Adds a shortcut that makes the property/field easier to access from the console
        /// </summary>
        public string Shortcut
        {
            get => shortcut;
            set => shortcut = value;
        } 

        #endregion

        #region --- [FIELDS] ---

        protected string description = null;
        protected string shortcut = null;
        protected int priority = 0;
        protected bool hideInBuild = false;

        #endregion
    }
}


