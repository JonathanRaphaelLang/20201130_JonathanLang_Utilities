using System;
using System.Reflection;
using UnityEngine;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class StringExtensions
    {
        #region --- [STRING FORMATTING] ---

        public static bool[] GetUpperLowerConfig(this string input)
        {
            var config = new bool[input.Length];

            for (var i = 0; i < config.Length; i++)
            {
                config[i] = input[i].ToString() == input[i].ToString().ToUpper();
            }

            return config;
        }
        
        public static string SetUpperLowerConfig(this string input, bool[] config)
        {
            var returnValue = string.Empty;

            for (var i = 0; i < input.Length; i++)
            {
                if(config.Length -1 >= i)
                    returnValue += config[i]? input[i].ToString().ToUpper() : input[i].ToString().ToLower();
                else
                {
                    returnValue += input[i].ToString().ToLower();
                }
            }

            return returnValue;
        }

        public static string EmptyString(this string input)
        {
            return string.Empty;
        }

        public enum CutEnum
        {
            Start,
            End,
            StartAndEnd
        }

        public static string RemoveBreaks(this string input)
        {
            return input.Replace("\n", string.Empty);
        }

        public static string Cut(this string input, CutEnum cut = CutEnum.StartAndEnd, char character = ' ')
        {
            switch (cut)
            {
                case CutEnum.Start:
                    while (input.StartsWith(character.ToString()))
                    {
                        input = input.Remove(0, 1);
                    }
                    return input;
                
                case CutEnum.End:
                    //remove last char if its empty space
                    while (input.EndsWith(character.ToString()))
                    {
                        input = input.Remove(input.Length - 1, 1);
                    }
                    return input;
                
                case CutEnum.StartAndEnd:
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
                    throw new ArgumentOutOfRangeException(nameof(cut), cut, null);
            }
        }
        

        /// <summary>
        /// Remove whitespace at the end of the string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cutNum">the amount of characters cut from the start</param>
        /// <param name="character">set a character instead of whitespace that will be removed</param>
        /// <returns></returns>
        public static string CutStart(this string input, out int cutNum, char character = ' ')
        {
            cutNum = 0;
            //remove first char if its empty space
            while (input.StartsWith(character.ToString()))
            {
                input = input.Remove(0, 1);
                cutNum++;
            }
            return input;
        }    


        public static string RemoveFormBeginning(this string input, string remove)
        {
            var i = 0;
            while (string.Equals(input[0].ToString(), remove[i].ToString(),
                StringComparison.OrdinalIgnoreCase) && input.Length > 0 && remove.Length -1 >= i)
            {
                i++;
                input = input.Remove(0,1);
            }
            return input;
        }

        #endregion

    }
}
