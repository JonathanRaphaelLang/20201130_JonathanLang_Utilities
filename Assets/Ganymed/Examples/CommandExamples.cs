
//#define ENABLE_EXAMPLES // => toggle to activate / deactivate examples.

using System;
using Ganymed.Console;
using Ganymed.Console.Attributes;
using UnityEngine;


#if ENABLE_EXAMPLES
#pragma warning disable 414

namespace Ganymed.Examples
{
    #region --- [SUMMARY] ---

    /// <summary>
    /// class contains a collection of descriptions and examples for commands and a collection
    /// of additional custom attributes.
    /// ---
    /// You can enable / disable individual parts of this script and test everything yourself.
    /// Check the documentation for additional information and feel free to contact me by mail:
    /// TODO: mail
    /// </summary>
    #endregion

    public static class CommandExamples
    {
        
        #region --- [CONSOLE COMMANDS] ---
        

        #region --- [CREATING AND ACCESSING COMMANDS] ---

        // The following regions contain examples and descriptions on how to declare and use console commands. 
        
        // Every static method can be declared as a console command by adding the [ConsoleCommand] attribute.
        // Those methods are not required to have a public modifier. Each command requires a unique accessor (key).
        // To invoke a command via the console, use the prefix ("/" by default) followed by the key. e.g. "/ExampleKey"
        
        // Note: The accessor of a console command is not case-sensitive.

        [ConsoleCommand(key: "ExampleKey")]
        public static void ConsoleCommand()
        {
            Debug.Log("Example Command");
        }

        #endregion
        
        
        
        #region --- [COMMAND PROPERTIES] ---

        // [ConsoleCommand] Properties:
        // => Key (string): Accessor for the command. Key is required. (Not a property but a constructor parameter)
        // => Description (string): Custom description for the command.
        // => Priority (int): Higher priorities will be prefered by autocompletion and are shown higher up in listings.
        // => DisableNBP (bool): Determines if numeric input for boolean parameter for this command is disabled.
        //    Note that nbp (numeric boolean processing) can also be controlled via global settings.
        //    Use this property to disable nbp for specific commands. 
        // => BuildSettings (Enum): Bitmask containing instructions for alternative handling of commands in builds.
        //    If you dont want alt command behaviour in your build leave this property as it is.

        [ConsoleCommand(
            key: "ExampleKey", 
            Description = "description",
            Priority = 0,
            DisableNBP = false,
            BuildSettings = CmdBuildSettings.None)]
        public static void ConsoleCommandProperties()
        {
            Debug.Log("Example Command");
        }

        #endregion

        
        
        #region --- [PARAMETER] ---

        // Console Commands can have parameters. The input string will automatically be separated and parsed into multiple 
        // arguments that will be converted into the type of the current parameter. Primitives, strings and enums as
        // well as some structs are viable parameter types. Reference types are invalid an will always be passed as null. 

        
        [ConsoleCommand("ParameterExample")]
        private static void ParameterExample(int param1, float param2, string param3)
        {
            // e.g: "/ParameterExample 20 3,2 "hello world"
            Debug.Log($"Integer: {param1} Float: {param2} String: {param3}");
        }
        
        #endregion
        
        
        
        #region --- [PARAMETER: PRIMITIVES] ---

        // The input of every primitive numeric parameter is filtered.
        // Every character that is not a number will be removed.
        [ConsoleCommand("NumericExample")]
        private static void LongCommand(
            sbyte param1, short param2, int param3, long param4, byte param5, ushort param6, uint param7, ulong param8)
        {
            Debug.Log($"{param1} | {param2} | {param3}");
        }

        
        // If multiple characters are passed as an argument. The first will be used if it is not null or whitespace.
        [ConsoleCommand("CharExample")]
        private static void CharCommand(char param)
        {
            Debug.Log($"{param}");
        }

        
        // Booleans can be passed as "true" / "false" or additionally "1" / "0" if NBP (numeric bool processing) is
        // enabled in the console settings as well as the attribute. By default NBP is enabled.
        [ConsoleCommand("BoolExampleA", DisableNBP = false)]
        private static void BoolCommandA(bool param)
        {
            Debug.Log($"{param}");
        }
        
        [ConsoleCommand("BoolExampleB", DisableNBP = true)]
        private static void BoolCommandB(bool param)
        {
            Debug.Log($"{param}");
        }

        
        // Floating points can be used with "." or ","
        [ConsoleCommand("FloatExample")]
        private static void FloatCommand(float param)
        {
            Debug.Log($"{param}");
        }

        #endregion
        
        
        //TODO: Parameter: Enum
        
        
        #region --- [PARAMETER: STRUCTS] ---
        
        // Some often used structures are supported: Vector2, Vector3, Vector4, Vector2Int, Vector3Int, Color, Color32
        // Arguments for those types are filtered automatically for numeric values. This means that you characters that
        // are not a number, ",", "." or space will be ignored. e.g: "x:20,00 y:30.5 z:1" would be a valid input for a
        // Vector3 and result in: (20f, 30.5f, 1f)

        [ConsoleCommand("VectorExample")]
        private static void VectorCommand(Vector2 param1, Vector3 param2, Vector4 param3)
        {
            Debug.Log($"{param1} | {param2} | {param3}");
        }
        
        
        [ConsoleCommand("VectorIntExample")]
        private static void VectorIntCommand(Vector2Int param1, Vector3Int param2)
        {
            Debug.Log($"{param1} | {param2}");
        }

        
        [ConsoleCommand("ColorExample")]
        private static void ColorCommand(Color param1, Color32 param2)
        {
            Debug.Log($"{param1} | {param2}");
        }

