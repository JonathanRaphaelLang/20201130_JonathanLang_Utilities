using System;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Attribute can be applied on any parameter in command methods.
    /// Customize displayed information in the console about the target parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HintAttribute : Attribute
    {
        #region --- [PROPERTIES] ---

        /// <summary>
        /// Includes information about what should and what should not be shown as a hint
        /// </summary>
        public HintShowFlags Show
        {
            get => show;
            set
            {
                showDefaultValue = value.HasFlag(HintShowFlags.ShowValue);
                showValueType = value.HasFlag(HintShowFlags.ShowType);
                showParameterName = value.HasFlag(HintShowFlags.ShowName);
                show = value;
            }
        }

        /// <summary>
        /// General description of the parameter
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// Show the default value as a hint when proposing
        /// </summary>
        public bool ShowDefaultValue => showDefaultValue;

        /// <summary>
        /// Show the parameters type as a hint when proposing
        /// </summary>
        public bool ShowValueType => showValueType;

        /// <summary>
        /// Show the parameters name as a hint when proposing
        /// </summary>
        public bool ShowParameterName => showParameterName;

        #endregion

        #region --- [FIELDS] ---

        private HintShowFlags show;
        private readonly string description;
        private bool showDefaultValue = false;
        private bool showValueType = true;
        private bool showParameterName = true; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---
        
        /// <summary>
        /// Create a new instance of the Hint attribute
        /// </summary>
        /// <param name="description"></param>
        public HintAttribute(string description)
        {
            this.description = description;
        }

        #endregion
    }
}
