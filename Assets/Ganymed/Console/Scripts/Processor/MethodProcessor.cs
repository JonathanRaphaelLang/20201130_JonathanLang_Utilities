using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Core;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    public static class MethodProcessor
    {
        #region --- [FIELDS] ---

        private static readonly Dictionary<string, Command> Methods = new Dictionary<string, Command>(StringComparer.CurrentCultureIgnoreCase);
      
        private static InputValidation cachedInputValidation = InputValidation.None;
        private static bool cachedProposeReturnValue = default;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMAND & SIGNATURE] ---

        private class Command
        {
            public readonly string Key;
            public readonly List<Signature> Signatures = new List<Signature>();
            public int Priority { get; private set; }
            public readonly bool hasNativeAttribute;

            public Command(Signature signature, string key, bool hasNativeAttribute)
            {
                Key = key;
                this.hasNativeAttribute = hasNativeAttribute;
                Signatures.Add(signature);
                Priority = signature.priority;
            }

            public void AddOverload(Signature overload)
            {
                Signatures.Add(overload);
                if (overload.priority > Priority)
                    Priority = overload.priority;
            }
        }
        
        private class Signature
        {
            public readonly MethodInfo methodInfo;
            public readonly string description;
            public readonly bool disableNBP;
            public readonly int priority;

            public Signature(MethodInfo methodInfo, int priority, string description, bool disableNbp)
            {
                this.methodInfo = methodInfo;
                this.priority = priority;
                this.description = description;
                this.disableNBP = disableNbp;
            }
        }        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMANDS] ---

        [NativeCommand]
        [Command(CommandProcessor.CommandsKey, "Log available commands and their descriptions", 1000)]
        public static void LogMethodCommands()
        {
            Transmission.Start(TransmissionOptions.Enumeration);
            
            Transmission.AddLine(
                new Message($"[Key]", Core.Console.colorTitles, MessageOptions.Bold),
                new Message($"[Description]", Core.Console.colorTitles, MessageOptions.Bold),
                new Message($"[Priority]", Core.Console.colorTitles , MessageOptions.Bold),
                new Message($"[Overloads]", Core.Console.colorTitles, MessageOptions.Bold));
            Transmission.AddBreak();
            
            var cmdList = Methods.Select(cmd => cmd.Value).ToList();
            cmdList.Sort(delegate(Command a, Command b)
            {
                if (a.hasNativeAttribute && !b.hasNativeAttribute) return -1;
                else if (b.hasNativeAttribute && !a.hasNativeAttribute) return 1;
                else
                {
                    if (a.Priority > b.Priority) return -1;
                    else if (b.Priority > a.Priority) return 1;
                    else return 0;    
                }
            });

            foreach (var cmd in cmdList)
            {
                var count = cmd.Signatures.Count;
                for (byte i = 0; i < count; i++)
                {
                    if (count > 1)
                    {
                        Transmission.AddLine(
                            $"[{cmd.Key}]",
                            $"[{cmd.Signatures[i].description}]",
                            $"[{cmd.Signatures[i].priority}]",
                            new Message($"[{i}]",Core.Console.colorEmphasize));
                    }
                    else if(cmd.hasNativeAttribute)
                    {
                        Transmission.AddLine(
                            new Message($"[{cmd.Key}]", Core.Console.colorEmphasize),
                            new Message($"[{cmd.Signatures[i].description}]", Core.Console.colorEmphasize),
                            new Message($"[{cmd.Signatures[i].priority}]", Core.Console.colorEmphasize));
                    }
                    else
                    {
                        Transmission.AddLine(
                            $"[{cmd.Key}]",
                            $"[{cmd.Signatures[i].description}]",
                            $"[{cmd.Signatures[i].priority}]");
                    }
                }
            }

            Transmission.ReleaseAsync();
        }

        #endregion
        
        #region --- [COMMAND INFORMATION] ---

        private static void LogCommandInformation(string key)
        {
            key = key.Remove(key.Length-1, 1);
            
            if (!Methods.TryGetValue(key, out var command)) return;

            Transmission.Start();
            
            var index = 0;
            foreach (var signature in command.Signatures)
            {
                var msg = $"Key:[{key}]{(command.Signatures.Count > 1? $"[{index}]" : string.Empty)} | Description:[{signature.description}]";
                Transmission.AddLine(msg);
                
                var parameterInformation = signature.methodInfo.GetParameters();
                
                for (var i = 0; i < parameterInformation.Length; i++)
                {
                    var description = string.Empty;
                    if (parameterInformation[i].TryGetAttribute(out Hint spec))
                    {
                        description = $" | Description: {'"'}{spec.description}{'"'}";
                    }

                    #region --- [ENUM] ---

                    var enumInfo = string.Empty;
                    if (parameterInformation[i].ParameterType.IsEnum)
                    {
                        var values = Enum.GetValues(parameterInformation[i].ParameterType);
                        foreach (var value in values)
                        {
                            enumInfo += $"[{value}]"; 
                        }   
                    }                    

                    #endregion
                   
                    var message =
                        $"Parameter [{i}]:{(parameterInformation[i].IsOptional? " (<color=#CCC>optional)" : string.Empty)} {parameterInformation[i].Name}{description}" +
                        $"{(parameterInformation[i].IsOptional? $" | Default: [{parameterInformation[i].DefaultValue}]" : string.Empty)}" +
                        $" | Type: [{(parameterInformation[i].ParameterType.IsEnum? $"Enum ({enumInfo})": parameterInformation[i].TryGetAttribute(out SuggestionAttribute suggestion)? $"string > Suggestions: {suggestion.GetAllSuggestions()}" :  parameterInformation[i].ParameterType.Name)}]";

                    Transmission.AddLine(message);
                    if(i == parameterInformation.Length - 1)
                        Transmission.AddBreak();
                }

                index++;
            }
            
            Transmission.ReleaseAsync();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMAND GENERATION (RuntimeInitializeOnLoadMethod)] ---
        
        internal static Task FindMethodCommandsInAssembly()
        {
            var time = Time.realtimeSinceStartup;
            var types =  System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(object));

            foreach (var type in types)
            {
                foreach (var methodInfo in type.GetMethods(CommandProcessor.MethodFlags))
                {
                    var attribute = methodInfo.GetCustomAttribute(typeof(Attributes.CommandAttribute)) as Attributes.CommandAttribute;
                    var isNative = methodInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) is NativeCommandAttribute;
                
                    if (attribute == null) continue;
                    var key = attribute.key;
                  
                    var signature = new Signature(methodInfo, attribute.priority, attribute.description, attribute.disableNBP);
                    if (!Methods.ContainsKey(key))
                    {
                        Methods.Add(key, new Command(signature, key, isNative));
                    }
                    else
                    {
                        Methods[key].AddOverload(signature);
                    }
                }
            }

            var passed = Time.realtimeSinceStartup - time;

            if (CommandProcessor.logCommandsLoadedOnStart)
            {
                const MessageOptions options = MessageOptions.Brackets | MessageOptions.Bold;
                
                Transmission.Start(TransmissionOptions.None, "Command Handler");
                Transmission.AddLine(
                    new Message($"{Methods.Count}", Core.Console.colorEmphasize, options),
                    $"Commands are loaded | Time passed: {passed:0.0000}s | ",
                    new Message($"{CommandProcessor.Prefix}{CommandProcessor.CommandsKey}", Core.Console.colorEmphasize, options),
                    new Message(" To receive a list of commands"));
                Transmission.Release();
            }
            
            return Task.CompletedTask;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMAND PROCCESSING] ---

        public static void ProcessCommand(string input)
        {
            if (!input.StartsWith(CommandProcessor.Prefix)) return;
            
            input = input.Remove(0, CommandProcessor.Prefix.Length);
            
            input.ToCommandArgs(out var command, out var args);
            
            if (command.EndsWith(CommandProcessor.InfoOperator))
            {
                LogCommandInformation(command);
                return;
            }
            
            ProcessCommand(command, args);
        }

        
        // --- ProcessCommand variables 
        private static ParameterInfo[] parameterInfos;
        private static object[] parameters;
        
        private static void ProcessCommand(string commandKey, IList<string> args)
        {
            if (!Methods.TryGetValue(commandKey, out var command)) return;

            var viewedSignature = 0;
            foreach (var signature in command.Signatures)
            {
                viewedSignature++;
                parameterInfos = signature.methodInfo.GetParameters();
                parameters = new object[signature.methodInfo.GetParameters().Length];

                var checkNextSignature = true;
                
                for (var i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        #region --- [FLOAT] ---

                        //  --- Replace . with , to ensure that the value is interpreted correctly
                        if (parameterInfos[i].ParameterType == typeof(float))
                        {
                            args[i] = args[i].Replace('.', ',');
                            parameters[i] = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                        }

                        #endregion
                        
                        #region --- [STRING] ---

                        //  --- Replace . with , to ensure that the value is interpreted correctly
                        if (parameterInfos[i].ParameterType == typeof(string))
                        {
                            args[i] = args[i].Replace('"'.ToString(), string.Empty);
                            parameters[i] = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                        }

                        #endregion

                        #region --- [ENUM] ---

                        else if (parameterInfos[i].ParameterType.IsEnum)
                        {
                            var enumValues = Enum.GetValues(parameterInfos[i].ParameterType);
                            var converted = false;
                        
                            // --- Check if args (string) equals parameters enum value
                            foreach (var enumValue in enumValues)
                            {
                                if (string.Equals(enumValue.ToString(), args[i], StringComparison.CurrentCultureIgnoreCase))
                                {
                                    parameters[i] = enumValue;
                                    converted = true;
                                    break;
                                }
                            }

                            // --- Check if args (string) as int equals parameters enum value
                            if (!converted && int.TryParse(args[i], out var parsed))
                            {
                                foreach (var enumValue in enumValues)
                                {
                                    if (parsed == (int) enumValue)
                                    {
                                        parameters[i] = enumValue;
                                        converted = true;
                                        break;
                                    }
                                }
                            }
                        
                            if (!converted) throw new Exception();
                        }

                        #endregion

                        #region --- [BOOLEAN] ---

                        else if (parameterInfos[i].ParameterType == typeof(bool) && CommandProcessor.allowNumericBoolProcessing && !signature.disableNBP)
                        {
                            if (int.TryParse(args[i], out var resultBool))
                            {
                                args[i] = resultBool == 0 ? "false" : resultBool == 1? "true" : throw new Exception();
                            }
                            parameters[i] = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                        }

                        #endregion
                        
                        //----------------------------------------------------------------------------------------------
                        
                        #region --- [VECTOR2] ---
                            
                        else if (parameterInfos[i].ParameterType == typeof(Vector2))
                        {
                            var vectorArgsString = new List<string> {args[i]};

                            if (i < args.Count - 1) {
                                vectorArgsString.Add(args[i + 1]);
                                args.RemoveAt(i + 1);
                            }
                               
                            if (vectorArgsString.Count != 2) continue;
                            var XY = new float[2]; 
                            for (var j = 0; j < vectorArgsString.Count; j++)
                            {
                                vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                if(float.TryParse(vectorArgsString[j], out var f))
                                {
                                    XY[j] = f;
                                }
                            }
                            parameters[i] = new Vector2(XY[0],XY[1]);
                        }

                        #endregion

                        #region --- [VECTOR3] ---

                        else if (parameterInfos[i].ParameterType == typeof(Vector3))
                        {
                            var vectorArgsString = new List<string> {args[i]};

                            if (i < args.Count - 2) {
                                vectorArgsString.Add(args[i + 1]);
                                vectorArgsString.Add(args[i + 2]);
                                args.RemoveAt(i + 1);
                                args.RemoveAt(i + 1);
                            }
                            else if (i < args.Count - 1) {
                                vectorArgsString.Add(args[i + 1]);
                                args.RemoveAt(i + 1);
                            }

                            
                            if (vectorArgsString.Count != 3) continue;
                            var XYZ = new float[3]; 
                            for (var j = 0; j < vectorArgsString.Count; j++)
                            {
                                if(float.TryParse(vectorArgsString[j], out var result))
                                {
                                    XYZ[j] = result;
                                }
                            }    
                            parameters[i] = new Vector3(XYZ[0],XYZ[1],XYZ[2]);
                        }

                        #endregion

                        #region --- [VECTOR4] ---
                            
                            else if (parameterInfos[i].ParameterType == typeof(Vector4))
                            {
                                var vectorArgsString = new List<string> {args[i]};
                                
                                if (i < args.Count - 3) {
                                    vectorArgsString.Add(args[i + 1]);
                                    vectorArgsString.Add(args[i + 2]);
                                    vectorArgsString.Add(args[i + 3]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 2) {
                                    vectorArgsString.Add(args[i + 1]);
                                    vectorArgsString.Add(args[i + 2]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 1) {
                                    vectorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                }
                                
                                if (vectorArgsString.Count != 4) continue;
                                var XYZW = new float[4]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    if(float.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZW[j] = f;
                                    }
                                }
                                parameters[i] = new Vector4(XYZW[0],XYZW[1],XYZW[2],XYZW[3]);
                            }
                            
                            #endregion

                        //----------------------------------------------------------------------------------------------
                        
                        #region --- [COLOR] ---

                        else if (parameterInfos[i].ParameterType == typeof(Color))
                        {
                            var colorArgsString = new List<string> {args[i]};

                            if (i < args.Count - 3)
                            {
                                colorArgsString.Add(args[i + 1]);
                                colorArgsString.Add(args[i + 2]);
                                colorArgsString.Add(args[i + 3]);
                                args.RemoveAt(i + 1);
                                args.RemoveAt(i + 1);
                                args.RemoveAt(i + 1);
                            }
                            else if (i < args.Count - 2)
                            {
                                colorArgsString.Add(args[i + 1]);
                                colorArgsString.Add(args[i + 2]);
                                args.RemoveAt(i + 1);
                                args.RemoveAt(i + 1);
                            }
                            else if (i < args.Count - 1)
                            {
                                colorArgsString.Add(args[i + 1]);
                                args.RemoveAt(i + 1);
                            }

                            // --- Try to convert 

                            if (colorArgsString.Count != 4) continue;
                            var RGBA = new float[4];

                            for (var j = 0; j < colorArgsString.Count; j++)
                            {
                                colorArgsString[j] = colorArgsString[j].Replace('.', ',');
                                colorArgsString[j] = colorArgsString[j].Replace("(", string.Empty);
                                colorArgsString[j] = colorArgsString[j].Replace(")", string.Empty);
                                if (float.TryParse(colorArgsString[j], out var f))
                                {
                                    RGBA[j] = f;
                                }
                            }

                            parameters[i] = new Color(RGBA[0], RGBA[1], RGBA[2], RGBA[3]);
                        }

                        #endregion
                        
                        #region --- [COLOR32] ---

                            else if (parameterInfos[i].ParameterType == typeof(Color32))
                            {
                                var colorArgsString = new List<string> {args[i]};
                                
                                if (i < args.Count - 3) {
                                    colorArgsString.Add(args[i + 1]);
                                    colorArgsString.Add(args[i + 2]);
                                    colorArgsString.Add(args[i + 3]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 2) {
                                    colorArgsString.Add(args[i + 1]);
                                    colorArgsString.Add(args[i + 2]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 1) {
                                    colorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                }
                                
                                // --- Try to convert 
                                
                                if (colorArgsString.Count != 4) continue;
                                var RGBA = new byte[4]; 
                                
                                for (var j = 0; j < colorArgsString.Count; j++)
                                {
                                    colorArgsString[j] = colorArgsString[j].Replace('.', ',');
                                    colorArgsString[j] = colorArgsString[j].Replace("(", string.Empty);
                                    colorArgsString[j] = colorArgsString[j].Replace(")", string.Empty);
                                    if(byte.TryParse(colorArgsString[j], out var f))
                                    {
                                        RGBA[j] = f;
                                    }
                                }
                                parameters[i] = new Color32(RGBA[0],RGBA[1],RGBA[2],RGBA[3]);
                            }

                            #endregion
                        
                        //----------------------------------------------------------------------------------------------

                        else // --- If no special case applies try to convert it "blind"
                        {
                            parameters[i] = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                        }
                    }
                    catch
                    {
                        // --- Use the default value of parameter not the default value of type
                        if (parameterInfos[i].IsOptional)
                            parameters[i] = parameterInfos[i].DefaultValue;
                        
                        checkNextSignature = viewedSignature == command.Signatures.Count; 
                    }
                }

                if (!checkNextSignature) continue;
                //  --- Check if the last parameter has params attribute and create a params array... 
                if(parameterInfos.Length > 0)
                    if (parameterInfos[parameterInfos.Length -1].isParams())
                    {
                        Debug.Log("Last parameter has params attribute");
                        //TODO: create params array
                    }

                #region --- [INVOKE] ---

                //  --- Invoke the commands method
                if (signature.methodInfo.IsStatic && parameters.Length > 0)
                {
                    signature.methodInfo.Invoke(null, parameters);
                    return;
                }
                //  --- Invoke without any parameters if the no arguments are given 
                else 
                {
                    signature.methodInfo.Invoke(null, null);
                    return;
                }

                #endregion
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMAND PRE PROCESSING / PROPOSAL & INPUT VALIDATION] ---
        
        public static bool IsValid(string input)
            => cachedProposeReturnValue 
               && cachedInputValidation != InputValidation.Incomplete
               && cachedInputValidation != InputValidation.Incorrect;
        
        
        /// <summary>
        /// Returns true if the given input can be converted into a command
        /// or if a suggestion for completing the input could be found.
        /// </summary>
        /// <param name="input">The input string received by the console.</param>
        /// <param name="description">The proposed description of the current input or or the next parameter.</param>
        /// <param name="proposedInput"></param>
        /// <param name="inputValidation">The state of the input (valid, incomplete, etc.)</param>
        /// <returns></returns>
        public static bool Propose(string input, out string description, out string proposedInput, out InputValidation inputValidation)
        {
            #region --- [OUT PARAMETER SETUP] ---

            // --- The original input string.
            var rawInput = input;
            
            // --- Format input string and get command key and args
            input = input.Cut();
            input = input.Remove(0, CommandProcessor.Prefix.Length);
            input.ToCommandArgs(out var key, out var args);

            if (key.Equals(CommandProcessor.GetKey, StringComparison.OrdinalIgnoreCase))
            {
                if (GetterProcessor.Propose(rawInput, out description, out proposedInput, out inputValidation))
                {
                    cachedInputValidation = inputValidation;
                    cachedProposeReturnValue = true;
                    return true;
                }
                else
                {
                    cachedInputValidation = inputValidation;
                    cachedProposeReturnValue = false;
                    return false;
                }
            }
            if (key.Equals(CommandProcessor.SetKey, StringComparison.OrdinalIgnoreCase))
            {
                return SetterProcessor.Propose(rawInput, out description, out proposedInput, out inputValidation);
            }
            
            
            // --- Declare proposal strings
            description = rawInput;
            proposedInput = rawInput;
            inputValidation = InputValidation.Optional;
            
            // --- Return if there is no key
            if (key.Length <= 0)  return false;
            if (key.EndsWith(CommandProcessor.InfoOperator))
            {
                if (Methods.ContainsKey(key.Remove(key.Length - 1, 1)))
                    inputValidation = InputValidation.CommandInfo;

                description += $" // Receive information about the command.";
                return true;
            }
            
            // --- Save upper lower key configurations
            var keyConfiguration = key.GetUpperLowerConfig();
            var isStringEven = rawInput.Count(x => x == '"') % 2 == 0;
            
            // --- Are input & key a perfect match
            var isKeyPerfectMatch = Methods.ContainsKey(key);
            
            // --- Initialise proposal strings
            var proposalDescription = string.Empty;
            var proposal = string.Empty;
            
            #endregion

            #region --- [PRIO] ---
            
            string bestKey = null;
            Command bestMatch = null;
            var bestPriority = 0;

            if (!isKeyPerfectMatch)
            {
                //--- Iterate through every console command in dictionary to check for priorities
                foreach (var consoleCommand in Methods)
                {
                    if (!consoleCommand.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)) continue;
                    foreach (var signature in consoleCommand.Value.Signatures.Where(s => s.priority > bestPriority))
                    {
                        bestKey = consoleCommand.Key;
                        bestMatch = consoleCommand.Value;
                        bestPriority = signature.priority;
                    }
                }
            
                if (bestMatch != null)
                {
                    proposalDescription = bestKey.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);
                    proposal = bestKey.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);
                    
                    description += proposalDescription;
                    proposedInput += proposal;
                    inputValidation = InputValidation.Incomplete;
                    cachedProposeReturnValue = true;
                    return true;
                }    
            }

            #endregion
            
            //--- Iterate through every console command in dictionary to check for a potential match (key)
            foreach (var consoleCommand in Methods)
            {
                #region --- [SEARCH FOR MATCH] ---

                // --- Check every command for potential match
                if(isKeyPerfectMatch && !consoleCommand.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
                if (!consoleCommand.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)) continue;

                #endregion
              
                #region --- [SETUP IF POTENTIAL MATCH WAS FOUND] ---

                // --- Multi argument parameter information
                var isMultiArgsParam = false;

                // --- If there is no perfect match we can return and ignore signatures & parameter
                if (!isKeyPerfectMatch)
                {
                    proposalDescription = consoleCommand.Key.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);
                    proposal = consoleCommand.Key.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);
                    
                    description += proposalDescription;
                    proposedInput += proposal;
                    inputValidation = InputValidation.Incomplete;
                    cachedInputValidation = inputValidation;
                    cachedProposeReturnValue = true;
                    return true;
                }
                
                #endregion

                // --- Index + 1 of the inspected signature
                var viewedSignature = 0;
                
                // --- Iterate thought every signature and check if it will work
                foreach (var signature in consoleCommand.Value.Signatures)
                {
                    viewedSignature++;
                    var parameterInformation = signature.methodInfo.GetParameters();
                    var checkNextSignature = false;
                    var isValid = viewedSignature <= 1;
                    
                    // --- Iterate thought every parameter
                    for (var i = 0; i < parameterInformation.Length; i++)
                    {
                        // --- If the following code throws, we know that the current input cannot be converted.
                        try 
                        {
                            #region --- [DISPLAY INFORMATION ABOUT NEXT PARAMETER] ---

                            // --- Check if there are args left to enter & autocomplete
                            if (args.Count < parameterInformation.Length && isKeyPerfectMatch || isMultiArgsParam && isKeyPerfectMatch)
                            {
                                // --- Check if args are string with more then one word and therefore should end with " 
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                
                                // if the next element is a parameter
                                if (i == args.Count && rawInput.EndsWith(ends) && isStringEven && !isMultiArgsParam)
                                {
                                    #region --- [PROPOSE] ---
                                    
                                    proposalDescription = parameterInformation[i].TryGetAttribute(out Hint hint)
                                        // --- Hint attribute was found
                                        ? $" //" +
                                          $"{(hint.showDefaultValue && parameterInformation[i].HasDefaultValue && parameterInformation[i].DefaultValue != null ? $" [default: {parameterInformation[i].DefaultValue}]" : string.Empty)}" +
                                          $"{(hint.showValueType ? $" [type: {(parameterInformation[i].ParameterType.IsEnum ? "Enum" : parameterInformation[i].ParameterType.Name)}]" : string.Empty)}" +
                                          $"{(hint.showParameterName ? $" [param name: {parameterInformation[i].Name}]" : string.Empty)}" +
                                          $" {hint?.description}"
                                        // --- Hint attribute was not found
                                        : $" //[param name: {parameterInformation[i].Name}] [type: {(parameterInformation[i].ParameterType.IsEnum ? "Enum" : parameterInformation[i].ParameterType.Name)}]";

                                    #endregion
                                    
                                    // --- Try to get the default value of the parameter
                                    try 
                                    {
                                        if (parameterInformation[i].TryGetAttribute(out SuggestionAttribute suggestionAttribute))
                                        {
                                            proposal = $"{'"'}{suggestionAttribute.Suggestions[0]}{'"'}";
                                        }
                                        else
                                        {
                                            var defaultValue = parameterInformation[i].DefaultValue;
                                            if (defaultValue != null)
                                                proposal = parameterInformation[i].HasDefaultValue
                                                    ? defaultValue.ToString()
                                                    : parameterInformation[i].ParameterType.GetDefault().ToString();

                                            proposal = proposal.Replace("RGBA", string.Empty);
                                            proposal = proposal.Replace(",", string.Empty);
                                            proposal = proposal.Replace("(", string.Empty);
                                            proposal = proposal.Replace(")", string.Empty);
                                            proposal = proposal.Replace('.', ',');    
                                        }
                                    }
                                    catch
                                    {
                                        // ignored --- there is no default value
                                    }
                                }
                            }

                            #endregion

                            #region --- [TRY TO CONVERT INPUT INTO PARAMETER TYPE] ---
                            
                            isMultiArgsParam = false;
                            
                            if (i > args.Count - 1) continue; // --- We continue because there are no args at this point

                            #region --- [BOOLEAN] ---

                            if (parameterInformation[i].ParameterType == typeof(bool))
                            {
                                // --- Check if we have integer input that can be converted into true or false
                                if (int.TryParse(args[i], out var resultBool))
                                {
                                    args[i] = resultBool > 0 ? "true" : "false";
                                    proposal = resultBool > 0 ? "true" : "false";
                                }
                                
                                // ---Check if args could result in "true" or "false"
                                else
                                {
                                    // ---Check if args could result in "true"
                                    if ("true".StartsWith(args[i], StringComparison.OrdinalIgnoreCase))
                                    {
                                        proposalDescription = "true".Remove(0, args[i].Length);
                                        proposal = proposalDescription;

                                        if (string.Equals(args[i], "true", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if(inputValidation != InputValidation.Optional)
                                                inputValidation = InputValidation.Valid;
                                        }
                                        else
                                        {
                                            if(inputValidation != InputValidation.Optional)
                                                inputValidation = InputValidation.Incomplete;
                                            description += proposalDescription;
                                            proposedInput += proposal;
                                            cachedInputValidation = inputValidation;
                                            cachedProposeReturnValue = true;
                                            return true;
                                        }
                                    }

                                    // ---Check if args could result in "false"
                                    if ("false".StartsWith(args[i], StringComparison.OrdinalIgnoreCase))
                                    {
                                        proposalDescription = "false".Remove(0, args[i].Length);
                                        proposal = proposalDescription;
                                            
                                        if (string.Equals(args[i], "false", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Valid;
                                        }
                                        else
                                        {
                                            if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incomplete;
                                            description += proposalDescription;
                                            proposedInput += proposal;
                                            return true;
                                        }
                                    }
                                }
                                
                                // --- Check if input can be converted 
                                var obj = Convert.ChangeType(args[i], parameterInformation[i].ParameterType);
                            }

                            #endregion

                            #region --- [STRING] ---

                            if (parameterInformation[i].TryGetAttribute(out SuggestionAttribute suggestion))
                            {
                                foreach (var sug in suggestion.Suggestions)
                                {
                                    if (!sug.StartsWith(args[i],
                                        suggestion.IgnoreCase
                                            ? StringComparison.OrdinalIgnoreCase
                                            : StringComparison.CurrentCulture)) continue;
                                    
                                    proposalDescription = sug.Remove(0, args[i].Length);
                                    proposal = proposalDescription;
                                
                                    if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incomplete;
                                    description += proposalDescription;
                                    proposedInput = proposedInput.Remove(proposedInput.Length - args[i].Length, args[i].Length);
                                    proposedInput += $"{'"'}{sug}{'"'}";
                                    return true;
                                }
                            }

                            #endregion
                            
                            #region --- [ENUM] ---

                            else if (parameterInformation[i].ParameterType.IsEnum)
                            {
                                var values = Enum.GetValues(parameterInformation[i].ParameterType);

                                foreach (var value in values)
                                {
                                    if (!value.ToString().StartsWith(args[i], StringComparison.OrdinalIgnoreCase))
                                        continue; // --- Continue until match was found. Then execute the logic...
                                    proposalDescription = value.ToString().Remove(0, args[i].Length);
                                    proposal = proposalDescription;
                                    isValid = true;
                                }
                            }

                            #endregion
                            
                            //------------------------------------------------------------------------------------------

                            #region --- [VECTOR2] ---
                            
                            else if (parameterInformation[i].ParameterType == typeof(Vector2))
                            {
                                isMultiArgsParam = true;
                                var vectorArgsString = new List<string> {args[i]};

                                if (i < args.Count - 1) {
                                    vectorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                
                                var XY = new float[2]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    if (vectorArgsString.Count < j)
                                        break;
                                        
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(float.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XY[j] = f;
                                    }
                                    else
                                    {
                                        throw CommandProcessor.mdaException;
                                    }
                                }

                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{vectorArgsString.Count.AsVectorHint()}";
                            }

                            #endregion
                            
                            #region --- [VECTOR3] ---
                            
                            else if (parameterInformation[i].ParameterType == typeof(Vector3))
                            {
                                isMultiArgsParam = true;
                                var vectorArgsString = new List<string> {args[i]};
                                
                                if (i < args.Count - 2) {
                                    vectorArgsString.Add(args[i + 1]);
                                    vectorArgsString.Add(args[i + 2]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                else if (i < args.Count - 1) {
                                    vectorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                }
                                
                                var XYZ = new float[3]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    if (vectorArgsString.Count < j)
                                        break;
                                        
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(float.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZ[j] = f;
                                    }
                                    else
                                    {
                                        throw CommandProcessor.mdaException;
                                    }
                                }
                                
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{vectorArgsString.Count.AsVectorHint()}";
                            }

                            #endregion

                            #region --- [VECTOR4] ---
                            
                            else if (parameterInformation[i].ParameterType == typeof(Vector4))
                            {
                                isMultiArgsParam = true;
                                var vectorArgsString = new List<string> {args[i]};
                                
                                if (i < args.Count - 3) {
                                    vectorArgsString.Add(args[i + 1]);
                                    vectorArgsString.Add(args[i + 2]);
                                    vectorArgsString.Add(args[i + 3]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                else if (i < args.Count - 2) {
                                    vectorArgsString.Add(args[i + 1]);
                                    vectorArgsString.Add(args[i + 2]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 1) {
                                    vectorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                }
                                
                                var XYZW = new float[4]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    if (vectorArgsString.Count < j)
                                        break;
                                        
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(float.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZW[j] = f;
                                    }
                                    else
                                    {
                                        throw CommandProcessor.mdaException;
                                    }
                                }
                                
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{vectorArgsString.Count.AsVectorHint()}";
                            }
                            
                            #endregion
                            
                            //------------------------------------------------------------------------------------------

                            #region --- [COLOR] ---

                            else if (parameterInformation[i].ParameterType == typeof(Color))
                            {
                                isMultiArgsParam = true;
                                
                                var colorArgsString = new List<string> {args[i]};
                                
                                if (i < args.Count - 3) {
                                    colorArgsString.Add(args[i + 1]);
                                    colorArgsString.Add(args[i + 2]);
                                    colorArgsString.Add(args[i + 3]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                else if (i < args.Count - 2) {
                                    colorArgsString.Add(args[i + 1]);
                                    colorArgsString.Add(args[i + 2]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 1) {
                                    colorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                }
                                
                                // --- Try to convert 
                                var RGBA = new float[4]; 
                                for (var j = 0; j < colorArgsString.Count; j++)
                                {
                                    if (colorArgsString.Count < j)
                                        break;
                                    
                                    colorArgsString[j] = colorArgsString[j].Replace('.', ',');
                                    colorArgsString[j] = colorArgsString[j].Replace("(", string.Empty);
                                    colorArgsString[j] = colorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(float.TryParse(colorArgsString[j], out var f))
                                    {
                                        RGBA[j] = f;
                                    }
                                    else
                                    {
                                        throw CommandProcessor.mdaException;
                                    }
                                }
                                
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{colorArgsString.Count.AsColorHint()}";
                            }

                            #endregion
                            
                            #region --- [COLOR32] ---

                            else if (parameterInformation[i].ParameterType == typeof(Color32))
                            {
                                isMultiArgsParam = true;
                                var colorArgsString = new List<string> {args[i]};
                                
                                if (i < args.Count - 3) {
                                    colorArgsString.Add(args[i + 1]);
                                    colorArgsString.Add(args[i + 2]);
                                    colorArgsString.Add(args[i + 3]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                else if (i < args.Count - 2) {
                                    colorArgsString.Add(args[i + 1]);
                                    colorArgsString.Add(args[i + 2]);
                                    args.RemoveAt(i + 1);
                                    args.RemoveAt(i + 1);
                                }
                                else if (i < args.Count - 1) {
                                    colorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                }
                                
                                
                                var RGBA = new byte[4]; 
                                for (var j = 0; j < colorArgsString.Count; j++)
                                {
                                    if (colorArgsString.Count < j)
                                        break;
                                    
                                    colorArgsString[j] = colorArgsString[j].Replace('.', ',');
                                    colorArgsString[j] = colorArgsString[j].Replace("(", string.Empty);
                                    colorArgsString[j] = colorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(byte.TryParse(colorArgsString[j], out var b))
                                    {
                                        RGBA[j] = b;
                                    }
                                    else
                                    {
                                        throw CommandProcessor.mdaException;
                                    }
                                }
                                
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{colorArgsString.Count.AsColorHint()}";
                            }

                            #endregion
                            
                            //------------------------------------------------------------------------------------------
                            
                            #region --- [OTHER & UNKNOWN TYPES] ---

                            // --- Convert args to param type. This could throw an Exception
                            else
                            {
                                var obj = Convert.ChangeType(args[i], parameterInformation[i].ParameterType);
                                // --- if conversion does not throw it is valid 
                                isValid = true;
                            }
                            #endregion

                            #endregion
                        }
                    
                        // --- Catch Exception thrown by convention from args to param type  
                        catch (Exception exc)
                        {
                            if (exc == CommandProcessor.mdaException)
                            {
                                if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incorrect;
                                cachedInputValidation = inputValidation;
                                cachedProposeReturnValue = false;
                                return false;
                            }
                                
                            checkNextSignature = viewedSignature != consoleCommand.Value.Signatures.Count;
                            if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incorrect;
                            break;
                        }
                    }
                    
                    // if an exception was thrown before we've checked the last signature continue
                    if (checkNextSignature) {
                        continue;
                    }

                    
                    if (args.Count == parameterInformation.Length && isKeyPerfectMatch && isValid)
                    {
                        inputValidation = InputValidation.Valid;
                        description += proposalDescription;
                        proposedInput += proposal;
                        cachedInputValidation = inputValidation;
                        cachedProposeReturnValue = true;
                        return true;
                    }

                    if (args.Count < parameterInformation.Length && isKeyPerfectMatch && isValid)
                    {
                        if(inputValidation != InputValidation.Optional) inputValidation = parameterInformation[args.Count].IsOptional
                            ? InputValidation.Optional
                            : InputValidation.Incomplete;
                        description += proposalDescription;
                        proposedInput += proposal;
                        cachedInputValidation = inputValidation;
                        cachedProposeReturnValue = true;
                        return true;
                    }
                    
                    else
                    {
                        if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incorrect;
                        cachedInputValidation = inputValidation;
                        cachedProposeReturnValue = false;
                        return false;
                    }
                }
            }

            // --- Return false because we don't have a proposal
            if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incorrect;
            cachedInputValidation = inputValidation;
            cachedProposeReturnValue = false;
            return false;
        }

        #endregion

    }
}
