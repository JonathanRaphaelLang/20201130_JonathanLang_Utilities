using System;
using System.Collections.Generic;
using System.Linq;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class StringExtensions
    {

        /// <summary>
        /// Use this to format the name of a variable to automatically add breaks before a upper case character. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string AsLabel(this string name, string prefix = "")
        {
            var chars = new List<char>();
            
            for (var i = 0; i < name.Length; i++)
            {
                if (i == 0)
                {
                    chars.Add(char.ToUpper(name[i]));
                }
                else
                {
                    if (i < name.Length - 1)
                    {
                        if(char.IsUpper(name[i]) && !char.IsUpper(name[i+1])
                        || char.IsUpper(name[i]) && !char.IsUpper(name[i-1]))
                        {
                            if (i > 1)
                            {
                                chars.Add(' ');    
                            }
                        }
                    }
                    chars.Add(name[i]);    
                }
            }
            return $"{prefix}" +
                   $"{(prefix.IsNullOrWhiteSpace()? "": " ")}" +
                   $"{chars.Aggregate(string.Empty, (current, character) => current + character)}";
        }
        
        
        /// <summary>
        /// Get an array of boolean values for each character of a string. Each boolean value will be true if the
        /// associated character is uppercase and false if it is lowercase.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool[] GetUpperLowerConfig(this string input)
        {
            var config = new bool[input.Length];

            for (var i = 0; i < config.Length; i++)
            {
                config[i] = input[i].ToString() == input[i].ToString().ToUpper();
            }

            return config;
        }
        
        
        /// <summary>
        /// Set an array of boolean values for each character of a string. Each boolean value represents if the
        /// associated character is uppercase (true) or lowercase (false). The default value if the configuration is
        /// shorter than the string is lowercase (false)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="caseConfiguration"></param>
        /// <returns></returns>
        public static string SetUpperLowerConfig(this string input, bool[] caseConfiguration)
        {
            var returnValue = string.Empty;

            for (var i = 0; i < input.Length; i++)
            {
                if(caseConfiguration.Length -1 >= i)
                    returnValue += caseConfiguration[i]? input[i].ToString().ToUpper() : input[i].ToString().ToLower();
                else
                {
                    returnValue += input[i].ToString().ToLower();
                }
            }

            return returnValue;
        }
        

        /// <summary>
        /// Remove breaks (\n) from the target string. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveBreaks(this string input)
        {
            return input.Replace("\n", string.Empty);
        }
        

        /// <summary>
        /// Cut either the start/end or both of a string removing unnecessary characters.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cut"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Cut(this string input, StartEnd cut = StartEnd.StartAndEnd, char character = ' ')
        {
            switch (cut)
            {
                case StartEnd.Start:
                    while (input.StartsWith(character.ToString()))
                    {
                        input = input.Remove(0, 1);
                    }
                    return input;
                
                case StartEnd.End:
                    while (input.EndsWith(character.ToString()))
                    {
                        input = input.Remove(input.Length - 1, 1);
                    }
                    return input;
                
                case StartEnd.StartAndEnd:
                    while (input.StartsWith(character.ToString()))
                    {
                        input = input.Remove(0, 1);
                    }
                    while (input.EndsWith(character.ToString()))
                    {
                        input = input.Remove(input.Length - 1, 1);
                    }
                    return input;

                default:
                    return input;
            }
        }

        /// <summary>
        /// Remove a given string form the beginning of the target string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public static string RemoveFormBeginning(this string input, string remove)
            => input.StartsWith(remove) ? input.Remove(0, remove.Length) : input;
        
        /// <summary>
        /// remove the given length from the beginning of the target string. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public static string RemoveFormBeginning(this string input, int remove)
            => input.Length > remove ? input.Remove(0, remove) : input;
        
        
        /// <summary>
        /// remove the given length from the end of the target string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RemoveFormEnd(this string input, int length)
            => input.Length > length ? input.Remove(input.Length - length, length) : input;
        
        /// <summary>
        /// remove the given string from the target string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public static string Delete(this string input, string remove)
            => input.Replace(remove, "");
        
        
        /// <summary>
        /// Repeat the given string for target values times.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public static string Repeat(this int value, string character = " ")
        {
            var returnValue = string.Empty;
            
            for (var i = 0; i < value; i++) {
                returnValue += character;
            }

            return returnValue;
        }

        public static bool IsNullOrWhiteSpace(this string target)
        {
            return string.IsNullOrWhiteSpace(target);
        }
    }
}
