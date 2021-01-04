using System;
using Ganymed.Console;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Monitoring.Core;
using Ganymed.Monitoring.Modules;
using UnityEngine;

namespace Ganymed.Ganymed
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
        
        [Command("Modules", "Receive a list of every loaded monitoring module")]
        private static void Modules()
        {
            const MessageOptions options = MessageOptions.Bold | MessageOptions.Brackets;
        
            Transmission.Start(TransmissionOptions.Enumeration);
        
            Transmission.AddLine(
                new Message($"Module", global::Ganymed.Console.Core.Console.colorTitles, options),
                new Message($"Description", global::Ganymed.Console.Core.Console.colorTitles, options),
                new Message($"Id", global::Ganymed.Console.Core.Console.colorTitles, options),
                new Message($"Active", global::Ganymed.Console.Core.Console.colorTitles , options),
                new Message($"Enabled", global::Ganymed.Console.Core.Console.colorTitles , options),
                new Message($"Type", global::Ganymed.Console.Core.Console.colorTitles , options));
            Transmission.AddBreak();
        
            foreach (var module in Module.ModuleDictionary)
            {
                Transmission.AddLine(
                    module.Value.UniqueName,
                    module.Value.Description ?? "n/a",
                    module.Value.UniqueId,
                    module.Value.IsActive,
                    module.Value.IsEnabled,
                    module.Value.GetTypeOfTAsString());
            }
            Transmission.ReleaseAsync();
        }

        
        [Command("SetModule", "Set the state of a monitoring module", 1000)]
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
    
        [Command("SetModule","Set the state of a monitoring module", 1000)]
        private static void SetModule(string id, ModuleActions action, bool value)
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
    
    
        [Command("SetAllModules", "Set the state of every loaded monitoring module")]
        private static void SetAllModules(ModuleActions action, bool value)
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
        }
    }
}
