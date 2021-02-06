using System.Collections.Generic;
using System.Linq;
using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Core;
using Ganymed.Utils;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Optimization;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitorBehaviour))]
    public class MonitorBehaviourInspector : UnityEditor.Editor
    {
        private MonitorBehaviour Target = null;

        private SerializedObject so;
        
        private SerializedProperty modulesUpperLeft;
        private SerializedProperty modulesUpperRight;
        private SerializedProperty modulesLowerLeft;
        private SerializedProperty modulesLowerRight;

        private static readonly Color removeButtonColor = new Color(1f, 0.54f, 0.52f);

        private void OnEnable()
        {
            Target = (MonitorBehaviour) target;
            so = new SerializedObject(target);
            
            modulesUpperLeft = so.FindProperty(nameof(Target.modulesUpperLeft));
            modulesUpperRight = so.FindProperty(nameof(Target.modulesUpperRight));
            modulesLowerLeft = so.FindProperty(nameof(Target.modulesLowerLeft));
            modulesLowerRight = so.FindProperty(nameof(Target.modulesLowerRight));
            
            modulesUpperLeft.isExpanded = true;
            modulesUpperRight.isExpanded = true;
            modulesLowerLeft.isExpanded = true;
            modulesLowerRight.isExpanded = true;
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .5f));
            Target.config = EditorGUILayout.ObjectField(
                new GUIContent("Configuration", Target.GetTooltip(nameof(Target.config))),
                Target.config,
                typeof(MonitoringConfiguration), true) as MonitoringConfiguration;
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .5f));
            
            EditorGUILayout.Space();
            
            Target.enableLifePreview = EditorGUILayout.ToggleLeft(new GUIContent("Enable Life Preview",
                    Target.GetTooltip(nameof(Target.enableLifePreview))), Target.enableLifePreview);
            
            Target.hideCanvas = EditorGUILayout.ToggleLeft(new GUIContent("Hide Canvas GameObject",
                Target.GetTooltip(nameof(Target.hideCanvas))), Target.hideCanvas);
            
            Target.showReferences = EditorGUILayout.ToggleLeft(new GUIContent("Misc".Cut(),
                Target.GetTooltip(nameof(Target.showReferences))), Target.showReferences);
            
            if (Target.showReferences) { DrawMisc(); }
            
            
            if(Target.SetRootObject != null)
                Target.SetRootObject.hideFlags = Target.showRootComponent ? HideFlags.None : HideFlags.HideInInspector;
            
            
            MonitoringCanvasBehaviour.SetHideFlags(Target.hideCanvas? HideFlags.HideInHierarchy : HideFlags.None);
            
            EditorGUILayout.Space();

            DrawArea(ref Target.modulesUpperLeft, ref modulesUpperLeft, "Modules Upper Left", true);
            DrawArea(ref Target.modulesUpperRight, ref modulesUpperRight, "Modules Upper Right");
            DrawArea(ref Target.modulesLowerLeft, ref modulesLowerLeft, "Modules Lower Left");
            DrawArea(ref Target.modulesLowerRight, ref modulesLowerRight, "Modules Lower Right");
            
            so.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            
            if (GUI.changed && Target.enableLifePreview)
            {
                Target.Repaint(); 
            }
        }

        private void DrawMisc()
        {
            Target.GUIElementPrefab =
                EditorGUILayout.ObjectField(
                    new GUIContent("GUI Element Prefab", Target.GetTooltip(nameof(Target.GUIElementPrefab))),
                    Target.GUIElementPrefab,
                    typeof(GameObject), true) as GameObject;
                
            Target.GUIObjectPrefab =
                EditorGUILayout.ObjectField(
                    new GUIContent("GUI Object Prefab", Target.GetTooltip(nameof(Target.GUIObjectPrefab))),
                    Target.GUIObjectPrefab,
                    typeof(GameObject), true) as GameObject;
                
            EditorGUILayout.Space();
            
            Target.SetRootObject =
                EditorGUILayout.ObjectField(
                    new GUIContent("Set Root On Load", Target.GetTooltip(nameof(Target.GUIObjectPrefab))),
                    Target.SetRootObject,
                    typeof(SetRootOnLoad), true) as SetRootOnLoad;
            
            Target.showRootComponent = EditorGUILayout.ToggleLeft(new GUIContent("Show SetRoot Component",
                Target.GetTooltip(nameof(Target.showRootComponent))), Target.showRootComponent);

            EditorGUILayout.Space();
            if (GUILayout.Button("Validate Canvas Instance"))
            {
                Target.ValidateCanvas(InvokeOrigin.GUI);
            }
            EditorGUILayout.Space();
        }

        private static void DrawArea(
            ref List<Module> moduleList,
            ref SerializedProperty property,
            string title,
            bool mainTitle = false,
            int labelWidth = 160)
        {
            #region --- [AREA LOWER LEFT] ---

            EditorGUILayout.Space();
            if(mainTitle) EditorGUILayout.LabelField("Modules", GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .5f));
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, GUILayout.MaxWidth(labelWidth));
            if (GUILayout.Button("Add"))
            {
                moduleList = new List<Module>(moduleList) {null};
            }
            if (GUILayout.Button("Clear Empty Fields"))
            {
                moduleList = moduleList.Where(element => element != null).ToList();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(property.arraySize > 0? 4f : 0);
            
            for (var i = 0; i < property.arraySize; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal();
                var originalValue = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;  
                EditorGUILayout.PropertyField(
                    property.GetArrayElementAtIndex(i),
                    new GUIContent($"Module [{i:00}]"));
                EditorGUIUtility.labelWidth = originalValue;
                
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("[Up]"))
                {
                    if (i > 0)
                    {
                        var list = new List<Module>(moduleList);

                        var cachedA = moduleList[i - 1];
                        var cachedB = moduleList[i];

                        list[i - 1] = cachedB;
                        list[i] = cachedA;

                        moduleList = list;
                    }
                }
                
                if (GUILayout.Button("[Down]"))
                {
                    if (i < moduleList.Indices())
                    {
                        var list = new List<Module>(moduleList);

                        var cachedA = moduleList[i + 1];
                        var cachedB = moduleList[i];

                        list[i + 1] = cachedB;
                        list[i] = cachedA;

                        moduleList = list;
                    }
                }
                GUILayout.EndHorizontal();
                
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = removeButtonColor;
                if (GUILayout.Button("[REMOVE]", new GUIStyle(GUI.skin.button) {normal = {textColor = Color.white}}))
                {
                    var list = new List<Module>(moduleList);
                    list.RemoveAt(i);
                    moduleList = list;
                }
                GUI.backgroundColor = oldColor;
                GUILayout.EndHorizontal();
            }
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .5f));
            EditorGUILayout.Space();

            #endregion
        }
    }
}