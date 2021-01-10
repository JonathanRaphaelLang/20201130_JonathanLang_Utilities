namespace Ganymed.Console
{
    /// <summary>
    /// Use this interface as an entry point for custom input systems.
    /// </summary>
    public interface IConsoleEntry
    {
        void Toggle();
        void ApplyProposedInput();
        void SelectPreviousInputFromCache();
        void SelectSubsequentInputFromCache();

        string GetProposedDescription();
        string GetProposedCommand();
    }
}
