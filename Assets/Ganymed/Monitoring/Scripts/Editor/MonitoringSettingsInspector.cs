using System.Collections.Generic;
using System.Linq;
using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Core;
using Ganymed.Utils;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringSettings))]
    [CanEditMultipleObjects]
    public class MonitoringSettingsInspector : UnityEditor.Editor
    {
        private MonitoringSettings Target = null;
        
        private SerializedObject so;
        
        private SerializedProperty modulesUpperLeft;
        private SerializedProperty modulesUpperRight;
        private SerializedProperty modulesLowerLeft;
        private SerializedProperty modulesLowerRight;

        private static readonly Color removeButtonColor = new Color(1f, 0.54f, 0.52f);

        private void OnEnable()
        {
            Target = (MonitoringSettings) target;
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
            Target.active = EditorGUILayout.ToggleLeft("Monitoring", Target.active, GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));
            
            EditorGUILayout.Space();
            
            Target.automateCanvasState = EditorGUILayout.Toggle("Activate OnPlay / OnEdit", Target.automateCanvasState);
            if (Target.automateCanvasState)
            {
                Target.openCanvasOnEnterPlay = EditorGUILayout.Toggle(new GUIContent(nameof(Target.openCanvasOnEnterPlay).AsLabel(),
                    Target.GetTooltip(nameof(Target.openCanvasOnEnterPlay))), Target.openCanvasOnEnterPlay);
                
                Target.closeCanvasOnEdit = EditorGUILayout.Toggle(new GUIContent(nameof(Target.closeCanvasOnEdit).AsLabel(),
                    Target.GetTooltip(nameof(Target.closeCanvasOnEdit))), Target.closeCanvasOnEdit);
            }
            EditorGUILayout.Space();
            
            Target.toggleKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent(nameof(Target.toggleKey).AsLabel(),
                Target.GetTooltip(nameof(Target.toggleKey))), Target.toggleKey);
            
            Target.sortingOrder = EditorGUILayout.IntField(new GUIContent(nameof(Target.sortingOrder).AsLabel(),
                Target.GetTooltip(nameof(Target.sortingOrder))), Target.sortingOrder);
            
            Target.enableWarnings = EditorGUILayout.Toggle(new GUIContent(nameof(Target.enableWarnings).AsLabel(),
                Target.GetTooltip(nameof(Target.enableWarnings))), Target.enableWarnings);
            
            EditorGUILayout.Space(10);
            
            
            Target.hideCanvasGameObject = EditorGUILayout.ToggleLeft(new GUIContent(nameof(Target.hideCanvasGameObject).AsLabel(),
                Target.GetTooltip(nameof(Target.hideCanvasGameObject))), Target.hideCanvasGameObject);
            
            Target.enableLifePreview = EditorGUILayout.ToggleLeft(new GUIContent(nameof(Target.enableLifePreview).AsLabel(),
                Target.GetTooltip(nameof(Target.enableLifePreview))), Target.enableLifePreview);
            
            MonitoringCanvasBehaviour.SetHideFlags(Target.hideCanvasGameObject? HideFlags.HideInHierarchy : HideFlags.None);
            
            EditorGUILayout.Space();


            #region --- [MODULE LAYOUT] ---

            so.Update();
            
            EditorGUILayout.Space();

            DrawArea(ref Target.modulesUpperLeft, ref modulesUpperLeft, "Modules Upper Left", true);
            DrawArea(ref Target.modulesUpperRight, ref modulesUpperRight, "Modules Upper Right");
            DrawArea(ref Target.modulesLowerLeft, ref modulesLowerLeft, "Modules Lower Left");
            DrawArea(ref Target.modulesLowerRight, ref modulesLowerRight, "Modules Lower Right");
            
            so.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            
            // Repaint if GUI has changed and LifePreview is enabled
            if (GUI.changed && Target.enableLifePreview)
            {
                MonitorBehaviour.Instance.Repaint(); 
            }

            #endregion

            DrawGlobalStyleSettings();

            
            EditorGUILayout.Space();
            Target.showReferences = EditorGUILayout.ToggleLeft(new GUIContent("Misc".Cut(),
                Target.GetTooltip(nameof(Target.showReferences))), Target.showReferences);
            
            if (Target.showReferences) { DrawMisc(); }
            
            EditorUtility.SetDirty(target);
            if(GUI.changed)
                Target.OnValidate();
        }

        private void DrawGlobalStyleSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Style", GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));
            EditorGUILayout.Space();
            
            Target.Style = EditorGUILayout.ObjectField(
                new GUIContent(nameof(Target.Style).AsLabel("Default"), Target.GetTooltip(nameof(Target.Style))),
                Target.Style,
                typeof(Style), true) as Style;
            
            if (Target.Style == null)
            {
                EditorGUILayout.HelpBox("Default style must be assigned an cannot be null!", MessageType.Error);
            }

            EditorGUILayout.Space();
            
            EditorGUILayout.Space();

            Target.canvasPadding = EditorGUILayout.FloatField("Global Padding", Target.canvasPadding);
            Target.canvasMargin = EditorGUILayout.FloatField("Global Margin", Target.canvasMargin);
            Target.elementSpacing = EditorGUILayout.FloatField("Global Element Spacing", Target.elementSpacing);
            Target.areaSpacing = EditorGUILayout.FloatField("Global Area Spacing", Target.areaSpacing);

            EditorGUILayout.Space();

            //Background
            Target.showBackground = EditorGUILayout.Toggle("Enable Canvas Background", Target.showBackground);
            if (Target.showBackground)
            {
                Target.colorCanvasBackground = EditorGUILayout.ColorField("Enable Background", Target.colorCanvasBackground);
                EditorGUILayout.Space();
            }

            //Background Areas 
            Target.showAreaBackground = EditorGUILayout.Toggle("Enable Area Background", Target.showAreaBackground);
            if (Target.showAreaBackground)
            {
                Target.colorTopLeft = EditorGUILayout.ColorField(new GUIContent(nameof(Target.colorTopLeft).AsLabel(),
                    Target.GetTooltip(nameof(Target.colorTopLeft))), Target.colorTopLeft);

                Target.colorTopRight = EditorGUILayout.ColorField(new GUIContent(nameof(Target.colorTopRight).AsLabel(),
                    Target.GetTooltip(nameof(Target.colorTopRight))), Target.colorTopRight);

                Target.colorBottomLeft = EditorGUILayout.ColorField(new GUIContent(nameof(Target.colorBottomLeft).AsLabel(),
                    Target.GetTooltip(nameof(Target.colorBottomLeft))), Target.colorBottomLeft);

                Target.colorBottomRight = EditorGUILayout.ColorField(new GUIContent(nameof(Target.colorBottomRight).AsLabel(),
                    Target.GetTooltip(nameof(Target.colorBottomRight))), Target.colorBottomRight);
            }

            EditorGUILayout.Space();
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
            if (GUILayout.Button("Validate Canvas Instance"))
            {
                MonitorBehaviour.Instance.ValidateCanvas(InvokeOrigin.GUI);
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
            if (GUILayout.Button("Clear BreakContext Fields"))
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
            //.DrawLine(new Color(.8f, .8f, .9f, .5f));
            EditorGUILayout.Space(12);

            #endregion
        }
    }
}
