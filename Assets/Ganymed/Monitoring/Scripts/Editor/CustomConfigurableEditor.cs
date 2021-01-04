using Ganymed.Monitoring.Configuration;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(CustomStyle))]
    [CanEditMultipleObjects]
    public class CustomConfigurableEditor : ConfigurableEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Select Configuration"))
            {
                Selection.activeObject = GlobalConfiguration.Instance;
            }
            if (GUILayout.Button("Apply Values as default"))
            {
                EditorUtility.SetDirty(GlobalConfiguration.Instance);
                GlobalConfiguration.Instance.SetValues((CustomStyle)target); 
                EditorUtility.SetDirty(GlobalConfiguration.Instance);
            }
        }
    }
}