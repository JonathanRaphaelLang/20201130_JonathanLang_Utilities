using System;
using System.Linq;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SuggestionAttribute : Attribute
    {
        /// <summary>
        /// Contains input strings to suggest
        /// </summary>
        public readonly string[] Suggestions;

        public string GetAllSuggestions()
        {
            var aggregate = Suggestions.Aggregate(string.Empty, (current, VARIABLE) => current + $" {'"'}{VARIABLE}{'"'} |");
            return aggregate.Remove(aggregate.Length - 1, 1);
        }
        
        /// <summary>
        /// Should case be ignored when comparing input with suggestions
        /// </summary>
        public readonly bool IgnoreCase = true;
        
        //--------------------------------------------------------------------------------------------------------------
        
        public SuggestionAttribute(params string[] suggestions)
        {
            Suggestions = suggestions;
            if(Suggestions.Length <= 0) Suggestions = new string[1]{"N/A"};
        }
        
        public SuggestionAttribute(string[] suggestions, bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
            Suggestions = suggestions;
            if(Suggestions.Length <= 0) Suggestions = new string[1]{"N/A"};
        }
        
        public SuggestionAttribute(bool ignoreCase, params string[] suggestions)
        {
            IgnoreCase = ignoreCase;
            Suggestions = suggestions;
            if(Suggestions.Length <= 0) Suggestions = new string[1]{"N/A"};
        }
    }
}
