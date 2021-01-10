using System;
using Ganymed.Console;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Monitoring.Core;
using UnityEngine;

namespace Ganymed
{
    public class ModuleCommands : MonoBehaviour
    {
        
        #region --- [FIELDS] ---

        private enum ModuleActions
        {
            Active,
            Enabled,
            ActiveAndEnabled
        }

        #endregion
        
        [Command("GetModules", Description = "Receive a list of every loaded monitoring module")]
        private static void GetModules()
        {
            const MessageOptions options = MessageOptions.Brackets;
            Transmission.Start(TransmissionOptions.Enumeration);
        
            Transmission.AddLine(
                new Message($"Module", Console.Core.Console.ColorTitleSub, options),
                new Message($"Id", Console.Core.Console.ColorTitleSub, options),
                new Message($"Active", Console.Core.Console.ColorTitleSub , options),
                new Message($"Enabled", Console.Core.Console.ColorTitleSub , options),
                new Message($"Type", Console.Core.Console.ColorTitleSub , options),
                new Message($"Description", Console.Core.Console.ColorTitleSub, options));
            Transmission.AddBreak();
        
            foreach (var module in Module.ModuleDictionary)
            {
                Transmission.AddLine(
                    module.Value.UniqueName,
                    module.Value.UniqueId,
                    module.Value.IsActive,
                    module.Value.IsEnabled,
                    module.Value.GetTypeOfTAsString(),
                    module.Value.Description ?? "n/a");
            }
            Transmission.ReleaseAsync();
        }

        
        [Command("SetModule", Description = "Set the state of a monitoring module", Priority = 1000)]
        private static void SetModule(int id, ModuleActions action, bool value)
        {
            switch (action)
            {
                case ModuleActions.Active:
                    Module.GetModule(id)?.SetActive(value);
                    break;
                case ModuleActions.Enabled:
                    Module.GetModule(id)?.SetEnabled(value);
                    break;
                case ModuleActions.ActiveAndEnabled:
                    Module.GetModule(id)?.SetActiveAndEnabled(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    
        [Command("SetModule", Description = "Set the state of a monitoring module", Priority = 1000)]
        private static void SetModule([Suggestion("System")]string id, ModuleActions action, bool value)
        {
            switch (action)
            {
                case ModuleActions.Active:
                    Module.GetModule(id)?.SetActive(value);
                    break;
                case ModuleActions.Enabled:
                    Module.GetModule(id)?.SetEnabled(value);
                    break;
                case ModuleActions.ActiveAndEnabled:
                    Module.GetModule(id)?.SetActiveAndEnabled(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    
    
        [Command("Monitoring", Description = "Set the state of every loaded monitoring module")]
        private static void MonitoringState(ModuleActions action, bool value)
        {
            switch (action)
            {
                case ModuleActions.Active:
                    Module.ActivateAll(value);
                    break;
                case ModuleActions.Enabled:
                    Module.EnableAll(value);
                    break;
                case ModuleActions.ActiveAndEnabled:
                    Module.ActivateAndEnableAll(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
            Console.Core.Console.Log($"Set monitoring to {action} state to {value}", LogOptions.Tab);
        }
    }
}
