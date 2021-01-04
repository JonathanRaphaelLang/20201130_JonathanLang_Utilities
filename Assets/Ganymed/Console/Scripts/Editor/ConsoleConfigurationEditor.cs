using Ganymed.Console.Core;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Scripts.Editor
{
    [CustomEditor(typeof(ConsoleConfiguration))]
    public class ConsoleConfigurationEditor : UnityEditor.Editor
    {
        private bool toggleKeySelected = true;
        private bool subsequentInputKeySelected = true;
        private bool previousInputKeySelected = true;
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Command Configuration", InspectorDrawer.H1);
            InspectorDrawer.DrawLine(Color.gray);
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            InspectorDrawer.DrawLine(Color.gray);
            
            var config = (ConsoleConfiguration) target;
            var _event = Event.current;

            #region --- [TOGGLE] ---

            if (GUILayout.Button("Select TOGGLE Key"))
            {
                toggleKeySelected = false;
            }

            if (toggleKeySelected == false)
            {
                EditorGUILayout.LabelField("SELECT KEY!");
                if (_event.isKey)
                {
                    Debug.Log($"Selected Key: {_event.keyCode}");
                    toggleKeySelected = true;
                    config.ToggleKey = _event.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            
            EditorGUILayout.LabelField("TOGGLE Key:", config.ToggleKey.ToString());
            EditorGUILayout.Space();

            #endregion
            
            //----------------------------------------------------------------------------------------------------------
            
            #region --- [SUBSEQUENT] ---

            if (GUILayout.Button("Select SUBSEQUENT Key"))
            {
                subsequentInputKeySelected = false;
            }

            if (subsequentInputKeySelected == false)
            {
                EditorGUILayout.LabelField("SELECT KEY!");
                if (_event.isKey)
                {
                    Debug.Log($"Selected Key: {_event.keyCode}");
                    subsequentInputKeySelected = true;
                    config.SubsequentInput = _event.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            
            EditorGUILayout.LabelField("SUBSEQUENT Key:", config.SubsequentInput.ToString());
            EditorGUILayout.Space();

            #endregion

            //----------------------------------------------------------------------------------------------------------
            
            #region --- [PREVOIUS] ---

            if (GUILayout.Button("Select PREVIOUS Key"))
            {
                previousInputKeySelected = false;
            }

            if (previousInputKeySelected == false)
            {
                EditorGUILayout.LabelField("SELECT KEY!");
                if (_event.isKey)
                {
                    Debug.Log($"Selected Key: {_event.keyCode}");
                    previousInputKeySelected = true;
                    config.PreviousInput = _event.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            
            EditorGUILayout.LabelField("PREVIOUS Key:", config.PreviousInput.ToString());
            EditorGUILayout.Space();

            #endregion


            if (UnityEngine.GUI.changed)
            {
                config.ConfigurationAltered();
            }
        }
    }
}
