using Ganymed.Utils;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// This attribute will expose the property/field to be issued and altered via console commands
    /// </summary>
    [PropertyAccessRequirement(RequiresRead = true, RequiresWrite = true)]
    [TargetTypeRestriction(ValidTypeAffiliations = TypeAffiliations.String | TypeAffiliations.Primitive | TypeAffiliations.Struct | TypeAffiliations.Enum)]
    public sealed class GetSetAttribute : GetterSetterAttribute, ISetter
    {
        #region --- [PROPERTIES] ---
        
        /// <summary>
        /// Determines a default value for the auto-complete feature of the console.
        /// </summary>
        public object Default { get; set; }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        /// <summary>
        /// Flag a property / field as gettable and settable (read/write) by console commands.
        /// Use /get or /set to view a list of available getter and setter.
        /// Initialize a new instance of the GetAndSetAttribute class. 
        /// </summary>
        public GetSetAttribute() { }
        
        
        /// <summary>
        /// Flag a property / field as gettable and settable (read/write) by console commands.
        /// Use /get or /set to view a list of available getter and setter.
        /// Initialize a new instance of the GetAndSetAttribute class. 
        /// </summary>
        /// <param name="shortcut"></param>
        public GetSetAttribute(string shortcut) => this.shortcut = shortcut;

        /// <summary>
        /// Flag a property / field as gettable and settable (read/write) by console commands.
        /// Use /get or /set to view a list of available getter and setter.
        /// Initialize a new instance of the GetAndSetAttribute class. 
        /// </summary>
        /// <param name="shortcut"></param>
        /// <param name="description"></param>
        public GetSetAttribute(string shortcut, string description)
        {
            this.shortcut = shortcut;
            this.description = description;
        }

        /// <summary>
        /// Flag a property / field as gettable and settable (read/write) by console commands.
        /// Use /get or /set to view a list of available getter and setter.
        /// Initialize a new instance of the GetAndSetAttribute class. 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="shortcut"></param>
        /// <param name="default"></param>
        public GetSetAttribute(string shortcut, string description, object @default)
        {
            this.shortcut = shortcut;
            this.description = description;
            this.Default = @default;
        }

        #endregion
    }
}
