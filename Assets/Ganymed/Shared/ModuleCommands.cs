using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ganymed.Console;
using Ganymed.Console.Attributes;
using Ganymed.Console.Core;
using Ganymed.Console.Transmissions;
using Ganymed.Monitoring.Modules;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;
using Module = Ganymed.Monitoring.Core.Module;

namespace Ganymed.Shared
{
#if GANYMED_CONSOLE && GANYMED_MONITORING
    
    public static class ModuleCommands
    {
        
        
        #region --- [FIELDS] ---

        private enum ModuleActions
        {
            Enabled,
            Active,
            Visible,
            EnabledActiveAndVisible
        }
        
            
        private enum CoreModules
        {
            System,
            Notes,
            RecentFPS,
            Cursor,
            Vsync,
            FPS,
            TargetFrameRate,
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [MONITORING] ---
       
        
        [ConsoleCommand("GetModules", Description = "Receive a list of every loaded monitoring module")]
        private static void GetModules()
        {
            const MessageOptions options = MessageOptions.Brackets;
            if(!Transmission.Start(TransmissionOptions.Enumeration)) return;
        
            Transmission.AddLine(
                new MessageFormat($"Module", ConsoleSettings.ColorTitleSub, options),
                new MessageFormat($"Id", ConsoleSettings.ColorTitleSub, options),
                new MessageFormat($"Enabled", ConsoleSettings.ColorTitleSub , options),
                new MessageFormat($"Active", ConsoleSettings.ColorTitleSub , options),
                new MessageFormat($"Visible", ConsoleSettings.ColorTitleSub , options),
                new MessageFormat($"Type", ConsoleSettings.ColorTitleSub , options),
                new MessageFormat($"Description", ConsoleSettings.ColorTitleSub, options));
            Transmission.AddBreak();
        
            foreach (var module in Module.ModuleDictionary)
            {
                Transmission.AddLine(
                    module.Value.UniqueName,
                    module.Value.UniqueId,
                    new MessageFormat(module.Value.IsEnabled, module.Value.IsEnabled? RichText._green : RichText._red),
                    new MessageFormat(module.Value.IsActive, module.Value.IsActive? RichText._green : RichText._red),
                    new MessageFormat(module.Value.IsVisible, module.Value.IsVisible? RichText._green : RichText._red),
                    module.Value.GetTypeOfTAsString(),
                    module.Value.Description ?? "n/a");
            }
            Transmission.ReleaseAsync();
        }

        
        [ConsoleCommand("SetModule", Description = "Set the state of a monitoring module", Priority = 1000)]
        private static void SetModule(int id, ModuleActions action, bool value)
        {
            switch (action)
            {
                case ModuleActions.Enabled:
                    Module.GetModule(id)?.SetEnabled(value);
                    break;
                case ModuleActions.Active:
                    Module.GetModule(id)?.SetActive(value);
                    break;
                case ModuleActions.Visible:
                    Module.GetModule(id)?.SetVisible(value);
                    break;
                case ModuleActions.EnabledActiveAndVisible:
                    Module.GetModule(id)?.SetStates(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        
        [ConsoleCommand("SetModule", Description = "Set the state of a monitoring module", Priority = 1000)]
        private static void SetModule(CoreModules id, ModuleActions action, bool value)
        {
            switch (action)
            {
                case ModuleActions.Enabled:
                    Module.GetModule(id.ToString())?.SetEnabled(value);
                    break;
                case ModuleActions.Active:
                    Module.GetModule(id.ToString())?.SetActive(value);
                    break;
                case ModuleActions.Visible:
                    Module.GetModule(id.ToString())?.SetVisible(value);
                    break;
                case ModuleActions.EnabledActiveAndVisible:
                    Module.GetModule(id.ToString())?.SetStates(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    
    
        [ConsoleCommand("Monitoring", Description = "Set the state of every loaded monitoring module")]
        private static void MonitoringState(ModuleActions action, bool value)
        {
            switch (action)
            {
                case ModuleActions.Active:
                    Module.EnableAll(value);
                    break;
                case ModuleActions.Enabled:
                    Module.ActivateAll(value);
                    break;
                case ModuleActions.Visible:
                    Module.ShowAll(value);
                    break;
                case ModuleActions.EnabledActiveAndVisible:
                    Module.EnableActivateAndShowAllModules(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
            Console.Core.Console.Log($"Set monitoring to {action} state to {value}", LogOptions.Tab);
        }
        
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMANDS MODULE NOTES] ---
        
        private const int CommandPriority = 100;
        private const CmdBuildSettings EditorOnly = CmdBuildSettings.ExcludeFromBuild;
        
        
        
        private const string AddNoteDescription= "Add a note to the list and synchronize it with the TextAsset";
        
        [ConsoleCommand("AddNote", Description = AddNoteDescription, Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void AddNote([Hint("The content you want to add")]string add)
        {
            if (add.IsNullOrWhiteSpace())
            {
                Console.Core.Console.Log($"Cannot add a note without content", LogOptions.Tab);
                return;
            }
            ModuleNotes.Instance.AddNoteAndCompile(add);
            Console.Core.Console.Log($"Added Note: {add}", LogOptions.Tab);
        }
        
        
        
        private const string CheckNoteDescription= "Label a note as completed without removing it";
        
        [ConsoleCommand("CheckNote", Description = CheckNoteDescription, DisableNBP = true, Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void CheckNote([Hint("The index of the note")]int index, [Hint("Label as completed or not")]bool @checked = true)
        {
            Console.Core.Console.Log($"{(@checked? "Checked" : "Unchecked")} Note: {index}", LogOptions.Tab);
            ModuleNotes.Instance.SetNote(index, @checked);
        }

        
        
        private const string RemoveNoteDescription= "Remove the specified note from the list";
        
        [ConsoleCommand("RemoveNote", Description = RemoveNoteDescription, Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void RemoveNote([Hint("The index of the note")]int index)
        {
            Console.Core.Console.Log($"Removed Note: {index}", LogOptions.Tab);
            ModuleNotes.Instance.RemoveNoteAndCompile(index);
        }

        
        
        private const string ClearNotesDescription= "Remove either checked (default) or all notes form the list.";
        
        [ConsoleCommand("ClearNotes", Description = ClearNotesDescription, Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void ClearNote(NoteClearOptions options = NoteClearOptions.ClearCompleted)
        {
            switch (options)
            {
                case NoteClearOptions.ClearAll:
                    ModuleNotes.Instance.RemoveAllDataAndCompile();
                    Console.Core.Console.Log($"Cleared all Notes:", LogOptions.Tab);
                    return;
                case NoteClearOptions.ClearCompleted:
                    ModuleNotes.Instance.RemoveCompletedDataAndCompile();
                    Console.Core.Console.Log($"Cleared completed Notes:", LogOptions.Tab);
                    return;
            }
        }

        public enum NoteClearOptions
        {
            ClearAll,
            ClearCompleted,
        }

        #endregion
    }
#endif
}
