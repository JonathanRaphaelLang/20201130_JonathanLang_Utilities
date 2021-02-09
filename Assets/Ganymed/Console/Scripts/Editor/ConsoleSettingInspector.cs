using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Scripts.Editor
{
    [CustomEditor(typeof(ConsoleSettings))]
    public class ConsoleSettingInspector : CleanEditor
    {
        private ConsoleSettings Target;
        
        private void OnEnable()
        {
            Target = (ConsoleSettings) target;
        }

        protected override void OnBeforeDefaultInspector()
        {
            EditorGUILayout.LabelField("Console / Command Settings", GUIHelper.H1);
            GUIHelper.DrawLine(Color.gray);
        }

        protected override void OnAfterDefaultInspector()
        {
            
            EditorGUILayout.Space();
            GUIHelper.DrawLine(Color.gray);
            EditorUtility.SetDirty(target);
        }
    }
}
