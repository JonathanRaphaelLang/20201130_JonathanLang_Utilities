using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganymed.Console.Enumerations;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ganymed.Console.Core
{
    public class Console : MonoSingleton<Console>, IVisible
    {
        #region --- [INSPECOTR] ---
#pragma warning disable 649
#pragma warning disable 414
        
        [Header("Configuration")] [Tooltip("The configuration file for the console")]
        [SerializeField] private ConsoleConfiguration config; 
        
        [Space]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI chatTextField;
        [SerializeField] private TextMeshProUGUI placeHolder;
        [SerializeField] private TextMeshProUGUI proposalField;
        [SerializeField] private TextMeshProUGUI inputFieldText;
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
        
        private static TMP_InputField InputField = null;
        private static TextMeshProUGUI ChatTextField = null;
        private static TextMeshProUGUI InputFieldText = null;

        private string proposedCommandDescription = string.Empty;
        private string proposedCommand = string.Empty;
        private bool applyProposedCommand = false;
        
        #endregion

        #region --- [PORPERTIES] ---
        public static ConsoleConfiguration Configuration => Instance.config;

        #endregion

        #region --- [CONST] ---

        public const int MaxFontSize = 20;
        public const int MinFontSize = 6;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        private ConsoleConfiguration lastConsoleConfiguration = null;


        //---- COLOR
        private static string warningColor = string.Empty;
        private static string errorColor = string.Empty;

        private static Color defaultInputColor = Color.magenta;
        private static Color validColor = Color.magenta;
        private static Color optionalParamsLeftColor = Color.magenta;
        private static Color incompleteColor = Color.magenta;
        private static Color incorrectColor = Color.magenta;
        private static Color infoColor = Color.magenta;
        
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

        #region --- [CONFIGURATION] ---

        /// <summary>
        /// This methods applies the configuration settings to the Console
        /// </summary>
        private async void SetConfiguration()
        {
            if(this == null) return;
            
            inputFieldText.fontSize = config.inputFontSize;
            proposalField.fontSize = config.inputFontSize;
            placeHolder.fontSize = config.inputFontSize;
            chatTextField.fontSize = config.chatFontSize;

            #region --- [COLOR] ---

            defaultInputColor = config.inputColor;
            validColor = config.validColor;
            optionalParamsLeftColor = config.optionalParamsLeftColor;
            incompleteColor = config.incompleteColor;
            incorrectColor = config.incorrectColor;
            infoColor = config.infoColor;
            warningColor = config.warningColor.ToRichTextMarkup();
            errorColor = config.errorColor.ToRichTextMarkup();

            if (chatTextField != null)
                chatTextField.color = config.chatColor;
            
            if(backgroundImage != null)
                backgroundImage.color = config.consoleBackgroundColor;

            if (config.visibility != VisibilityFlags)
                SetVisibility(config.visibility);

            #endregion

            if(frostedGlass != null)
                frostedGlass.enabled = config.allowFrostedGlassEffect;
            
            // --- BIND DEBUG CONSOLE
            if (Application.isPlaying)
            {
                if (config.bindDebugConsoleToConsole) {
                    Application.logMessageReceived -= OnLogMessageReceived;
                    Application.logMessageReceived += OnLogMessageReceived;
                }
                else {
                    Application.logMessageReceived -= OnLogMessageReceived;
                }    
            }

            scrollbar.color = config.scrollbarColor;
            scrollbarHandle.color = config.scrollbarHandleColor;

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
        //TODO: Outsource
        
        #region --- [TOGGLE] ---

        public void ToggleConsole()
        {
            SetVisibility(VisibilityFlags == Visibility.ActiveAndVisible
                ? Visibility.ActiveAndHidden
                : Visibility.ActiveAndVisible);    
        }
        
        private void _Update()
        {
            if (Input.GetKeyDown(config.ToggleKey))
            {
                ToggleConsole();  
            }
        }       
        
        public void ButtonClearConsole() => ClearConsole(true);
        
        public void DeselectInputField()
        {
            inputField.DeactivateInputField();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COMMANDS] ---

        [Command("Clear")]
        private static void ClearConsole(bool clearCache)
        {
            InputField.text = string.Empty;
            ChatTextField.text = string.Empty;

            if (!clearCache) return;
            Instance.InputCacheStack.Clear();
            Instance.viewedIndex = 0;
        }
        
        [Command("Console", true)]
        private static void SetProperty(ConsoleProperties property, bool value)
        {
            switch (property)
            {
                case ConsoleProperties.AllowFrostedGlass:
                    Instance.config.allowFrostedGlassEffect = value;
                    break;
                case ConsoleProperties.InputFontSize:
                case ConsoleProperties.OutputFontSize:
                case ConsoleProperties.FontSize:
                    return;
            }
            
            Instance.SetConfiguration();
        }
        
        [Command("Console")]
        private static void SetProperty(ConsoleProperties property, int value)
        {
            switch (property)
            {
                case ConsoleProperties.AllowFrostedGlass:
                    return;
                case ConsoleProperties.InputFontSize:
                    Instance.config.inputFontSize = Mathf.Clamp(value, MinFontSize, MaxFontSize);
                    break;
                case ConsoleProperties.OutputFontSize:
                    Instance.config.chatFontSize = Mathf.Clamp(value, MinFontSize, MaxFontSize);
                    break;
                case ConsoleProperties.FontSize:
                    Instance.config.inputFontSize = Mathf.Clamp(value, MinFontSize, MaxFontSize);
                    Instance.config.chatFontSize = Mathf.Clamp(value, MinFontSize, MaxFontSize);
                    break;
            }
            Instance.SetConfiguration();
        }

        private enum ConsoleProperties
        {
            AllowFrostedGlass,
            FontSize,
            InputFontSize,
            OutputFontSize
        }

        [Command("ConsoleConfig")]
        private static void LogConsoleConfiguration()
        {
            BeginTransmission(true);
            Transmit("Info Operator", Configuration.infoOperator);
            Transmit("Command Prefix:", Configuration.CommandPrefix);
            Transmit("Chat FontSize:", Configuration.chatFontSize);
            Transmit("Input FontSize:", Configuration.inputFontSize);
            Transmit("Chat FontSize:", Configuration.infoOperator);
            SendTransmission();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VISIBLILTY] ---

        public Visibility VisibilityFlags { get; private set; } = Visibility.Inactive;
        public void SetVisibility(Visibility visibility)
        {
            if(gameObject == null) return;
            
            OnVisibilityChanged?.Invoke(VisibilityFlags, visibility);
            VisibilityFlags = visibility;
            
            switch (visibility)
            {
                case Visibility.ActiveAndVisible:
                    consoleFrame.SetActive(true);
                    gameObject.SetActive(true);
                    inputField.ActivateInputField();
                    break;
                case Visibility.ActiveAndHidden:
                    consoleFrame.SetActive(false);
                    break;
                case Visibility.Inactive:
                    gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
            }
        }

        public event VisibilityDelegate OnVisibilityChanged;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INPUT] ---

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && proposedCommand != string.Empty)
            {
                inputField.text = proposedCommand;
                StartCoroutine(MoveSelectionToEndOfLine());
            }
            if(Input.GetKeyDown(config.PreviousInput)) SelectPreviousFromCache();
            if(Input.GetKeyDown(config.SubsequentInput)) SelectSubsequentFromCache();
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

            if (input.StartsWith(CommandHandler.Prefix))
            {
                Log(input, MessageFormat.Sender, 120);
                CommandHandler.ProcessCommand(input);
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

            if (config.allowCommandPreProcessing && input.Cut(StringExtensions.CutEnum.Start).StartsWith(CommandHandler.Prefix))
            {
                if (CommandHandler.Propose(input, out var descriptiveProposal, out var proposal, out var validationFlag)) //TODO: out color
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
            
            proposalField.text = proposedCommandDescription;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [UNITY CONSOLE] ---

        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            if(!Application.isPlaying) return;
            Log(condition);
            if(type == LogType.Error)
                Log(stacktrace);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        //--- MESSAGE
        
        #region --- [CONSOLE TRANSMISSION] ---

        private static bool isTransmitting = false;
        private static bool isEnumeration = false;
        private static int transmissionIndex = 0;
        private static MessageOptions usedOptions;
        private static readonly string EvenColor = new Color(0.72f, 0.7f, 0.85f).ToRichTextMarkup();
        private static readonly List<List<string>> transmissions = new List<List<string>>();
        private static readonly List<List<MessageOptions>> transmissionOptions = new List<List<MessageOptions>>();
        private static readonly List<int> maxLength = new List<int>();
        
        [Flags]
        public enum MessageOptions
        {
            None = 0,
            Break = 1,
            Bold = 2,
            Cursive = 4
        }

        /// <summary>
        /// Begin a transition. Use Transmit() to append messages you would like to add to the transmission.
        /// Use SendTransmission (out param) to Send the Transmission.
        /// </summary>
        /// <param name="enumeration"></param>
        public static void BeginTransmission(bool enumeration = false)
        {
            isTransmitting = true;
            transmissions.Clear();
            maxLength.Clear();
            transmissionIndex = 0;
            usedOptions = MessageOptions.None;
            isEnumeration = enumeration;
        }
        
        public static void SendTransmission()
        {
            var message = string.Empty;
            
            for (var i = 0; i < transmissions.Count; i++)
            {
                for (var j = 0; j < transmissions[i].Count; j++)
                {
                    if(maxLength.Cut() < j)
                        maxLength.Add(0);
                    
                    if (transmissions[i][j].Length > maxLength[j])
                        maxLength[j] = transmissions[i][j].Length;
                }
            }
            
            for (var i = 0; i < transmissions.Count; i++)
            {
                for (var j = 0; j < transmissions[i].Count; j++)
                {
                    if (transmissions[i].Cut() >= j)
                    {
                        message += $"{(j == 0 && i != 0? "\n" : string.Empty)}" +
                                   $"{(isEnumeration && j == 0? i.IsEven()? EvenColor : "</color>" : string.Empty)}" +
                                   //------------------------
                                   $"{transmissions[i][j]}" +
                                   //------------------------
                                   $"{(j < transmissions[i].Cut()? (maxLength[j] - transmissions[i][j].Length + 1).Repeat() : string.Empty)}" +
                                   $"{(i == transmissions.Cut()? LineHeight(150) : string.Empty)}";     
                    }
                }
            }
            
            isTransmitting = false;
            
            Log(message);
        }
        
        public static void Transmit(params object[] messages)
        {
            if(!isTransmitting)
                Debug.LogWarning("You are transmitting a message while no transmission has begun!");

            for (var i = 0; i < messages.Length; i++)
            {
                if (i == 0)
                {
                    var message = $"{messages[i]}";
                    transmissions.Add(new List<string>(){message});
                }
                else if (transmissions[transmissions.Cut()].Cut() < i)
                {
                    var message = $"{messages[i]}";
                    transmissions[transmissions.Cut()].Add(message);
                }
            }
        }
        
        public static void Transmit(object message, int column = 0, MessageOptions options = MessageOptions.None)
        {
            if(!isTransmitting)
                Debug.LogWarning("You are transmitting a message while no transmission has begun!");
            
            var br = options.HasFlag(MessageOptions.Break);
            var bold = options.HasFlag(MessageOptions.Bold);
            var cursive = options.HasFlag(MessageOptions.Cursive);

            if (br && !usedOptions.HasFlag(MessageOptions.Break))
                usedOptions |= MessageOptions.Break;
            if (bold && !usedOptions.HasFlag(MessageOptions.Bold))
                usedOptions |= MessageOptions.Bold;
            if (cursive && !usedOptions.HasFlag(MessageOptions.Cursive))
                usedOptions |= MessageOptions.Cursive;
            
            var formatted = 
                $"{(bold? "<b>" : string.Empty)}" +
                $"{message}" +
                $"{(br? LineHeight(150) : LineHeight(100))}";

            
            if (column == 0)
            {
                transmissions.Add(new List<string>(){formatted});
                transmissionIndex++;
            }
            else if (transmissions[transmissions.Cut()].Cut() < column)
            {
                transmissions[transmissions.Cut()].Add(formatted);
            }
        }

        #endregion

        #region --- [CONSOLE LOG] ---
        
        public static void Log(object message, MessageFormat format = MessageFormat.None, int lineHeight = 100)
            => Log(message.ToString(), format, lineHeight);
        
        public static void Log(string message, MessageFormat format = MessageFormat.None, int lineHeight = 100)
        {
            switch (format)
            {
                case MessageFormat.None:
                    ChatTextField.text += $"\n</color>{LineHeight(lineHeight)}{message}";
                    break;
                case MessageFormat.Warning:
                    ChatTextField.text += $"\n{warningColor}Warning\n{LineHeight(lineHeight)}{message}";
                    break;
                case MessageFormat.Error:
                    ChatTextField.text += $"\n{errorColor}Error\n{LineHeight(lineHeight)}{message}";
                    break;
                case MessageFormat.NoBreak:
                    ChatTextField.text += $"</color>{LineHeight(lineHeight)}{message}";
                    break;
                case MessageFormat.Sender:
                    ChatTextField.text += $"\n</color>{new Color(0.16f, 0.9f, 1f).ToRichTextMarkup()}> {LineHeight(lineHeight)}{message}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [COLOR] ---

        private static void SetColor(InputValidation signature)
        {
            switch (signature)
            {
                case InputValidation.None:
                    InputFieldText.color = defaultInputColor;
                    break;
                case InputValidation.Valid:
                    InputFieldText.color = validColor;
                    break;
                case InputValidation.Incomplete:
                    InputFieldText.color = incompleteColor;
                    break;
                case InputValidation.Incorrect:
                    InputFieldText.color = incorrectColor;
                    break;
                case InputValidation.CommandInfo:
                    InputFieldText.color = infoColor;
                    break;
                case InputValidation.Optional:
                    InputFieldText.color = optionalParamsLeftColor;
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

        #region --- [CREATE GAMEOBJECT (Editor)] ---

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

                    var obj = Instantiate(prefab);
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

        private void Initialize()
        {
            UnityEventCallbacks.ValidateUnityEventCallbacks();
            
            if(isInitialized || this == null) return;
            SetVisibility(config.visibility);
            isInitialized = true;
            
            chatTextField.text = string.Empty;
            
            UnityEventCallbacks.AddEventListener(
                _Update,
                true,
                CallbackDuring.PlayMode,
                UnityEventType.Update);
            
            ChatTextField = chatTextField;
            InputFieldText = inputFieldText;
            InputField = inputField;
            
            if (config.bindDebugConsoleToConsole)
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
            
            UnityEventCallbacks.RemoveEventListener(
                _Update,
                CallbackDuring.PlayMode,
                UnityEventType.Update);
        }

        private void ResetConsoleOnEdit() => chatTextField.text = string.Empty;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CACHING] ---

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
                //TODO: Check endless loop
                break;
                
            } while (string.IsNullOrWhiteSpace(InputCacheStack[viewedIndex].text));

            StartCoroutine(MoveSelectionToEndOfLine());
        }
        

        private void CacheInput(string input)
        {
            var i = 0;
            var Input = new InputCache(input, InputFieldText.color);
            //validate that the input wont cached multiple times.
            while (InputCacheStack.Contains(Input))    
            {
                //remove the new input to prevent repetitions.
                InputCacheStack.Remove(Input);
                
                i++;
                if (i < InputCacheStack.Count) continue;
                //TODO: Check endless loop
                break;
            }

            if (!InputCacheStack.Contains(Input))
                InputCacheStack.Add(Input);
        }
       
        private void RemoveLastItemFromCache()
        {
            if(InputCacheStack.Count > config.inputCacheSize)
                InputCacheStack.RemoveAt(0);

            viewedIndex = InputCacheStack.Count;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EXTENSION] ---

        private static string LineHeight(int value)
        {
            return $"<line-height={value}%>";
        }
        
        private static readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
        
        private IEnumerator MoveSelectionToEndOfLine()
        {
            yield return endOfFrame;
            inputField.MoveToEndOfLine(false,false);
        }
        
        
        private static void SetPrefix()
        {
            CommandHandler.Prefix = Instance.config.CommandPrefix;
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
