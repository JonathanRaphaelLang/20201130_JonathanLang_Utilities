using System;
using Ganymed.Console.Attributes;
using UnityEngine;

namespace Ganymed
{
#if true 
    
    [DeclaringName("Examples")]
    public class CommandExamples
    {
        #region --- [PROPERTIES] ---

        [GetSet(Shortcut = "CameraColor", Default = ".2 .3 .5 1")]
        public static Color CameraBackground
        {
            get => MainCamera.backgroundColor;
            set => MainCamera.backgroundColor = value;
        }

        [Getter(Shortcut = "Camera")]
        public static Camera MainCamera
        {
            get
            {
                if(mainCamera != null) return mainCamera;
                return mainCamera = Camera.main;
            }
        }
        
        
        private static Camera mainCamera;
        
       
    
        #endregion
        
        #region --- [TYPE TEST] ---
      
        [GetSet(Default = 'F')]
        public static char Char
        {
            get => testChar;
            set => testChar = value;
        }
        private static char testChar;
        
        

        [GetSet(Default = false, Priority = 100)]
        public static bool Bool
        {
            get => @bool;
            set => @bool = value;
        }
        private static bool @bool;
        
        
        
        [GetSet(Default = 5.7f)]
        public static float Float
        {
            get => @float;
            set => @float = value;
        }
        [GetSet(Default = "10, 10, 10", Priority = 150)]
        private static float @float;
        
        
        
        [GetSet(Default = "10, 10, 10", Priority = 50)]
        public static Vector3 Vector
        {
            get => vector;
            set => vector = value;
        }
        [GetSet(Default = "10, 10, 10", Priority = 50)]
        private static Vector3 vector;
        
        
        [GetSet(Default = 255)]
        public static byte Byte
        {
            get => @byte;
            set => @byte = value;
        }
        private static byte @byte;
        
        
        [GetSet(Description = "This is a test Color")]
        public static Color Color
        {
            get => color;
            set => color = value;
        }
        private static Color color;
        
        
        [GetSet]
        public static Color32 Color32
        {
            get => color32;
            set
            {
                color32 = value;
            }
        }
        private static Color32 color32;
        
        
        
        [GetSet(Default = "this is a string")]
        public static string String
        {
            get => @string;
            set => @string = value;
        }
        private static string @string;
        
        
        
        [GetSet(Default = Test.Other, Shortcut = "Enum")]
        public static Test Enum
        {
            get => testEnum;
            set => testEnum = value;
        }
        private static Test testEnum;
        
        
        [Flags]
        public enum Test
        {
            Player = 1,
            Object = 2,
            PlayerObject = 3,
            Other = 4,
            OtherObject = 6
        }

        #endregion
    }
#endif
}
