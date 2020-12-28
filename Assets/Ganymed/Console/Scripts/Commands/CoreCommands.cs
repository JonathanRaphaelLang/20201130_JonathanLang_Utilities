using System;
using System.Threading.Tasks;
using Ganymed.Console.Core;
using Ganymed.Utils;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Commands
{
    public static class CoreCommands
    {
        #region --- [FIELDS] ---

        private const string LoremIpsum =
            "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";

        #endregion
        
        #region --- [ENUM] ---
        
        public enum PlayerTypeTeleport
        {
            player = 0,
            enemy = 5,
            obj = 488
        }
        
        public enum LogMethod
        {
            Message,
            Waring,
            Error
        }
        
        #endregion
        
        //TODO: Include GANYMED_MONITORING
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [TEST COMMANDS] ---
        
        [Command("Teleport", "Teleport an entity to the provided coords")]
        public static void TestCommand2(
            [Hint("this is an enum")]
            PlayerTypeTeleport target,
            [Hint("X coordinates")] 
            int x,
            [Hint("Y coordinates")] 
            int y = 30,
            [Hint("Z coordinates")] 
            float z = 3.4f)
        {
            switch (target)
            {
                case PlayerTypeTeleport.player:
                    Debug.Log("PLAYER");
                    break;
                case PlayerTypeTeleport.enemy:
                    Debug.Log("ENEMY");
                    break;
                case PlayerTypeTeleport.obj:
                    Debug.Log("OBJECT");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
            Debug.Log($"{x} | {y} | {z}");
        }
        
        
        
        [Command("Log", "Log a message to the Unity Console")]
        public static void Log(
            [Hint("input string (message) that will be send to Unity Console")]
            string message = null,
            [Hint("Log message as")]
            LogMethod method = LogMethod.Message)
        {
            if (message == null)
                message = LoremIpsum;
            
            switch (method)
            {
                case LogMethod.Message:
                    Debug.Log(message);
                    break;
                case LogMethod.Waring:
                    Debug.LogWarning(message);
                    break;
                case LogMethod.Error:
                    Debug.LogError(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        [Command("Vector")]
        private static void VEC(Vector3 vector3, string test, Vector4 vector2, bool boolean)
        {
            Debug.Log(vector3 + " " + test);
        }
        
        [Command("Color")]
        private static void Col(Color color)
        {
            Debug.Log(color);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SET FPS] ---
        
        [Command("SetFPS", "limit fps to set amount (0 for unlimited framerate)")]
        public static void SetFPS(
            [Hint("the target frames per second. min: 10, set 0 for unlimited fps")]
            int limit,
            [Hint("log message", Hint.Flags.ShowType)]
            bool log = true)
        {   
            Application.targetFrameRate = limit > 0 ? limit.Min(10) : int.MaxValue;
            
            if (!log) return;
            
            var fps = Application.targetFrameRate;
            Core.Console.Log(
                $"Set target frames per second to [{(fps == int.MaxValue? "unlimited" : fps.ToString("000"))}]");
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [SET VSYNC] ---

        [Command("SetVsync", "Set Vsync Count to set amount (0 for unlimited framerate)")]
        public static void SetVsync(VSyncCount vSyncCount)
        {
            vSyncCount = (VSyncCount) Mathf.Clamp((int) vSyncCount, 0, 4);
            QualitySettings.vSyncCount = (int)vSyncCount;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [QUIT] ---

        /// <summary>
        /// Quit either the application or exit playmode depending on context
        /// </summary>
        [Command("Quit", "Terminate the application")]
        public static async void Quit(
            [Hint(
                description: "quit the application after given delay",
                hint: Hint.Flags.ShowValue | Hint.Flags.ShowName | Hint.Flags.ShowValue)]
            int secondsDelay = 0,
            [Hint(description: "should the countdown be logged to the console")]
            bool logCountdown = false)
        {
            // --- Exit playmode in editor 
            
            if (logCountdown)
            {
                for (var i = secondsDelay; i > 0; i--)
                {
                    Core.Console.Log($"Quitting Application in {i} seconds");
                    await Task.Delay(1000);
                }    
            }
            else
            {
                await Task.Delay(secondsDelay * 1000);    
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CURSOR] ---
        
        [Command("Cursor")]
        private static void SetCursor(bool visible, CursorLockMode lockMode)
        {
            Debug.Log(visible + " " + lockMode);
            Cursor.visible = visible;
            Cursor.lockState = lockMode;
        }

        [Command("Cursor")]
        private static void SetCursor(bool visible)
        {
            Debug.Log(visible);
            Cursor.visible = visible;
        }
        
        [Command("Cursor")]
        private static void SetCursor(CursorLockMode lockMode)
        {
            Debug.Log(lockMode);
            Cursor.lockState = lockMode;
        }
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HIERARCHY] ---
        
        // --- Hierarchy Command Fields ---
        private static readonly string rootColor = new Color(0.42f, 0.92f, 1f).ToRichTextMarkup();
        private static readonly string defaultColor = Color.white.ToRichTextMarkup();
        private static readonly string staticColor = new Color(1f, 0f, 0.78f).ToRichTextMarkup();

        //TODO: outsource to task
        //TODO: add flag support for enums as console command parameter
        
        [Flags]
        public enum Include : short
        {
            None = 0,
            Scene = 1,
            DontDestroyOnLoad = 2
        }

        [Command("Hierarchy", "Outputs the current hierarchy layout. Note that this operation is expensive!")]
        private static void Hierarchy(
            [Hint("include every GameObject marked as [Dont Destroy On Load]", Hint.Flags.ShowType)]
            bool includeDontDestroyOnLoad = true)
        {
            #region --- [LABELING] ---

            Core.Console.Log($"{100.Repeat("-")}");
            Core.Console.Log($"---[HIERARCHY]---");
            Core.Console.Log($"{rootColor}[I AM A ROOT OBJECT]");
            Core.Console.Log($"{defaultColor}[I AM A DEFAULT OBJECT]");
            Core.Console.Log($"{staticColor}[I AM A STATIC OBJECT]");            

            #endregion
            
            var time = Time.realtimeSinceStartup;
            
            Core.Console.Log($"\n{rootColor}---[ACTIVE SCENE]---");
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var child in roots) {
                Traverse (child.gameObject, string.Empty);
            }

            // --- DONT DESTROY ON LOAD ---
            if (includeDontDestroyOnLoad)
            {
                Core.Console.Log($"\n{rootColor}---[DONT DESTROY ON LOAD]---");
                foreach (var child in DontDestroyOnLoadHandler.DontDestroyOnLoadObjects) {
                    Traverse (child.gameObject, string.Empty);
                }    
            }

            Core.Console.Log($"Finished: {Time.realtimeSinceStartup - time}s");
            Core.Console.Log($"{100.Repeat("-")}");
        }
        
        
        private static void Traverse(GameObject obj, string prefix, float depth = 0)
        {
            Core.Console.Log($"{prefix}|--{(obj.isStatic? staticColor : defaultColor)}{obj.name}");
            prefix += $"{new Color(1f - (depth / 30f), 1f - (depth / 30f), 1f - (depth / 30f), 1f - (depth / 30f)).ToRichTextMarkup()}|  ";
            depth += 1;
            
            foreach (Transform child in obj.transform)
            {
                var childCount = obj.transform.childCount;

                Traverse(
                    obj: child.gameObject,
                    prefix: prefix,
                    depth: depth);
            }
        }

        #endregion
    }
}
