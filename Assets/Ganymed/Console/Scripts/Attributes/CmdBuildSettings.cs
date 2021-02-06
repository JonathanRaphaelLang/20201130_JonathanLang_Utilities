using System;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Enum containing instructions for handling commands in builds. 
    /// </summary>
    [Flags]
    public enum CmdBuildSettings
    {
        None = 0,
        
        /// <summary>
        /// Determines if the commands auto completion will be disabled. This is primarily to hinder the accessibility
        /// of the command and will mostly be used in minor edge cases. (e.g for hidden cheat codes)
        /// </summary>
        DisableAutoCompletion = 1,
        
        /// <summary>
        /// Determines if the command will be excluded from listings. This is primarily to hinder the accessibility
        /// of the command and will mostly be used in minor edge cases. (e.g for hidden cheat codes)
        /// </summary>
        DisableListings = 2,
        
        /// <summary>
        /// Determines if the commands auto completion will be disabled and if the command will be excluded from listings.
        /// This is primarily to hinder the accessibility of the command and will mostly be used in minor edge cases.
        /// (e.g for hidden cheat codes)
        /// </summary>
        DisableListingsAndAutoCompletion = 3,
        
        /// <summary>
        /// Determines if the command will be excluded from builds and is only available in Edit mode. 
        /// Note that the command will still be compiled. If the command should really be excluded from builds
        /// use preprocessor directives instead.
        /// </summary>
        ExcludeFromBuild = 4
    }
}