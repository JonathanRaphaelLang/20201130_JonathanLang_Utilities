using System.Collections.Generic;
using System.Linq;
using Ganymed.Monitoring.Core;
using UnityEngine;

namespace Ganymed
{
    [CreateAssetMenu(fileName = "Module_Console", menuName = "Monitoring/ModuleDictionary/Console")]
    public class ModuleConsole : Module<string>
    {
        [SerializeField] private ushort cacheSize = 3;
        private readonly List<string> cache = new List<string>();

        protected override void OnBeforeUpdate(string currentValue)
        {
            if (cache.Contains(currentValue))
                cache.Remove(currentValue);
        
            if(cache.Count >= cacheSize)
                cache.RemoveAt(0);
        
            cache.Add(currentValue);
        }

        protected override string ValueToString(string currentValue)
        {
            return cache.Aggregate(string.Empty, (current, cached) => current + cached);
        }
    
        protected override void OnInitialize()
        {
            SetUpdateDelegate(ref Console.Core.Console.OnLog);
            SetActiveDelegate(ref Console.Core.Console.OnToggle, true);
        }
    }
}
