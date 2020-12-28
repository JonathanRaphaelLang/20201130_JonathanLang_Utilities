namespace Ganymed.Monitoring.Enumerations
{
    public enum ValueInterpretationOption
    {

        /// <summary>
        /// Returns the current value
        /// </summary>
        CurrentValue = 0,
        
        /// <summary>
        /// Returns the cached value 
        /// </summary>
        LastValue = 1,
        
        /// <summary>
        /// Returns the default value of the type
        /// </summary>
        DefaultValue = 2,
        
        /// <summary>
        /// Returns the type of the value
        /// </summary>
        Type = 3,
    }
}