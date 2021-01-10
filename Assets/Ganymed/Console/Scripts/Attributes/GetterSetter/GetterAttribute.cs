using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// This attribute will expose the property/field to be issued via console commands
    /// </summary>
    [PropertyAccessRequirement(RequiresRead = true)]
    public sealed class GetterAttribute : GetterSetterAttribute
    {
        #region --- [CONSTRUCTOR] ---

        /// <summary>
        /// Flag a property / field as gettable (read) by console commands.
        /// Use /get (or /set) to view a list of available getter and (setter).
        /// Initialize a new instance of the GetAttribute class. 
        /// </summary>
        public GetterAttribute() { }
        
        
        /// <summary>
        /// Flag a property / field as gettable (read) by console commands.
        /// Use /get (or /set) to view a list of available getter and (setter).
        /// Initialize a new instance of the GetAttribute class. 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="shortcut"></param>
        internal GetterAttribute(string shortcut, string description)
        {
            this.description = description;
            this.shortcut = shortcut;
        }

        #endregion
    }
}
