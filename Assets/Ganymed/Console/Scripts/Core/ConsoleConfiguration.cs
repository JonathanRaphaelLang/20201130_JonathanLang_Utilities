﻿using System;
using System.Reflection;
using Ganymed.Console.Attributes;
using Ganymed.Console.Processor;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Core
{
    [CreateAssetMenu(fileName = "Console_Configuration", menuName = "Console/Configuration")]
    [DeclaringName("Console")]
    public sealed class ConsoleConfiguration : ScriptableObject
    {
#pragma warning disable 414
#pragma warning disable 67

        

        #region --- [INSPECTOR] ---
        
        [Tooltip("NA")]
        [SerializeField] internal bool active = true;
        
        [Header("Commands")]
        [Tooltip("use this as a prefix")]
        [SerializeField] internal string commandPrefix = "/";
        
        [Tooltip("use this character to receive additional information about a command. eg: /Help?")]
        [SerializeField] internal string infoOperator = "?";
        
        [Tooltip("allows the console to precalculate inputs and to generate suggestions for automatic completion")]
        [SerializeField] internal bool allowCommandPreProcessing = true;
        
        [Tooltip("log information about commands on start")]
        [SerializeField] internal bool logCommandsLoadedOnStart = true;
                
        [Tooltip("allow numeric input for boolean parameter in console commands")]
        [SerializeField] internal bool allowNumericBoolProcessing = true;
        
        
        
        [Header("Console")]
        [Tooltip("should the console be activated on start")]
        [SerializeField] internal bool activateConsoleOnStart = true;
                
        [Tooltip("should the cursor be active if the console is visible")]
        [SerializeField] internal bool enableCursorOnActivation = true;
        
        [Tooltip("log configuration of the console on start")]
        [SerializeField] internal bool logConfigurationOnStart = true;
        
        [Tooltip("should the current time be logged to the console")]
        [SerializeField] internal bool logTimeOnInput = true;
        
        [Tooltip("how many previous inputs are cached")]
        [SerializeField] [Range(0,100)] internal byte inputCacheSize = 20;
        
        [Tooltip("Limit the amount of logs displayed in the console")]
        [SerializeField] internal bool clearConsoleAutomatically = true;
        
        [Tooltip("how many logs are allowed")]
        [SerializeField] [Range(10,1000)] internal int maxLogs = 20;
        
        
        
        [Header("Unity Console Integration")]
        [Tooltip("bind Unities Debug console to this console ")]
        [SerializeField] internal bool bindConsoles = true;
        
        [Tooltip("which types of messages are logged by this console (Log, Warning, Error, Exception, Assert)")]
        [SerializeField] internal LogTypeFlags allowedUnityMessages = LogTypeFlags.None;
        
        [Tooltip("which types messages are allowed to show their stacktrace (Log, Warning, Error, Exception, Assert)")]
        [SerializeField] internal LogTypeFlags logStackTraceOn = LogTypeFlags.None;
        
        [Header("Eye Candy & Performance Optimization")]
        [Tooltip("NA")]
        [SerializeField] internal bool allowShaderAndAnimations = true;
        
        [Tooltip("Is the frosted glass shader active. This shader will drain a lot of performance")]
        [SerializeField] internal bool allowShader = true;
        
        [Tooltip("NA")]
        [SerializeField] internal bool allowAnimations = true;

        [Tooltip("Debug option to show RichText")]
        [SerializeField] internal bool showRichText = false;

        [Tooltip("When disabled the content of the console will be disabled when the console is dragged to reduce rendering cost")]
        [SerializeField] internal bool renderContentOnDrag = false;
        
        [Tooltip("When disabled the content of the console will be disabled when the console is scaled to reduce rendering cost")]
        [SerializeField] internal bool renderContentOnScale = false;
        
        
                
        [Header("Font")]
        [Tooltip("font size of the input")]
        [SerializeField] [Range(MinFontSize, MaxFontSize)] internal float inputFontSize = 6;
        [Tooltip("font size of the console")]
        [SerializeField] [Range(MinFontSize, MaxFontSize)] internal float fontSize = 6;
        [Space]
        [Tooltip("default line height")]
        [SerializeField] [Range(0, 300)] internal int breakLineHeight = 130;
        [Tooltip("line height after a break")]
        [SerializeField] [Range(0, 300)] internal int defaultLineHeight = 100;
        
        
        [Header("Console")]
        [SerializeField] internal Color colorConsoleBackground = Color.magenta;
        [SerializeField] internal Color colorScrollbar = Color.magenta;
        [SerializeField] internal Color colorScrollbarHandle = Color.magenta;
        
        [Header("Text")]
        [SerializeField] internal Color colorDefault = Color.magenta;
        [SerializeField] internal Color colorTitles = Color.magenta;
        [SerializeField] internal Color colorSubHeading = Color.magenta;
        [SerializeField] internal Color colorEmphasize = Color.magenta;
        [SerializeField] internal Color colorInputLines = Color.magenta;
        [SerializeField] internal Color colorVariables = Color.magenta;

        [Header("Validation (Input)")]
        [SerializeField] internal Color colorValidInput = Color.magenta;
        [SerializeField] internal Color colorOptionalParamsLeft = Color.magenta;
        [SerializeField] internal Color colorIncompleteInput = Color.magenta;
        [SerializeField] internal Color colorIncorrectInput = Color.magenta;
        [SerializeField] internal Color colorInformation = Color.magenta;
        [SerializeField] internal Color colorAutocompletion = Color.magenta;
       
        [Header("Color Unity Console Log/Warning/Error")]
        [SerializeField] internal Color colorUnityLog = Color.magenta;
        [SerializeField] internal Color colorUnityWarning = Color.magenta;
        [SerializeField] internal Color colorUnityError = Color.magenta;
        [SerializeField] internal Color colorStackTrace = Color.magenta;
        
        [Header("Custom Color")]
        [SerializeField] internal Color customColor1 = Color.magenta;
        [SerializeField] internal Color customColor2 = Color.magenta;
        [SerializeField] internal Color customColor3 = Color.magenta;

        #endregion
        
        
        #region --- [PROPERTIES] ---

        public static ConsoleConfiguration Instance { get; set; } = null;
        
        [Getter] public static string CommandPrefix => Instance.commandPrefix;

        [GetSet(Description = "Enable / Disable autocompletion and console color validation")]
        public static bool AllowCommandPreProcessing
        {
            get => Instance.allowCommandPreProcessing;
            set
            {
                Instance.allowCommandPreProcessing = value;
                Instance.ValidateIntegrity();
            }
        }

        [GetSet]
        public static bool AllowNumericBoolProcessing
        {
            get => Instance.allowNumericBoolProcessing;
            set
            {
                Instance.allowNumericBoolProcessing = value;
                Instance.ValidateIntegrity();
            }
        }
        
        [GetSet(Description = "Enable / Disable the background shader of the console.")]
        public static bool AllowBackgroundShader
        {
            get => Instance.allowShader;
            set
            {
                Instance.allowShader = value;
                Instance.ValidateIntegrity();
            }
        }
        
        
        [GetSet(Description = "Enable / Disable RichText.")]
        public static bool RichText
        {
            get => Instance.showRichText;
            set
            {
                Instance.showRichText = value;
                Instance.ValidateIntegrity();
            }
        }


        [GetSet]
        public static char InfoOperator
        {
            get => Instance.infoOperator.Cut()[0];
            set
            {
                Instance.infoOperator = value.ToString();
                Instance.ValidateIntegrity();
            }
        }
        



        #endregion
        
        #region --- [EVENTS] ---

        public event Action OnConsoleConfigurationChanged;

        #endregion

        #region --- [CONST] ---

        public const int MaxFontSize = 22;
        public const int MinFontSize = 5;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMANDS] ---
        
        [NativeCommand]
        [ConsoleCommand("Configuration", Priority = 500, Description = "Log the current configuration of the console")]
        private static void LogConsoleConfiguration(
            [Hint("Log the configuration of the consoles color scheme")]bool includeColorScheme = false)
            => Console.Configuration.LogConfiguration(includeColorScheme);

        public void LogConfiguration(bool includeColorScheme)
        {
            if(!Transmission.Start(TransmissionOptions.Enumeration)) return;
            
            const MessageOptions options = MessageOptions.Brackets;

            Transmission.AddBreak();
            Transmission.AddTitle("Console Configuration");
            Transmission.AddBreak();
            Transmission.AddTitle("Commands", TitlePreset.Sub);
            
            Transmission.AddLine("Command Prefix:",
                new MessageFormat(commandPrefix, options),
                new MessageFormat($"// {GetTooltip(nameof(commandPrefix))}", options));
            
            Transmission.AddLine("Info Operator",
                new MessageFormat(infoOperator, options),
                new MessageFormat($"// {GetTooltip(nameof(infoOperator))}", options));
            
            Transmission.AddLine("Command Pre- Processing",
                new MessageFormat(allowCommandPreProcessing, options),
                new MessageFormat($"// {GetTooltip(nameof(allowCommandPreProcessing))}", options));
            
            Transmission.AddLine("Numeric Bool Processing",
                new MessageFormat(allowNumericBoolProcessing, options),
                new MessageFormat($"// {GetTooltip(nameof(allowNumericBoolProcessing))}", options));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddTitle("Eye Candy & Performance Optimization", TitlePreset.Sub);
            
            Transmission.AddLine("Allow Shader and Animations",
                new MessageFormat(allowShaderAndAnimations, options),
                new MessageFormat($"// {GetTooltip(nameof(allowShaderAndAnimations))}", options));
            
            Transmission.AddLine("Forested Glass Shader",
                new MessageFormat(allowShader, options),
                new MessageFormat($"// {GetTooltip(nameof(allowShader))}", options));
            
            Transmission.AddLine("Animations",
                new MessageFormat(allowAnimations, options),
                new MessageFormat($"// {GetTooltip(nameof(allowAnimations))}", options));
            
            Transmission.AddLine(nameof(renderContentOnDrag).AsLabel(),
                new MessageFormat(renderContentOnDrag, options),
                new MessageFormat($"// {GetTooltip(nameof(renderContentOnDrag))}", options));
            
            Transmission.AddLine(nameof(renderContentOnScale).AsLabel(),
                new MessageFormat(renderContentOnScale, options),
                new MessageFormat($"// {GetTooltip(nameof(renderContentOnScale))}", options));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddTitle("Console", TitlePreset.Sub);
            
            Transmission.AddLine("Input Cache:",
                new MessageFormat(inputCacheSize, options),
                new MessageFormat($"// {GetTooltip(nameof(inputCacheSize))}", options));
            
            Transmission.AddLine("Enable Cursor:",
                new MessageFormat(enableCursorOnActivation, options),
                new MessageFormat($"// {GetTooltip(nameof(enableCursorOnActivation))}", options));
            
            Transmission.AddLine("Activate OnStart:",
                new MessageFormat(activateConsoleOnStart, options),
                new MessageFormat($"// {GetTooltip(nameof(activateConsoleOnStart))}", options));
            
            Transmission.AddLine("Log Config OnStart:",
                new MessageFormat(logConfigurationOnStart, options),
                new MessageFormat($"// {GetTooltip(nameof(logConfigurationOnStart))}", options));
            
            Transmission.AddLine(nameof(clearConsoleAutomatically).AsLabel(),
                new MessageFormat(clearConsoleAutomatically, options),
                new MessageFormat($"// {GetTooltip(nameof(clearConsoleAutomatically))}", options));
            
            Transmission.AddLine(nameof(maxLogs).AsLabel(),
                new MessageFormat(maxLogs, options),
                new MessageFormat($"// {GetTooltip(nameof(maxLogs))}", options));
            
            
            
            Transmission.AddBreak();
            Transmission.AddLine("Unity Console Integration:",
                new MessageFormat(bindConsoles, options));
            
            Transmission.AddLine("Allowed Messages:",
                new MessageFormat(allowedUnityMessages, options),
                new MessageFormat($"// {GetTooltip(nameof(allowedUnityMessages))}", options));
            
            Transmission.AddLine("Allowed Stacktrace:",
                new MessageFormat(logStackTraceOn, options),
                new MessageFormat($"// {GetTooltip(nameof(logStackTraceOn))}", options));
            
            Transmission.AddBreak();
            
            Transmission.AddTitle("Format", TitlePreset.Sub);
            
            Transmission.AddLine("FontSize Input:",
                new MessageFormat(inputFontSize, options),
                new MessageFormat($"// {GetTooltip(nameof(inputFontSize))}", options));
            
            Transmission.AddLine("FontSize Command:",
                new MessageFormat(fontSize, options),
                new MessageFormat($"// {GetTooltip(nameof(fontSize))}", options));
            
            Transmission.AddLine("LineHeight (Break):",
                new MessageFormat(breakLineHeight, options),
                new MessageFormat($"// {GetTooltip(nameof(breakLineHeight))}", options));
            
            Transmission.AddLine("LineHeight (Default):",
                new MessageFormat(defaultLineHeight, options),
                new MessageFormat($"// {GetTooltip(nameof(defaultLineHeight))}", options));
            
            Transmission.AddBreak();
            
            Transmission.AddBreak();

            
            if (includeColorScheme)
            {
                Transmission.AddTitle("Color", TitlePreset.Sub);
                Transmission.AddLine("Background", new MessageFormat(colorConsoleBackground, options));
                Transmission.AddLine("Scrollbar", new MessageFormat(colorScrollbar, options));
                Transmission.AddLine("Scrollbar Handle", new MessageFormat(colorScrollbarHandle, options));
                Transmission.AddBreak();
                
                Transmission.AddLine("Default Color", new MessageFormat(colorDefault, colorDefault, options));
                Transmission.AddBreak();

                Transmission.AddLine("Input: Valid", new MessageFormat(colorValidInput, colorValidInput, options));
                Transmission.AddLine("Input: Optional Params Left", new MessageFormat(colorOptionalParamsLeft, colorOptionalParamsLeft, options));
                Transmission.AddLine("Input: Incomplete", new MessageFormat(colorIncompleteInput, colorIncompleteInput, options));
                Transmission.AddLine("Input: Incorrect", new MessageFormat(colorIncorrectInput, colorIncorrectInput, options));
                Transmission.AddLine("Input: Information", new MessageFormat(colorInformation, colorInformation, options));
                Transmission.AddLine("Autocompletion", new MessageFormat(colorAutocompletion, colorAutocompletion, options));
                Transmission.AddBreak();

                Transmission.AddLine("Formatting: Titles", new MessageFormat(colorTitles, colorTitles, options));
                Transmission.AddLine("Formatting: Subheadings", new MessageFormat(colorSubHeading, colorSubHeading, options));
                Transmission.AddLine("Formatting: Emphasize", new MessageFormat(colorEmphasize, colorEmphasize, options));
                Transmission.AddLine("Formatting: Input Lines", new MessageFormat(colorInputLines, colorInputLines, options));
                Transmission.AddLine("Formatting: Variables", new MessageFormat(colorVariables, colorVariables, options));
                Transmission.AddBreak();    
                
                Transmission.AddLine("Unity Log", new MessageFormat(colorUnityLog, colorUnityLog, options));
                Transmission.AddLine("Unity Warning", new MessageFormat(colorUnityWarning, colorUnityWarning, options));
                Transmission.AddLine("Unity Error", new MessageFormat(colorUnityError, colorUnityError, options));
                Transmission.AddLine("Stacktrace", new MessageFormat(colorStackTrace, colorStackTrace, options));
                
                Transmission.AddBreak();    
                
                Transmission.AddLine("Custom 1", new MessageFormat(customColor1, customColor1, options));
                Transmission.AddLine("Custom 2", new MessageFormat(customColor2, customColor2, options));
                Transmission.AddLine("Custom 3", new MessageFormat(customColor3, customColor3, options));
                Transmission.AddBreak();
            }

            Transmission.ReleaseAsync();
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HELPER] ---

        private void OnValidate() => ValidateIntegrity();

        public void ValidateIntegrity()
        {
            if (!allowShaderAndAnimations)
            {
                allowShader = false;
                allowAnimations = false;
            }
            OnConsoleConfigurationChanged?.Invoke();
            if(Console.Configuration == this)
                CommandProcessor.SetConfiguration(this);
        }
   
        private string GetTooltip(string fieldName, bool inherit = true)
        {
            var field = GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return string.Empty;
            var attributes = field.GetCustomAttributes();

            foreach (var attribute in field.GetCustomAttributes())
            {
                if (attribute is TooltipAttribute tooltipAttribute)
                {
                    return tooltipAttribute.tooltip;
                }
            }

            return string.Empty;
        }
        #endregion
    }
}
