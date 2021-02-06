using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Utils;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Structures;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    /// <summary>
    /// Partial CommandProcessor class responsible for handling of console command methods. 
    /// </summary>
    public static partial class CommandProcessor
    {
        #region --- [INITIALISATION] ---

        private static void FindMethodConsoleCommandsInAssembly(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                foreach (var methodInfo in type.GetMethods(CommandFlags))
                {
                    // --- Check for Command Attributes...
                    var attribute = methodInfo.GetCustomAttribute(typeof(ConsoleCommandAttribute)) as ConsoleCommandAttribute;
                    if (attribute == null) continue;
                    if (attribute.BuildSettings.HasFlag(CmdBuildSettings.ExcludeFromBuild) && !Application.isEditor)continue;
                    
                    var isNative = methodInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) is NativeCommandAttribute;
                    var key = attribute.Key;
                  
                    // --- create a new Signature for the found method...
                    var signature = new Signature(
                        methodInfo: methodInfo,
                        priority: attribute.Priority,
                        hiddenPriority: isNative? attribute.Priority + short.MaxValue : attribute.Priority,
                        description: attribute.Description,
                        disableNBP: attribute.DisableNBP,
                        disableListings: attribute.BuildSettings.HasFlag(CmdBuildSettings.DisableListings),
                        disableAutoCompletion: attribute.BuildSettings.HasFlag(CmdBuildSettings.DisableAutoCompletion));
                    
                    if (!MethodCommands.ContainsKey(key))
                    {
                        // --- if the method is the only signature (for now) 
                        MethodCommands.Add(key, new Command(signature, key, isNative));
                    }
                    else
                    {
                        // --- if a method with the same key already exists add an overload to the command. 
                        MethodCommands[key].AddOverload(signature);
                    }
                }
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMANDS] ---

        [NativeCommand]
        [ConsoleCommand(CommandsKey, Priority = 1000, Description = "Log available commands and their descriptions")]
        private static void LogCommands()
        {
            Transmission.Start(TransmissionOptions.Enumeration);
            Transmission.AddBreak();

            var commands = MethodCommands.Select(cmd => cmd.Value).ToList();

            commands.Sort(delegate(Command a, Command b)
            {
                if (a.hasNativeAttribute && !b.hasNativeAttribute) return -1;

                if (b.hasNativeAttribute && !a.hasNativeAttribute) return 1;

                if (a.Priority > b.Priority) return -1;
                if (b.Priority > a.Priority) return 1;
                return 0;
            });


            if (commands.Any(command => command.hasNativeAttribute))
            {
                Transmission.AddTitle("Native Commands");
                Transmission.AddLine(
                    new MessageFormat("Key", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Priority", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Signature", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Description", Core.Console.ColorTitleSub, MessageOptions.Brackets));
                Transmission.AddBreak();
            }
            
            
            var nativeCount = commands.Count(command => command.hasNativeAttribute);

            for (var j = 0; j < commands.Count; j++)
            {
                var count = commands[j].Signatures.Count;
                for (byte i = 0; i < count; i++)
                    if (count > 1)
                        Transmission.AddLine(
                            new MessageFormat($"{commands[j].Key}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{commands[j].Signatures[i].priority}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{i}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{commands[j].Signatures[i].description}",
                                MessageOptions.Brackets));
                    
                    else if (commands[j].hasNativeAttribute)
                        Transmission.AddLine(
                            new MessageFormat($"{commands[j].Key}", Core.Console.ColorEmphasize,
                                MessageOptions.Brackets),
                            new MessageFormat($"{commands[j].Signatures[i].priority}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{i}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{commands[j].Signatures[i].description}",
                                MessageOptions.Brackets));
                    
                    else if(!commands[j].Signatures[i].disableListings || Application.isEditor)
                        Transmission.AddLine(
                            new MessageFormat($"{commands[j].Key}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{commands[j].Signatures[i].priority}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{i}",
                                MessageOptions.Brackets),
                            new MessageFormat($"{commands[j].Signatures[i].description}",
                                MessageOptions.Brackets));

                if (j == nativeCount - 1)
                {
                    Transmission.AddBreak();
                    Transmission.AddTitle("Commands");
                    Transmission.AddLine(
                        new MessageFormat("Key", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                        new MessageFormat("Priority", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                        new MessageFormat("Signature", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                        new MessageFormat("Description", Core.Console.ColorTitleSub, MessageOptions.Brackets));
                    Transmission.AddBreak();
                }
            }

            Transmission.ReleaseAsync();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMAND PROCCESSING] ---
        
        // --- ProcessCommand variables 
        private static ParameterInfo[] parameterInfos;
        private static object[] parameters;
        
        internal static void ProcessMethodCommand(string input)
        {
            input.ToCommandArgs(out var commandKey, out var args);
            
            if (!MethodCommands.TryGetValue(commandKey, out var command)) return;

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

                        else if (parameterInfos[i].ParameterType == typeof(string))
                        {
                            //TODO: FIX
                            if (parameterInfos.Indices() == i && args.Count > parameterInfos.Length)
                            {
                                for (var j = i + 1; j < args.Count; j++)
                                {
                                    args[i] = $"{args[i]} {args[j]}";
                                }
                            }
                                
                            
                            args[i] = args[i].Replace('"'.ToString(), string.Empty);
                            parameters[i] = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                        }

                        #endregion

                        #region --- [CHAR] ---

                        //
                        else if (parameterInfos[i].ParameterType == typeof(char))
                        {
                            args[i] = args[i][0].ToString();
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
                                vectorArgsString[j] = Regex.Replace(vectorArgsString[j].Replace('.',','), "[^., 0-9]", "");
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
                                vectorArgsString[j] = Regex.Replace(vectorArgsString[j].Replace('.',','), "[^., 0-9]", "");
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
                                    vectorArgsString[j] = Regex.Replace(vectorArgsString[j].Replace('.',','), "[^., 0-9]", "");
                                    if(float.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZW[j] = f;
                                    }
                                }
                                parameters[i] = new Vector4(XYZW[0],XYZW[1],XYZW[2],XYZW[3]);
                            }
                            
                            #endregion

                        //----------------------------------------------------------------------------------------------
                        
                        #region --- [VECTOR2INT] ---
                            
                        else if (parameterInfos[i].ParameterType == typeof(Vector2Int))
                        {
                            var vectorArgsString = new List<string> {args[i]};

                            if (i < args.Count - 1) {
                                vectorArgsString.Add(args[i + 1]);
                                args.RemoveAt(i + 1);
                            }
                               
                            if (vectorArgsString.Count != 2) continue;
                            var XY = new int[2]; 
                            for (var j = 0; j < vectorArgsString.Count; j++)
                            {
                                vectorArgsString[j] = Regex.Replace(vectorArgsString[j].Replace('.',','), "[^., 0-9]", "");
                                if(int.TryParse(vectorArgsString[j], out var f))
                                {
                                    XY[j] = f;
                                }
                            }
                            parameters[i] = new Vector2Int(XY[0],XY[1]);
                        }

                        #endregion

                        #region --- [VECTOR3INT] ---

                        else if (parameterInfos[i].ParameterType == typeof(Vector3Int))
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
                            var XYZ = new int[3]; 
                            for (var j = 0; j < vectorArgsString.Count; j++)
                            {
                                if(int.TryParse(vectorArgsString[j], out var result))
                                {
                                    XYZ[j] = result;
                                }
                            }    
                            parameters[i] = new Vector3Int(XYZ[0],XYZ[1],XYZ[2]);
                        }

                        #endregion

                        #region --- [VECTOR4INT] ---
                            
                            else if (parameterInfos[i].ParameterType == typeof(Vector4Int))
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
                                var XYZW = new int[4]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    vectorArgsString[j] = Regex.Replace(vectorArgsString[j].Replace('.',','), "[^., 0-9]", "");
                                    if(int.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZW[j] = f;
                                    }
                                }
                                parameters[i] = new Vector4Int(XYZW[0],XYZW[1],XYZW[2],XYZW[3]);
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
                                colorArgsString[j] = Regex.Replace(colorArgsString[j].Replace('.',','), "[^., 0-9]", "");
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
                                    colorArgsString[j] = Regex.Replace(colorArgsString[j].Replace('.',','), "[^., 0-9]", "");
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

        /// <summary>
        /// Returns true if the given input can be converted into a command
        /// or if a suggestion for completing the input could be found.
        /// </summary>
        /// <returns></returns>
        public static bool ProposeMethodCommand(string input, out string description, out string proposedInput, out InputValidation inputValidation)
        {
            #region --- [OUT PARAMETER SETUP] ---

            // --- The original input string.
            var rawInput = input;
            
            // --- Format input string and get command key and args
            input = input.Cut();
            input.ToCommandArgs(out var key, out var args);

            if (key.Equals(CommandProcessor.GetterKey, StringComparison.OrdinalIgnoreCase))
            {
                if (CommandProcessor.ProposeGetter(rawInput, out description, out proposedInput, out inputValidation))
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
            if (key.Equals(CommandProcessor.SetterKey, StringComparison.OrdinalIgnoreCase))
            {
                return CommandProcessor.ProposeSetter(rawInput, out description, out proposedInput, out inputValidation);
            }
            
            
            // --- Declare proposal strings
            description = rawInput;
            proposedInput = rawInput;
            inputValidation = InputValidation.Optional;
            
            // --- Return if there is no key
            if (key.Length <= 0)  return false;
            if (key.EndsWith(CommandProcessor.InfoOperator))
            {
                if (MethodCommands.ContainsKey(key.Remove(key.Length - 1, 1)))
                    inputValidation = InputValidation.CommandInfo;

                description += $" // Receive information about the command.";
                return true;
            }
            
            // --- Save upper lower key configurations
            var keyConfiguration = key.GetUpperLowerConfig();
            var isStringEven = rawInput.Count(x => x == '"') % 2 == 0;
            
            // --- Are input & key a perfect match
            var isKeyPerfectMatch = MethodCommands.ContainsKey(key);
            
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
                foreach (var consoleCommand in MethodCommands)
                {
                    if (!consoleCommand.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)) continue;
                    foreach (var signature in consoleCommand.Value.Signatures.Where(s => s.hiddenPriority > bestPriority))
                    {
                        bestKey = consoleCommand.Key;
                        bestMatch = consoleCommand.Value;
                        bestPriority = signature.hiddenPriority;
                    }
                }
            
                if (bestMatch != null)
                {
                    proposalDescription = bestKey.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);

                    var proposalDescriptionArgs = proposalDescription.Split('.');

                    proposalDescription = $"{proposalDescriptionArgs[0]}{(proposalDescriptionArgs.Length > 1? "." : "")}";
                    proposal = proposalDescription;
                    
                    description += proposalDescription.Replace(".", "");
                    proposedInput += proposal;
                    inputValidation = InputValidation.Incomplete;
                    cachedProposeReturnValue = true;
                    return true;
                }    
            }

            #endregion
            
            //--- Iterate through every console command in dictionary to check for a potential match (key)
            foreach (var consoleCommand in MethodCommands)
            {
                if(consoleCommand.Value.Signatures.Count == 1 && !Application.isEditor)
                    if(consoleCommand.Value.Signatures[0].disableAutoCompletion)
                        continue;
                
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
                    
                    var proposalDescriptionArgs = proposalDescription.Split('.');
                    proposalDescription = $"{proposalDescriptionArgs[0]}{(proposalDescriptionArgs.Length > 1? "." : "")}";

                    proposal = proposalDescription;
                    
                    description +=  proposalDescription.Replace(".", "");
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
                    if(signature.disableAutoCompletion) continue;
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
                                    
                                    proposalDescription = parameterInformation[i].TryGetAttribute(out HintAttribute hint)
                                        // --- Hint attribute was found
                                        ? $" {(!hint.Description.IsNullOrWhiteSpace() && hint.Show != HintConfig.None? " //" : "")}" +
                                          $"{(hint.ShowDefaultValue && parameterInformation[i].HasDefaultValue && parameterInformation[i].DefaultValue != null ? $" [default: {parameterInformation[i].DefaultValue}]" : string.Empty)}" +
                                          $"{(hint.ShowValueType ? $" [type: {(parameterInformation[i].ParameterType.IsEnum ? "Enum" : parameterInformation[i].ParameterType.Name)}]" : string.Empty)}" +
                                          $"{(hint.ShowParameterName ? $" [param name: {parameterInformation[i].Name}]" : string.Empty)}" +
                                          $" {hint?.Description}"
                                        // --- Hint attribute was not found
                                        : $" //[param name: {parameterInformation[i].Name}] [type: {(parameterInformation[i].ParameterType.IsEnum ? "Enum" : parameterInformation[i].ParameterType.Name)}]";

                                    #endregion
                                    
                                    // --- Try to get the default value of the parameter
                                    try 
                                    {
                                        if (parameterInformation[i].TryGetAttribute(out SuggestionAttribute suggestionAttribute))
                                        {
                                            var useMarks = suggestionAttribute.Suggestions[0] != null;
                                            proposal = $"{(useMarks? '"'.ToString() : "")}" +
                                                       $"{suggestionAttribute.Suggestions[0]}" +
                                                       $"{(useMarks? '"'.ToString() : "")}";
                                        }
                                        else
                                        {
                                            var defaultValue = parameterInformation[i].DefaultValue;
                                            
                                            if (defaultValue != null)

                                                if (parameterInformation[i].HasDefaultValue)
                                                {
                                                    var useMarks = defaultValue is string;
                                                        
                                                    proposal = $"{(useMarks? '"'.ToString() : "")}" +
                                                               $"{defaultValue}" +
                                                               $"{(useMarks? '"'.ToString() : "")}";
                                                }
                                                else
                                                {
                                                    proposal = parameterInformation[i].ParameterType.GetDefault().ToString();
                                                }

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
                                    if (args[i].Equals($"{'"'}{sug}{'"'}")) return false;
                                    if (args[i].Equals($"{sug}")) return false;
                                    
                                    var _case = suggestion.IgnoreCase
                                        ? StringComparison.OrdinalIgnoreCase
                                        : StringComparison.CurrentCulture;

                                    var spaces = 0;
                                    for (var j = rawInput.Length - 1; j >= 0; j--)
                                    {
                                        if (rawInput[j] == ' ')
                                            spaces++;
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    
                                    if (sug.StartsWith(args[i], _case))
                                    {
                                        args[i] = args[i];
                                        proposalDescription = sug.Remove(0, args[i].Length + spaces);
                                        proposal = proposalDescription;
                                
                                        if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incomplete;
                                        
                                        description += proposalDescription;
                                        proposedInput = proposedInput.Remove(proposedInput.Length - args[i].Length, args[i].Length);
                                        proposedInput = proposedInput.Replace('"'.ToString(), string.Empty);
                                        proposedInput += $"{'"'}{sug}{'"'}";
                                        return true;
                                    }
                                    else if($"{'"'}{sug}".StartsWith(args[i], _case))
                                    {
                                        args[i] = args[i];
                                        proposalDescription = sug.Remove(0, args[i].Length - 1);
                                        proposal = proposalDescription;
                                
                                        if(inputValidation != InputValidation.Optional) inputValidation = InputValidation.Incomplete;
                                        description += proposalDescription;
                                        proposedInput = proposedInput.Remove(proposedInput.Length - args[i].Length, args[i].Length);
                                        proposedInput = proposedInput.Replace('"'.ToString(), string.Empty);
                                        proposedInput += $"{'"'}{sug}{'"'}";
                                        return true;
                                    }
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
                                        throw multiParameterException;
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
                                        throw multiParameterException;
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
                                        throw multiParameterException;
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
                            
                            #region --- [VECTOR2INT] ---
                            
                            else if (parameterInformation[i].ParameterType == typeof(Vector2Int))
                            {
                                isMultiArgsParam = true;
                                var vectorArgsString = new List<string> {args[i]};

                                if (i < args.Count - 1) {
                                    vectorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                
                                var XY = new int[2]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    if (vectorArgsString.Count < j)
                                        break;
                                        
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(int.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XY[j] = f;
                                    }
                                    else
                                    {
                                        throw multiParameterException;
                                    }
                                }

                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{vectorArgsString.Count.AsVectorHint()}";
                            }

                            #endregion
                            
                            #region --- [VECTOR3INT] ---
                            
                            else if (parameterInformation[i].ParameterType == typeof(Vector3Int))
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
                                
                                var XYZ = new int[3]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    if (vectorArgsString.Count < j)
                                        break;
                                        
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(int.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZ[j] = f;
                                    }
                                    else
                                    {
                                        throw multiParameterException;
                                    }
                                }
                                
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInformation[i - 1].ParameterType == typeof(string) ? $"{'"'} " : " ";
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                    proposalDescription = $" //{vectorArgsString.Count.AsVectorHint()}";
                            }

                            #endregion

                            #region --- [VECTOR4INT] ---
                            
                            else if (parameterInformation[i].ParameterType == typeof(Vector4Int))
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
                                
                                var XYZW = new int[4]; 
                                for (var j = 0; j < vectorArgsString.Count; j++)
                                {
                                    if (vectorArgsString.Count < j)
                                        break;
                                        
                                    vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                    vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                    vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                    
                                    if(int.TryParse(vectorArgsString[j], out var f))
                                    {
                                        XYZW[j] = f;
                                    }
                                    else
                                    {
                                        throw multiParameterException;
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
                                        throw multiParameterException;
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
                                        throw multiParameterException;
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
                            if (exc == multiParameterException)
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
