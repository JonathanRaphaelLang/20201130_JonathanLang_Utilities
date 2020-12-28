using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(Configurable))]
    [CanEditMultipleObjects]
    public class StyleEditor : UnityEditor.Editor
    {
        private Configurable configurable;

        public override void OnInspectorGUI()
        {
            DrawStyleInspector();
        }

        protected void DrawStyleInspector(string title = "Config")
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
            
            configurable = (Configurable) target;
            base.OnInspectorGUI();

            EditorGUILayout.Space();
    
            EditorGUILayout.LabelField(title, InspectorDrawer.H1);
            InspectorDrawer.DrawLine(new Color(.8f, .8f, .9f, .8f));
    
            EditorGUILayout.Space();
    
            configurable.individualFontSize = EditorGUILayout.Toggle("Set Individual FontSize", configurable.individualFontSize);
            if (configurable.individualFontSize)
            {
                configurable.prefixFontSize = EditorGUILayout.IntSlider(
                    "Prefix",
                    configurable.prefixFontSize,
                    rightValue: Configurable.MAXFONTSIZE,
                    leftValue: Configurable.MINFONTSIZE);
    
                configurable.infixFontSize = EditorGUILayout.IntSlider(
                    "Infix",
                    configurable.infixFontSize,
                    rightValue: Configurable.MAXFONTSIZE,
                    leftValue: Configurable.MINFONTSIZE);
    
                configurable.suffixFontSize = EditorGUILayout.IntSlider(
                    "Suffix",
                    configurable.suffixFontSize,
                    rightValue: Configurable.MAXFONTSIZE,
                    leftValue: Configurable.MINFONTSIZE);
            }
            else
            {
                configurable.fontSize = EditorGUILayout.IntSlider(
                    "Fontsize",
                    configurable.fontSize,
                    rightValue: Configurable.MAXFONTSIZE,
                    leftValue: Configurable.MINFONTSIZE);
            }
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Margins:");
    
            configurable.individualMargins = EditorGUILayout.Toggle("Set Individual Margin", configurable.individualMargins);
    
            if (configurable.individualMargins)
            {
                configurable.marginLeft = EditorGUILayout.Slider(
                    "Margin Left",
                    configurable.marginLeft,
                    rightValue: Configurable.MAXMARGIN,
                    leftValue: Configurable.MINMARGIN);
    
                configurable.marginTop = EditorGUILayout.Slider(
                    "Margin Top",
                    configurable.marginTop,
                    rightValue: Configurable.MAXMARGIN,
                    leftValue: Configurable.MINMARGIN);
    
                configurable.marginRight = EditorGUILayout.Slider(
                    "Margin Right",
                    configurable.marginRight,
                    rightValue: Configurable.MAXMARGIN,
                    leftValue: Configurable.MINMARGIN);
    
                configurable.marginBottom = EditorGUILayout.Slider(
                    "Margin Bottom",
                    configurable.marginBottom,
                    rightValue: Configurable.MAXMARGIN,
                    leftValue: Configurable.MINMARGIN);
            }
            else
            {
                configurable.marginsAll = EditorGUILayout.Slider(
                    "Margin",
                    configurable.marginsAll,
                    rightValue: Configurable.MAXMARGIN,
                    leftValue: Configurable.MINMARGIN);
    
                configurable.marginBottom = configurable.marginsAll;
                configurable.marginTop = configurable.marginsAll;
                configurable.marginLeft = configurable.marginsAll;
                configurable.marginRight = configurable.marginsAll;
            }
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Formatting", EditorStyles.boldLabel);
            configurable.autoSpace = EditorGUILayout.Toggle("Use Autospacing", configurable.autoSpace);
            configurable.autoBrackets = EditorGUILayout.Toggle("Use Brackets", configurable.autoBrackets);
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
    
            configurable.useTextStyle = EditorGUILayout.Toggle("Custom Markup Text", configurable.useTextStyle);
            if (configurable.useTextStyle)
            {
                configurable.infixTextStyle = EditorGUILayout.TextField("Config", configurable.infixTextStyle);
                configurable.prefixTextStyle = EditorGUILayout.TextField("Config Prefix", configurable.prefixTextStyle);
                configurable.suffixTextStyle = EditorGUILayout.TextField("Config Suffix", configurable.suffixTextStyle);
            }
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Color", EditorStyles.boldLabel);
            configurable.colorBackground = EditorGUILayout.ColorField("Background Color", configurable.colorBackground);
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Color", EditorStyles.boldLabel);
            configurable.infixColor = EditorGUILayout.ColorField("Main", configurable.infixColor);
            configurable.prefixColor = EditorGUILayout.ColorField("Prefix", configurable.prefixColor);
            configurable.suffixColor = EditorGUILayout.ColorField("Suffix", configurable.suffixColor);
    
            if (GUI.changed) configurable.Validate(UnityEventType.Unknown);
            EditorUtility.SetDirty(configurable);
            EditorUtility.SetDirty(target);
        }
    }
}