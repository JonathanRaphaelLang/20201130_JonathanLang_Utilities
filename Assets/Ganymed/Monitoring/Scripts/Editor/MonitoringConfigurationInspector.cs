﻿using Ganymed.Monitoring.Configuration;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringConfiguration))]
    [CanEditMultipleObjects]
    public class MonitoringConfigurationInspector : StyleBaseInspector
    {
        public override void OnInspectorGUI()
        {
            var ctx = (MonitoringConfiguration) target;
            
            EditorGUILayout.LabelField("Monitoring", GUIHelper.H0);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));
            
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
            EditorGUILayout.LabelField("Debug", GUIHelper.H3);
            ctx.logValidationEvents = EditorGUILayout.Toggle("Log Validation", ctx.logValidationEvents);
            
            EditorGUILayout.Space(10);
            ctx.active = EditorGUILayout.ToggleLeft("Monitoring Canvas", ctx.active, GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));

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
            
            DrawStyleInspector("Default Module Style");
        }
    }
}