using Ganymed.Console.Attributes;
using Ganymed.Utils.Singleton;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Placeholder
{
    /// <summary>
    /// Standalone Console Input module.
    /// This class is a placeholder and can be replaced by other input systems. 
    /// </summary>
    [DeclaringName("Console")]
    public class ConsoleInputStandalone : MonoSingleton<ConsoleInputStandalone>
    {
        [Getter]
        private static KeyCode ToggleKey => Instance.toggleKey;
        [Getter]
        private static KeyCode ApplyProposedKey => Instance.applyProposedKey;
        [Getter]
        private static KeyCode PreviousInputKey => Instance.previousInputKey;
        [Getter]
        private static KeyCode SubsequentInputKey => Instance.subsequentInputKey;
        
        
        [SerializeField] internal KeyCode toggleKey = KeyCode.Slash;
        [SerializeField] internal KeyCode applyProposedKey = KeyCode.Tab;
        [SerializeField] internal KeyCode previousInputKey = KeyCode.UpArrow;
        [SerializeField] internal KeyCode subsequentInputKey= KeyCode.DownArrow;


        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Core.Console.ToggleConsole();
            if (Input.GetKeyDown(applyProposedKey)) Core.Console.ApplyInputProposedByAutocompletion();
            if (Input.GetKeyDown(previousInputKey)) Core.Console.SelectPreviousInputFromCache();
            if (Input.GetKeyDown(subsequentInputKey)) Core.Console.SelectSubsequentInputFromCache();
        }
    }
    
    
    #if UNITY_EDITOR
    
    [UnityEditor.CustomEditor(typeof(ConsoleInputStandalone))]
    [UnityEditor.CanEditMultipleObjects]
    internal class ConsoleInputStandaloneInspector : UnityEditor.Editor
    {
        private ConsoleInputStandalone Target;
        
        private bool toggleKeySelected = true;
        private bool applyProposedKeySelected = true;
        private bool subsequentInputKeySelected = true;
        private bool previousInputKeySelected = true;

        private const int labelWidth = 300;


        private void OnEnable()
        {
            Target = (ConsoleInputStandalone) target;
        }

        public override void OnInspectorGUI()
        {
            var currentEvent = Event.current;
            
            GUILayout.BeginHorizontal();
            if (toggleKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField($"Toggle Key:", $"{Target.toggleKey}");
                // --- Toggle Key
                if (GUILayout.Button($"Select", GUILayout.MaxWidth(labelWidth))) toggleKeySelected = false;    
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    toggleKeySelected = true;
                    Target.toggleKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
            
            
            GUILayout.BeginHorizontal();
            if (applyProposedKeySelected)
            {
                EditorGUILayout.LabelField($"Apply Proposed Input Key:", $"{Target.applyProposedKey}");
                // --- Apply Proposed Key
                if (GUILayout.Button($"Select", GUILayout.MaxWidth(labelWidth))) applyProposedKeySelected = false;
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    applyProposedKeySelected = true;
                    Target.applyProposedKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
            
            
            GUILayout.BeginHorizontal();
            if (subsequentInputKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField($"Previous Input Key:", $"{Target.subsequentInputKey}");
                // --- Subsequent Input Key Selected
                if (GUILayout.Button($"Select", GUILayout.MaxWidth(labelWidth))) subsequentInputKeySelected = false;    
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    subsequentInputKeySelected = true;
                    Target.subsequentInputKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
            
            
            GUILayout.BeginHorizontal();
            if (previousInputKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField($"Subsequent Input Key:", $"{Target.previousInputKey}");
                // --- previousInputKeySelected
                if (GUILayout.Button($"Select", GUILayout.MaxWidth(labelWidth))) previousInputKeySelected = false;    
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    previousInputKeySelected = true;
                    Target.previousInputKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
    
    #endif
}
