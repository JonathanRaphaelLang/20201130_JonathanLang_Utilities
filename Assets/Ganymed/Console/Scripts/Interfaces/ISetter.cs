namespace Ganymed.Console
{
    /// <summary>
    /// Interface for every Attribute that has setter properties. [Setter] & [GetSet]
    /// </summary>
    public interface ISetter
    {   
        /// <summary>
        /// Determines a default value for the auto-complete feature of the console.
        /// </summary>
        object Default { get; set; }
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [GETTER AND SETTER] ---
        
        // The following properties are also present in the GetterSetterBase class and are only here for entirety.

        /// <summary>
        /// Description shown in the console and when using the "info operator "?"
        /// </summary>
        string Description { get; set; } 

        /// <summary>
        /// Boolean value that determines if the getter/setter will be excluded from builds and is only available in Edit mode. 
        /// Note that the getter/setter will still be compiled. If the getter/setter should really be excluded from builds
        /// use preprocessor directives instead.
        /// </summary>
        bool HideInBuild { get; set; }
        
        /// <summary>
        /// Getter / Setter with a higher priority will be prefered by autocompletion and are shown higher up in listings.
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Adds a shortcut that makes the property/field easier to access from the console
        /// </summary>
        string Shortcut { get; set; }

        #endregion
    }
}
 