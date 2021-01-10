using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    [TargetParamRestrictions(typeof(object), AllowPrimitives = true, AllowStruct = true, AllowStrings = true, AllowEnums = true)]
    public sealed class CommandAttribute : CommandBaseAttribute
    {
        #region --- [PROPERTIES] ---
        
        /// <summary>
        /// The primary key to address the command
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// Disable numeric input for boolean parameter for this command.
        /// Note that nbp (numeric boolean processing) can also be controlled via global configuration.
        /// Use this property to disable nbp for specific commands. 
        /// </summary>
        public bool DisableNBP { get; set; } = default;

        #endregion

        #region --- [FIELDS] ---

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [OVERLOADS: KEY & DESCRIPTION] ---

        /// <summary>
        /// Declare a method as console command that can be invoked via console input.
        /// Use /commands to receive a list of available console commands.
        /// Initialize a new instance of the CommandAttribute. 
        /// </summary>
        /// <param name="key"></param>
        public CommandAttribute(string key)
        {
            Key = key;
        }

        #endregion
        
       
    }
}