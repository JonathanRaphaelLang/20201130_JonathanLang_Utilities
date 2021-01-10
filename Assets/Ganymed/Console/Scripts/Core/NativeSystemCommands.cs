using System;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Utils;
using Ganymed.Utils.ColorTables;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Handler;
using UnityEngine;

namespace Ganymed.Console.Core
{
    [DeclaringName("System")]
    public static class NativeSystemCommands
    {
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [TIME] ---

        [NativeCommand]
        [Getter(Shortcut = "DateTime")]
        public static DateTime DateTime 
            => DateTime.Now;
        
        [NativeCommand]
        [Getter(Shortcut = "Time")]
        public static string DateTimeTime 
            => $"{DateTime.Now:hh:mm:ss}";
        
        [NativeCommand]
        [Getter(Shortcut = "Date")]
        public static string DateTimeDate 
            => $"{DateTime.Now:dd:MM:yy}";
        
        [NativeCommand]
        [GetSet(Shortcut = "TimeScale", Default = 1, Priority = 100)]
        public static float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [TARGET FRAMERATE] ---

        private const string FPSHint =
            "The applications target framerate. (0 for unlimited)";
        
        [NativeCommand]
        [GetSet(Shortcut = "FPS", Description = FPSHint, Default = 165, Priority = 1000)]
        public static int FPS
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = (value > 0 ? value.Min(10) : int.MaxValue);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VSYNC] ---
        
        private const string VsyncHint =
            "Applictaion Vsync count (0-4)";
        
        [NativeCommand]
        [GetSet(Priority = 2000, Description = VsyncHint)]
        public static int Vsync
        {
            get => QualitySettings.vSyncCount;
            set => QualitySettings.vSyncCount = (int)(VSyncCount) Mathf.Clamp((int) value, 0, 4);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [QUIT] ---

        /// <summary>
        /// Quit either the application or exit playmode depending on context
        /// </summary>
        [Command("Quit", Priority = 1000, Description = "Quit the application")]
        [NativeCommand]
        public static async void Quit(
            [Hint(
                description: "quit the application after given delay",
                Show = HintShowFlags.ShowType)]
            int secondsDelay = 0,
            [Hint(description: "should the countdown be logged to the console")]
            bool logCountdown = false)
        {
            // --- Exit playmode in editor 
            
            if (logCountdown)
            {
                for (var i = secondsDelay; i > 0; i--)
                {
                    Console.Log($"Quitting Application in {i} seconds");
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
        
        [NativeCommand]
        [GetSet(Shortcut = "Cursor", Priority = 1000)]
        public static bool CursorVisibility
        {
            get => Cursor.visible;
            set
            {
                if (value) Cursor.lockState = 
                    Cursor.lockState == CursorLockMode.Confined ? 
                        CursorLockMode.Confined : CursorLockMode.None;
                Cursor.visible = value;
            }
        }
        
        [NativeCommand]
        [GetSet]
        public static CursorLockMode CursorLockMode
        {
            get => Cursor.lockState;
            set => Cursor.lockState = value;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HIERARCHY] ---
        
        /// <summary>
        /// Traverse and log scene hierarchy. 
        /// </summary>
        /// <param name="includeDontDestroyOnLoad"></param>
        [Command("Hierarchy", Description = "Log the current hierarchy layout. Note that this operation is expensive!")]
        private static async void Hierarchy(
            [Hint("include every GameObject marked as [Dont Destroy On Load]", Show = HintShowFlags.ShowAll)]
            bool includeDontDestroyOnLoad = true)
        {
            
            var time = Time.realtimeSinceStartup;
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            await Task.Run(delegate
            {
                Transmission.Start();
                Transmission.AddLine(new Message("Root GameObject", Console.ColorTitleMain, MessageOptions.Brackets));
                Transmission.AddLine(new Message("GameObject", Paint.cyan, MessageOptions.Brackets));
                
#if UNITY_EDITOR
                Transmission.AddLine(new Message("[Static GameObject]", Paint.violet));
#endif
                Transmission.AddLine(new Message("\n---[ACTIVE SCENE]---",
                    Console.ColorTitleMain, MessageOptions.Bold));
                Transmission.AddBreak();
            });
            
            
            foreach (var child in roots) {
                Traverse (child.gameObject, string.Empty);
            }

            // --- DONT DESTROY ON LOAD ---
            if (includeDontDestroyOnLoad)
            {
                Transmission.AddLine(new Message("\n---[DONT DESTROY ON LOAD]---",
                    Console.ColorTitleMain, MessageOptions.Bold));
                Transmission.AddBreak();
                
                foreach (var child in DontDestroyOnLoadHandler.DontDestroyOnLoadObjects) {
                    Traverse (child.gameObject, string.Empty);
                }    
            }
            
            Transmission.ReleaseAsync(
                callback: delegate {  Console.Log($"Finished: {Time.realtimeSinceStartup - time}s");});
        }
        
        
        private static void Traverse(GameObject obj, string prefix, float depth = 0)
        {
#if UNITY_EDITOR
            Transmission.AddLine(new Message(
                $"{prefix}|--{(obj.isStatic? CS.Violet : CS.Cyan)}{obj.name}"));
#else
            Transmission.AddLine(new Message(
                $"{prefix}|--{CS.Cyan}{obj.name}"));
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
