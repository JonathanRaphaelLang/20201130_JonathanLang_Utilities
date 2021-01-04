using System;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// The primary key to address the command
        /// </summary>
        public readonly string key;
        
        /// <summary>
        /// Description shown in the console and when using the "info operator "?"
        /// </summary>
        public readonly string description = "n/a";

        /// <summary>
        /// Disable numeric input for boolean parameter for this command.
        /// Note that nbp (numeric boolean processing) can also be controlled via global configuration.
        /// Use this property to disable nbp for specific commands. 
        /// </summary>
        public readonly bool disableNBP = false;

        
        public readonly int priority = 0;

        //--------------------------------------------------------------------------------------------------------------

        #region --- [OVERLOADS: KEY & DESCRIPTION] ---
        
        public CommandAttribute(string key)
        {
            this.key = key;
        }
        
        public CommandAttribute(string key, string description)
        {
            this.key = key;
            this.description = description;
        }
        
        public CommandAttribute(string key, string description, bool disableNBP, int priority)
        {
            this.key = key;
            this.description = description;
            this.disableNBP = disableNBP;
            this.priority = priority;
        }
        
        public CommandAttribute(string key, string description, bool disableNBP)
        {
            this.key = key;
            this.description = description;
            this.disableNBP = disableNBP;
        }
        
        public CommandAttribute(string key, string description, int priority)
        {
            this.key = key;
            this.description = description;
            this.priority = priority;
        }
        
        public CommandAttribute(string key, bool disableNBP, int priority)
        {
            this.key = key;
            this.disableNBP = disableNBP;
            this.priority = priority;
        }
        
        public CommandAttribute(string key, bool disableNBP)
        {
            this.key = key;
            this.disableNBP = disableNBP;
        }
        
        public CommandAttribute(string key, int priority)
        {
            this.key = key;
            this.priority = priority;
        }

        
        #endregion
        
       
    }
}