namespace Ganymed.Console.Enumerations
{
    public enum InputValidation
    {
        /// <summary>
        /// Input is not a command
        /// </summary>
        None,
        
        /// <summary>
        /// The input is valid and will execute a command.
        /// </summary>
        Valid,
        
        /// <summary>
        /// The input matches a signature but is incomplete.
        /// </summary>
        Incomplete,
        
        /// <summary>
        /// The input is valid but optional parameters remain.
        /// </summary>
        Optional,

        /// <summary>
        /// The input does not match a signature and cannot even witch additional characters.
        /// </summary>
        Incorrect,
        
        /// <summary>
        /// The input is valid and will display information about the associated key.
        /// </summary>
        CommandInfo
    }
}
