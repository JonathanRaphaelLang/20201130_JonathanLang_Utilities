using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Core;
using Ganymed.Console.Transmissions;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

// ReSharper disable PossibleMultipleEnumeration

namespace Ganymed.Console.Processor
{
    /// <summary>
    /// class responsible for command processing and management. Responsibilities include..
    /// Console Commands, Getter, Setter.
    /// Validation & Preprocessing / Autocompletion of command input.
    /// Gathering of commands in Assembly.
    /// </summary>
    public static partial class CommandProcessor
    {
        #region --- [FIELDS] ---

        public static string Prefix { get; set; } = "/";
        private static string InfoOperator = "?";

        private const string GetterKey = "Get";
        private const string SetterKey = "Set";
        private const string CommandsKey = "Commands";

        private const BindingFlags CommandFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

        private static bool logCommandsLoadedOnStart = true;
        private static bool allowNumericBoolProcessing = true;
        
        // --- Task 
        private static CancellationTokenSource source = new CancellationTokenSource();
        private static CancellationToken ct;
        private static float time;


        #region --- [COMMANDS (Methods)] ---

        private static volatile Dictionary<string, Command> MethodCommands = new Dictionary<string, Command>(StringComparer.CurrentCultureIgnoreCase);
        private static InputValidation cachedInputValidation = InputValidation.None;
        private static bool cachedProposeReturnValue = default;
        private static readonly Exception multiParameterException = new Exception();

        #endregion
        
        #region --- [GETTER] ---
        
        private static volatile Dictionary<string, Dictionary<string, GetterInfo>> Getter = new Dictionary<string, Dictionary<string, GetterInfo>>();
        private static volatile Dictionary<string, GetterInfo> GetterShortcuts = new Dictionary<string, GetterInfo>();
        
        private const string GetterDescription = "Get the value of an expsoed propertiy/field. " +
                                                 "/get for more information and a list of getter";
        #endregion

        #region --- [SETTER] ---

        private static volatile Dictionary<string, Dictionary<string, SetterInfo>> Setter = new Dictionary<string, Dictionary<string, SetterInfo>>();
        private static volatile Dictionary<string, SetterInfo> SetterShortcuts = new Dictionary<string, SetterInfo>();

        private const string SetterDescription = "Set the value of an expsoed propertiy/field. " +
                                                 "/set for more information and a list of setter";

        #endregion

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZE] ---

