using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Processor;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Ganymed.Console.Core
{
    /// <summary>
    /// Custom console class.
    /// </summary>
    public sealed class Console : MonoSingleton<Console>, IActive, IConsoleInterface
    {
        #region --- [INSPECOTR] ---
        
        [HideInInspector] [SerializeField] public bool showReferences = false;

        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private TextMeshProUGUI outputField = null;
        [SerializeField] private TextMeshProUGUI inputPlaceHolder = null;
        [SerializeField] private TextMeshProUGUI inputProposal = null;
        [SerializeField] private TextMeshProUGUI inputText = null;
        [SerializeField] private Image frostedGlass = null; 
        [SerializeField] private Image backgroundImage = null;
        [SerializeField] private GameObject consoleFrame = null;
        [SerializeField] private RectTransform inputRect = null;
        [SerializeField] private RectTransform outputRect = null;
        [SerializeField] private Image scrollbar = null;
        [SerializeField] private Image scrollbarHandle = null;
       

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMPONENT PROPERTIES] ---

        private static TMP_InputField Input
        {
            get
            {
                try
                {
                    return input != null ? input : (input = Instance.inputField);
                }
                catch
                {
                    return null;
                }
            }
            set => input = value;
        }

        private static TMP_InputField input = null;
        
        
        /// <summary>
        /// The input field of the console.
        /// </summary>
        private static TextMeshProUGUI InputText
        {
            get
            {
                try
                {
                    return inputTextField ? inputTextField : (inputTextField = Instance.inputText);
                }
                catch
                {
                    return null;
                }
            }
            set => inputTextField = value;
        }
        private static TextMeshProUGUI inputTextField = null;
        
        
        
        /// <summary>
        /// The main text field of the console. 
        /// </summary>
        private static TextMeshProUGUI Output
        {
            get
            {
                try
                {
                    return output != null ? output : (output = Instance.outputField);
                }
                catch
                {
                    return null;
                }
            }
            set => output = value;
        }

        private static TextMeshProUGUI output = null;

        #endregion
        
        #region --- [PORPERTIES] ---
        
        
        public static bool IsInitialized => isInitialized ? isInitialized : Instance != null;
        private static bool isInitialized = false;
        
        public static float FontSize
        {
            get => Output.fontSize;
            set
            {
                ConsoleSettings.Instance.fontSize
                    = Mathf.Clamp(value, ConsoleSettings.MinFontSize, ConsoleSettings.MaxFontSize);
                Instance.ApplySettings(ConsoleSettings.Instance);
            }
        }

        public static float FontSizeInput
        {
            get => Input.pointSize;
            set
            {
                ConsoleSettings.Instance.inputFontSize
                    = Mathf.Clamp(value, ConsoleSettings.MinFontSize, ConsoleSettings.MaxFontSize);
                Instance.ApplySettings(ConsoleSettings.Instance);
            }
        }
        
        public bool FrostedGlassShader
        {
            get => ConsoleSettings.Instance.allowShader;
            set
            {
                ConsoleSettings.Instance.allowShader = value;
                Instance.ApplySettings(ConsoleSettings.Instance);
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---

        
        private string proposedCommandDescription = string.Empty;
        private string proposedCommand = string.Empty;
        private static LogTypeFlags allowedUnityMessages = LogTypeFlags.None;
        private static LogTypeFlags logStackTraceOn = LogTypeFlags.None;
        private static bool enableCursorOnActivation = false;
        private static bool logTimeOnInput = true;
        private static int breakLineHeight = 150;
        private static int defaultLineHeight = 100;
        private static bool clearConsoleAutomatically = false;
        private static int maxLogs = 20;
        private static readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

        private static readonly Queue<string> LogCache = new Queue<string>();
        private readonly List<InputCache> InputCache = new List<InputCache>();
        private int viewedInputCacheIndex = 0;

        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [DELEGATES] ---

        public delegate void ConsoleLogCountUpdateCallback(int current, int max);
        public delegate void ConsoleToggleCallback(bool newState);
        public delegate void ConsoleLogCallback(string message);
        public delegate void ConsoleRenderSettingsUpdateCallback(bool renderContentOnDrag, bool renderContentOnScale);
        
        #endregion

        #region --- [EVENTS] ---

        public static event ConsoleLogCallback OnConsoleLog;
        public static event ConsoleToggleCallback OnConsoleToggle;
        public static event ConsoleLogCountUpdateCallback OnConsoleLogCountUpdate;
        public static event ConsoleRenderSettingsUpdateCallback OnConsoleRenderSettingsUpdate;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---
        
        
        
        
        /// <summary>
        /// This methods applies the settings settings to the Command
        /// </summary>
        private void ApplySettings(ConsoleSettings settings)
        {
            if(this == null) return;
            
            if (ConsoleSettings.Instance.active != isActive)  SetActive(ConsoleSettings.Instance.active);
            
            ApplyGeneralSettingsFromConfiguration(settings);
            ApplyFontSizeFromConfiguration(settings);
            ApplyColorFromConfiguration(settings);
            ApplyConsoleColorFromConfiguration(settings);
            ApplyShaderFromConfiguration(settings);
            ApplyConsoleLink(settings);
            ValidateConsolesRectOffsets();
            ApplyRichTextFromConfiguration(settings);
            
            OnConsoleRenderSettingsUpdate?.Invoke(settings.renderContentOnDrag, settings.renderContentOnScale);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [APPLY SETTINGS VALIDATION] ---
        
        private void ApplyShaderFromConfiguration(ConsoleSettings settings)
        {
            if (frostedGlass != null)
                frostedGlass.enabled = settings.allowShader;
        }

        private void ApplyRichTextFromConfiguration(ConsoleSettings settings)
        {
            try
            {
                Output.richText = settings.showRichText;
            }
            catch
            {
                // ignored
            }
        }

        private void ApplyGeneralSettingsFromConfiguration(ConsoleSettings settings)
        {
            enableCursorOnActivation = settings.enableCursorOnActivation;
            breakLineHeight = settings.breakLineHeight;
            defaultLineHeight = settings.defaultLineHeight;
            allowedUnityMessages = settings.allowedUnityMessages;
            logStackTraceOn = settings.logStackTraceOn;
            logTimeOnInput = settings.logTimeOnInput;
            clearConsoleAutomatically = settings.clearConsoleAutomatically;
            maxLogs = settings.maxLogs; }
        

        private void ApplyConsoleLink(ConsoleSettings settings)
        {
            if (!Application.isPlaying) return;
            
            if (settings.bindConsoles) SubscribeToUnityConsoleCallback();
            else Application.logMessageReceived -= OnLogMessageReceived;
        }
        
        private static void SubscribeToUnityConsoleCallback()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;
        }
        
        private void ApplyFontSizeFromConfiguration(ConsoleSettings settings)
        {
            inputField.pointSize = settings.inputFontSize;
            inputProposal.fontSize = settings.inputFontSize;
            inputText.fontSize = settings.inputFontSize;
            inputPlaceHolder.fontSize = settings.inputFontSize;
            outputField.fontSize = settings.fontSize;
        }

        private void ApplyConsoleColorFromConfiguration(ConsoleSettings settings)
        {
            scrollbar.color = settings.colorScrollbar;
            scrollbarHandle.color = settings.colorScrollbarHandle;
        }

        private void ApplyColorFromConfiguration(ConsoleSettings settings)
        {
            
            if (outputField == null || inputProposal == null || backgroundImage == null) return;
            
            outputField.color = settings.colorDefault;
            inputProposal.color = settings.colorAutocompletion;
            backgroundImage.color = settings.colorConsoleBackground;
        }

        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMANDS] ---

        [NativeCommand]
        [ConsoleCommand("Clear", Description = "Clear console and input cache")]
        private static void ClearConsole(bool clearCache)
        {
            LogCache.Clear();
            
            Input.text = string.Empty;
            Output.text = string.Empty;

            OnConsoleLogCountUpdate?.Invoke(LogCache.Count, maxLogs);
            
            if (!clearCache) return;
            Instance.InputCache.Clear();
            Instance.viewedInputCacheIndex = 0;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [ENTRY POINTS] ---

        /// <summary>
        /// Use this method to toggle the console On/Off
        /// </summary>
        public void Toggle() => SetActive(!isActive);         
        
        /// <summary>
        /// Use the current proposed input.
        /// </summary>
        public void ApplyProposedInput()
        {
            if(proposedCommand == string.Empty) return;
            inputField.text = proposedCommand;
            StartCoroutine(MoveSelectionToEndOfLine());             
        }

        public void SelectPreviousInputFromCache() => PreviousFromCache();

        public void SelectSubsequentInputFromCache() => SubsequentFromCache();

        public string GetProposedDescription() => proposedCommandDescription;

        public string GetProposedCommand() => proposedCommand;

        public void CopyLastLogToClipboard()
        {
            if(LogCache.Count > 0) LogCache.Last().CopyToClipboard(true);
        }

        public void CopyAllToClipboard()
        {
            Output.text.CopyToClipboard(true);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [STATE] ---

        
        public event ActiveDelegate OnActiveStateChanged;

        public bool IsActive
        {
            get => isActive;
            private set
            {
                try
                {
                    isActive = value;
                    ConsoleSettings.Instance.active = value;
                    
                    OnActiveStateChanged?.Invoke(value);
                
                    if (value) ActivateConsole();
                    else DeactivateConsole();
                }
                catch
                {
                    //ignored
                }
            }
        }
        [SerializeField] [HideInInspector] private bool isActive = true;
        public void SetActive(bool active)
            => IsActive = active;

        
        #endregion
        
        #region --- [ACTIVATION] ---
        
        private bool cachedCursorVisibility = false;
        private CursorLockMode cachedCursorState = CursorLockMode.None;


        private async void ActivateConsole()
        {
            await Task.Delay(0b1);
            try
            {
                consoleFrame.SetActive(true);
                inputField.ActivateInputField();
                if (enableCursorOnActivation)
                {
                    cachedCursorVisibility = Cursor.visible;
                    cachedCursorState = Cursor.lockState;
                    await Task.Delay(0b110010);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                OnConsoleToggle?.Invoke(true);
            }
            catch
            {
                return;
            }
        }


        private void DeactivateConsole()
        {
#if UNITY_EDITOR
            if(Application.isPlaying) ClearInput();
#else
            ClearInput();
#endif
            
            consoleFrame.SetActive(false);
            if (enableCursorOnActivation)
            {
                Cursor.visible = cachedCursorVisibility;
                Cursor.lockState = cachedCursorState;
            }
            OnConsoleToggle?.Invoke(false);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INPUT PROCESSING] ---

        public async void OnInputEnter(string inputString)
        {
            inputString = inputString.Cut();
            
            if (string.IsNullOrWhiteSpace(inputString)) {
                ClearInput(false);
                return;
            }

            CacheInput(inputString);

            if (inputString.StartsWith(CommandProcessor.Prefix))
            {
                Log(inputString,
                    ConsoleSettings.ColorInputLine,
                    breakLineHeight,
                    LogOptions.IsInput);
                await CommandProcessor.Process(inputString);
            }
                
            else
            {
                // --- Remove richText color
                Log($"</color>{inputString}");
            }

            RemoveLastItemFromCache();
            
            SetColor(InputValidation.None);
            ClearInput();
        }

        public void OnInputChanged(string inputString)
        {
            proposedCommandDescription = string.Empty;
            proposedCommand = string.Empty;

            if (ConsoleSettings.Instance.allowCommandPreProcessing && inputString.Cut(StartEnd.Start).StartsWith(CommandProcessor.Prefix))
            {
                if (CommandProcessor.ProposeMethodCommand(
                    inputString,
                    out var descriptiveProposal,
                    out var proposal,
                    out var validationFlag))
                {
                    proposedCommandDescription = descriptiveProposal;
                    proposedCommand = proposal;
                }

                SetColor(validationFlag);
            }
            else
            {
                SetColor(InputValidation.None);
            }
            
            inputProposal.text = proposedCommandDescription;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [UNITY CONSOLE] ---

        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            if(!Application.isPlaying) return;

            const LogOptions tab = LogOptions.Tab;
            const LogOptions endLine = LogOptions.EndLine | LogOptions.Tab;
            
            switch (type)
            {
                case LogType.Error when allowedUnityMessages.HasFlag(LogTypeFlags.Error):
                    Log("System Error:", ConsoleSettings.ColorUnityError, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Error))
                    {
                        Log(condition, tab);
                        Log(stacktrace.RemoveBreaks(), ConsoleSettings.ColorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, endLine);
                    }
                    break;
                
                
                case LogType.Assert when allowedUnityMessages.HasFlag(LogTypeFlags.Assert):
                    Log("System Assert:", LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Assert))
                    {
                        Log(condition, tab);
                        Log(stacktrace.RemoveBreaks(), ConsoleSettings.ColorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, endLine);
                    }
                    break;
                
                
                case LogType.Warning when allowedUnityMessages.HasFlag(LogTypeFlags.Warning):
                    Log("System Warning:", ConsoleSettings.ColorUnityWarning, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Warning))
                    {
                        Log(condition, tab);
                        Log(stacktrace.RemoveBreaks(), ConsoleSettings.ColorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, endLine);
                    }
                    break;
                
                
                case LogType.Log when allowedUnityMessages.HasFlag(LogTypeFlags.Log):
                    Log("System:", ConsoleSettings.ColorUnityLog, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Log))
                    {
                        Log(condition, tab);
                        Log(stacktrace.RemoveBreaks(), ConsoleSettings.ColorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, endLine);
                    }
                    break;
                
                
                case LogType.Exception when allowedUnityMessages.HasFlag(LogTypeFlags.Exception):
                    Log("System Exception:", ConsoleSettings.ColorUnityError, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Exception))
                    {
                        Log(condition, tab);
                        Log(stacktrace.RemoveBreaks(), ConsoleSettings.ColorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, endLine);
                    }
                    break;
                
                
                default:
                    return;
            }
        }

        #endregion
       
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONSOLE LOG] ---

        private const LogOptions logOptions = LogOptions.None;
        
        public static void Log(object message, LogOptions options = logOptions) 
            => Log(message?.ToString(), null, null, options);
        
        public static void Log(object message, int lineHeight, LogOptions options = logOptions)
            => Log(message?.ToString(), null, lineHeight, options);
        
        public static void Log(object message, Color? color, LogOptions options = logOptions)
            => Log(message?.ToString(), color, null, options);
        
        
        public static void Log(string message, LogOptions options = logOptions) 
            => Log(message, null, defaultLineHeight, options);
        
        public static void Log(string message, int lineHeight, LogOptions options = logOptions)
            => Log(message, null, lineHeight, options);
        
        public static void Log(string message, Color? color, LogOptions options = logOptions)
            => Log(message, color, null, options);
        
        
        public static void Log(string message, Color? color, int? lineHeight, LogOptions options)
        {
            if(Output == null || message == null) return;
            
            var prefix = string.Empty;
            var suffix = string.Empty;

            if (!options.HasFlag(LogOptions.IgnoreFormatting))
                prefix = $"{prefix}</color>{(lineHeight ?? defaultLineHeight).AsLineHeight()}{color?.AsRichText()}";

            if (!options.HasFlag(LogOptions.DontBreak))
                prefix = $"\n{prefix}";
            
            if (options.HasFlag(LogOptions.Cross))
            {
                prefix = $"{prefix}<s>";
                suffix = $"{suffix}</s>";
            }
                
            if (options.HasFlag(LogOptions.Bold))
            {
                prefix = $"{prefix}<b>";
                suffix = $"{suffix}</b>";
            }
            
            if (options.HasFlag(LogOptions.IsInput))
            {
                prefix = $"{prefix}{(logTimeOnInput ? $"\n[{DateTime.Now:hh:mm:ss}] > " : "> ")}";
            }
            
            if (options.HasFlag(LogOptions.Tab))
            {
                prefix = $"{prefix}<indent=4px>";
                suffix = "</indent>";
            }

            if (options.HasFlag(LogOptions.EndLine))
                suffix = $"{suffix}{breakLineHeight.AsLineHeight()}";

            var compiled = $"{prefix}{message}{suffix}";

            if (clearConsoleAutomatically)
            {
                CacheMessageAndLog(compiled);    
            }
            else
            {
                Output.text = $"{Output.text}{compiled}";
            }

            OnConsoleLog?.Invoke(compiled);
        }
        
        public static void LogRaw(object message)
        {
            if(Output == null) return;
            if (clearConsoleAutomatically)
            {
                CacheMessageAndLog(message);    
            }
            else
            {
                Output.text = $"{Output.text}{message}";
            }
            OnConsoleLog?.Invoke(message.ToString());
        }


        private static void CacheMessageAndLog(object message) => CacheMessageAndLog(message.ToString());
        private static void CacheMessageAndLog(string message)
        {
            LogCache.Enqueue(message);

            Output.text = string.Empty;
            foreach (var msg in LogCache)
            {
                Output.text = $"{Output.text}{msg}";
            }
            
            if(LogCache.Count > maxLogs)
                LogCache.Dequeue();
            
            OnConsoleLogCountUpdate?.Invoke(LogCache.Count, maxLogs);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COLOR] ---

        private static void SetColor(InputValidation signature)
        {
            if(InputText == null) return;

            switch (signature)
            {
                case InputValidation.None:
                    InputText.color = ConsoleSettings.ColorDefault;
                    break;
                case InputValidation.Valid:
                    InputText.color = ConsoleSettings.ColorInputValid;
                    break;
                case InputValidation.Incomplete:
                    InputText.color = ConsoleSettings.ColorInputIncomplete;
                    break;
                case InputValidation.Incorrect:
                    InputText.color = ConsoleSettings.ColorInputIncorrect;
                    break;
                case InputValidation.CommandInfo:
                    InputText.color = ConsoleSettings.ColorOperator;
                    break;
                case InputValidation.Optional:
                    InputText.color = ConsoleSettings.ColorInputOptional;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signature), signature, null);
            }
        }

        private static void SetColor(Color color)
        {
            InputText.color = color;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        public void OnInstantiation()
        {
            Awake();
        }
        
        protected override void Awake()
        {
            base.Awake();
            ApplySettings(ConsoleSettings.Instance);
            Initialize();
        }

        private async void Start()
        {
            await Task.Delay(1);
            if(ConsoleSettings.Instance.logConfigurationOnStart)
                ConsoleSettings.Instance.LogConfiguration(false);
            
            if(ConsoleSettings.Instance.activateConsoleOnStart && !isActive)
                SetActive(true);
        }

        private void OnEnable()
        {
            UnityEventCallbacks.ValidateUnityEventCallbacks();
            ConsoleSettings.OnConsoleSettingsChanged -= ApplySettings;
            ConsoleSettings.OnConsoleSettingsChanged += ApplySettings;
        }

        private void OnDisable()
        {
            ConsoleSettings.OnConsoleSettingsChanged -= ApplySettings;
        }
        


        private void Initialize()
        {
            UnityEventCallbacks.ValidateUnityEventCallbacks();
            
            if(isInitialized || this == null) return;
            SetActive(ConsoleSettings.Instance.active);
            isInitialized = true;
            
            ClearConsole();
            
            Output = outputField;
            InputText = inputText;
            Input = inputField;

            if (!ConsoleSettings.Instance.bindConsoles) return;
            SubscribeToUnityConsoleCallback();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        static Console()
        {
            UnityEventCallbacks.AddEventListener(
                SetPrefix, 
                UnityEventType.Start);
        }
        
        public Console()
        {
            UnityEventCallbacks.AddEventListener(
                ValidateConsolesRectOffsets,
                UnityEventType.Recompile,
                UnityEventType.OnEnable,
                UnityEventType.ApplicationQuit);
            
            ConsoleSettings.OnConsoleSettingsChanged -= ApplySettings;
            ConsoleSettings.OnConsoleSettingsChanged += ApplySettings;
        }

        ~Console()
        {
            UnityEventCallbacks.RemoveEventListener(
                SetPrefix, 
                UnityEventType.Start);
            
            UnityEventCallbacks.RemoveEventListener(
                ValidateConsolesRectOffsets,
                UnityEventType.EnteredEditMode);
        }

        private void OnDestroy()
        {
            UnityEventCallbacks.RemoveEventListener(
                SetPrefix, 
                UnityEventType.Start);
            
            UnityEventCallbacks.RemoveEventListener(
                ValidateConsolesRectOffsets,
                UnityEventType.EnteredEditMode);
            
            ConsoleSettings.OnConsoleSettingsChanged -= ApplySettings;
        }
        
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INPUT CACHING] ---

        private void PreviousFromCache()
        {
            if(InputCache.Count <= 0)
                return;
            
            SetColor(InputValidation.None);
            var i = 0;
            do
            {
                viewedInputCacheIndex--;
                if (viewedInputCacheIndex < 0)
                    viewedInputCacheIndex = InputCache.Count -1;
                
                inputField.text = InputCache[viewedInputCacheIndex].text;
                
                i++;
                if (i <= InputCache.Count) continue;
                Debug.Log("Warning possible endless loop");
                break;
                
            } while (string.IsNullOrWhiteSpace(InputCache[viewedInputCacheIndex].text));
            
            StartCoroutine(MoveSelectionToEndOfLine());
        }

        private void SubsequentFromCache()
        {
            if(InputCache.Count <= 0)
                return;
            
            SetColor(InputValidation.None);
            var i = 0;
            
            do
            {
                viewedInputCacheIndex++;
                if (viewedInputCacheIndex > InputCache.Count -1)
                    viewedInputCacheIndex = 0;
                
                inputField.text = InputCache[viewedInputCacheIndex].text;
                SetColor(InputCache[viewedInputCacheIndex].color);
                
                i++;
                if (i < InputCache.Count) continue;
                break;
                
            } while (string.IsNullOrWhiteSpace(InputCache[viewedInputCacheIndex].text));

            StartCoroutine(MoveSelectionToEndOfLine());
        }
        

        private void CacheInput(string inputString)
        {
            var i = 0;
            var inputCache = new InputCache(inputString, InputText.color);
            //validate that the input wont cached multiple times.
            while (InputCache.Contains(inputCache))    
            {
                //remove the new input to prevent repetitions.
                InputCache.Remove(inputCache);
                
                i++;
                if (i < InputCache.Count) continue;
                break;
            }

            if (!InputCache.Contains(inputCache))
                InputCache.Add(inputCache);
        }
       
        private void RemoveLastItemFromCache()
        {
            if(InputCache.Count > ConsoleSettings.Instance.inputCacheSize)
                InputCache.RemoveAt(0);

            viewedInputCacheIndex = InputCache.Count;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HELPER] ---
        
        public void ButtonClearConsole() => ClearConsole();
        private static void ClearConsole() => ClearConsole(true);
        
        

        /// <summary>
        /// Move the cursor to the end of the input field.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveSelectionToEndOfLine() 
        {
            yield return endOfFrame;
            inputField.MoveToEndOfLine(false,false);
        }
        
        
        private static void SetPrefix()
        {
            try
            {
                CommandProcessor.Prefix = ConsoleSettings.Instance.commandPrefix;
            }
            catch
            {
                // ignored
            }
        }

        private void ClearInput(bool reactivateInputField = true)
        {
            inputField.text = string.Empty;
            if(reactivateInputField)
                inputField.ActivateInputField();
            
            SetColor(InputValidation.None);
        }
        
        
        private async void ValidateConsolesRectOffsets()
        {
            await Task.CompletedTask.BreakContext();
            if(outputRect != null && inputRect != null)
                outputRect.offsetMin = new Vector2(outputRect.offsetMin.x, Mathf.Clamp(inputRect.sizeDelta.y, 16f, float.MaxValue));   
        }
        
        
        #endregion
    }
}
