using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Scripts.Editor
{
    [CustomEditor(typeof(ConsoleConfiguration))]
    public class ConsoleConfigurationEditor : UnityEditor.Editor
    {
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Command Configuration", InspectorDrawer.H1);
            InspectorDrawer.DrawLine(Color.gray);
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            InspectorDrawer.DrawLine(Color.gray);
        }
    }
}
