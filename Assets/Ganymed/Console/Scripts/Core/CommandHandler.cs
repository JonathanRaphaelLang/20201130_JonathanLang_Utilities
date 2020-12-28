using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ganymed.Console.Enumerations;
using Ganymed.Utils.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Core
{
    public static class CommandHandler
    {
        #region --- [FIELDS] ---

        private static readonly Dictionary<string, Command> ConsoleCommands = new Dictionary<string, Command>(StringComparer.CurrentCultureIgnoreCase);
        
        // --- string shortcuts 
        private static readonly string Marks = '"'.ToString();
        private const string Hint = " //";
        private static string InfoOperator = "?";

        private const BindingFlags MethodFlags = BindingFlags.Public   | BindingFlags.Static   | BindingFlags.NonPublic;
        
        // --- Message cache is used to prevent List allocation between method calls. Clear cache before use.
        private static readonly List<string> MessageCache_1 = new List<string>();
        private static readonly List<string> MessageCache_2 = new List<string>();
        private static readonly List<string> MessageCache_3 = new List<string>();

        #endregion

        #region --- [PROPETRIES] ---

        public static string Prefix { get; set; } = "/";

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CUSTOM CLASSES & STRUCTURES] ---

        private class Command
        {
            public readonly List<Signature> Signatures = new List<Signature>();

            public Command(Signature signature)
            {
                Signatures.Add(signature);
            }

            public void AddOverload(Signature overload)
            {
                Signatures.Add(overload);
            }
        }
        
        private class Signature
        {
            public readonly MethodInfo methodInfo;
            public readonly  object[] defaultParams;
            public readonly string description;
            public readonly bool disableNBP;

            public Signature(MethodInfo methodInfo, object[] defaultParams, string description, bool disableNbp)
            {
                this.methodInfo = methodInfo;
                this.defaultParams = defaultParams;
                this.description = description;
                this.disableNBP = disableNbp;
            }
        }        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONFIGURATION] ---

        private static bool logCommandsLoadedOnStart = true;
        private static bool logConfigurationOnStart = true;
        private static bool logWarnings = true;
        private static bool allowNumericBoolProcessing = true;
        
        public static void SetConfiguration(ConsoleConfiguration configuration)
        {
            logCommandsLoadedOnStart = configuration.logCommandsLoadedOnStart;
            logConfigurationOnStart = configuration.logConfigurationOnStart;
            logWarnings = configuration.logWarnings;
            allowNumericBoolProcessing = configuration.allowNumericBoolProcessing;
            InfoOperator = configuration.infoOperator;
            
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        //TODO: ADD: format commands for display
        
        #region --- [COMMANDS] ---

        private const string CommandsKey = "Commands";
        private const string ConfigKey = "CommandConfig";
        private const string HelpKey = "Help";
       
        [Core.Command(CommandsKey, "Log available commands and their descriptions")]
        public static void LogCommands()
        {
            Console.BeginTransmission(true);
            
            foreach (var cmd in ConsoleCommands)
            {
                var count = cmd.Value.Signatures.Count;
                for (byte i = 0; i < count; i++)
                {
                    Console.Transmit($"Key> [{cmd.Key}]",$"Description> [{cmd.Value.Signatures[i].description}]");
                    if(count > 1)
                        Console.Transmit($"Signature> [{i}]",2);    
                }
            }

            Console.SendTransmission();
        }

        
        [Core.Command("CommandConfig", "Receive information regarding the configuration of console commands")]
        private static void LogConfiguration()
        {
            Console.BeginTransmission();
            
            Console.Transmit("Command prefix: ", $"[{Prefix}]");
            Console.Transmit("Info operator: ", $"[{InfoOperator}]");
            Console.Transmit($"Additional Help ", $"[{Prefix}Help]");
            
            Console.SendTransmission();
        }

        
        [Core.Command(HelpKey, "Receive help on how to use the console")]
        private static void Help()
        {
            Console.Log($"/{CommandsKey} to get a list of commands");
            Console.Log($"/{ConfigKey} to view the current command configuration");
        }

        #endregion
        
        #region --- [COMMAND INFORMATION] ---

        private static void LogCommandInformation(string key)
        {
            key = key.Remove(key.Length-1, 1);
            
            if (!ConsoleCommands.TryGetValue(key, out var command)) return;

            Console.BeginTransmission();
            
            var index = 0;
            foreach (var signature in command.Signatures)
            {
                var msg = $"Key:[{key}]{(command.Signatures.Count > 1? $"[{index}]" : string.Empty)} | Description:[{signature.description}]";
                Console.Transmit(msg);
                
                var parameterInfos = signature.methodInfo.GetParameters();
                
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var description = string.Empty;
                    if (parameterInfos[i].TryGetAttribute(out Hint spec))
                    {
                        description = $" | Description: {Marks}{spec.description}{Marks}";
                    }

                    #region --- [ENUM] ---

                    var enumInfo = string.Empty;
                    if (parameterInfos[i].ParameterType.IsEnum)
                    {
                        var values = Enum.GetValues(parameterInfos[i].ParameterType);
                        foreach (var value in values)
                        {
                            enumInfo += $"[{value}]"; 
                        }   
                    }                    

                    #endregion
                   
                    var message =
                        $"Parameter [{i}]:{(parameterInfos[i].IsOptional? " (<color=#CCC>optional)" : string.Empty)} {parameterInfos[i].Name}{description}" +
                        $"{(parameterInfos[i].IsOptional? $" | Default: [{parameterInfos[i].DefaultValue}]" : string.Empty)}" +
                        $" | Type: [{(parameterInfos[i].ParameterType.IsEnum? $"Enum ({enumInfo})": parameterInfos[i].ParameterType.Name)}]";
                    
                    Console.Transmit(message, 0 , (i == parameterInfos.Length-1 ? Console.MessageOptions.Break : Console.MessageOptions.None));
                }

                index++;
            }
            
            Console.SendTransmission();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMAND GENERATION (RuntimeInitializeOnLoadMethod)] ---
        
        [RuntimeInitializeOnLoadMethod]
        private static void FindConsoleCommandsInAssembly()
        {
            var time = Time.realtimeSinceStartup;
            var types =  System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(object));
            var executeOnLoad = new Dictionary<Signature, int>();

            foreach (var type in types)
            {
                foreach (var methodInfo in type.GetMethods(MethodFlags))
                {
                    var attribute = methodInfo.GetCustomAttribute(typeof(Core.Command)) as Core.Command;
                
                    if (attribute == null) continue;
                    var key = attribute.key;
                  
                    var signature = new Signature(methodInfo, attribute.defaults, attribute.description, attribute.disableNBP);
                    if (!ConsoleCommands.ContainsKey(key))
                    {
                        ConsoleCommands.Add(key, new Command(signature));
                    }
                    else
                    {
                        ConsoleCommands[key].AddOverload(signature);
                    }

                    if (attribute.executeOnLoad)
                        executeOnLoad.Add(signature, attribute.millisecondsDelayOnLoad);
                }
            }

            var passed = Time.realtimeSinceStartup - time;

            if (logCommandsLoadedOnStart)
            {
                Console.Log($"<line-height=150%>>Assembly {ConsoleCommands.Count} Commands are loaded | Time passed: {passed:0.0000}s");
            }

            foreach (var cmd in executeOnLoad)
            {
                ExecuteMethodOnLoad(cmd.Key, cmd.Value);
            }

            if (logConfigurationOnStart)
                LogConfiguration();
        }

        private static async void ExecuteMethodOnLoad(Signature cmd, int delay)
        {
            await Task.Delay(delay);
            try
            {
                cmd.methodInfo.Invoke(null, cmd.defaultParams);
            }
            catch
            {
                if(logWarnings)
                    Debug.LogWarning($"COMMAND:[{cmd}] Warning: default params did not match method signature");
            }
        }
                

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMAND PROCCESSING] ---

        public static void ProcessCommand(string input)
        {
            if (!input.StartsWith(Prefix)) return;
            
            input = input.Remove(0, Prefix.Length);
            
            input.ToCommandArgs(out var command, out var args);
            
            if (command.EndsWith(InfoOperator))
            {
                LogCommandInformation(command);
                return;
            }
            
            ProcessCommand(command, args);
        }

        
        // --- ProcessCommand variables 
        private static ParameterInfo[] methodParams;
        private static object[] parameters;
        
        private static void ProcessCommand(string commandKey, IList<string> args)
        {
            if (!ConsoleCommands.TryGetValue(commandKey, out var command)) return;

            var viewedSignature = 0;
            foreach (var signature in command.Signatures)
            {
                viewedSignature++;
                methodParams = signature.methodInfo.GetParameters();
                parameters = new object[signature.methodInfo.GetParameters().Length];

                var checkNextSignature = true;
                
                for (var i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        #region --- [FLOAT] ---

                        //  --- Replace . with , to ensure that the value is interpreted correctly
                        if (methodParams[i].ParameterType == typeof(float))
                        {
                            args[i] = args[i].Replace('.', ',');
                            parameters[i] = Convert.ChangeType(args[i], methodParams[i].ParameterType);
                        }

                        #endregion

                        #region --- [ENUM] ---

                        else if (methodParams[i].ParameterType.IsEnum)
                        {
                            var enumValues = Enum.GetValues(methodParams[i].ParameterType);
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

                        else if (methodParams[i].ParameterType == typeof(bool) && allowNumericBoolProcessing && !signature.disableNBP)
                        {
                            if (int.TryParse(args[i], out var resultBool))
                            {
                                args[i] = resultBool == 0 ? "false" : resultBool == 1? "true" : throw new Exception();
                            }
                            parameters[i] = Convert.ChangeType(args[i], methodParams[i].ParameterType);
                        }

                        #endregion

                        #region --- [VECTOR3] ---

                        else if (methodParams[i].ParameterType == typeof(Vector3))
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

                        else // --- If no special case applies try to convert it "blind"
                        {
                            parameters[i] = Convert.ChangeType(args[i], methodParams[i].ParameterType);
                        }
                    }
                    catch
                    {
                        // --- Use the default value of parameter not the default value of type
                        if (methodParams[i].IsOptional)
                            parameters[i] = methodParams[i].DefaultValue;
                        
                        checkNextSignature = viewedSignature == command.Signatures.Count; 
                    }
                }

                if (!checkNextSignature) continue;
                //  --- Check if the last parameter has params attribute and create a params array... 
                if(methodParams.Length > 0)
                    if (methodParams[methodParams.Length -1].isParams())
                    {
                        if(logWarnings) Debug.Log("Last parameter has params attribute");
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

        //TODO: ADD: Vector2, Vector3 
        //TODO: REWORK: rearrange param descriptions

        #region --- [COMMAND PRE PROCESSING / PROPOSAL] ---
        
        /// <summary>
        /// Returns true if the given input can be converted into a command
        /// or if a suggestion for completing the input could be found.
        /// </summary>
        /// <param name="input">The input string received by the console.</param>
        /// <param name="proposedDescription">The proposed description of the current input or or the next parameter.</param>
        /// <param name="proposedInput"></param>
        /// <param name="validationFlag">The state of the input (valid, incomplete, etc.)</param>
        /// <returns></returns>
        public static bool Propose(string input,
            // --- Out parameter
            out string proposedDescription,
            out string proposedInput,
            out InputValidation validationFlag)
        {
            #region --- [OUT PARAMETER SETUP] ---

            // --- The original input string.
            var rawInput = input;
            
            // --- Declare proposal strings
            proposedDescription = rawInput;
            proposedInput = rawInput;
            validationFlag = InputValidation.None;

            // --- Format input string and get command key and args
            input = input.Cut();
            input = input.Remove(0, Prefix.Length);
            input.ToCommandArgs(out var key, out var args);
            
            // --- Return if there is no key
            if (key.Length <= 0)  return false;
            if (key.EndsWith(InfoOperator))
            {
                if (ConsoleCommands.ContainsKey(key.Remove(key.Length - 1, 1)))
                    validationFlag = InputValidation.CommandInfo;

                proposedDescription += $" // Receive information about the command.";
                return true;
            }
            
            // --- Save upper lower key configurations
            var keyConfiguration = key.GetUpperLowerConfig();
            var isStringEven = rawInput.Count(x => x == '"') % 2 == 0;
            
            #endregion
            
            //--- Iterate through every console command in dictionary to check for a potential match (key)
            foreach (var consoleCommand in ConsoleCommands)
            {
                #region --- [SEARCH FOR MATCH] ---

                // --- Check every command for potential match
                if (!consoleCommand.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase)) continue;

                #endregion
              
                #region --- [SETUP IF POTENTIAL MATCH WAS FOUND] ---

                // --- Are input & key a perfect match
                var isKeyPerfectMatch = ConsoleCommands.ContainsKey(key);

                // --- Initialise proposal strings
                var proposalDescription = string.Empty;
                var proposal = string.Empty;

                // --- Multi argument parameter information
                var isMultiArgsParam = false;
                var multiArgsParamIndex = 0;
                var multiArgumentParameterType = MultiArgumentParameterType.None;

                // --- If there is no perfect match we can return and ignore signatures & parameter
                if (!isKeyPerfectMatch) 
                {
                    proposalDescription = consoleCommand.Key.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);
                    proposal = consoleCommand.Key.SetUpperLowerConfig(keyConfiguration).Remove(0,key.Length);
                    
                    proposedDescription += proposalDescription;
                    proposedInput += proposal;
                    validationFlag = InputValidation.Incomplete;
                    return true;
                }
                
                #endregion

                // --- Index + 1 of the inspected signature
                var viewedSignature = 0;
                
                // --- Iterate thought every signature and check if it will work
                foreach (var signature in consoleCommand.Value.Signatures)
                {
                    viewedSignature++;
                    var parameterInfos = signature.methodInfo.GetParameters();
                    var checkNextSignature = false;
                    var isValid = viewedSignature <= 1;
                    
                    // --- Iterate thought every parameter
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        // --- If the following code throws, we know that the current input cannot be converted.
                        try 
                        {
                            #region --- [DISPLAY INFORMATION ABOUT NEXT PARAMETER] ---

                            // --- Check if there are args left to enter & autocomplete
                            if (args.Count < parameterInfos.Length && isKeyPerfectMatch || isMultiArgsParam && isKeyPerfectMatch)
                            {
                                // --- Check if args are string with more then one word and therefore should end with " 
                                var ends = " ";
                                if (i > 0)
                                    ends = parameterInfos[i - 1].ParameterType == typeof(string) ? $"{Marks} " : " ";

                                // if we are dealing with a multi argument parameter (aka Vector2 / Vector3 etc)
                                if (isMultiArgsParam && rawInput.EndsWith(ends) && isStringEven)
                                {
                                    proposalDescription = $" //{multiArgsParamIndex.IndexToHint(multiArgumentParameterType)}";
                                }
                                // if the next element is a parameter
                                else if (i == args.Count && rawInput.EndsWith(ends) && isStringEven && !isMultiArgsParam)
                                {
                                    #region --- [PROPOSE] ---
                                    
                                    proposalDescription = parameterInfos[i].TryGetAttribute(out Hint spec)
                                        // --- Hint attribute was found
                                        ? $"{Hint}" +
                                          $"{(spec.showDefaultValue && parameterInfos[i].HasDefaultValue && parameterInfos[i].DefaultValue != null ? $" [default: {parameterInfos[i].DefaultValue}]" : string.Empty)}" +
                                          $"{(spec.showValueType ? $" [type: {(parameterInfos[i].ParameterType.IsEnum ? "Enum" : parameterInfos[i].ParameterType.Name)}]" : string.Empty)}" +
                                          $"{(spec.showParameterName ? $" [param name: {parameterInfos[i].Name}]" : string.Empty)}" +
                                          $" {spec?.description}" //TODO: markup
                                        // --- Hint attribute was not found
                                        : $"{Hint}[param name: {parameterInfos[i].Name}] [type: {(parameterInfos[i].ParameterType.IsEnum ? "Enum" : parameterInfos[i].ParameterType.Name)}]";

                                    #endregion
                                    
                                    try // --- Try to get the default value of the parameter
                                    {
                                        var defaultValue = parameterInfos[i].DefaultValue;
                                        if (defaultValue != null)
                                            proposal = parameterInfos[i].HasDefaultValue
                                                ? defaultValue.ToString()
                                                : parameterInfos[i].ParameterType.GetDefault().ToString();

                                        proposal = proposal.Replace(",", string.Empty);
                                        proposal = proposal.Replace("(", string.Empty);
                                        proposal = proposal.Replace(")", string.Empty);
                                        proposal = proposal.Replace('.', ',');
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
                            multiArgsParamIndex = 0;
                            
                            if (i > args.Count - 1) continue; // --- We continue because there are no args at this point

                            #region --- [BOOLEAN] ---

                            if (parameterInfos[i].ParameterType == typeof(bool))
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
                                            validationFlag = InputValidation.Valid;
                                        }
                                        else
                                        {
                                            validationFlag = InputValidation.Incomplete;
                                            proposedDescription += proposalDescription;
                                            proposedInput += proposal;
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
                                            validationFlag = InputValidation.Valid;
                                        }
                                        else
                                        {
                                            validationFlag = InputValidation.Incomplete;
                                            proposedDescription += proposalDescription;
                                            proposedInput += proposal;
                                            return true;
                                        }
                                    }
                                }
                                
                                // --- Check if input can be converted 
                                var obj = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                            }

                            #endregion

                            #region --- [ENUM] ---

                            else if (parameterInfos[i].ParameterType.IsEnum)
                            {
                                var values = Enum.GetValues(parameterInfos[i].ParameterType);

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

                            //TODO: CONVERT VECTOR & COLOR
                            
                            #region --- [COLOR] ---

                            else if (parameterInfos[i].ParameterType == typeof(Color))
                            {
                                multiArgumentParameterType = MultiArgumentParameterType.Color;
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
                                if (colorArgsString.Count == 4)
                                {
                                    var RGBA = new float[4]; 
                                    for (var j = 0; j < colorArgsString.Count; j++)
                                    {
                                        colorArgsString[j] = colorArgsString[j].Replace('.', ',');
                                        colorArgsString[j] = colorArgsString[j].Replace("(", string.Empty);
                                        colorArgsString[j] = colorArgsString[j].Replace(")", string.Empty);
                                        if(float.TryParse(colorArgsString[j], out var f))
                                        {
                                            RGBA[j] = f;
                                        }
                                    }
                                }
                                
                                multiArgsParamIndex = colorArgsString.Count;
                            }

                            #endregion
                            
                            //------------------------------------------------------------------------------------------

                            #region --- [VECTOR2] ---
                            
                            else if (parameterInfos[i].ParameterType == typeof(Vector2))
                            {
                                multiArgumentParameterType = MultiArgumentParameterType.Vector;
                                isMultiArgsParam = true;
                                var vectorArgsString = new List<string> {args[i]};

                                if (i < args.Count - 1) {
                                    vectorArgsString.Add(args[i + 1]);
                                    args.RemoveAt(i + 1);
                                    isMultiArgsParam = false;
                                }
                                
                                if (vectorArgsString.Count == 2)
                                {
                                    var XYZ = new float[2]; 
                                    for (var j = 0; j < vectorArgsString.Count; j++)
                                    {
                                        vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                        vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                        vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                        if(float.TryParse(vectorArgsString[j], out var f))
                                        {
                                            XYZ[j] = f;
                                        }
                                    }
                                }

                                multiArgsParamIndex = vectorArgsString.Count;
                            }

                            #endregion
                            
                            #region --- [VECTOR3] ---
                            
                            else if (parameterInfos[i].ParameterType == typeof(Vector3))
                            {
                                multiArgumentParameterType = MultiArgumentParameterType.Vector;
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
                                
                                if (vectorArgsString.Count == 3)
                                {
                                    var XYZ = new float[3]; 
                                    for (var j = 0; j < vectorArgsString.Count; j++)
                                    {
                                        vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                        vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                        vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                        if(float.TryParse(vectorArgsString[j], out var f))
                                        {
                                            XYZ[j] = f;
                                        }
                                    }
                                }
                                
                                multiArgsParamIndex = vectorArgsString.Count;
                            }

                            #endregion

                            #region --- [VECTOR4] ---
                            
                            else if (parameterInfos[i].ParameterType == typeof(Vector4))
                            {
                                multiArgumentParameterType = MultiArgumentParameterType.Vector;
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
                                
                                if (vectorArgsString.Count == 4)
                                {
                                    var XYZ = new float[4]; 
                                    for (var j = 0; j < vectorArgsString.Count; j++)
                                    {
                                        vectorArgsString[j] = vectorArgsString[j].Replace('.', ',');
                                        vectorArgsString[j] = vectorArgsString[j].Replace("(", string.Empty);
                                        vectorArgsString[j] = vectorArgsString[j].Replace(")", string.Empty);
                                        if(float.TryParse(vectorArgsString[j], out var f))
                                        {
                                            XYZ[j] = f;
                                        }
                                    }
                                }
                                
                                multiArgsParamIndex = vectorArgsString.Count;
                            }
                            
                            #endregion
                            
                            //------------------------------------------------------------------------------------------
                            
                            #region --- [OTHER & UNKNOWN TYPES] ---

                            // --- Convert args to param type. This could throw an Exception
                            else
                            {
                                var obj = Convert.ChangeType(args[i], parameterInfos[i].ParameterType);
                                // --- if conversion does not throw it is valid 
                                isValid = true;
                            }
                            #endregion

                            #endregion
                        }
                    
                        // --- Catch Exception thrown by convention from args to param type  
                        catch 
                        {
                            checkNextSignature = viewedSignature != consoleCommand.Value.Signatures.Count;
                            validationFlag = InputValidation.Incorrect;
                            break;
                        }
                    }
                    
                    // if an exception was thrown before we've checked the last signature continue
                    if (checkNextSignature) {
                        continue;
                    }

                    
                    if (args.Count == parameterInfos.Length && isKeyPerfectMatch && isValid)
                    {
                        validationFlag = InputValidation.Valid;
                        proposedDescription += proposalDescription;
                        proposedInput += proposal;
                        return true;
                    }

                    if (args.Count < parameterInfos.Length && isKeyPerfectMatch && isValid)
                    {
                        validationFlag = parameterInfos[args.Count].IsOptional
                            ? InputValidation.Optional
                            : InputValidation.Incomplete;
                        proposedDescription += proposalDescription;
                        proposedInput += proposal;
                        return true;
                    }
                    
                    else
                    {
                        validationFlag = InputValidation.Incorrect;
                        return false;
                    }
                }
            }

            validationFlag = InputValidation.Incorrect;
            // --- Return false because we don't have a proposal
            return false;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EXTENSION] ---

        private static void ToCommandArgs(this string input, out string key, out List<string> args, char split = ' ')
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
                    if(args[viewedIndex].EndsWith(Marks))
                        break;
                    args[viewedIndex] += $" {args[viewedIndex + 1]}";
                    args.RemoveAt(viewedIndex + 1);
                }

                if (args[viewedIndex].EndsWith(Marks))
                    args[viewedIndex] = args[viewedIndex].Remove(args[viewedIndex].Length -1, 1);
            }
        }

        private static string IndexToHint(this int value, MultiArgumentParameterType type)
        {
            switch (type)
            {
                case MultiArgumentParameterType.Vector:
                    return value == 0 ? "X" : value == 1 ? "Y" : value == 2 ? "Z" : "W";
                case MultiArgumentParameterType.Color:
                    return value == 0 ? "R" : value == 1 ? "G" : value == 2 ? "B" : "A";
                case MultiArgumentParameterType.None:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private enum MultiArgumentParameterType
        {
            None,
            Vector,
            Color
        }

        #endregion
    }
}
