using System;
using Ganymed.Console.Attributes;
using Ganymed.Console.Processor;
using Ganymed.Console.Transmissions;
using UnityEngine;

namespace Ganymed.Console.Core
{
    [CreateAssetMenu(fileName = "Console_Configuration", menuName = "Command/Configuration")]
    public sealed class ConsoleConfiguration : ScriptableObject
    {
#pragma warning disable 414
#pragma warning disable 67

        #region --- [INSPECTOR] ---
        
        [Tooltip("NA")]
        [SerializeField] public bool enabled = true;
        
        [Tooltip("NA")]
        [SerializeField] public bool active = true;
        
        
        
        [Header("Commands")]
        [Tooltip("use this as a prefix")]
        [SerializeField] public string CommandPrefix = "/";
        
        [Tooltip("use this character to receive additional information about a command. eg: /Help?")]
        [SerializeField] public string infoOperator = "?";
        
        [Tooltip("allows the console to precalculate inputs and to generate suggestions for automatic completion")]
        [SerializeField] public bool allowCommandPreProcessing = true;
        
        [Tooltip("log information about commands on start")]
        [SerializeField] public bool logCommandsLoadedOnStart = true;
                
        [Tooltip("allow numeric input for boolean parameter in console commands")]
        [SerializeField] public bool allowNumericBoolProcessing = true;
        
        
        
        [Header("Console")]
        [Tooltip("should the console be activated on start")]
        [SerializeField] public bool activateConsoleOnStart = true;
                
        [Tooltip("should the cursor be enabled if the console is active")]
        [SerializeField] public bool enableCursorOnActivation = true;
        
        [Tooltip("log configuration of the console on start")]
        [SerializeField] public bool logConfigurationOnStart = true;
        
        [Tooltip("should the current time be logged to the console")]
        [SerializeField] public bool logTimeOnInput = true;
        
        [Tooltip("how many previous inputs are cached")]
        [SerializeField] public int inputCacheSize = 20;

        
        
        [Header("Unity Console Integration")]
        [Tooltip("bind Unities Debug console to this console ")]
        [SerializeField] public bool bindConsoles = true;
        
        [Tooltip("which types of messages are logged by this console (Log, Warning, Error, Exception, Assert)")]
        [SerializeField] public LogTypeFlags allowedUnityMessages = LogTypeFlags.None;
        
        [Tooltip("which types messages are allowed to show their stacktrace (Log, Warning, Error, Exception, Assert)")]
        [SerializeField] public LogTypeFlags logStackTraceOn = LogTypeFlags.None;
  
        
        
        [Tooltip("Key to enable / disable the console")]
        [SerializeField] [HideInInspector] public KeyCode ToggleKey = KeyCode.None;
        
        [Tooltip("NA")]
        [SerializeField] [HideInInspector] public KeyCode PreviousInput = KeyCode.UpArrow;
        
        [Tooltip("NA")]
        [SerializeField] [HideInInspector] public KeyCode SubsequentInput = KeyCode.DownArrow;



        [Header("Eye Candy")]
        [Tooltip("NA")]
        [SerializeField] public bool allowShaderAndAnimations = true;
        
        [Tooltip("Is the frosted glass shader enabled. This shader will drain a lot of performance")]
        [SerializeField] public bool allowFrostedGlassEffect = true;
        
        [Tooltip("NA")]
        [SerializeField] public bool allowAnimations = true;
        
        
        
        [Header("Command Color")]
        [SerializeField] public Color colorConsoleBackground = Color.magenta;
        [SerializeField] public Color colorScrollbar = Color.magenta;
        [SerializeField] public Color colorScrollbarHandle = Color.magenta;
        
        
        
        [Header("Font Color")]
        [SerializeField] public Color colorOutput = Color.magenta;
        [SerializeField] public Color colorInput = Color.magenta;
        
        
        
        [Header("Color Input")]
        [SerializeField] public Color colorValid = Color.magenta;
        [SerializeField] public Color colorOptionalParamsLeft = Color.magenta;
        [SerializeField] public Color colorIncompleteInput = Color.magenta;
        [SerializeField] public Color colorIncorrectInput = Color.magenta;
        [SerializeField] public Color colorInformation = Color.magenta;
        [SerializeField] public Color colorAutocompletion = Color.magenta;

        
        
