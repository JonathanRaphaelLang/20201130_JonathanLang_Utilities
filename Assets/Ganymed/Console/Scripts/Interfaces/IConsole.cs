namespace Ganymed.Console
{
    /// <summary>
    /// Use this interface as an entry point for custom input systems.
    /// </summary>
    public interface IConsole
    {
        void Toggle();
        void ApplyProposedInput();
        void SelectPreviousInputFromCache();
        void SelectSubsequentInputFromCache();

        /// <summary>
        /// returns the currently cached hint/description string from the command processor.
        /// </summary>
        /// <returns></returns>
        string GetProposedDescription();
        
        /// <summary>
        /// returns the currently cached proposed input by the command processor.
        /// </summary>
        /// <returns></returns>
        string GetProposedCommand();
    }
}
