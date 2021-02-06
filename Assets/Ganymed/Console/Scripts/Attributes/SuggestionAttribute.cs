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
        /// Contains a collection of potential suggestions
        /// </summary>
        public readonly string[] Suggestions;

        /// <summary>
        /// Should case be ignored when comparing input strings with suggestions
        /// </summary>
        public bool IgnoreCase { get; set; }

        #endregion

        #region --- [METHODS] ---

        /// <summary>
        /// Aggregate and return a string containing every suggestion split by character. 
        /// </summary>
        /// <returns>Every suggestion</returns>
        public string GetAllSuggestions(char split = '&')
        {
            var all = Suggestions.Aggregate(
                string.Empty, (current, VARIABLE) => current + $" {'"'}{VARIABLE}{'"'} {split}");
            
            return all.Remove(all.Length - 1, 1); //remove the last split character
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        /// <summary>
        /// Suggestions are used by the console to make custom autocompletion suggestions during runtime.
        /// </summary>
        /// <param name="suggestion"></param>
        /// <param name="suggestions"></param>
        public SuggestionAttribute(string suggestion, params string[] suggestions)
        {
            Suggestions = new string[suggestions.Length + 1];
            Suggestions[0] = suggestion;

            for (var i = 0; i < suggestions.Length; i++)
            {
                Suggestions[i + 1] = suggestions[i];
            }
        }
        
        private SuggestionAttribute() { }

        #endregion
    }
}
