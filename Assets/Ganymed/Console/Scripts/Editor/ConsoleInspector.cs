using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ganymed.Console.Scripts.Editor
{
    [CustomEditor(typeof(Core.Console))]
    [CanEditMultipleObjects]
    public sealed class ConsoleInspector : CleanEditor
    {
        private Core.Console console;

        private SerializedObject so;
        
        private SerializedProperty OnConsoleEnabled;
        private SerializedProperty OnConsoleDisabled;
        private SerializedProperty OnConsoleLogReceived;

        private void OnEnable()
        {
            console = (Core.Console) target;
            so = new SerializedObject(target);

            OnConsoleEnabled = so.FindProperty(nameof(console.OnConsoleEnabled));
            OnConsoleDisabled = so.FindProperty(nameof(console.OnConsoleDisabled));
            OnConsoleLogReceived = so.FindProperty(nameof(console.OnConsoleLogReceived));
        }

        protected override void OnBeforeDefaultInspector()
        {
            EditorGUILayout.Space();
            so.Update();
            EditorGUILayout.PropertyField(OnConsoleEnabled);
            EditorGUILayout.PropertyField(OnConsoleDisabled);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(OnConsoleLogReceived);
            
            so.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnDefaultInspector()
        {
            console.misc = EditorGUILayout.ToggleLeft("Show Miscellaneous", console.misc);
            if(!console.misc) return;
            
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit Console Settings")) { ConsoleSettings.EditSettings(); }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            base.OnDefaultInspector();
        }

        protected override void OnAfterDefaultInspector()
        {
            EditorUtility.SetDirty(target);
        }
    }
}