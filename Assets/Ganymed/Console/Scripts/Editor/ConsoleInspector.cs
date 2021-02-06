using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
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
            EditorGUILayout.Space();
            if (console.config == null)
            {
                UnityEditor.EditorGUILayout.HelpBox(
                    "Configuration must be assigned!",
                    UnityEditor.MessageType.Error);
                EditorGUILayout.Space();
            }

            var cached = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;   
            console.config =
                EditorGUILayout.ObjectField(
                    new GUIContent(
                        "Configuration",
                        console.GetTooltip(nameof(console.config))),
                    console.config,
                    typeof(ConsoleConfiguration),
                    true) as ConsoleConfiguration;
            EditorGUIUtility.labelWidth = cached;   
            
            
            EditorGUILayout.Space();
            GUIHelper.DrawLine();
            console.showReferences = EditorGUILayout.ToggleLeft("Misc", console.showReferences);
            if (!console.showReferences) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
}