        [RuntimeInitializeOnLoadMethod]
        private static void FindConsoleCommandsInAssembly()
        {
            time = Time.realtimeSinceStartup;

            if (Transmission.Start(TransmissionOptions.None, "Commands"))
            {
                Transmission.AddLine("Searching for console commands in Assembly...");
                Transmission.Release();    
            };
            
            
            Task.Run(delegate
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var types = AppDomain.CurrentDomain.GetAllTypes();
                    
                    FindMethodConsoleCommandsInAssembly(types);
                    FindGetterInAssembly(types);
                    FindSetterInAssembly(types);
                }
                catch (Exception exception)
                {    
                    // --- Log the exception if it was not thrown by the task cancellation. 
                    if (!(exception is ThreadAbortException)) {
                        Debug.LogException(exception);
                    }
                    Debug.Log("CommandSearchCancel");
                }
                finally
                {
                    ResetCommandSearchTaskResources();
                }
            }, ct).Then(delegate
            {
                if (!logCommandsLoadedOnStart || !Application.isPlaying )
                    return;

                const MessageOptions options = MessageOptions.Brackets | MessageOptions.Bold;

                if(!Transmission.Start(TransmissionOptions.None, "Commands")) return;

                var count = 0;
                foreach (var cmd in MethodCommands)
                {
                    if(cmd.Value.Signatures.Count == 1 && !Application.isEditor)
                        if(cmd.Value.Signatures[0].disableListings) continue;
                    count++;
                }
                
                Transmission.AddLine(
                    new MessageFormat($"{count}", Core.Console.ColorEmphasize, options),
                    $"Commands are loaded | Time passed: " +
                    $"{Time.realtimeSinceStartup - time:00.00}s |",
                    new MessageFormat($"{Prefix}{CommandsKey}", Core.Console.ColorEmphasize, options),
                    new MessageFormat("To receive a list of commands"));
                Transmission.Release();
            });
        }
        
        /// <summary>
        /// Method cancels ongoing reflection tasks if present and resets the CancellationTokenSource and
        /// CancellationToken. 
        /// </summary>
        private static void ResetCommandSearchTaskResources()
        {
            source.Cancel();
            source.Dispose();
            source = new CancellationTokenSource();
            ct = source.Token;
        }
        
        static CommandProcessor()
        {
            UnityEventCallbacks.AddEventListener(
                listener:ResetCommandSearchTaskResources, 
                removePreviousListener: true, 
                ApplicationState.EditAndPlayMode,
                UnityEventType.ApplicationQuit,
                UnityEventType.PreProcessorBuild);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [PROCESSING] ---

        public static Task Process(string input)
        {
            var command = ReturnCommand(input);
            
            if (command.EndsWith(CommandProcessor.InfoOperator)) {
                LogCommandInformation(command);
            }
            
            else if (command.Equals(GetterKey, StringComparison.OrdinalIgnoreCase)) {
                ProcessGetter(input);
            }
            
            else if (command.Equals(SetterKey, StringComparison.OrdinalIgnoreCase)) {
                ProcessSetter(input);
            }
            
            else {
                ProcessMethodCommand(input);
            }
            return Task.CompletedTask;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [? OPERATOR] ---
        
        private static void LogCommandInformation(string key)
        {
            key = key.Remove(key.Length - 1, 1);

            if (!MethodCommands.TryGetValue(key, out var command)) return;

            if(!Transmission.Start()) return;

            var index = 0;
            foreach (var signature in command.Signatures)
            {
                Transmission.AddLine($"Key:[{key}]{(command.Signatures.Count > 1 ? $"[{index}]" : string.Empty)}");
                Transmission.AddLine($"Description: {signature.description}");

                var parameterInformation = signature.methodInfo.GetParameters();

                for (var i = 0; i < parameterInformation.Length; i++)
                {
                    var description = string.Empty;
                    if (parameterInformation[i].TryGetAttribute(out HintAttribute spec))
                        description = $" | Description: {'"'}{spec.Description}{'"'}";

                    #region --- [ENUM] ---

                    var enumInfo = string.Empty;
                    if (parameterInformation[i].ParameterType.IsEnum)
                    {
                        var values = Enum.GetValues(parameterInformation[i].ParameterType);
                        foreach (var value in values) enumInfo += $"[{value}]";
                    }

                    #endregion

                    var message =
                        $"Parameter [{i}]:{(parameterInformation[i].IsOptional ? " (<color=#CCC>optional)" : string.Empty)} {parameterInformation[i].Name}{description}" +
                        $"{(parameterInformation[i].IsOptional ? $" | Default: [{parameterInformation[i].DefaultValue}]" : string.Empty)}" +
                        $" | Type: [{(parameterInformation[i].ParameterType.IsEnum ? $"Enum ({enumInfo})" : parameterInformation[i].TryGetAttribute(out SuggestionAttribute suggestion) ? $"string > Suggestions: {suggestion.GetAllSuggestions()}" : parameterInformation[i].ParameterType.Name)}]";

                    Transmission.AddLine(message);
                    if (i == parameterInformation.Length - 1)
                        Transmission.AddBreak();
                }

                index++;
            }

            Transmission.ReleaseAsync();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [UTILS] ---
                
        public static bool WasLastInputValid()
            => cachedProposeReturnValue 
               && cachedInputValidation != InputValidation.Incomplete
               && cachedInputValidation != InputValidation.Incorrect;


        public static void SetConfiguration(ConsoleConfiguration configuration)
        {
            logCommandsLoadedOnStart = configuration.logCommandsLoadedOnStart;
            allowNumericBoolProcessing = configuration.allowNumericBoolProcessing;
            InfoOperator = configuration.infoOperator;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HELPER] ---

        private static string GetDeclaringKey(MemberInfo target)
        {
            if (target.GetCustomAttribute(typeof(DeclaringNameAttribute)) is DeclaringNameAttribute targetName)
                return targetName.DeclaringName;
            return $"{target.Name}";
        }
        
                
        private static string ReturnCommand(string input)
        {
            var split = input.Replace("/", string.Empty).Cut().Split(' ');
            return split[0];
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [EXTENSION] ---

        private static void RemoveSetterPrefix(this string input, out string[] target, char split = '.')
        {
            input = input.Cut().RemoveFormBeginning($"/set");
            target = input.Cut().Split(' ')[0].Split(split);
        }

        private static void SimpleTarget(this string input, out string[] target, char split = '.')
        {
            target = input.Cut().Split(split);
        }

        private static void ToCommandArgs(this string input, out string key, out List<string> args, char split = ' ')
        {
            input = input.Cut().Remove(0, CommandProcessor.Prefix.Length).Cut();
            
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

        private static string[] AsStructInputArgs(this string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                if (i < input.Length - 1)
                {
                    if (input[i] == ',' && input[i+1] == ' ')
                    {
                        input = input.Remove(i, 1);
                    }
                }
                else
                {
                    if (input[i] == ',')
                    {
                        input = input.Remove(i, 1);
                    }
                }
                
            }
            return Regex.Replace(input.Cut().Replace('.',','), "[^., 0-9]", "").Split(' ');
        }

        private static string AsVectorHint(this int value)
            => value == 0 ? "X" : value == 1 ? "Y" : value == 2 ? "Z" : "W";
        
        private static string AsColorHint(this int value)
            => value == 0 ? "R" : value == 1 ? "G" : value == 2 ? "B" : "A";
        

        #endregion
    }
}
