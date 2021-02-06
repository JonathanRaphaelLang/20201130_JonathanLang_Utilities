using Ganymed.Utils.Attributes;
using UnityEngine;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// This attribute will expose the property/field to be altered via console commands.
    /// </summary>
    [PropertyAccessRequirement(RequiresWrite = true)]
    [TargetTypeRestriction(AllowStrings = true, AllowPrimitives = true, AllowEnums = true, AllowStruct = false)]
    [TargetTypeRestriction(typeof(Vector2), typeof(Vector3), typeof(Vector4))]
    [TargetTypeRestriction(typeof(Vector2Int), typeof(Vector3Int))]
    [TargetTypeRestriction(typeof(Color), typeof(Color32))]
    public sealed class SetterAttribute : GetterSetterBase, ISetter
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
        /// Flag a property / field as settable (write) by console commands.
        /// Use /set (or /get) to view a list of available setter and (getter).
        /// Initialize a new instance of the SetAttribute class. 
        /// </summary>
        public SetterAttribute() { }
        

        /// <summary>
        /// Flag a property / field as settable (write) by console commands.
        /// Use /set (or /get) to view a list of available setter and (getter).
        /// Initialize a new instance of the SetAttribute class. 
        /// </summary>
        /// <param name="shortcut"></param>
        public SetterAttribute(string shortcut) => this.shortcut = shortcut;

        
        /// <summary>
        /// Flag a property / field as settable (write) by console commands.
        /// Use /set (or /get) to view a list of available setter and (getter).
        /// Initialize a new instance of the SetAttribute class. 
        /// </summary>
        /// <param name="shortcut"></param>
        /// <param name="description"></param>
        public SetterAttribute(string shortcut, string description)
        {
            this.shortcut = shortcut;
            this.description = description;
        }
        
        
        /// <summary>
        /// Flag a property / field as settable (write) by console commands.
        /// Use /set (or /get) to view a list of available setter and (getter).
        /// Initialize a new instance of the SetAttribute class. 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="shortcut"></param>
        /// <param name="defaultValue"></param>
        internal SetterAttribute(string shortcut, string description, object defaultValue)
        {
            this.shortcut = shortcut;
            this.description = description;
            this.Default = defaultValue;
        }

        #endregion
    }
}