        [Header("Color Console")]
        [SerializeField] public Color colorTitles = Color.magenta;
        [Tooltip("Color to stress certain areas of interest")]
        [SerializeField] public Color colorEmphasize = Color.magenta;
        [SerializeField] public Color colorCommandInput = Color.magenta;
        [SerializeField] public Color colorTextInput = Color.magenta;
        [SerializeField] public Color colorMarker = Color.magenta;
        [SerializeField] public Color colorComment = Color.magenta;
        [SerializeField] public Color colorSender = Color.magenta;
        
        
        
        [Header("Color Unity Console Log/Warning/Error")]
        [SerializeField] public Color colorLog = Color.magenta;
        [SerializeField] public Color colorWarning = Color.magenta;
        [SerializeField] public Color colorError = Color.magenta;
        [SerializeField] public Color colorLogCondition = Color.magenta;
        [SerializeField] public Color colorStackTrace = Color.magenta;
        
        
        
        [Header("Font")]
        [Tooltip("font size of the input")]
        [SerializeField] [Range(MinFontSize, MaxFontSize)] public float inputFontSize = 6;
        [Tooltip("font size of the console")]
        [SerializeField] [Range(MinFontSize, MaxFontSize)] public float fontSize = 6;
        [Space]
        [Tooltip("default line height")]
        [SerializeField] [Range(0, 300)] public int breakLineHeight = 130;
        [Tooltip("line height after a break")]
        [SerializeField] [Range(0, 300)] public int defaultLineHeight = 100;

        #endregion
        
        #region --- [EVENTS] ---

        public event Action OnGUIChanged;

        #endregion

        #region --- [CONST] ---

        public const int MaxFontSize = 22;
        public const int MinFontSize = 5;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMANDS] ---
        
        [Command("Config", "Log the current configuration of the console", 500)]
        private static void LogConsoleConfiguration(
            [Hint("Log the configuration of the consoles color scheme")]bool includeColorScheme = false)
            => Console.Configuration.LogConfiguration(includeColorScheme);

        public void LogConfiguration(bool includeColorScheme)
        {
            const MessageOptions options = MessageOptions.Bold | MessageOptions.Brackets;
            const MessageOptions comment = MessageOptions.Italics;

            Transmission.Start(TransmissionOptions.Enumeration);
            Transmission.AddBreak();
            
            
            
            Transmission.AddTitle("Commands");
            
            Transmission.AddLine("Command Prefix:",
                new Message(CommandPrefix, options),
                new Message($"// {GetTooltip(nameof(CommandPrefix))}", colorComment, comment));
            
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
            
            
            
            Transmission.AddTitle("Eye Candy");
            
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
            
            
            
            Transmission.AddTitle("Console");
            
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
            
            
            
            Transmission.AddTitle("Format");
            
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

            
            
            Transmission.AddTitle("Keys");
            
            Transmission.AddLine("Toggle:",
                new Message(ToggleKey, options),
                new Message($"// {GetTooltip(nameof(ToggleKey))}", colorComment, comment));
            
            Transmission.AddLine("PreviousInput",
                new Message(PreviousInput, options),
                new Message($"// {GetTooltip(nameof(PreviousInput))}", colorComment, comment));
            
            Transmission.AddLine("SubsequentInput",
                new Message(SubsequentInput, options),
                new Message($"// {GetTooltip(nameof(SubsequentInput))}", colorComment, comment));
            
            
            Transmission.AddBreak();

            
            if (includeColorScheme)
            {
                Transmission.AddTitle("Color");
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
                Transmission.AddLine("Marker", new Message(colorMarker, options | MessageOptions.Mark));
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

        private void OnValidate() => ConfigurationAltered();

        public void ConfigurationAltered()
        {
            if (!allowShaderAndAnimations)
            {
                allowFrostedGlassEffect = false;
                allowAnimations = false;
            }
            OnGUIChanged?.Invoke();
            if(Console.Configuration == this)
                CommandProcessor.SetConfiguration(this);
        }
   
        private string GetTooltip(string fieldName, bool inherit = true)
        {
            var field = GetType().GetField(fieldName);
            
            var attributes
                = field.GetCustomAttributes(typeof(TooltipAttribute), inherit)
                    as TooltipAttribute[];
 
            var ret = "";
            if (attributes != null && attributes.Length > 0)
                ret = attributes[0].tooltip;
 
            return ret;
        }
        #endregion
    }
}
