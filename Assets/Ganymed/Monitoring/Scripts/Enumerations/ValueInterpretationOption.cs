namespace Ganymed.Monitoring
{
    public enum ValueInterpretationOption
    {

        /// <summary>
        /// Returns the current value
        /// </summary>
        Value = 0,
        
        /// <summary>
        /// Returns the cached value 
        /// </summary>
        Cached = 1,
        
        /// <summary>
        /// Returns the default value of the type
        /// </summary>
        Default = 2,
        
        /// <summary>
        /// Returns the type of the value
        /// </summary>
        Type = 3,
    }
}