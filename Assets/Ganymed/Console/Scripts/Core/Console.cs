using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Processor;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Singleton;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Ganymed.Console.Core
{
    public sealed class Console : MonoSingleton<Console>, IState
    {
        
        #region --- [INSPECOTR] ---
#pragma warning disable 649
#pragma warning disable 414
        
        [Header("Configuration")] [Tooltip("The configuration file for the console")]
        [SerializeField] private ConsoleConfiguration config; 
        
        [Space]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI outputField;
        [SerializeField] private TextMeshProUGUI inputPlaceHolder;
        [SerializeField] private TextMeshProUGUI inputProposal;
        [SerializeField] private TextMeshProUGUI inputText;
        [Space]
        [SerializeField] private Image frostedGlass; 
        [SerializeField] private Image backgroundImage; 
        [Space]
        [SerializeField] private GameObject consoleFrame = null;
        [Space]
        [SerializeField] private RectTransform inputRect = null;
        [SerializeField] private RectTransform outputRect = null;
        [Space]
        [SerializeField] private Image scrollbar;
        [SerializeField] private Image scrollbarHandle;
       

        #endregion

        #region --- [FIELDS] ---

        private readonly List<InputCache> InputCacheStack = new List<InputCache>();
        private int viewedIndex = 0;
        
        private readonly struct InputCache
        {
            public readonly string text;
            public readonly Color color;

            public InputCache(string text, Color color)
            {
                this.text = text;
                this.color = color;
            }
        }
        
        private static TMP_InputField Input
        {
            get => _input != null? _input : Instance.inputField;
            set => _input = value;
        }
        
        private static TMP_InputField _input = null;
        
        private static TextMeshProUGUI Output
        {
            set => _output = value;
            get => _output != null ? _output : null;
        }
        
        private static TextMeshProUGUI _output = null;
        
        private static TextMeshProUGUI InputFieldText = null;

        private string proposedCommandDescription = string.Empty;
        private string proposedCommand = string.Empty;
        private bool applyProposedCommand = false;

        private static LogTypeFlags allowedUnityMessages = LogTypeFlags.None;
        private static LogTypeFlags logStackTraceOn = LogTypeFlags.None;

        private static bool enableCursorOnActivation = false;
        private static bool logTimeOnInput = true;
        
        private static int breakLineHeight = 150;
        private static int defaultLineHeight = 100;

        
        private static Color colorLog = Color.magenta;
        private static Color colorWarning = Color.magenta;
        private static Color colorError = Color.magenta;
        private static Color colorLogCondition = Color.magenta;
        private static Color colorStackTrace = Color.magenta;
        private static Color colorMarker = Color.magenta;
        private static Color colorInput = Color.magenta;
        private static Color colorValidInput = Color.magenta;
        private static Color colorOptionalParamsLeft = Color.magenta;
        private static Color colorIncompleteInput = Color.magenta;
        [GetSet]private static Color colorIncorrectInput = Color.magenta;
        private static Color colorInformation = Color.magenta;
        private static Color colorAutocompletion = Color.magenta;
        public static Color colorSender = Color.magenta;
        public static Color colorTitles = Color.magenta;
        [Setter]public static Color colorEmphasize = Color.magenta;
        [Setter]private static Color colorCommandInput = Color.magenta;
        [Setter] private static Color colorTextInput = Color.magenta;

        private ConsoleConfiguration lastConsoleConfiguration = null;
        
        private static readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        
        #endregion

        #region --- [PORPERTIES] ---

        [Getter]
        private static int field = 0;

        [GetSet("Prop")]
        private static int Property
        {
            get => field;
            set
            {
                Debug.Log(value);
                field = value;
            }
        }
        
        [Getter]
        public static float FontSize
        {
            get => Output.fontSize;
            set
            {
                Instance.config.fontSize
                    = Mathf.Clamp(value, ConsoleConfiguration.MinFontSize, ConsoleConfiguration.MaxFontSize);
                Instance.SetConfiguration();
            }
        }

        [Getter]
        public static float FontSizeInput
        {
            get => Input.pointSize;
            set
            {
                Instance.config.inputFontSize
                    = Mathf.Clamp(value, ConsoleConfiguration.MinFontSize, ConsoleConfiguration.MaxFontSize);
                Instance.SetConfiguration();
            }
        }

        [Getter]
        public bool FrostedGlassShader
        {
            get => Instance.config.allowFrostedGlassEffect;
            set
            {
                Instance.config.allowFrostedGlassEffect = value;
                Instance.SetConfiguration();
            }
        }

        [Getter]
        public static ConsoleConfiguration Configuration 
            => TryGetInstance(out var instance) ? instance.config : null;

        #endregion

        #region --- [EVENTS] ---

        public static Action<string> OnLog;
        public static Action<bool> OnToggle;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---
        
        private void OnValidate()
        {
            ValidateConfiguration();
            
            if(config != null)
                SetConfiguration();
        }

        private void ValidateConfiguration()
        {
            if(config == null)
                Debug.Log("Warning! No Configuration selected!");
            
            if (config == lastConsoleConfiguration) return;
            
            if (lastConsoleConfiguration != null)
                lastConsoleConfiguration.OnGUIChanged -= SetConfiguration;

            if (config != null)
                BindConfig();
                
            lastConsoleConfiguration = config;
        }

        private void BindConfig()
        {
            config.OnGUIChanged -= SetConfiguration;
            config.OnGUIChanged += SetConfiguration;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONFIGURATION VALIDATION] ---

        /// <summary>
        /// This methods applies the configuration settings to the Command
        /// </summary>
        private async void SetConfiguration()
        {
            if(this == null) return;
            
            enableCursorOnActivation = config.enableCursorOnActivation;
            breakLineHeight = config.breakLineHeight;
            defaultLineHeight = config.defaultLineHeight;
            allowedUnityMessages = config.allowedUnityMessages;
            logStackTraceOn = config.logStackTraceOn;
            logTimeOnInput = config.logTimeOnInput;

            #region --- [FONTSIZE] ---

            inputField.pointSize = config.inputFontSize;
            inputProposal.fontSize = config.inputFontSize;
            inputText.fontSize = config.inputFontSize;
            inputPlaceHolder.fontSize = config.inputFontSize;
            outputField.fontSize = config.fontSize;

            #endregion
            
            #region --- [COLOR] ---

            colorInput = config.colorInput;
            colorValidInput = config.colorValid;
            colorIncompleteInput = config.colorIncompleteInput;
            colorIncorrectInput = config.colorIncorrectInput;
            colorOptionalParamsLeft = config.colorOptionalParamsLeft;
            colorInformation = config.colorInformation;
            colorLog = config.colorLog;
            colorWarning = config.colorWarning;
            colorError = config.colorError;
            colorLogCondition = config.colorLogCondition;
            colorStackTrace = config.colorStackTrace;
            colorMarker = config.colorMarker;
            colorCommandInput = config.colorCommandInput;
            colorTextInput = config.colorTextInput;
            colorAutocompletion = config.colorAutocompletion;
            colorTitles = config.colorTitles;
            colorEmphasize = config.colorEmphasize;
            colorSender = config.colorSender;

            if (outputField != null)
                outputField.color = config.colorOutput;

            if (inputProposal != null)
                inputProposal.color = colorAutocompletion;
            
            if(backgroundImage != null)
                backgroundImage.color = config.colorConsoleBackground;

            #endregion
            
            if (config.enabled != isEnabled)
                SetEnabled(config.enabled);
            
            if (config.active != isActive)
                SetActive(config.active);

            if(frostedGlass != null)
                frostedGlass.enabled = config.allowFrostedGlassEffect;
            
            // --- BIND DEBUG CONSOLE
            if (Application.isPlaying)
            {
                if (config.bindConsoles) {
                    Application.logMessageReceived -= OnLogMessageReceived;
                    Application.logMessageReceived += OnLogMessageReceived;
                }
                else {
                    Application.logMessageReceived -= OnLogMessageReceived;
                }    
            }

            scrollbar.color = config.colorScrollbar;
            scrollbarHandle.color = config.colorScrollbarHandle;

            //This is a hack to prevent the fucking annoying "sEnDMesSAge caNnOt bE cAlLEd DuRinG AWaKe" Waring.
            await Task.Delay(1);
            CheckRectOffsets();
        }

        private void CheckRectOffsets()
        {
            if(outputRect != null && inputRect != null)
                outputRect.offsetMin = new Vector2(outputRect.offsetMin.x, Mathf.Clamp(inputRect.sizeDelta.y, 16f, float.MaxValue));
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMANDS] ---

        [Command("RichText", "Enable / disable RichText. Use for text/formatting debugging")]
        private static void SetRichText(bool enable = true)
            => Output.richText = enable;

        [Command("Clear", "Clear the console")]
        private static void ClearConsole(bool clearCache)
        {
            Input.text = string.Empty;
            Output.text = string.Empty;

            if (!clearCache) return;
            Instance.InputCacheStack.Clear();
            Instance.viewedIndex = 0;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [STATE] ---
        
        //TODO: rework Editor script affecting isEnabled and isActive
   
        /// <summary>
        /// Use this method to toggle the console On/Off
        /// </summary>
        public void ToggleConsole() => SetActive(!isActive); 

        public event ActiveAndEnabledDelegate OnActiveAndEnabledStateChanged;
        public event ActiveDelegate OnActiveStateChanged;
        public event EnabledDelegate OnEnabledStateChanged;

        public bool IsEnabled
        {
            get => isEnabled;
            private set
            {
                isEnabled = value;
                Configuration.enabled = value;

                

                OnActiveAndEnabledStateChanged?.Invoke(IsActive, IsEnabled);
                OnEnabledStateChanged?.Invoke(value);
                
                if (isActive && value) ActivateConsole();
                if (!value) IsActive = false;
                else DeactivateConsole();
            }
        }
        [SerializeField] [HideInInspector] private bool isEnabled = true;
        public void SetEnabled(bool enable)
            => IsEnabled = enable;


        public bool IsActive
        {
            get => isActive;
            private set
            {
                isActive = value;
                Configuration.active = value;
                
                OnActiveAndEnabledStateChanged?.Invoke(IsActive, IsEnabled);
                OnActiveStateChanged?.Invoke(value);
                
                if (isEnabled && value) ActivateConsole();
                else DeactivateConsole();
            }
        }
        
        [SerializeField] [HideInInspector] private bool isActive;
        public void SetActive(bool active)
            => IsActive = isEnabled? active : isActive;
        

        
        public bool IsActiveAndEnabled
        {
            get => IsEnabled && IsActive;
            private set
            {
                isEnabled = value;
                isActive = value;
                
                OnActiveAndEnabledStateChanged?.Invoke(IsActive, IsEnabled);
                OnEnabledStateChanged?.Invoke(value);
                OnActiveStateChanged?.Invoke(value);

                if (value) ActivateConsole();
                else DeactivateConsole();
            }
        }

        public void SetActiveAndEnabled(bool value)
            => IsActiveAndEnabled = value;
        
        #endregion
        
        #region --- [ACTIVATION] ---
        
        private bool cachedCursorVisibility = false;
        private CursorLockMode cachedCursorState = CursorLockMode.None;


        private void ActivateConsole()
        {
            consoleFrame.SetActive(true);
            gameObject.SetActive(true);
            inputField.ActivateInputField();
            if (enableCursorOnActivation)
            {
                cachedCursorVisibility = Cursor.visible;
                cachedCursorState = Cursor.lockState;
                        
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            OnToggle?.Invoke(true);
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
            OnToggle?.Invoke(false);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE] ---

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(config.ToggleKey))
            {
                ToggleConsole();  
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) && proposedCommand != string.Empty)
            {
                inputField.text = proposedCommand;
                StartCoroutine(MoveSelectionToEndOfLine()); 
            }
            if(UnityEngine.Input.GetKeyDown(config.PreviousInput)) SelectPreviousFromCache();
            if(UnityEngine.Input.GetKeyDown(config.SubsequentInput)) SelectSubsequentFromCache();
        }

        private void LateUpdate()
        {
            applyProposedCommand = false;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INPUT PROCESSING] ---

        public void OnInputEnter(string input)
        {
            input = input.Cut();
            
            if (string.IsNullOrWhiteSpace(input)) {
                ClearInput(false);
                return;
            }

            CacheInput(input);

            if (input.StartsWith(CommandProcessor.Prefix))
            {
                if (MethodProcessor.IsValid(input))
                {
                    Log(input,
                        colorCommandInput,
                        breakLineHeight,
                        LogOptions.IsInput);
                }
                else
                {
                    Log(input,
                        colorEmphasize,
                        breakLineHeight,
                        LogOptions.IsInput | LogOptions.Cross);
                }
                
                
                MethodProcessor.ProcessCommand(input);
            }
                
            else
            {
                // --- Remove richText color
                Log($"</color>{input}");
            }

            RemoveLastItemFromCache();
            
            SetColor(InputValidation.None);
            ClearInput();
        }

        public void OnInputChanged(string input)
        {
            proposedCommandDescription = string.Empty;
            proposedCommand = string.Empty;

            if (config.allowCommandPreProcessing && input.Cut(StringExtensions.CutEnum.Start).StartsWith(CommandProcessor.Prefix))
            {
                if (MethodProcessor.Propose(
                    input,
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
                    Log("System Error:", colorError, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Error))
                    {
                        Log(condition, colorLogCondition, tab);
                        Log(stacktrace.RemoveBreaks(), colorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, colorLogCondition, endLine);
                    }
                    break;
                
                
                case LogType.Assert when allowedUnityMessages.HasFlag(LogTypeFlags.Assert):
                    Log("System Assert:", LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Assert))
                    {
                        Log(condition, colorLogCondition, tab);
                        Log(stacktrace.RemoveBreaks(), colorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, colorLogCondition, endLine);
                    }
                    break;
                
                
                case LogType.Warning when allowedUnityMessages.HasFlag(LogTypeFlags.Warning):
                    Log("System Warning:", colorWarning, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Warning))
                    {
                        Log(condition, colorLogCondition, tab);
                        Log(stacktrace.RemoveBreaks(), colorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, colorLogCondition, endLine);
                    }
                    break;
                
                
                case LogType.Log when allowedUnityMessages.HasFlag(LogTypeFlags.Log):
                    Log("System:", colorLog, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Log))
                    {
                        Log(condition, colorLogCondition, tab);
                        Log(stacktrace.RemoveBreaks(), colorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, colorLogCondition, endLine);
                    }
                    break;
                
                
                case LogType.Exception when allowedUnityMessages.HasFlag(LogTypeFlags.Exception):
                    Log("System Exception:", colorError, LogOptions.IsInput);
                    if (logStackTraceOn.HasFlag(LogTypeFlags.Exception))
                    {
                        Log(condition, colorLogCondition, tab);
                        Log(stacktrace.RemoveBreaks(), colorStackTrace, endLine);
                    }
                    else
                    {
                        Log(condition, colorLogCondition, endLine);
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
            => Log(message.ToString(), null, defaultLineHeight, options);
        
        public static void Log(object message, int lineHeight, LogOptions options = logOptions)
            => Log(message.ToString(), null, lineHeight, options);
        
        public static void Log(object message, Color? color, LogOptions options = logOptions)
            => Log(message.ToString(), color, defaultLineHeight, options);
        
        
        public static void Log(string message, LogOptions options = logOptions) 
            => Log(message, null, defaultLineHeight, options);
        
        public static void Log(string message, int lineHeight, LogOptions options = logOptions)
            => Log(message, null, lineHeight, options);
        
        public static void Log(string message, Color? color, LogOptions options = logOptions)
            => Log(message, color, defaultLineHeight, options);
        
        public static void Log(string message, Color? color, int lineHeight, LogOptions options = logOptions)
        {
            if(Output == null) return;
            
            var prefix = string.Empty;
            var suffix = string.Empty;
            
            if (!options.HasFlag(LogOptions.IgnoreFormatting))
                prefix += $"</color>{lineHeight.AsLineHeight()}{color?.AsRichText()}";

            if (!options.HasFlag(LogOptions.DontBreak))
                prefix = $"\n{prefix}";

            if (options.HasFlag(LogOptions.Cross))
            {
                prefix += "<s>";
                suffix += "</s>";
            }
            
            if (options.HasFlag(LogOptions.IsInput))
            {
                prefix += logTimeOnInput? $"[{DateTime.Now:hh:mm:ss}] > " : "> ";
            }
            
            if (options.HasFlag(LogOptions.Tab))
            {
                prefix += "<indent=4px>";
                suffix = "</indent>";
            }
            
            if (options.HasFlag(LogOptions.EndLine))
                suffix += breakLineHeight.AsLineHeight();

            var compiled = $"{prefix}{message}{suffix}";
            Output.text += compiled;
            OnLog?.Invoke(compiled);
        }

        public static void LogRaw(object message)
        {
            if(Output == null) return;
            
            Output.text += message.ToString();
            OnLog?.Invoke(message.ToString());
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COLOR] ---

        private static void SetColor(InputValidation signature)
        {
            switch (signature)
            {
                case InputValidation.None:
                    InputFieldText.color = colorInput;
                    break;
                case InputValidation.Valid:
                    InputFieldText.color = colorValidInput;
                    break;
                case InputValidation.Incomplete:
                    InputFieldText.color = colorIncompleteInput;
                    break;
                case InputValidation.Incorrect:
                    InputFieldText.color = colorIncorrectInput;
                    break;
                case InputValidation.CommandInfo:
                    InputFieldText.color = colorInformation;
                    break;
                case InputValidation.Optional:
                    InputFieldText.color = colorOptionalParamsLeft;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signature), signature, null);
            }
        }

        private static void SetColor(Color color)
        {
            InputFieldText.color = color;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [GAMEOBJECT CREATION (Editor)] ---

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Ganymed/Console",false, 12)]
        public static void CreateGameObjectInstance()
        {
            Debug.Log("Creating Instance");
            
            try
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:prefab", new[] { "Assets" });

                foreach (var guid in guids)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);

                    if (prefab.name != "Console") continue;

                    PrefabUtility.InstantiatePrefab(prefab);
                    Console.Instance.Awake();
                    Debug.Log("Instantiated Console Prefab");
                    break;
                }
            }
            catch
            {
                Debug.LogWarning("Failed to instantiate Console! Make sure that the corresponding prefab" +
                                 "[Console] can be found within the project.");
            }
        }
