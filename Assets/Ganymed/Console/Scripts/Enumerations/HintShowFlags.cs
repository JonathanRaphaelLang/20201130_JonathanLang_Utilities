using System;

namespace Ganymed.Console
{
    [Flags]
    public enum HintShowFlags : short
    {
        /// <summary>
        /// dont show anything
        /// </summary>
        None = 0,              
            
        /// <summary>
        /// Show value only
        /// </summary>
        ShowValue = 1,         
            
        /// <summary>
        /// Show type only
        /// </summary>
        ShowType = 2,          
        
        /// <summary>
        /// Show type & value
        /// </summary>
        ExcludeName = 3,
        
        /// <summary>
        /// Show name only
        /// </summary>
        ShowName = 4,           
        
        /// <summary>
        /// Show value & name
        /// </summary>
        ExcludeType = 5,
        
        /// <summary>
        /// Show name & type
        /// </summary>
        ExcludeValue = 6,
        
        /// <summary>
        /// Show name type & value
        /// </summary>
        ShowAll = 7,
    }
}