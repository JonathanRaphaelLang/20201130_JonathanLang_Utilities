using System;
using System.Reflection;
using Ganymed.Console.Attributes;
using Ganymed.Console.Processor;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Core
{
    [CreateAssetMenu(fileName = "Console_Configuration", menuName = "Command/Configuration")]
    [DeclaringName("Console")]
    public sealed class ConsoleConfiguration : ScriptableObject
    {
#pragma warning disable 414
#pragma warning disable 67

        [Getter]
        public static ConsoleConfiguration Instance { get; set; } = null;


        #region --- [INSPECTOR] ---
        
        [Tooltip("NA")]
        [SerializeField] internal bool enabled = true;
        
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
                
        [Tooltip("should the cursor be enabled if the console is active")]
        [SerializeField] internal bool enableCursorOnActivation = true;
        
        [Tooltip("log configuration of the console on start")]
        [SerializeField] internal bool logConfigurationOnStart = true;
        
        [Tooltip("should the current time be logged to the console")]
        [SerializeField] internal bool logTimeOnInput = true;
        
        [Tooltip("how many previous inputs are cached")]
        [SerializeField] internal int inputCacheSize = 20;

        
        
        [Header("Unity Console Integration")]
        [Tooltip("bind Unities Debug console to this console ")]
        [SerializeField] internal bool bindConsoles = true;
        
        [Tooltip("which types of messages are logged by this console (Log, Warning, Error, Exception, Assert)")]
        [SerializeField] internal LogTypeFlags allowedUnityMessages = LogTypeFlags.None;
        
        [Tooltip("which types messages are allowed to show their stacktrace (Log, Warning, Error, Exception, Assert)")]
        [SerializeField] internal LogTypeFlags logStackTraceOn = LogTypeFlags.None;
        
        [Header("Eye Candy")]
        [Tooltip("NA")]
        [SerializeField] internal bool allowShaderAndAnimations = true;
        
        [Tooltip("Is the frosted glass shader enabled. This shader will drain a lot of performance")]
        [SerializeField] internal bool allowFrostedGlassEffect = true;
        
        [Tooltip("NA")]
        [SerializeField] internal bool allowAnimations = true;

        [Tooltip("Debug option to show RichText")]
        [SerializeField] internal bool showRichText = false;
        
        
        
        [Header("Command Color")]
        [SerializeField] internal Color colorConsoleBackground = Color.magenta;
        [SerializeField] internal Color colorScrollbar = Color.magenta;
        [SerializeField] internal Color colorScrollbarHandle = Color.magenta;
        
        
        
        [Header("Font Color")]
        [SerializeField] internal Color colorOutput = Color.magenta;
        [SerializeField] internal Color colorInput = Color.magenta;
        
        
        
        [Header("Color Input")]
        [SerializeField] internal Color colorValid = Color.magenta;
        [SerializeField] internal Color colorOptionalParamsLeft = Color.magenta;
        [SerializeField] internal Color colorIncompleteInput = Color.magenta;
        [SerializeField] internal Color colorIncorrectInput = Color.magenta;
        [SerializeField] internal Color colorInformation = Color.magenta;
        [SerializeField] internal Color colorAutocompletion = Color.magenta;

        
        
        [Header("Color Console")]
        [SerializeField] internal Color colorTitles = Color.magenta;
        [SerializeField] internal Color colorSubHeading = Color.magenta;
        [Tooltip("Color to stress certain areas of interest")]
        [SerializeField] internal Color colorEmphasize = Color.magenta;
        [SerializeField] internal Color colorCommandInput = Color.magenta;
        [SerializeField] internal Color colorTextInput = Color.magenta;
        [SerializeField] internal Color colorComment = Color.magenta;
        [SerializeField] internal Color colorSender = Color.magenta;
        
        
        [Header("Color Unity Console Log/Warning/Error")]
        [SerializeField] internal Color colorLog = Color.magenta;
        [SerializeField] internal Color colorWarning = Color.magenta;
        [SerializeField] internal Color colorError = Color.magenta;
        [SerializeField] internal Color colorLogCondition = Color.magenta;
        [SerializeField] internal Color colorStackTrace = Color.magenta;
        
        
        
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

        #endregion

        #region --- [PROPERTIES] ---

                
        [Getter] public static bool Enabled => Instance.enabled;
        
        [Getter] public static bool Active => Instance.active;
        [Getter] public static string CommandPrefix => Instance.commandPrefix;



        [GetSet]
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
        
        
        [GetSet]
        public static bool ShowRichText
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
        
        [Command("Configuration", Priority = 500, Description = "Log the current configuration of the console")]
        private static void LogConsoleConfiguration(
            [Hint("Log the configuration of the consoles color scheme")]bool includeColorScheme = false)
            => Console.Configuration.LogConfiguration(includeColorScheme);

        public void LogConfiguration(bool includeColorScheme)
        {
            const MessageOptions options = MessageOptions.Bold | MessageOptions.Brackets;
            const MessageOptions comment = MessageOptions.Brackets;

            Transmission.Start(TransmissionOptions.Enumeration);
            Transmission.AddBreak();
            Transmission.AddTitle("Console Configuration");
            Transmission.AddBreak();
            Transmission.AddSubheading("Commands");
            
            Transmission.AddLine("Command Prefix:",
                new Message(commandPrefix, options),
                new Message($"// {GetTooltip(nameof(commandPrefix))}", colorComment, comment));
            
            Transmission.AddLine("Info Operator",
                new Message(infoOperator, options),
                new Message($"// {GetTooltip(nameof(infoOperator))}", colorComment, comment));
            
            Transmission.AddLine("Command Pre- Processing",
                new Message(allowCommandPreProcessing, options),
                new Message($"// {GetTooltip(nameof(allowCommandPreProcessing))}", colorComment, comment));
            
            Transmission.AddLine("Numeric Bool Processing",
                new Message(allowNumericBoolProcessing, options),
                new Message($"// {GetTooltip(nameof(allowNumericBoolProcessing))}", colorComment, comment));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddSubheading("Eye Candy");
            
            Transmission.AddLine("Allow Shader and Animations",
                new Message(allowShaderAndAnimations, options),
                new Message($"// {GetTooltip(nameof(allowShaderAndAnimations))}", colorComment, comment));
            
            Transmission.AddLine("Forested Glass Shader",
                new Message(allowFrostedGlassEffect, options),
                new Message($"// {GetTooltip(nameof(allowFrostedGlassEffect))}", colorComment, comment));
            
            Transmission.AddLine("Animations",
                new Message(allowAnimations, options),
                new Message($"// {GetTooltip(nameof(allowAnimations))}", colorComment, comment));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddSubheading("Console");
            
            Transmission.AddLine("Input Cache:",
                new Message(inputCacheSize, options),
                new Message($"// {GetTooltip(nameof(inputCacheSize))}", colorComment, comment));
            
            Transmission.AddLine("Enable Cursor:",
                new Message(enableCursorOnActivation, options),
                new Message($"// {GetTooltip(nameof(enableCursorOnActivation))}", colorComment, comment));
            
            Transmission.AddLine("Activate OnStart:",
                new Message(activateConsoleOnStart, options),
                new Message($"// {GetTooltip(nameof(activateConsoleOnStart))}", colorComment, comment));
            
            Transmission.AddLine("Log Config OnStart:",
                new Message(logConfigurationOnStart, options),
                new Message($"// {GetTooltip(nameof(logConfigurationOnStart))}", colorComment, comment));
            
            Transmission.AddBreak();
            Transmission.AddLine("Unity Console Integration:",
                new Message(bindConsoles, options));
            
            Transmission.AddLine("Allowed Messages:",
                new Message(allowedUnityMessages, options),
                new Message($"// {GetTooltip(nameof(allowedUnityMessages))}", colorComment, comment));
            
            Transmission.AddLine("Allowed Stacktrace:",
                new Message(logStackTraceOn, options),
                new Message($"// {GetTooltip(nameof(logStackTraceOn))}", colorComment, comment));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddSubheading("Format");
            
            Transmission.AddLine("FontSize Input:",
                new Message(inputFontSize, options),
                new Message($"// {GetTooltip(nameof(inputFontSize))}", colorComment, comment));
            
            Transmission.AddLine("FontSize Command:",
                new Message(fontSize, options),
                new Message($"// {GetTooltip(nameof(fontSize))}", colorComment, comment));
            
            Transmission.AddLine("LineHeight (Break):",
                new Message(breakLineHeight, options),
                new Message($"// {GetTooltip(nameof(breakLineHeight))}", colorComment, comment));
            
            Transmission.AddLine("LineHeight (Default):",
                new Message(defaultLineHeight, options),
                new Message($"// {GetTooltip(nameof(defaultLineHeight))}", colorComment, comment));
            
            Transmission.AddBreak();
            
            Transmission.AddBreak();

            
            if (includeColorScheme)
            {
                Transmission.AddSubheading("Color");
                Transmission.AddLine("Background", new Message(colorConsoleBackground, colorConsoleBackground, options));
                Transmission.AddLine("Scrollbar", new Message(colorScrollbar, colorScrollbar, options));
                Transmission.AddLine("Scrollbar Handle", new Message(colorScrollbarHandle, colorScrollbarHandle, options));
                Transmission.AddBreak();

                Transmission.AddLine("Default", new Message(colorOutput, colorOutput, options));
                Transmission.AddLine("Input", new Message(colorInput, colorInput, options));
                Transmission.AddBreak();

                Transmission.AddLine("Valid", new Message(colorValid, colorValid, options));
                Transmission.AddLine("Optional Params Left",
                    new Message(colorOptionalParamsLeft, colorOptionalParamsLeft, options));
                Transmission.AddLine("Incomplete", new Message(colorIncompleteInput, colorIncompleteInput, options));
                Transmission.AddLine("Incorrect", new Message(colorIncorrectInput, colorIncorrectInput, options));
                Transmission.AddLine("Information", new Message(colorInformation, colorInformation, options));
                Transmission.AddLine("Autocompletion", new Message(colorAutocompletion, colorAutocompletion, options));
                Transmission.AddBreak();

                Transmission.AddLine("Titles", new Message(colorTitles, colorTitles, options));
                Transmission.AddLine("Emphasize", new Message(colorEmphasize, colorEmphasize, options));
                Transmission.AddLine("Command", new Message(colorCommandInput, colorCommandInput, options));
                Transmission.AddLine("Text", new Message(colorTextInput, colorTextInput, options));
                Transmission.AddBreak();    
                
                Transmission.AddLine("Warning", new Message(colorWarning, colorWarning, options));
                Transmission.AddLine("Error", new Message(colorError, colorError, options));
                Transmission.AddLine("Condition", new Message(colorLogCondition, colorLogCondition, options));
                Transmission.AddLine("Stacktrace", new Message(colorStackTrace, colorStackTrace, options));
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
                allowFrostedGlassEffect = false;
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
            // try
            // {
            //     
            // }
            // catch (Exception exception)
            // {
            //     Debug.Log(exception);
            //     return string.Empty;
            // }
        }
        #endregion
    }
}
