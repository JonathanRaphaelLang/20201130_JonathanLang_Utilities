using System;
using System.Linq;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// The SuggestionAttribute is used by the console to make custom autocompletion suggestions during runtime.
    /// This attribute will only work on a parameter of type string. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SuggestionAttribute : Attribute
    {
        #region --- [PROPERTIES] ---

        /// <summary>
        /// Contains input strings to suggest
        /// </summary>
        public readonly string[] Suggestions;

        /// <summary>
        /// Should case be ignored when comparing input with suggestions
        /// </summary>
        public bool IgnoreCase { get; set; }

        #endregion

        #region --- [METHODS] ---

        /// <summary>
        /// Aggregate and return a string containing every suggestion split by '&'. 
        /// </summary>
        /// <returns>Every suggestion</returns>
        public string GetAllSuggestions()
        {
            var aggregate = Suggestions.Aggregate(string.Empty, (current, VARIABLE) => current + $" {'"'}{VARIABLE}{'"'} &");
            return aggregate.Remove(aggregate.Length - 1, 1);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        /// <summary>
        /// Create a new instance of the Suggestion attribute.
        /// Suggestions are used by the console to make custom autocompletion suggestions during runtime.
        /// </summary>
        /// <param name="suggestions"></param>
        public SuggestionAttribute(params string[] suggestions)
        {
            Suggestions = suggestions;
            if(Suggestions.Length <= 0) Suggestions = new string[1]{"N/A"};
        }

        #endregion
    }
}
