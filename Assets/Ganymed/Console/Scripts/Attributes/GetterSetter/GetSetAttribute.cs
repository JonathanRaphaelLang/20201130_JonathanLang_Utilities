using Ganymed.Utils;
using Ganymed.Utils.Attributes;
using UnityEngine;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Attributes that exposes the target property/field to be issued and altered via console commands
    /// </summary>
    [PropertyAccessRequirement(RequiresRead = true, RequiresWrite = true)]
    [TargetTypeRestriction(AllowStrings = true, AllowPrimitives = true, AllowEnums = true, AllowStruct = false)]
    [TargetTypeRestriction(typeof(Vector2), typeof(Vector3), typeof(Vector4))]
    [TargetTypeRestriction(typeof(Vector2Int), typeof(Vector3Int))]
    [TargetTypeRestriction(typeof(Color), typeof(Color32))]
    public sealed class GetSetAttribute : GetterSetterBase, ISetter
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
        /// Mark a property / field as gettable and settable (read/write) by commands.
        /// Use /get or /set to view a list of available getter and setter.
        /// Initialize a new instance of the GetAndSetAttribute class. 
        /// </summary>
        public GetSetAttribute() { }
        
        
        /// <summary>
        /// Mark a property / field as gettable and settable (read/write) by commands.
        /// Use /get or /set to view a list of available getter and setter.
        /// Initialize a new instance of the GetAndSetAttribute class. 
        /// </summary>
        /// <param name="shortcut"></param>
        public GetSetAttribute(string shortcut) => this.shortcut = shortcut;

        /// <summary>
        /// Mark a property / field as gettable and settable (read/write) by commands.
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
        /// Mark a property / field as gettable and settable (read/write) by commands.
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