        #endregion

        

        #region --- [PARAMETER: NULLABLE / REFFERENCE TYPE PARAMETER] ---

        // You can use the [AllowUnsafeCommand] attribute to suppress custom warning messages if the parameters type is
        // seen as invalid by the command processor. 

        [ConsoleCommand("NullableParameterExample")]
        [AllowUnsafeCommand]
        private static void NullableParameterExample(int value, Exception exception)
        {
            Debug.Log($"Nullable: {exception} Value: {value}");
        }

        #endregion

        

        #region --- [PARAMETER: DEFAULT VALUES] ---

        // Default parameters are supported. Values of default parameters will also be suggested by autocompletion.

        [ConsoleCommand("DefaultParameterExample")]
        public static void DefaultParameterExampleA(bool paramA = true, int paramB = 20, string paramC = "example")
        {
            Debug.Log($"paramA: {paramA} paramB: {paramB} paramC: {paramC}");
        }

        #endregion

        

        #region --- [PARAMETER: STRINGS] ---

        // Strings might require some additional attention. Arguments are split by spaces which will cause individual
        // words in strings to be individual arguments. As a workaround it is required to wrap string inputs with
        // multiple words in quotation marks to label them as a related argument. This is not a prerequisite for single
        // word strings but is recommended because the marks will also tell the autocompletion when a string argument
        // is completed and the hint for the next parameter can be shown.

        [ConsoleCommand("StringExample")]
        public static void StringExample(string paramA = "multiple words", string paramB = "single",
            bool paramC = false)
        {
            Debug.Log($"paramA: {paramA} paramB: {paramB} paramC: {paramC}");
        }

        #endregion

        
        
        #region --- [OVERLOADS / MULITPLE SIGNATURES] ---

        // if the same key is used multiple times the command processor will automatically add additional
        // methods with the same key as overloads to the already existing key. This can result in unintentional 
        // behaviour if two keys are used by accident.

        [ConsoleCommand(key: "OverloadExample")]
        private static void OverloadExampleA(int param1, string param2)
        {
            Debug.Log($"Signature 1 : Integer: {param1} String: {param2}");
        }

        [ConsoleCommand(key: "OverloadExample")]
        private static void OverloadExampleB(string param1)
        {
            Debug.Log($"Signature 2 : String: {param1}");
        }
        
        //Another option if you want to have several related commands is to add a '.' between individual words in the key.
        //Autocompletion will only suggest a key up to the  '.'
        //Inserting "/a"  into the command line (when preprocessing is enabled) will suggest "/add.".
        
        [ConsoleCommand("Insert.Int")]
        private static void CmdA(int param)
        {
        }

        [ConsoleCommand("Insert.Float")]
        private static void CmdB(float param) 
        {    
        }

        [ConsoleCommand("Insert.Bool")]
        private static void CmdC(bool param) 
        {    
        }


        #endregion
        
        
        
        #region --- [ATTRIBUTE: HINT] ---

        // There are two attributes providing additional customization and functionality to commands.

        // The [Hint] Attribute can be used to create a custom description for the parameter of a console command.
        // Hints are shown in the input field of the console right before typing in the argument of the related
        // parameter. It can also be used to determine if and what additional information like name, type and default
        // value of the parameter will be shown in the consoles input field. By default, with or without the attribute,
        // the name and type of the parameter will be shown. 

        // [Hint] Properties:
        // => Description: Custom description of the parameter.
        // => Show: What should and what should not be shown. 

        [ConsoleCommand("HintA")]
        private static void HintExampleA([Hint(HintConfig.None)] string param)
        {
            Debug.Log(param);
        }

        [ConsoleCommand("HintB")]
        private static void HintExampleB([Hint("Example B")] string param)
        {
            Debug.Log(param);
        }

        [ConsoleCommand("HintC")]
        private static void HintExampleC([Hint("Example C", Show = HintConfig.ExcludeValue)] string param)
        {
            Debug.Log(param);
        }

        [ConsoleCommand("HintC")]
        private static void HintExampleD([Hint(HintConfig.ShowName, Description = "Example D")] string param)
        {
            Debug.Log(param);
        }

        [ConsoleCommand("HintE")]
        private static void HintExampleE([Hint(HintConfig.ShowName | HintConfig.ShowValue)] string param)
        {
            Debug.Log(param);
        }

        #endregion

        

        #region --- [ATTRIBUTE: SUGGESTION] ---

        // The [Suggestion] Attribute can be used for string parameter to add custom suggestions for the
        // autocompletion of a string. This is a quality-of-life attribute to compensate for the lack of an adequate
        // default value for strings.

        // [Suggestion] attribute Properties:
        // => IgnoreCase (bool): ignore case when comparing input with potential suggestions. (recommended true)

        [ConsoleCommand("Suggestion")]
        private static void SuggestionExample([Suggestion("Example", "Multiple Words Supported", IgnoreCase = true)] string param)
        {
            Debug.Log(param);
        }

        #endregion

        
        #endregion
        
        
        //--------------------------------------------------------------------------------------------------------------
        
        
        #region --- [MISC] ---

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void LogState() => Debug.Log("Note: CommandExamples are enabled\n");

        #endregion
    }
}
#endif