#endif

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        private bool isInitialized = false;

        protected override void Awake()
        {
            base.Awake();
            SetConfiguration();
            Initialize();
        }

        private async void Start()
        {
            await Task.Delay(1);
            if(config.logConfigurationOnStart)
                config.LogConfiguration(false);
            
            SetActive(config.activateConsoleOnStart);
        }


        private void Initialize()
        {
            UnityEventCallbacks.ValidateUnityEventCallbacks();
            
            if(isInitialized || this == null) return;
            SetActive(config.active);
            isInitialized = true;
            
            outputField.text = string.Empty;
            
            Output = outputField;
            InputFieldText = inputText;
            Input = inputField;
            
            if (config.bindConsoles)
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                Application.logMessageReceived += OnLogMessageReceived;
            }
        }


        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        static Console()
        {
            UnityEventCallbacks.AddEventListener(
                SetPrefix, 
                true,
                UnityEventType.Start);
        }
        
        public Console()
        {
            UnityEventCallbacks.AddEventListener(
                BindConfig,
                true,
                UnityEventType.Recompile);
            
            UnityEventCallbacks.AddEventListener(
                ResetConsoleOnEdit,
                true,
                UnityEventType.EnteredEditMode);
            
            UnityEventCallbacks.AddEventListener(
                CheckRectOffsets,
                true,
                UnityEventType.Recompile,
                UnityEventType.OnEnable,
                UnityEventType.ApplicationQuit);
        }

        private void OnDestroy()
        {
            UnityEventCallbacks.RemoveEventListener(
                SetPrefix, 
                UnityEventType.Start);
            
            UnityEventCallbacks.RemoveEventListener(
                ResetConsoleOnEdit,
                UnityEventType.EnteredEditMode);
        }

        private void ResetConsoleOnEdit() => outputField.text = string.Empty;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INPUT CACHING] ---

        private void SelectPreviousFromCache()
        {
            if(InputCacheStack.Count <= 0)
                return;
            
            SetColor(InputValidation.None);
            var i = 0;
            do
            {
                viewedIndex--;
                if (viewedIndex < 0)
                    viewedIndex = InputCacheStack.Count -1;
                
                inputField.text = InputCacheStack[viewedIndex].text;
                
                i++;
                if (i <= InputCacheStack.Count) continue;
                Debug.Log("Warning possible endless loop");
                break;
                
            } while (string.IsNullOrWhiteSpace(InputCacheStack[viewedIndex].text));
            
            StartCoroutine(MoveSelectionToEndOfLine());
        }

        private void SelectSubsequentFromCache()
        {
            if(InputCacheStack.Count <= 0)
                return;
            
            SetColor(InputValidation.None);
            var i = 0;
            
            do
            {
                viewedIndex++;
                if (viewedIndex > InputCacheStack.Count -1)
                    viewedIndex = 0;
                
                inputField.text = InputCacheStack[viewedIndex].text;
                SetColor(InputCacheStack[viewedIndex].color);
                
                i++;
                if (i < InputCacheStack.Count) continue;
                break;
                
            } while (string.IsNullOrWhiteSpace(InputCacheStack[viewedIndex].text));

            StartCoroutine(MoveSelectionToEndOfLine());
        }
        

        private void CacheInput(string input)
        {
            var i = 0;
            var inputCache = new InputCache(input, InputFieldText.color);
            //validate that the input wont cached multiple times.
            while (InputCacheStack.Contains(inputCache))    
            {
                //remove the new input to prevent repetitions.
                InputCacheStack.Remove(inputCache);
                
                i++;
                if (i < InputCacheStack.Count) continue;
                break;
            }

            if (!InputCacheStack.Contains(inputCache))
                InputCacheStack.Add(inputCache);
        }
       
        private void RemoveLastItemFromCache()
        {
            if(InputCacheStack.Count > config.inputCacheSize)
                InputCacheStack.RemoveAt(0);

            viewedIndex = InputCacheStack.Count;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [HELPER] ---
        
        public void ButtonClearConsole() => ClearConsole(true);
        

        /// <summary>
        /// Move the cursor to the end of the input field.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveSelectionToEndOfLine() 
        {
            // We use a coroutine because a task will not guarantee that we reached the end of the frame 
            yield return endOfFrame;
            inputField.MoveToEndOfLine(false,false);
        }
        
        
        private static void SetPrefix()
        {
            try
            {
                CommandProcessor.Prefix = Instance.config.CommandPrefix;
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
        #endregion
    }
}
