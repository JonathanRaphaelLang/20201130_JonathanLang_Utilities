using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Optimization;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Utils.Editor
{
    [CustomEditor(typeof(OptimizationManager), true), CanEditMultipleObjects]
    internal class OptimizationManagerInspector : UnityEditor.Editor
    {
        private OptimizationManager Target;
        
        private void OnEnable()
        {
            Target = (OptimizationManager) target;
        }
    
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Optimization", GUIHelper.H1);
            GUIHelper.DrawLine();
            
            EditorGUILayout.HelpBox("This is a WIP custom configuration for optimization helper.", MessageType.Info);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Hierarchy", GUIHelper.H2);
            GUIHelper.DrawLine();
            
            Target.enableUnfoldingOnLoad = EditorGUILayout.Toggle(
                new GUIContent(nameof(Target.enableUnfoldingOnLoad).AsLabel(), Target.GetTooltip(nameof(Target.enableUnfoldingOnLoad))),
                Target.enableUnfoldingOnLoad
            );
            Target.enableSetRootOnLoad = EditorGUILayout.Toggle(
                new GUIContent(nameof(Target.enableSetRootOnLoad).AsLabel(), Target.GetTooltip(nameof(Target.enableSetRootOnLoad))),
                Target.enableSetRootOnLoad
            );
            Target.enableDestroyOnLoad = EditorGUILayout.Toggle(
                new GUIContent(nameof(Target.enableDestroyOnLoad).AsLabel(), Target.GetTooltip(nameof(Target.enableDestroyOnLoad))),
                Target.enableDestroyOnLoad
            );
            
            EditorGUILayout.Space();
            Target.enableComponentWarnings = EditorGUILayout.Toggle(
                new GUIContent(nameof(Target.enableComponentWarnings).AsLabel(), Target.GetTooltip(nameof(Target.enableComponentWarnings))),
                Target.enableComponentWarnings);
            
            Target.enableLogs = EditorGUILayout.Toggle(
                new GUIContent(nameof(Target.enableLogs).AsLabel(), Target.GetTooltip(nameof(Target.enableLogs))),
                Target.enableLogs);
            
            Target.hideComponentsInInspector = EditorGUILayout.Toggle(
                new GUIContent(nameof(Target.hideComponentsInInspector).AsLabel(), Target.GetTooltip(nameof(Target.hideComponentsInInspector))),
                Target.hideComponentsInInspector);
            
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(nameof(UnfoldObjectOnLoad).AsLabel("Select")))
            {
                SelectionHelper.SelectComponents<UnfoldObjectOnLoad>(true, Target.enableLogs);
            }
            if (GUILayout.Button(nameof(SetRootOnLoad).AsLabel("Select")))
            {
                SelectionHelper.SelectComponents<SetRootOnLoad>(true, Target.enableLogs);
            }
            if (GUILayout.Button(nameof(DestroyOnLoad).AsLabel("Select")))
            {
                SelectionHelper.SelectComponents<DestroyOnLoad>(true, Target.enableLogs);
            }
            
            if (GUILayout.Button("Select All Components"))
            {
                SelectionHelper.SelectComponents<TransformOptimizationComponent>(true, Target.enableLogs);
            }
            
            EditorGUILayout.EndHorizontal();
    
            Target.Validate();
        }
    }
}