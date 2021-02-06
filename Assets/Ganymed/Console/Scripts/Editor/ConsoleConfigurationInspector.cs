using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Scripts.Editor
{
    [CustomEditor(typeof(ConsoleConfiguration))]
    public class ConsoleConfigurationInspector : UnityEditor.Editor
    {
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Console/Command Configuration", GUIHelper.H1);
            GUIHelper.DrawLine(Color.gray);
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            GUIHelper.DrawLine(Color.gray);
        }
    }
}
