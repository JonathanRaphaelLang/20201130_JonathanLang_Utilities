using System;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Utils;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Handler;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Console.Core
{
    public static class CoreCommands
    {
        #region --- [TEST COMMANDS] ---
      
        [Command("TestVector", "Used to test Vector2, Vector3 & Vector4 input")]
        private static void VEC(Vector2 vector2, Vector3 vector3, Vector4 vector4)
        {
            Debug.Log($"{vector2}  {vector3}  {vector4}");
        }
        
        [Command("TestColor", "Used to test Color32 & Color input")]
        private static void Color(Color32 color32, Color color)
        {
            Debug.Log($"{color.AsRichText()}{color}</color> {color32.AsRichText()}{color32}</color>");
        }
        
        [Command("TestSuggestion")]
        private static void Suggest([CanBeNull][Suggestion("My first suggestion", "Maybe try this", "or this perhaps?")] string target)
        {
            Debug.Log(target?? "NULL");
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [TARGET FRAMERATE] ---
        
        [Command("SetFPS", "Set a limit to the applications maximum frames per second (0 for unlimited framerate)")]
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
        
        #region --- [VSYNC] ---

        [GetSet][NativeCommand]
        public static int Vsync
        {
            get => QualitySettings.vSyncCount;
            set => QualitySettings.vSyncCount = (int)(VSyncCount) Mathf.Clamp((int) value, 0, 4);
        }

        [Command("SetVsync", "Set Vsync count to set amount (0 for unlimited framerate)")]
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
        [Command("Quit", "Quit the application", 1000)]
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
        
        
        
        [Command("Cursor", "Set cursors visibility and its lock state")]
        private static void SetCursor(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;
        }

        [Command("Cursor", "Set cursors visibility")]
        private static void SetCursor(bool visible)
            => Cursor.visible = visible;
        
        [Command("Cursor", "Set cursors lock state")]
        private static void SetCursor(CursorLockMode lockMode)
            => Cursor.lockState = lockMode;
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HIERARCHY] ---
        
        // --- Hierarchy Command Fields ---
        private static readonly Color defaultColor = new Color(0.38f, 1f, 0.91f);
#if UNITY_EDITOR
        private static readonly Color staticColor = new Color(0.52f, 0.5f, 1f);
#endif

        //TODO: add flag support for enums as console command parameter
        //TODO: AttributeTargets.Property
        
        [Flags]
        public enum Include : short
        {
            None = 0,
            Scene = 1,
            DontDestroyOnLoad = 2
        }

        [Command("Hierarchy", "Log the current hierarchy layout. Note that this operation is expensive!")]
        private static async void Hierarchy(
            [Hint("include every GameObject marked as [Dont Destroy On Load]", Hint.Flags.ShowType)]
            bool includeDontDestroyOnLoad = true)
        {
            
            var time = Time.realtimeSinceStartup;
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            await Task.Run(delegate
            {
                
                Transmission.Start();
                Transmission.AddLine(new Message("[Root GameObject]", Core.Console.colorTitles));
                Transmission.AddLine(new Message("[GameObject]", defaultColor));
                
#if UNITY_EDITOR
                Transmission.AddLine(new Message("[Static GameObject]", staticColor));
#endif
                Transmission.AddLine(new Message("\n---[ACTIVE SCENE]---",
                    Core.Console.colorTitles, MessageOptions.Bold));
                Transmission.AddBreak();
            });
            
            
            foreach (var child in roots) {
                Traverse (child.gameObject, string.Empty);
            }

            // --- DONT DESTROY ON LOAD ---
            if (includeDontDestroyOnLoad)
            {
                Transmission.AddLine(new Message("\n---[DONT DESTROY ON LOAD]---",
                    Core.Console.colorTitles, MessageOptions.Bold));
                Transmission.AddBreak();
                
                foreach (var child in DontDestroyOnLoadHandler.DontDestroyOnLoadObjects) {
                    Traverse (child.gameObject, string.Empty);
                }    
            }
            
            Transmission.ReleaseAsync(
                callback: delegate {  Core.Console.Log($"Finished: {Time.realtimeSinceStartup - time}s");});
        }
        
        
        private static void Traverse(GameObject obj, string prefix, float depth = 0)
        {
#if UNITY_EDITOR
            Transmission.AddLine(new Message(
                $"{prefix}|--{(obj.isStatic? staticColor.AsRichText(): defaultColor.AsRichText())}{obj.name}"));
#else
            Transmission.AddLine(new Message(
                $"{prefix}|--{defaultColor.AsRichText()}{obj.name}"));
#endif
            
            prefix += $"{new Color(1f - (depth / 30f), 1f - (depth / 30f), 1f - (depth / 30f), 1f - (depth / 30f)).AsRichText()}|  ";
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
