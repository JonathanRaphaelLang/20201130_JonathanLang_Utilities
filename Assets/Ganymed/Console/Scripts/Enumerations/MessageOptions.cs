using System;
using System.ComponentModel;

namespace Ganymed.Console
{
    [Flags]
    public enum MessageOptions
    {
        /// <summary>
        /// No formatting
        /// 0
        /// </summary>
        [Description("No formatting")]
        None = 0,
    
        /// <summary>
        /// Make content bold
        /// </summary>
        [Description("Make content bold")]
        Bold = 1,
    
        /// <summary>
        /// Make content cursive
        /// </summary>
        [Description("Make content cursive")]
        Italics  = 2,
        
        /// <summary>
        /// Make content crossed out
        /// </summary>
        [Description("Make content crossed out")]
        Strike  = 4,

        /// <summary>
        /// Underline the content
        /// </summary>
        [Description("Underline the content")]
        Underline = 8,
        
        /// <summary>
        /// Make content uppercase
        /// </summary>
        [Description("Make content uppercase")]
        Uppercase  = 16,
        
        /// <summary>
        /// Make content lowercase
        /// </summary>
        [Description("Make content lowercase")]
        Lowercase = 32,
        
        /// <summary>
        /// Make content smallcaps
        /// </summary>
        [Description("Make content smallcaps")]
        Smallcaps  = 64,
        
        /// <summary>
        /// Mark the content. Marker color can be selected in the console config.
        /// </summary>
        [Description("Mark the content. Marker color can be selected in the console config.")]
        Mark  = 128,
        
        /// <summary>
        /// Surround the content with brackets
        /// </summary>
        [Description("Surround the content with brackets")]
        Brackets = 256
    }
}
