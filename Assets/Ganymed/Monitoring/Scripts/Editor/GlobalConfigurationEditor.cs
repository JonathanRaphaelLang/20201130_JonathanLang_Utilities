using Ganymed.Monitoring.Configuration;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(GlobalConfiguration))]
    [CanEditMultipleObjects]
    public class GlobalConfigurationEditor : ConfigurableEditor
    {
        public override void OnInspectorGUI()
        {
            var ctx = (GlobalConfiguration) target;
            
            EditorGUILayout.LabelField("MonitorBehaviour", InspectorDrawer.H0);
            InspectorDrawer.DrawLine(new Color(.8f, .8f, .9f, .8f));
            
            ctx.automateCanvasState = EditorGUILayout.Toggle("Automate MonitoringCanvasBehaviour On Mode Change", ctx.automateCanvasState);
            if (ctx.automateCanvasState)
            {
                ctx.openCanvasOnEnterPlay = EditorGUILayout.Toggle("Open MonitoringCanvasBehaviour On Play", ctx.openCanvasOnEnterPlay);   
                ctx.closeCanvasOnEdit = EditorGUILayout.Toggle("Close MonitoringCanvasBehaviour On Edit", ctx.closeCanvasOnEdit);    
            }
            
            ctx.toggleKey = (KeyCode)EditorGUILayout.EnumPopup("Activate / Deactivate", ctx.toggleKey);
            EditorGUILayout.Space();
            ctx.sortingOrder = EditorGUILayout.IntField("Canvas Sorting Layer", ctx.sortingOrder);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", InspectorDrawer.H3);
            ctx.logSerializationEvents = EditorGUILayout.Toggle("Log Validation", ctx.logSerializationEvents);
            
            EditorGUILayout.Space(10);
            ctx.active = EditorGUILayout.ToggleLeft("MonitoringCanvasBehaviour", ctx.active, InspectorDrawer.H1);
            InspectorDrawer.DrawLine(new Color(.8f, .8f, .9f, .8f));

            ctx.canvasPadding = EditorGUILayout.FloatField("Padding", ctx.canvasPadding);
            ctx.canvasMargin = EditorGUILayout.FloatField("Margin", ctx.canvasMargin);
            ctx.elementSpacing = EditorGUILayout.FloatField("Element Spacing", ctx.elementSpacing);
            ctx.areaSpacing = EditorGUILayout.FloatField("Area Spacing", ctx.areaSpacing);
            
            EditorGUILayout.Space();
            
            //Background
            ctx.showBackground = EditorGUILayout.Toggle("Show Canvas Background", ctx.showBackground);
            if (ctx.showBackground)
            {
                ctx.colorCanvasBackground = EditorGUILayout.ColorField("Canvas Background", ctx.colorCanvasBackground);
            }
            EditorGUILayout.Space();
            
            //Background Areas 
            ctx.showAreaBackground = EditorGUILayout.Toggle("Show Area Background", ctx.showAreaBackground);
            if (ctx.showAreaBackground)
            {
                ctx.colorTopLeft = EditorGUILayout.ColorField("Canvas Background", ctx.colorTopLeft);
                ctx.colorTopRight = EditorGUILayout.ColorField("Canvas Background", ctx.colorTopRight);
                ctx.colorBottomLeft = EditorGUILayout.ColorField("Canvas Background", ctx.colorBottomLeft);
                ctx.colorBottomRight = EditorGUILayout.ColorField("Canvas Background", ctx.colorBottomRight);
            }
            EditorGUILayout.Space();
            if(GUI.changed)
                ctx.OnValidate();
            
            DrawStyleInspector("Config [DEFAULT]");
        }
    }
}
