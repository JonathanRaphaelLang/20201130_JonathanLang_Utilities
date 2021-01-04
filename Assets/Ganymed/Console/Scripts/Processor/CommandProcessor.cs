using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ganymed.Console.Core;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    public static class CommandProcessor
    {
        #region --- [OPERATORS] ---

        public static string Prefix { get; set; } = "/";
        internal static string InfoOperator = "?";
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [KEYS]

        internal const string GetKey = "Get";
        internal const string SetKey = "Set";
        internal const string CommandsKey = "Commands";

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FLAGS] ---

        internal const BindingFlags MethodFlags =
            BindingFlags.Public 
            | BindingFlags.Static
            | BindingFlags.NonPublic;
        
        internal const BindingFlags SetterPropertyFlags =
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.NonPublic
            | BindingFlags.SetProperty
            | BindingFlags.ExactBinding;
        
        internal const BindingFlags SetterFieldFlags =
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.NonPublic
            | BindingFlags.SetField
            | BindingFlags.ExactBinding;
        
        internal const BindingFlags GetterPropertyFlags =
            BindingFlags.Public 
            | BindingFlags.Static 
            | BindingFlags.NonPublic 
            | BindingFlags.GetProperty;
        
        internal const BindingFlags GetterFieldFlags =
            BindingFlags.Public 
            | BindingFlags.Static 
            | BindingFlags.NonPublic 
            | BindingFlags.GetField;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EXCEPTIONS] ---

        internal static readonly Exception exception = new Exception();
        internal static readonly Exception mdaException = new Exception();

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [PERMISSIONS] ---

        internal static bool logCommandsLoadedOnStart = true;
        internal static bool allowNumericBoolProcessing = true;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VALIDATE CONFIGURATION] ---

        public static void SetConfiguration(ConsoleConfiguration configuration)
        {
            logCommandsLoadedOnStart = configuration.logCommandsLoadedOnStart;
            allowNumericBoolProcessing = configuration.allowNumericBoolProcessing;
            InfoOperator = configuration.infoOperator;
        }

        #endregion
       
        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod]
        private static void FindConsoleCommandsInAssembly()
        {
           MethodProcessor.FindMethodCommandsInAssembly(); 
           GetterProcessor.FindGetterInAssembly();
           SetterProcessor.FindSetterInAssembly();
        }
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [EXTENSION] ---

        internal static void ToCommandArgs(this string input, out string key, out List<string> args, char split = ' ')
        {
            input = input.Cut();
            
            //split key and args
            var inputSplit = input.Split(split);
            
            //separate key from args
            key = inputSplit[0];
            
            args = inputSplit.Skip(1).ToList();

            //Combine args forming a string

            // --- Append arguments that are marked as a string
            for (var i = 0; i < args.Count -1; i++)
            {
                if (!args[i].StartsWith('"'.ToString())) continue;
                
                args[i] = args[i].Remove(0, 1);
                    
                var viewedIndex = i;
                while (viewedIndex < args.Count - 1)
                {
                    if(args[viewedIndex].EndsWith('"'.ToString()))
                        break;
                    args[viewedIndex] += $" {args[viewedIndex + 1]}";
                    args.RemoveAt(viewedIndex + 1);
                }

                if (args[viewedIndex].EndsWith('"'.ToString()))
                    args[viewedIndex] = args[viewedIndex].Remove(args[viewedIndex].Length -1, 1);
            }
        }

        internal static string AsVectorHint(this int value)
            => value == 0 ? "X" : value == 1 ? "Y" : value == 2 ? "Z" : "W";
        
        internal static string AsColorHint(this int value)
            => value == 0 ? "R" : value == 1 ? "G" : value == 2 ? "B" : "A";

        #endregion
    }
}
