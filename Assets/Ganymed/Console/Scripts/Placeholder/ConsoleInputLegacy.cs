using Ganymed.Console.Attributes;
using Ganymed.Utils.Singleton;
using UnityEngine;

namespace Ganymed.Console.Placeholder
{
    [DeclaringName("ConsoleInput")]
    public class ConsoleInputLegacy : MonoSingleton<ConsoleInputLegacy>
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

        private IConsoleEntry consoleEntry;

        protected override void Awake()
        {
            base.Awake();
            consoleEntry = GetComponent<IConsoleEntry>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) consoleEntry.Toggle();
            if (Input.GetKeyDown(applyProposedKey)) consoleEntry.ApplyProposedInput();
            if (Input.GetKeyDown(previousInputKey)) consoleEntry.SelectPreviousInputFromCache();
            if (Input.GetKeyDown(subsequentInputKey)) consoleEntry.SelectSubsequentInputFromCache();
        }
    }
    
    
    #if UNITY_EDITOR
    
    [UnityEditor.CustomEditor(typeof(ConsoleInputLegacy))]
    internal class ConsoleInputLegacyEditor : UnityEditor.Editor
    {
        private ConsoleInputLegacy Target;
        
        private bool toggleKeySelected = true;
        private bool applyProposedKeySelected = true;
        private bool subsequentInputKeySelected = true;
        private bool previousInputKeySelected = true;


        private void OnEnable()
        {
            Target = (ConsoleInputLegacy) target;
        }

        public override void OnInspectorGUI()
        {
            var currentEvent = Event.current;
            
            UnityEditor.EditorGUILayout.LabelField($"Toggle Key:", $"{Target.toggleKey}");
            UnityEditor.EditorGUILayout.LabelField($"Apply Proposed Input Key:", $"{Target.applyProposedKey}");
            UnityEditor.EditorGUILayout.LabelField($"Previous Input Key:", $"{Target.previousInputKey}");
            UnityEditor.EditorGUILayout.LabelField($"Subsequent Input Key:", $"{Target.subsequentInputKey}");
            
            
            UnityEditor.EditorGUILayout.Space();

            // --- Toggle Key
            if (GUILayout.Button($"Toggle Key: {Target.toggleKey}")) toggleKeySelected = false;
            if (!toggleKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    toggleKeySelected = true;
                    Target.toggleKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            
            // --- Apply Proposed Key
            if (GUILayout.Button($"Apply Proposed Input Key: {Target.applyProposedKey}")) applyProposedKeySelected = false;
            if (!applyProposedKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    applyProposedKeySelected = true;
                    Target.applyProposedKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            
            
            // --- Subsequent Input Key Selected
            if (GUILayout.Button($"Previous Input Key: {Target.previousInputKey}")) subsequentInputKeySelected = false;
            if (!subsequentInputKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    subsequentInputKeySelected = true;
                    Target.subsequentInputKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
            
            
            // --- previousInputKeySelected
            if (GUILayout.Button($"Subsequent Input Key: {Target.subsequentInputKey}")) previousInputKeySelected = false;
            if (!previousInputKeySelected)
            {
                UnityEditor.EditorGUILayout.LabelField("Please select a new Key");
                if (currentEvent.isKey)
                {
                    previousInputKeySelected = true;
                    Target.previousInputKey = currentEvent.keyCode;
                    UnityEditor.EditorUtility.SetDirty(target);
                }
            }
        }
    }
    
    #endif
}
