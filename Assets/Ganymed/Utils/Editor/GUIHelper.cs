﻿using UnityEditor;
using UnityEngine;

namespace Ganymed.Utils.Editor
{
    public static class GUIHelper
    {
        private static readonly Color defaultColor = new Color(.8f, .8f, .9f, .5f);
        
        /// <summary>
        /// Draw Line in Inspector
        /// </summary>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <param name="padding"></param>
        public static void DrawLine(Color? color = null, int thickness = 1, int padding = 10, bool space = false)
        {
            if(space)
                EditorGUILayout.Space(5);
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            rect.height = thickness;
            rect.y += (float)padding / 2;
            rect.x -= 2;
            rect.width += 6;
            EditorGUI.DrawRect(rect, color ?? defaultColor);
        }
        
        //--------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Title GUIStyle for main headlines
        /// </summary>
        public static readonly GUIStyle H0 = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 18
        };
        
        /// <summary>
        /// Title GUIStyle for main headlines
        /// </summary>
        public static readonly GUIStyle H1 = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 16
        };
        
        /// <summary>
        /// Title GUIStyle for bold headlines
        /// </summary>
        public static readonly GUIStyle H2 = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 14
        };
        
        /// <summary>
        /// Title GUIStyle for bold headlines
        /// </summary>
        public static readonly GUIStyle H3 = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 12
        };
    }
}
