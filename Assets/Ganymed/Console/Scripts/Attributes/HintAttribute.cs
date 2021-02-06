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
        /// Bitmap containing information about what should and what should not be shown as a hint
        /// </summary>
        public HintConfig Show
        {
            get => show;
            set
            {
                ShowDefaultValue = value.HasFlag(HintConfig.ShowValue);
                ShowValueType = value.HasFlag(HintConfig.ShowType);
                ShowParameterName = value.HasFlag(HintConfig.ShowName);
                show = value;
            }
        }

        /// <summary>
        /// General description of the parameter
        /// </summary>
        public string Description { get; set; } = null;

        /// <summary>
        /// Show the default value as a hint when proposing
        /// </summary>
        public bool ShowDefaultValue { get; private set; } = false;

        /// <summary>
        /// Show the parameters type as a hint when proposing
        /// </summary>
        public bool ShowValueType { get; private set; } = true;

        /// <summary>
        /// Show the parameters name as a hint when proposing
        /// </summary>
        public bool ShowParameterName { get; private set; } = true;

        #endregion

        #region --- [FIELDS] ---

        private HintConfig show = HintConfig.None;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---
        
        /// <summary>
        /// Create a new instance of the Hint attribute
        /// </summary>
        public HintAttribute()
        {
            
        }
        
        /// <summary>
        /// Create a new instance of the Hint attribute
        /// </summary>
        /// /// <param name="hintConfig"></param>
        public HintAttribute(HintConfig hintConfig)
        {
            Show = hintConfig;
        }
        
        /// <summary>
        /// Create a new instance of the Hint attribute
        /// </summary>
        /// <param name="description"></param>
        public HintAttribute(string description)
        {
            Description = description;
        }

        #endregion
    }
}
