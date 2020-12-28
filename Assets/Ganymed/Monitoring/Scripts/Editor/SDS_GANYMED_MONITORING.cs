using UnityEditor;

namespace Ganymed.Monitoring.Editor
{
    [InitializeOnLoad]
    internal sealed class SDS_GANYMED_MONITORING
    {
        private const string define = "GANYMED_MONITORING";
        
        static SDS_GANYMED_MONITORING()
        {
            var defineString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        
            if (defineString.Contains(define)) return;

            // Cut whitespace at the end of the string to determine if it ends with ";"
            while (defineString.EndsWith(" ") && defineString.Length > 0)
            {
                defineString = defineString.Remove(defineString.Length - 1, 1);
            }
            
            defineString += defineString.EndsWith(";") ? $"{define}" : $";{define}";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defineString);
        }
    }
}