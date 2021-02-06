using Ganymed.Monitoring.Configuration;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(CustomStyle))]
    [CanEditMultipleObjects]
    public class CustomStyleInspector : StyleBaseInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select Monitoring-Configuration"))
            {
                Selection.activeObject = MonitoringConfiguration.Instance;
            }
            if (GUILayout.Button("Apply Values of this Configuration as Default"))
            {
                EditorUtility.SetDirty(MonitoringConfiguration.Instance);
                MonitoringConfiguration.Instance.SetValues((CustomStyle)target); 
                EditorUtility.SetDirty(MonitoringConfiguration.Instance);
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}