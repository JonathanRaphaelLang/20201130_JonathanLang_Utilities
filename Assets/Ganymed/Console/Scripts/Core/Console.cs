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
using Ganymed.Utils.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Ganymed.Console.Core
{
    /// <summary>
    /// Custom console class. Only one instance of this class is allowed at any point.
    /// Prefabricated static access points can be used to control the active instance of this class. 
    /// </summary>
    [ExecuteInEditMode]
    public sealed class Console : MonoSingleton<Console>, IActive
    {
        #region --- [STATIC ACCESS POINTS] --- 

        // -------------------------------------------------------------------------------------------------------------
        // --- METHODS
        // -------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Activate / Deactivate the console instance depending on its last state.
        /// </summary>
        public static void ToggleConsole() => Instance.Toggle();
        
        /// <summary>
        /// Set the active state of the console manually.
        /// </summary>
        /// <param name="state"></param>
        public static void SetConsoleActive(bool state) => Instance.SetActive(state);

        /// <summary>
        /// Apply the input proposed by the autocompletion. (If any is proposed)
        /// </summary>
        public static void ApplyInputProposedByAutocompletion() => Instance.ApplyProposedInput();

        /// <summary>
        /// Replace the current input with the previous input text from the input cache.
        /// </summary>
        public static void SelectPreviousInputFromCache() => Instance.PreviousFromCache();

        /// <summary>
        /// Replace the current input with the subsequent input text from the input cache.
        /// </summary>
        public static void SelectSubsequentInputFromCache() => Instance.SubsequentFromCache();
        
        /// <summary>
        /// Copy the last received / logged message to your systems clipboard. RichText will be excluded.
        /// </summary>
        public static void CopyLastMessageToClipboard() => Instance.CopyLastLogToClipboard();

        /// <summary>
        /// Copy the whole content of the console to your systems clipboard. RichText will be excluded.
        /// </summary>
        public static void CopyConsoleTextToClipboard() => Instance.CopyAllToClipboard();

        // -------------------------------------------------------------------------------------------------------------
        // --- PROPERTIES
        // -------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// The cached proposed description string. This includes hints and is (in most cases) not a valid input string
        /// for console commands.
        /// </summary>
        public static string ProposedDescription => Instance.proposedCommandDescription;

        /// <summary>
        /// The cached proposed description input string. This is an actual string that can be used as an input
        /// or is an incomplete input.
        /// </summary>
        public static string ProposedCommand => Instance.proposedCommand;
        
        /// <summary>
        /// This property will return false if no instance of the console is present. Use this to check if a console
        /// is initialized before accessing it.
        /// </summary>
        public static bool IsInitialized => isInitialized ? isInitialized : Instance != null;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INSPECOTR] ---
        
        [SerializeField] [HideInInspector] public bool misc = false;
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
        
        #region --- [UNITY EVENTS] ---
        
        

        [HideInInspector] [SerializeField] public StringEvent OnConsoleLogReceived = new StringEvent();
        [HideInInspector] [SerializeField] public UnityEvent OnConsoleEnabled = new UnityEvent();
        [HideInInspector] [SerializeField] public UnityEvent OnConsoleDisabled = new UnityEvent();

        #endregion
                        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMPONENTS] ---

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
        private static bool isInitialized = false;
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

        public static event ConsoleLogCallback OnConsoleLogCallback;
        public static event ConsoleToggleCallback OnConsoleToggleCallback;
        public static event ConsoleLogCountUpdateCallback OnConsoleLogCountUpdateCallback;
        public static event ConsoleRenderSettingsUpdateCallback OnConsoleRenderSettingsUpdateCallback;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [APPLY / ALTER SETTINGS] ---

        /// <summary>
        /// This methods applies the ConsoleSettings to the Console.
        /// </summary>
        private void ApplySettings() => ApplySettings(ConsoleSettings.Instance);
        
        
        /// <summary>
        /// This methods applies the passed settings to the Console.
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
            
            OnConsoleRenderSettingsUpdateCallback?.Invoke(settings.renderContentOnDrag, settings.renderContentOnScale);
        }
        
        //--------------------------------------------------------------------------------------------------------------
        
        private void ApplyShaderFromConfiguration(ConsoleSettings settings)
        {
            if (frostedGlass != null)
                frostedGlass.enabled = settings.enableShader;
        }

        private void ApplyRichTextFromConfiguration(ConsoleSettings settings)
        {
            try
            {
                Output.richText = settings.enableRichText;
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
            clearConsoleAutomatically = settings.limitMessageCache;
            maxLogs = settings.messageCacheSize; 
        }
        
        private void ApplyConsoleLink(ConsoleSettings settings)
        {
            if (!Application.isPlaying) return;
            
            if (settings.bindConsoles) SubscribeToUnityConsoleCallback();
            else Application.logMessageReceived -= OnLogMessageReceived;
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

        private void SubscribeToUnityConsoleCallback()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;
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
                if(!ConsoleSettings.Instance.enabled) return;
                try
                {
                    isActive = value;
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
            try
            {
                await Task.CompletedTask.BreakContext();
                consoleFrame.SetActive(true);
                inputField.ActivateInputField();
                OnConsoleToggleCallback?.Invoke(true);
                OnConsoleLogCountUpdateCallback?.Invoke(LogCache.Count, maxLogs);
                OnConsoleEnabled?.Invoke();

                if (enableCursorOnActivation)
                {
                    cachedCursorVisibility = Cursor.visible;
                    cachedCursorState = Cursor.lockState;
                    await Task.Delay(50);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            finally
            {
                ConsoleSettings.Instance.active = isActive;
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
            OnConsoleToggleCallback?.Invoke(false);
            OnConsoleDisabled?.Invoke();
            
            ConsoleSettings.Instance.active = isActive;
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

            AddToInputCache(inputString);

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

            RemoveLastItemFromInputCache();
            SetColor(InputValidation.None);
            ClearInput();
        }

        public void OnInputChanged(string inputString)
        {
            proposedCommandDescription = string.Empty;
            proposedCommand = string.Empty;

            if (ConsoleSettings.Instance.enablePreProcessing && inputString.Cut(StartEnd.Start).StartsWith(CommandProcessor.Prefix))
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

            OnConsoleLogCallback?.Invoke(compiled);
            Instance.OnConsoleLogReceived?.Invoke(compiled);
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
            OnConsoleLogCallback?.Invoke(message.ToString());
            Instance.OnConsoleLogReceived?.Invoke(message.ToString());
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
            
            OnConsoleLogCountUpdateCallback?.Invoke(LogCache.Count, maxLogs);
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
            SetActive(isActive);
            ApplySettings(ConsoleSettings.Instance);
            Initialize();
            OnConsoleLogCountUpdateCallback?.Invoke(LogCache.Count, maxLogs);
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
            
            ClearTextFields();
            
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
        

        private void AddToInputCache(string inputString)
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
       
        private void RemoveLastItemFromInputCache()
        {
            if(InputCache.Count > ConsoleSettings.Instance.inputCacheSize)
                InputCache.RemoveAt(0);

            viewedInputCacheIndex = InputCache.Count;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HELPER] ---
        
        
        private void ApplyProposedInput()
        {
            if(proposedCommand == string.Empty) return;
            inputField.text = proposedCommand;
            StartCoroutine(MoveSelectionToEndOfLine());             
        }
        

        /// <summary>
        /// Move the cursor to the end of the input field.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveSelectionToEndOfLine() 
        {
            yield return endOfFrame;
            inputField.MoveToEndOfLine(false,false);
        }
        
                
        
        /// <summary>
        /// Increment or decrement the consoles FontSize
        /// </summary>
        /// <param name="increment"></param>
        public static void IncrementFontSize(float increment)
        {
            var clamped =  Mathf.Clamp(ConsoleSettings.Instance.fontSize + increment, ConsoleSettings.MinFontSize, ConsoleSettings.MaxFontSize);
            ConsoleSettings.Instance.fontSize = clamped;
            ConsoleSettings.Instance.inputFontSize = clamped;
            Instance.ApplySettings();
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
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UNITY EVENT METHODS] ---

        
        public void Toggle() => SetActive(!isActive);
                
        public void CopyLastLogToClipboard()
        {
            if(LogCache.Count > 0) LogCache.Last().CopyToClipboard();
        }

        public void CopyAllToClipboard() => Output.text.CopyToClipboard();


        public void ClearConsoleTextFields() => ClearTextFields();

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
                
        #region --- [COMMANDS] ---

        [NativeCommand]
        [ConsoleCommand("Clear", Description = "Clear console and input cache")]
        public static void ClearTextFields(bool clearCache)
        {
            LogCache.Clear();
            
            Input.text = string.Empty;
            Output.text = string.Empty;

            OnConsoleLogCountUpdateCallback?.Invoke(LogCache.Count, maxLogs);
            
            if (!clearCache) return;
            Instance.InputCache.Clear();
            Instance.viewedInputCacheIndex = 0;
        }

        private static void ClearTextFields() => ClearTextFields(true);

        #endregion
    }
}
