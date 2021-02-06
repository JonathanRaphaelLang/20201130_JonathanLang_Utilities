
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
    /// class contains a collection of descriptions and examples for getter, setter and a collection
    /// of additional custom attributes.
    /// ---
    /// You can enable / disable individual parts of this script and test everything yourself.
    /// Check the documentation for additional information and feel free to contact me by mail:
    /// TODO: mail
    /// </summary>
    [DeclaringName("Examples")] // Detailed description @ #region [GETTER & SETTER] => [ADDITIONAL ATTRIBUTES]

    #endregion

    public static class GetSetExamples
    {

        #region --- [GETTER & SETTER] ---
        
        
        #region --- [CREATING AND ACCESSING GETTER / SETTER] ---

        // Getter and setter attributes will expose any static property or field to be accessible via console input.
        // If you want to get (log) the value of a property / field you can use the [Getter] attribute. 

        // Like console commands, [Setter] attribute parameter only work with certain types. At the time every primitive
        // type is supported as well as some structures like Vectors and Color and enums. Enums also work as bitmask
        // with some limited functionality. Getter and setter can be combined using the [GetSet] attribute. 
        
        //Note: In contrast to a console command, the accessor of a getter / setter is case-sensitive.
        
        // Getter only
        [Getter] private static int PropertyGetter { get; set; } //Property does not actually require set;
        [Getter] private static int fieldGetter;
        
        // Setter only
        [Setter] private static int PropertySetter { get; set; } //Property does not actually require get;
        [Setter] private static int fieldSetter;
        
        // Getter and Setter combined
        [GetSet] private static int PropertyGetSet { get; set; } 
        [GetSet] private static int fieldGetSet;

        #endregion
        
        
       
        #region --- [GETTER AND SETTER PROPERTIES] ---

        // Getter and Setter share some optional properties
        
        // [GetterSetterBase] Properties:
        // => Shortcut (string): Shortcuts can be used as an abbreviation to access a getter/setter.
        // => Description (string): Custom description of the member.
        // => Priority (int): Higher priorities will be prefered by autocompletion and are shown higher up in listings.
        // => HideInBuild (bool): Determines if the getter/setter should be excluded from builds.
        
        // [ISetter] Properties:
        // => Default (object): Default value for the autocompletion of the member. (like default parameter)
        
        [Getter(Shortcut = "shortcut", Description = "description", Priority = 0, HideInBuild = false)]
        
        [Setter(Shortcut = "shortcut", Description = "description", Priority = 0, HideInBuild = false, Default = "value")]
        
        [GetSet(Shortcut = "shortcut", Description = "description", Priority = 0, HideInBuild = false, Default = 30)]
        
        private static string propertyExamples;

        #endregion
        
        
        
        #region --- [TYPES: PRIMITIVES] ---

        // The input of every primitive numeric ISetter is filtered. Every character that is not a number, "," or "." 
        // will be removed.
        [GetSet] private static long Long { get; set; } = 30285460;

        // If multiple characters are passed as an argument. The first will be used if it is not empty, null or space.
        [GetSet] private static char Char { get; set; } = 'e';

        // Booleans can be passed as "true" / "false" or "1" / "0".
        [GetSet] private static bool Bool { get; set; } = true;

        // Floating points can be marked with "." or ","
        [GetSet] private static float Float { get; set; } = .5f;

        [GetSet] private static byte Byte { get; set; } = 128;

        #endregion

        
        
        #region --- [TYPES: STRINGS] ---

        // Strings do not require to be wrapped in quotation marks because there is only ever one value to be passed.
        // e.g: /Set StringExample this is a string
        
        [GetSet(Shortcut = "StringExample")]
        private static string String { get; set; } = "Hello World";

        #endregion
        

        
        #region --- [TYPES: STRUCTURES] ---

        // Only applicable to ISetter:
        // Some often used structures are supported: Vector2, Vector3, Vector4, Vector2Int, Vector3Int, Color, Color32
        // Inputs for those types are filtered automatically for numeric values. This means that you characters that
        // are not a number, ",", "." or space will be ignored. e.g: "x:20,00 y:30.5 z:1" would be a valid input for a
        // Vector3 and result in: (20f, 30.5f, 1f)

        [GetSet] private static Color Color { get; set; } = new Color(.1f, 1f, .5f, .8f);

        [GetSet] private static Color32 Color32 { get; set; } = new Color32(255, 255, 0, 255);

        [GetSet] private static Vector3 Vector3 { get; set; } = new Vector3(.1f, .2f, .3f);

        [GetSet] private static Vector3Int Vector3Int { get; set; } = new Vector3Int(1, 2, 3);

        #endregion
        
        
        
        #region --- [TYPES: ENUMS] ---

        [GetSet] private static ExampleEnum Enum { get; set; } = ExampleEnum.None;

        [Flags]
        private enum ExampleEnum
        {
            None = 0,
            Player = 1,
            Object = 2,
            PlayerObject = 3,
        }

        #endregion

        
        
        #region --- [TYPES: CLASSES] ---
        
        // By default the value of an exposed member will be logged via ToString method. Classes and structures can
        // implement the IGettable interface that will replace the default ToString() method.

        [Getter] private static GettableInterface Interface { get; } = new GettableInterface("Interface Implemented");

        private class GettableInterface : IGettable
        {
            private readonly object value;
            public GettableInterface(object value)
            {
                this.value = value;
            }
            public string GetterValue() => value.ToString();
        }

        #endregion
       
        

        #region --- [ADDITIONAL ATTRIBUTES] ---

        // [DeclaringNameAttribute] determines a custom "DeclaringName" for Getter and Setter declared in the target class.
        // To avoid duplications each getter/setter is accessed by their class name (DeclaringName) + member name. 
        // e.g: getter and setter in this class will be accessible via
        // "/get Examples.GetterName" instead of "/get CommandExamples.GetterName"

        [DeclaringName("Examples")]
        private class DeclaringClass
        {
            // This property is accessible by "/get Examples.DeclaredProperty" 
            [Getter] private static string DeclaredProperty { get; set; } = "Hello World";
        }

        #endregion

        #endregion

        
        //--------------------------------------------------------------------------------------------------------------
        
        
        #region --- [MISC] ---

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void LogState() => Debug.Log("Note: GetSetExamples are enabled\n");

        #endregion
    }
}
#endif