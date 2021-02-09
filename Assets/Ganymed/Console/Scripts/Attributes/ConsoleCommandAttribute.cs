using System;
using Ganymed.Utils.Attributes;
using UnityEngine;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Command attribute class: Declare a method as console command that can be invoked via console input.
    /// Use /commands to receive a list of available console commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [TargetParamRestrictions(typeof(object), AllowPrimitives = true, AllowStruct = true, AllowStrings = true, AllowEnums = true)]
    [RequiredAccess(Static = true)]
    public class ConsoleCommandAttribute : Attribute
    {
        #region --- [PROPERTIES] ---
        
        /// <summary>
        /// Description shown in the console and when using the info operator (default)"?"
        /// </summary>
        public string Description { get; set; } 

        //---

        /// <summary>
        /// Commands with a higher priority will be prefered by autocompletion and are shown higher up in listings.
        /// </summary>
        public int Priority { get; set; }
        
        //---
        
        /// <summary>
        /// Unique accessor for the command.
        /// If the same key is assigned multiple times it will automatically be altered until it is unique.
        /// </summary>
        public string Key { get; }
        
        //---
        
        /// <summary>
        /// Determines if numeric input for boolean parameter for this command is disabled.
        /// Note that nbp (numeric boolean processing) can also be controlled via global settings.
        /// Use this property to disable nbp for specific commands. 
        /// </summary>
        public bool DisableNBP { get; set; } = default;
        
        //---   
        
        /// <summary>
        /// Bitmask containing instructions for alternative handling of commands in builds.
        /// If you dont want alt command behaviour in your build leave this property as it is.
        /// </summary>
        public CmdBuildSettings BuildSettings { get; set; }
       
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        /// <summary>
        /// Declare a method as console command that can be invoked via console input.
        /// Use /commands to receive a list of available console commands.
        /// </summary>
        /// <param name="key"></param>
        public ConsoleCommandAttribute(string key)
        {
            Key = key;
        }

        #endregion
    }
}