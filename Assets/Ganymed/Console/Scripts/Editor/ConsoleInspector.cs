using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Scripts.Editor
{
    [CustomEditor(typeof(Core.Console))]
    [CanEditMultipleObjects]
    public sealed class ConsoleInspector : UnityEditor.Editor
    {
        private Core.Console console;

        private void OnEnable()
        {
            console = (Core.Console) target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit Settings"))
            {
                ConsoleSettings.EditSettings();
            }
            
            
            EditorGUILayout.Space();
            if (ConsoleSettings.Instance == null)
            {
                UnityEditor.EditorGUILayout.HelpBox(
                    "Settings must be assigned!",
                    UnityEditor.MessageType.Error);
                EditorGUILayout.Space();
            }
            console.showReferences = EditorGUILayout.ToggleLeft("Misc", console.showReferences);
            if (!console.showReferences) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
            EditorUtility.SetDirty(target);
        }
    }
}