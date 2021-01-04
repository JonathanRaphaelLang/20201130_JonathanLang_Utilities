using System;

namespace Ganymed.Console
{
    [Flags]
    public enum TransmissionOptions
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
    
        /// <summary>
        /// Flag the transmission as an enumeration
        /// </summary>
        Enumeration = 1,
        
        /// <summary>
        /// Disable rich text for the transmission
        /// </summary>
        DisableRichText = 2,
    }
}
