using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(StyleBase))]
    [CanEditMultipleObjects]
    public class StyleBaseInspector : UnityEditor.Editor
    {
        private StyleBase styleBase;

        public override void OnInspectorGUI()
        {
            DrawStyleInspector();
        }

        protected void DrawStyleInspector(string title = "Config")
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
            
            styleBase = (StyleBase) target;
            base.OnInspectorGUI();

            EditorGUILayout.Space();
    
            EditorGUILayout.LabelField(title, GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));
    
            EditorGUILayout.Space();
    
            styleBase.individualFontSize = EditorGUILayout.Toggle("Set Individual FontSize", styleBase.individualFontSize);
            if (styleBase.individualFontSize)
            {
                styleBase.prefixFontSize = EditorGUILayout.Slider(
                    "Prefix",
                    styleBase.prefixFontSize,
                    rightValue: StyleBase.MAXFONTSIZE,
                    leftValue: StyleBase.MINFONTSIZE);
    
                styleBase.infixFontSize = EditorGUILayout.Slider(
                    "Infix",
                    styleBase.infixFontSize,
                    rightValue: StyleBase.MAXFONTSIZE,
                    leftValue: StyleBase.MINFONTSIZE);
    
                styleBase.suffixFontSize = EditorGUILayout.Slider(
                    "Suffix",
                    styleBase.suffixFontSize,
                    rightValue: StyleBase.MAXFONTSIZE,
                    leftValue: StyleBase.MINFONTSIZE);
            }
            else
            {
                styleBase.fontSize = EditorGUILayout.Slider(
                    "Fontsize",
                    styleBase.fontSize,
                    rightValue: StyleBase.MAXFONTSIZE,
                    leftValue: StyleBase.MINFONTSIZE);
            }
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Margins:");
    
            styleBase.individualMargins = EditorGUILayout.Toggle("Set Individual Margin", styleBase.individualMargins);
    
            if (styleBase.individualMargins)
            {
                styleBase.marginLeft = EditorGUILayout.Slider(
                    new GUIContent("Margin Left", styleBase.GetTooltip(nameof(styleBase.marginLeft))), 
                    styleBase.marginLeft,
                    rightValue: StyleBase.MAXMARGIN,
                    leftValue: StyleBase.MINMARGIN);
    
                styleBase.marginTop = EditorGUILayout.Slider(
                    new GUIContent("Margin Top", styleBase.GetTooltip(nameof(styleBase.marginTop))), 
                    styleBase.marginTop,
                    rightValue: StyleBase.MAXMARGIN,
                    leftValue: StyleBase.MINMARGIN);
    
                styleBase.marginRight = EditorGUILayout.Slider(
                    new GUIContent("Margin Right", styleBase.GetTooltip(nameof(styleBase.marginRight))), 
                    styleBase.marginRight,
                    rightValue: StyleBase.MAXMARGIN,
                    leftValue: StyleBase.MINMARGIN);
    
                styleBase.marginBottom = EditorGUILayout.Slider(
                    new GUIContent("Margin Bottom", styleBase.GetTooltip(nameof(styleBase.marginBottom))), 
                    styleBase.marginBottom,
                    rightValue: StyleBase.MAXMARGIN,
                    leftValue: StyleBase.MINMARGIN);
            }
            else
            {
                styleBase.marginsAll = EditorGUILayout.Slider(
                    "Margin",
                    styleBase.marginsAll,
                    rightValue: StyleBase.MAXMARGIN,
                    leftValue: StyleBase.MINMARGIN);
    
                styleBase.marginBottom = styleBase.marginsAll;
                styleBase.marginTop = styleBase.marginsAll;
                styleBase.marginLeft = styleBase.marginsAll;
                styleBase.marginRight = styleBase.marginsAll;
            }
    
            EditorGUILayout.Space();
            
            styleBase.alignment = (TextAlignmentOptions) EditorGUILayout.EnumPopup(
                new GUIContent("Text Alignment", styleBase.GetTooltip(nameof(styleBase.alignment))),
                styleBase.alignment);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Formatting", EditorStyles.boldLabel);
            styleBase.autoSpace = EditorGUILayout.Toggle(
                new GUIContent("Use Auto-Spacing", styleBase.GetTooltip(nameof(styleBase.autoSpace))), 
                styleBase.autoSpace);
            
            styleBase.autoBrackets = EditorGUILayout.Toggle(
                new GUIContent("Use Brackets", styleBase.GetTooltip(nameof(styleBase.autoBrackets))), 
                styleBase.autoBrackets);
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Config", EditorStyles.boldLabel);
    
            styleBase.useCustomRichText = EditorGUILayout.Toggle(
                new GUIContent("Use Custom Rich-Text", styleBase.GetTooltip(nameof(styleBase.useCustomRichText))), 
                styleBase.useCustomRichText);
            if (styleBase.useCustomRichText)
            {
                styleBase.infixTextStyle = EditorGUILayout.TextField("Main", styleBase.infixTextStyle);
                styleBase.prefixTextStyle = EditorGUILayout.TextField("Prefix", styleBase.prefixTextStyle);
                styleBase.suffixTextStyle = EditorGUILayout.TextField("Suffix", styleBase.suffixTextStyle);
            }
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Color", EditorStyles.boldLabel);
            styleBase.colorBackground = EditorGUILayout.ColorField("Background Color", styleBase.colorBackground);
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Color", EditorStyles.boldLabel);
            styleBase.infixColor = EditorGUILayout.ColorField("Main", styleBase.infixColor);
            styleBase.prefixColor = EditorGUILayout.ColorField("Prefix", styleBase.prefixColor);
            styleBase.suffixColor = EditorGUILayout.ColorField("Suffix", styleBase.suffixColor);
    
            if (GUI.changed) styleBase.Validate();
            EditorUtility.SetDirty(styleBase);
            EditorUtility.SetDirty(target);
        }
    }
}