using System;
using Ganymed.Monitoring.Configuration;
using Ganymed.Utils;
using Ganymed.Utils.Editor;
using Ganymed.Utils.ExtensionMethods;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(Style))]
    [CanEditMultipleObjects]
    public sealed class StyleInspector : UnityEditor.Editor
    {
        private Style styleBase;

        private void OnEnable()
        {
            styleBase = (Style) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Style", GUIHelper.H1);
            GUIHelper.DrawLine(new Color(.8f, .8f, .9f, .8f));
    
            EditorGUILayout.Space();
    
            styleBase.individualFontSize = EditorGUILayout.Toggle("Set Individual FontSize", styleBase.individualFontSize);
            if (styleBase.individualFontSize)
            {
                styleBase.prefixFontSize = EditorGUILayout.Slider(
                    "Prefix",
                    styleBase.prefixFontSize,
                    rightValue: Style.MaxFontSize,
                    leftValue: Style.MinFontSize);
    
                styleBase.infixFontSize = EditorGUILayout.Slider(
                    "Infix",
                    styleBase.infixFontSize,
                    rightValue: Style.MaxFontSize,
                    leftValue: Style.MinFontSize);
    
                styleBase.suffixFontSize = EditorGUILayout.Slider(
                    "Suffix",
                    styleBase.suffixFontSize,
                    rightValue: Style.MaxFontSize,
                    leftValue: Style.MinFontSize);
            }
            else
            {
                styleBase.fontSize = EditorGUILayout.Slider(
                    "Fontsize",
                    styleBase.fontSize,
                    rightValue: Style.MaxFontSize,
                    leftValue: Style.MinFontSize);
            }
    
            EditorGUILayout.Space();
    
            styleBase.individualMargins = EditorGUILayout.Toggle("Set Individual Margin", styleBase.individualMargins);
    
            if (styleBase.individualMargins)
            {
                styleBase.marginLeft = EditorGUILayout.Slider(
                    new GUIContent("Margin Left", styleBase.GetTooltip(nameof(styleBase.marginLeft))), 
                    styleBase.marginLeft,
                    rightValue: Style.MaxMargin,
                    leftValue: Style.MinMargin);
    
                styleBase.marginTop = EditorGUILayout.Slider(
                    new GUIContent("Margin Top", styleBase.GetTooltip(nameof(styleBase.marginTop))), 
                    styleBase.marginTop,
                    rightValue: Style.MaxMargin,
                    leftValue: Style.MinMargin);
    
                styleBase.marginRight = EditorGUILayout.Slider(
                    new GUIContent("Margin Right", styleBase.GetTooltip(nameof(styleBase.marginRight))), 
                    styleBase.marginRight,
                    rightValue: Style.MaxMargin,
                    leftValue: Style.MinMargin);
    
                styleBase.marginBottom = EditorGUILayout.Slider(
                    new GUIContent("Margin Bottom", styleBase.GetTooltip(nameof(styleBase.marginBottom))), 
                    styleBase.marginBottom,
                    rightValue: Style.MaxMargin,
                    leftValue: Style.MinMargin);
            }
            else
            {
                styleBase.marginsAll = EditorGUILayout.Slider(
                    "Margin",
                    styleBase.marginsAll,
                    rightValue: Style.MaxMargin,
                    leftValue: Style.MinMargin);
    
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
            
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select Monitoring-Settings"))
            {
                Selection.activeObject = MonitoringSettings.Instance;
            }
            if (GUILayout.Button("Use as default"))
            {
                EditorUtility.SetDirty(MonitoringSettings.Instance);
                MonitoringSettings.Instance.Style = (Style)target; 
                EditorUtility.SetDirty(MonitoringSettings.Instance);
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}