using System;
using Ganymed.Console;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Monitoring.Core;
using Ganymed.Monitoring.Modules;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

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

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [MONITORING] ---

        [ConsoleCommand("GetModules", Description = "Receive a list of every loaded monitoring module")]
        private static void GetModules()
        {
            const MessageOptions options = MessageOptions.Brackets;
            Transmission.Start(TransmissionOptions.Enumeration);
        
            Transmission.AddLine(
                new MessageFormat($"Module", Console.Core.Console.ColorTitleSub, options),
                new MessageFormat($"Id", Console.Core.Console.ColorTitleSub, options),
                new MessageFormat($"Enabled", Console.Core.Console.ColorTitleSub , options),
                new MessageFormat($"Active", Console.Core.Console.ColorTitleSub , options),
                new MessageFormat($"Visible", Console.Core.Console.ColorTitleSub , options),
                new MessageFormat($"Type", Console.Core.Console.ColorTitleSub , options),
                new MessageFormat($"Description", Console.Core.Console.ColorTitleSub, options));
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
        private static void SetModule([Suggestion("System")]string id, ModuleActions action, bool value)
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
                    Module.InitializeAll(value);
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
        
        [ConsoleCommand("AddNote", Priority = CommandPriority, BuildSettings = EditorOnly)]
        private static void AddNote(string add)
        {
            ModuleNotes.Instance.AddNoteAndCompile(add);
            Console.Core.Console.Log($"Added Note: {add}", LogOptions.Tab);
        }

        [ConsoleCommand("CheckNote", DisableNBP = true, Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void CheckNote(int index, bool @checked = true)
        {
            Console.Core.Console.Log($"{(@checked? "Checked" : "Unchecked")} TODO: {index}", LogOptions.Tab);
            ModuleNotes.Instance.SetNote(index, @checked);
        }


        [ConsoleCommand("RemoveNote", Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void RemoveNote(int index)
        {
            Console.Core.Console.Log($"Removed TODO: {index}", LogOptions.Tab);
            ModuleNotes.Instance.RemoveNoteAndCompile(index);
        }

        [ConsoleCommand("ClearNote", Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void ClearNote(NoteClearOptions options)
        {
            switch (options)
            {
                case NoteClearOptions.ClearAll:
                    ModuleNotes.Instance.RemoveAllDataAndCompile();
                    Console.Core.Console.Log($"Cleared all Notes", LogOptions.Tab);
                    return;
                case NoteClearOptions.ClearCompleted:
                    ModuleNotes.Instance.RemoveCompletedDataAndCompile();
                    Console.Core.Console.Log($"Cleared completed Notes", LogOptions.Tab);
                    return;
            }
        }
        
        [ConsoleCommand("AlterNote", Priority = CommandPriority, BuildSettings = EditorOnly)]
        public static void AlterNote(int index, bool test, string content)
        {
            RemoveNote(index);
            AddNote(content);
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
