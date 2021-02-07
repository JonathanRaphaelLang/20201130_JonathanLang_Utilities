using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringConfiguration))]
    [CanEditMultipleObjects]
    public class MonitoringConfigurationInspector : StyleBaseInspector
    {
        private MonitoringConfiguration Target;

        protected override void OnEnable()
        {
            base.OnEnable();
            Target = (MonitoringConfiguration) target;
        }

        public override void OnInspectorGUI()
        {
            Target.active = EditorGUILayout.ToggleLeft("Monitoring", Target.active, GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));
            
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
            
            Target.logValidationEvents = EditorGUILayout.Toggle(new GUIContent(nameof(Target.logValidationEvents).AsLabel(),
                Target.GetTooltip(nameof(Target.logValidationEvents))), Target.logValidationEvents);
            
            EditorGUILayout.Space(10);
            
            
            Target.hideCanvasGameObject = EditorGUILayout.ToggleLeft(new GUIContent(nameof(Target.hideCanvasGameObject).AsLabel(),
                Target.GetTooltip(nameof(Target.hideCanvasGameObject))), Target.hideCanvasGameObject);
            
            Target.enableLifePreview = EditorGUILayout.ToggleLeft(new GUIContent(nameof(Target.enableLifePreview).AsLabel(),
                Target.GetTooltip(nameof(Target.enableLifePreview))), Target.enableLifePreview);
            
            EditorGUILayout.Space();

            Target.canvasPadding = EditorGUILayout.FloatField("Padding", Target.canvasPadding);
            Target.canvasMargin = EditorGUILayout.FloatField("Margin", Target.canvasMargin);
            Target.elementSpacing = EditorGUILayout.FloatField("Element Spacing", Target.elementSpacing);
            Target.areaSpacing = EditorGUILayout.FloatField("Area Spacing", Target.areaSpacing);
            
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
            if(GUI.changed)
                Target.OnValidate();
            
            MonitoringCanvasBehaviour.SetHideFlags(Target.hideCanvasGameObject? HideFlags.HideInHierarchy : HideFlags.None);
            DrawStyleInspector("Default Module Style");
        }
    }
}
