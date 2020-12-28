using System;
using Ganymed.Utils;
using Ganymed.Utils.ExtensionMethods;

namespace Ganymed.Console.Core
{
    /// <summary>
    /// Attribute can be applied on any parameter in command methods.
    /// Customize displayed information in the console about the target parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Hint : Attribute
    {
        #region --- [FLAGS] ---

        [Flags]
        public enum Flags : short
        {
            /// <summary>
            /// Dont show anything as an additional hint 
            /// </summary>
            None = 0,              //00000000  
            
            /// <summary>
            /// Show the default value as a hint
            /// </summary>
            ShowValue = 1,         //00000001
            
            /// <summary>
            /// Show the type as a hint
            /// </summary>
            ShowType = 2,          //00000010
            
            /// <summary>
            /// Show the parameter name as a hint 
            /// </summary>
            ShowName = 4           //00000100
        }

        #endregion
        
        #region --- [PROPERTIES] ---
        
        /// <summary>
        /// General description of the parameter
        /// </summary>
        public readonly string description;
        
        /// <summary>
        /// Show the default value as a hint when proposing
        /// </summary>
        public readonly bool showDefaultValue = false;
        
        /// <summary>
        /// Show the parameters type as a hint when proposing
        /// </summary>
        public readonly bool showValueType = true;

        /// <summary>
        /// Show the parameters name as a hint when proposing
        /// </summary>
        public readonly bool showParameterName = true; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---
        
        public Hint(string description)
        {
            this.description = description;
        }
        
        public Hint(string description, Flags hint = Flags.ShowType | Flags.ShowName)
        {
            this.description = description;
            
            showDefaultValue = hint.HasFlag(Flags.ShowValue);
            showValueType = hint.HasFlag(Flags.ShowType);
            showParameterName = hint.HasFlag(Flags.ShowName);
        }

        #endregion
    }
}
