using System;
using System.Reflection;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Processor;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Singleton;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Core
{
    [DeclaringName("Console")]
    public sealed class ConsoleSettings : Settings<ConsoleSettings>
    {
#pragma warning disable 414
#pragma warning disable 67

        #region --- [OVERRIDE SETTINGS] ---

        public override string FilePath() => "Assets/Ganymed/Console";
        
        
#if UNITY_EDITOR
        [MenuItem("Ganymed/Edit Console Settings", priority = 0)]
        public static void EditSettings()
        {
            SelectObject(Instance);
        }
#endif

        #endregion

        #region --- [INSPECTOR] ---

        [SerializeField] internal bool enabled = true;
        [SerializeField] internal bool active = true;
        
        [Header("Commands")]

        [Tooltip("This character will tell the console that the subsequent string should be handled like a console command.")]
        [SerializeField] internal string commandPrefix = "/";
        
        [Tooltip("Adding this character after a command will display additional information. eg: /Help?")]
        [SerializeField] internal string infoOperator = "?";
        
        [Tooltip("When enabled the console can precalculate inputs and generate suggestions for automatic completion.")]
        [SerializeField] internal bool enablePreProcessing = true;
        
        [Tooltip("When enabled 1 and 0 can be used as valid replacements for true and false.")]
        [SerializeField] internal bool enableNBP = true;
   
        
        [Header("Caching")]
        
        [Tooltip("When enabled, the amount of messages cached and displayed in the console is limited")]
        [SerializeField] internal bool limitMessageCache = true;
        
        [Tooltip("Determines the amount of messages cached and displayed in the console (if Limit Message Cache is enabled)")]
        [SerializeField] [Range(10,1000)] internal int messageCacheSize = 50;
        
        [Tooltip("How many previous inputs are cached")]
        [SerializeField] [Range(0,100)] internal byte inputCacheSize = 10;        
        
        
        [Header("Misc")]
        
        [Tooltip("When enabled information about commands will be displayed after the command processor has finished searching for commands in the target assemblies.")]
        [SerializeField] internal bool logLoadedCommandsOnStart = true;
        
        [Tooltip("Should the console be activated on start")]
        [SerializeField] internal bool activateConsoleOnStart = true;
                
        [Tooltip("When enabled the cursor will be set to visible and be unlocked every time the console is activated.")]
        [SerializeField] internal bool enableCursorOnActivation = true;
        
        [Tooltip("Log settings of the console on start.")]
        [SerializeField] internal bool logConfigurationOnStart = true;
        
        [Tooltip("Should the current time be logged to the console.")]
        [SerializeField] internal bool logTimeOnInput = true;

        
        [Header("Unity Console Integration")]
        
        
        [Tooltip("Bind Unities Debug console to this console ")]
        [SerializeField] internal bool bindConsoles = true;
        
        [Tooltip("What messages are also logged by this console (Log, Warning, Error, Exception, Assert).")]
        [SerializeField] internal LogTypeFlags allowedUnityMessages = LogTypeFlags.Everything;
        
        [Tooltip("What messages are allowed to show their stacktrace (Log, Warning, Error, Exception, Assert).")]
        [SerializeField] internal LogTypeFlags logStackTraceOn = LogTypeFlags.Exception | LogTypeFlags.Error;
        
        
        [Header("Eye Candy & Performance Optimization")]
        
        
        [Tooltip("When enabled animations and shaders are allowed. The shader used by the console to blur its background is not optimized and should be replaces when working either with HDRP or URP.")]
        [SerializeField] internal bool enableShaderAndAnimations = true;
        
        [Tooltip("When enabled the background shader is active. This shader will drain a lot of performance.")]
        [SerializeField] internal bool enableShader = true;
        
        [Tooltip("When enabled animations are played. E.G. easing resizing of the console window.")]
        [SerializeField] internal bool enableAnimations = true;

        [Tooltip("When disabled the content of the console will be disabled when the console is dragged to reduce rendering cost.")]
        [SerializeField] internal bool renderContentOnDrag = true;
        
        [Tooltip("When disabled the content of the console will be disabled when the console is scaled to reduce rendering cost.")]
        [SerializeField] internal bool renderContentOnScale = false;
        
        
        [Header("Debug")]
        
        [Tooltip("Debug option to show RichText.")]
        [SerializeField] internal bool enableRichText = true;
        
        [Header("Font")]
        
        
        [Tooltip("font size of the input")]
        [SerializeField] [Range(MinFontSize, MaxFontSize)] internal float inputFontSize = 6;
        
        [Tooltip("font size of the console")]
        [SerializeField] [Range(MinFontSize, MaxFontSize)] internal float fontSize = 6;
        
        [Space]
        
        [Tooltip("default line height")]
        [SerializeField] [Range(0, 300)] internal int breakLineHeight = 150;
        
        [Tooltip("line height after a break")]
        [SerializeField] [Range(0, 300)] internal int defaultLineHeight = 100;
        
        
        [Header("Console")]
        
        
        [SerializeField] internal Color colorConsoleBackground = new Color(0f, 0f, 0f, 0.86f);
        [SerializeField] internal Color colorScrollbar = new Color(0.04f, 0f, 0.14f);
        [SerializeField] internal Color colorScrollbarHandle = new Color(0.04f, 0.03f, 0.07f);
        
        
        [Header("Text")]
        
        
        [SerializeField] internal Color colorDefault = new Color(0.85f, 0.93f, 1f);
        [SerializeField] internal Color colorTitles = new Color(0.44f, 0.38f, 1f);
        [SerializeField] internal Color colorSubHeading = new Color(0.43f, 0.72f, 1f);
        [SerializeField] internal Color colorEmphasize = new Color(1f, 0.57f, 0f);
        [SerializeField] internal Color colorInputLines = new Color(0.31f, 1f, 0.77f);
        [SerializeField] internal Color colorVariables = new Color(1f, 0f, 0.56f);

        
        [Header("Validation (Input)")]
        
        
        [SerializeField] internal Color colorValidInput = new Color(0.11f, 1f, 0.42f);
        [SerializeField] internal Color colorOptionalParamsLeft = new Color(0.24f, 1f, 0.85f);
        [SerializeField] internal Color colorIncompleteInput = new Color(1f, 0.96f, 0.6f);
        [SerializeField] internal Color colorIncorrectInput = new Color(1f, 0.01f, 0f);
        [SerializeField] internal Color colorInformation = new Color(0.63f, 0.36f, 1f);
        [SerializeField] internal Color colorAutocompletion = new Color(0.39f, 0.36f, 0.44f);
       
        
        [Header("Color Unity Console Log/Warning/Error")]
        
        
        [SerializeField] internal Color colorUnityLog = new Color(1f, 0.98f, 0.72f);
        [SerializeField] internal Color colorUnityWarning = new Color(1f, 0.99f, 0.35f);
        [SerializeField] internal Color colorUnityError = new Color(1f, 0.34f, 0f);
        [SerializeField] internal Color colorStackTrace = new Color(0.53f, 0.53f, 0.68f);
        
        
        [Header("Custom Color")]
        
        
        [SerializeField] internal Color customColor1 = Color.magenta;
        [SerializeField] internal Color customColor2 = Color.magenta;
        [SerializeField] internal Color customColor3 = Color.magenta;

        #endregion

        #region --- [COLOR] ---
        
        public static Color ColorDefault => Instance.colorDefault;
                
        public static Color ColorTitleMain => Instance.colorTitles;
        public static Color ColorTitleSub => Instance.colorSubHeading;
        public static Color ColorEmphasize => Instance.colorEmphasize;
        public static Color ColorInputLine => Instance.colorInputLines;
        public static Color ColorVariables => Instance.colorVariables;
        
        public static Color ColorInputValid => Instance.colorValidInput;
        public static Color ColorInputOptional => Instance.colorOptionalParamsLeft;
        public static Color ColorInputIncomplete => Instance.colorIncompleteInput;
        public static Color ColorInputIncorrect => Instance.colorIncorrectInput;
        public static Color ColorOperator => Instance.colorInformation;
        public static Color ColorAutocompletion => Instance.colorAutocompletion;
        
        public static Color ColorUnityLog => Instance.colorUnityLog;
        public static Color ColorUnityWarning => Instance.colorUnityWarning;
        public static Color ColorUnityError => Instance.colorUnityError;
        public static Color ColorStackTrace => Instance.colorStackTrace;


        public static Color CustomColor1 => Instance.customColor1;
        public static Color CustomColor2 => Instance.customColor2;
        public static Color CustomColor3 => Instance.customColor3;

        #endregion
        
        #region --- [PROPERTIES] ---

        public bool IsEnabled => enabled;
        
        [Getter] public static string CommandPrefix => Instance.commandPrefix;

        [GetSet(Description = "Enable / Disable autocompletion and console color validation")]
        public static bool AllowCommandPreProcessing
        {
            get => Instance.enablePreProcessing;
            set
            {
                Instance.enablePreProcessing = value;
                Instance.OnSettingsChanged();
            }
        }

        [GetSet]
        public static bool AllowNumericBoolProcessing
        {
            get => Instance.enableNBP;
            set
            {
                Instance.enableNBP = value;
                Instance.OnSettingsChanged();
            }
        }
        
        [GetSet(Description = "Enable / Disable the background shader of the console.")]
        public static bool AllowBackgroundShader
        {
            get => Instance.enableShader;
            set
            {
                Instance.enableShader = value;
                Instance.OnSettingsChanged();
            }
        }
        
        
        [GetSet(Description = "Enable / Disable RichText.")]
        public static bool RichText
        {
            get => Instance.enableRichText;
            set
            {
                Instance.enableRichText = value;
                Instance.OnSettingsChanged();
            }
        }


        [GetSet]
        public static char InfoOperator
        {
            get => Instance.infoOperator.Cut()[0];
            set
            {
                Instance.infoOperator = value.ToString();
                Instance.OnSettingsChanged();
            }
        }
        



        #endregion
        
        #region --- [EVENTS] ---

        public delegate void ConsoleSettingsChangedDelegate(ConsoleSettings settings);
        public static event ConsoleSettingsChangedDelegate OnConsoleSettingsChanged;

        #endregion

        #region --- [CONST] ---

        public const int MaxFontSize = 22;
        public const int MinFontSize = 5;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMANDS] ---
        
        [NativeCommand]
        [ConsoleCommand("Settings", Priority = 500, Description = "Log the current settings of the console")]
        private static void LogConsoleConfiguration(
            [Hint("Log the settings of the consoles color scheme")]bool includeColorScheme = false)
            => Instance.LogConfiguration(includeColorScheme);

        public void LogConfiguration(bool includeColorScheme)
        {
            if(!Transmission.Start(TransmissionOptions.Enumeration)) return;
            
            const MessageOptions options = MessageOptions.Brackets;

            Transmission.AddBreak();
            Transmission.AddTitle("Console Settings");
            Transmission.AddBreak();
            Transmission.AddTitle("Commands", TitlePreset.Sub);
            
            Transmission.AddLine("Command Prefix:",
                new MessageFormat(commandPrefix, options),
                new MessageFormat($"// {this.GetTooltip(nameof(commandPrefix))}", options));
            
            Transmission.AddLine("Info Operator",
                new MessageFormat(infoOperator, options),
                new MessageFormat($"// {this.GetTooltip(nameof(infoOperator))}", options));
            
            Transmission.AddLine("Command Pre- Processing",
                new MessageFormat(enablePreProcessing, options),
                new MessageFormat($"// {this.GetTooltip(nameof(enablePreProcessing))}", options));
            
            Transmission.AddLine("Numeric Bool Processing",
                new MessageFormat(enableNBP, options),
                new MessageFormat($"// {this.GetTooltip(nameof(enableNBP))}", options));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddTitle("Eye Candy & Performance Optimization", TitlePreset.Sub);
            
            Transmission.AddLine("Allow Shader and enableAnimations",
                new MessageFormat(enableShaderAndAnimations, options),
                new MessageFormat($"// {this.GetTooltip(nameof(enableShaderAndAnimations))}", options));
            
            Transmission.AddLine("Forested Glass Shader",
                new MessageFormat(enableShader, options),
                new MessageFormat($"// {this.GetTooltip(nameof(enableShader))}", options));
            
            Transmission.AddLine("enableAnimations",
                new MessageFormat(enableAnimations, options),
                new MessageFormat($"// {this.GetTooltip(nameof(enableAnimations))}", options));
            
            Transmission.AddLine(nameof(renderContentOnDrag).AsLabel(),
                new MessageFormat(renderContentOnDrag, options),
                new MessageFormat($"// {this.GetTooltip(nameof(renderContentOnDrag))}", options));
            
            Transmission.AddLine(nameof(renderContentOnScale).AsLabel(),
                new MessageFormat(renderContentOnScale, options),
                new MessageFormat($"// {this.GetTooltip(nameof(renderContentOnScale))}", options));
            
            Transmission.AddBreak();
            
            
            
            Transmission.AddTitle("Console", TitlePreset.Sub);
            
            Transmission.AddLine("Input Cache:",
                new MessageFormat(inputCacheSize, options),
                new MessageFormat($"// {this.GetTooltip(nameof(inputCacheSize))}", options));
            
            Transmission.AddLine("Enable Cursor:",
                new MessageFormat(enableCursorOnActivation, options),
                new MessageFormat($"// {this.GetTooltip(nameof(enableCursorOnActivation))}", options));
            
            Transmission.AddLine("Activate OnStart:",
                new MessageFormat(activateConsoleOnStart, options),
                new MessageFormat($"// {this.GetTooltip(nameof(activateConsoleOnStart))}", options));
            
            Transmission.AddLine("Log Config OnStart:",
                new MessageFormat(logConfigurationOnStart, options),
                new MessageFormat($"// {this.GetTooltip(nameof(logConfigurationOnStart))}", options));
            
            Transmission.AddLine(nameof(limitMessageCache).AsLabel(),
                new MessageFormat(limitMessageCache, options),
                new MessageFormat($"// {this.GetTooltip(nameof(limitMessageCache))}", options));
            
            Transmission.AddLine(nameof(messageCacheSize).AsLabel(),
                new MessageFormat(messageCacheSize, options),
                new MessageFormat($"// {this.GetTooltip(nameof(messageCacheSize))}", options));
            
            
            
            Transmission.AddBreak();
            Transmission.AddLine("Unity Console Integration:",
                new MessageFormat(bindConsoles, options));
            
            Transmission.AddLine("Allowed Messages:",
                new MessageFormat(allowedUnityMessages, options),
                new MessageFormat($"// {this.GetTooltip(nameof(allowedUnityMessages))}", options));
            
            Transmission.AddLine("Allowed Stacktrace:",
                new MessageFormat(logStackTraceOn, options),
                new MessageFormat($"// {this.GetTooltip(nameof(logStackTraceOn))}", options));
            
            Transmission.AddBreak();
            
            Transmission.AddTitle("Format", TitlePreset.Sub);
            
            Transmission.AddLine("FontSize Input:",
                new MessageFormat(inputFontSize, options),
                new MessageFormat($"// {this.GetTooltip(nameof(inputFontSize))}", options));
            
            Transmission.AddLine("FontSize Command:",
                new MessageFormat(fontSize, options),
                new MessageFormat($"// {this.GetTooltip(nameof(fontSize))}", options));
            
            Transmission.AddLine("LineHeight (Break):",
                new MessageFormat(breakLineHeight, options),
                new MessageFormat($"// {this.GetTooltip(nameof(breakLineHeight))}", options));
            
            Transmission.AddLine("LineHeight (Default):",
                new MessageFormat(defaultLineHeight, options),
                new MessageFormat($"// {this.GetTooltip(nameof(defaultLineHeight))}", options));
            
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

        private void OnValidate() => OnSettingsChanged();

        private async void OnSettingsChanged()
        {
            if (Instance == null)
            {
                await Task.Delay(10);
                if(Instance == null)
                    throw new Exception($"Cannot find {GetType().Name} file, creating default settings");
            }
            
            if (!enableShaderAndAnimations)
            {
                enableShader = false;
                enableAnimations = false;
            }
            OnConsoleSettingsChanged?.Invoke(this);
            CommandProcessor.SetConfiguration(this);
        }
        #endregion
    }
}
