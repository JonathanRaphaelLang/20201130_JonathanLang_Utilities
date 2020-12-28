using System;

namespace Ganymed.Console.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
        /// <summary>
        /// The primary key to address the command
        /// </summary>
        public readonly string key;
        
        /// <summary>
        /// Description shown in the console and when using the "info operator "?"
        /// </summary>
        public readonly string description = "none";
        
        /// <summary>
        /// Execute the command on load. This use is primarily for debug purposes.
        /// </summary>
        public readonly bool executeOnLoad = false;

        /// <summary>
        /// Disable numeric input for boolean parameter for this command.
        /// Note that nbp (numeric boolean processing) can also be controlled via global configuration.
        /// Use this property to disable nbp for specific commands. 
        /// </summary>
        public readonly bool disableNBP = false;
        
        /// <summary>
        /// Delays the execution on load by x milliseconds. 
        /// </summary>
        public readonly int millisecondsDelayOnLoad = 0;
        
        /// <summary>
        /// object[] defaults will only be used if the command is executed on load. 
        /// </summary>
        public readonly object[] defaults = null;
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [OVERLOADS: KEY & DESCRIPTION] ---

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        public Command(string key)
        {
            this.key = key;
        }

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        public Command(string key, string description)
        {
            this.key = key;
            this.description = description;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [OVERLOADS: NBP] ---

        public Command(string key, bool disableNbp)
        {
            this.key = key;
            this.disableNBP = disableNbp;
        }

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="disableNbp"></param>
        public Command(string key, string description, bool disableNbp)
        {
            this.key = key;
            this.description = description;
            this.disableNBP = disableNbp;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [OVERLOADS: EXECUTE ON LOAD] ---

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="disableNbp"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="millisecondsDelayOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, string description, bool disableNbp, bool executeOnLoad, int millisecondsDelayOnLoad,
            params object[] defaults)
        {
            this.key = key;
            this.description = description;

            this.disableNBP = disableNbp;
            
            this.executeOnLoad = executeOnLoad;
            this.millisecondsDelayOnLoad = millisecondsDelayOnLoad;
            this.defaults = defaults;
        }

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="disableNbp"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="millisecondsDelayOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, bool disableNbp, bool executeOnLoad, int millisecondsDelayOnLoad,
            params object[] defaults)
        {
            this.key = key;
            
            this.disableNBP = disableNbp;
            
            this.executeOnLoad = executeOnLoad;
            this.millisecondsDelayOnLoad = millisecondsDelayOnLoad;
            this.defaults = defaults;
        }

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="disableNbp"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, bool disableNbp, bool executeOnLoad,
            params object[] defaults)
        {
            this.key = key;
            
            this.disableNBP = disableNbp;
            
            this.executeOnLoad = executeOnLoad;
            this.defaults = defaults;
        }

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="disableNbp"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, string description, bool disableNbp, bool executeOnLoad,
            params object[] defaults)
        {
            this.key = key;
            this.description = description;
            
            this.disableNBP = disableNbp;
            
            this.executeOnLoad = executeOnLoad;
            this.defaults = defaults;
        }  

        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="millisecondsDelayOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, string description, bool executeOnLoad, int millisecondsDelayOnLoad,
            params object[] defaults)
        {
            this.key = key;
            this.description = description;
            
            this.executeOnLoad = executeOnLoad;
            this.millisecondsDelayOnLoad = millisecondsDelayOnLoad;
            this.defaults = defaults;
        }
        
        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="millisecondsDelayOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, bool executeOnLoad, int millisecondsDelayOnLoad,
            params object[] defaults)
        {
            this.key = key;
            
            this.executeOnLoad = executeOnLoad;
            this.millisecondsDelayOnLoad = millisecondsDelayOnLoad;
            this.defaults = defaults;
        }
        
        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, bool executeOnLoad,
            params object[] defaults)
        {
            this.key = key;
            
            this.executeOnLoad = executeOnLoad;
            this.defaults = defaults;
        }
        
        /// <summary>
        /// Create a new instance of the Command Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <param name="executeOnLoad"></param>
        /// <param name="defaults"></param>
        public Command(string key, string description, bool executeOnLoad,
            params object[] defaults)
        {
            this.key = key;
            this.description = description;
            
            this.executeOnLoad = executeOnLoad;
            this.defaults = defaults;
        }        

        #endregion
    }
}