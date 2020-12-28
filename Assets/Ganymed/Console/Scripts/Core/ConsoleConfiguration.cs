using System;
using Ganymed.Utils;
using Ganymed.Utils.Singleton;
using UnityEngine;

namespace Ganymed.Console.Core
{
    [CreateAssetMenu(fileName = "Console_Configuration", menuName = "Console/Configuration")]
    public class ConsoleConfiguration : ScriptableObject
    {
#pragma warning disable 414
#pragma warning disable 67
        
        //---- COMMANDS 
        
        [Header("Commands")]
        [SerializeField] public string CommandPrefix = "/";
        [SerializeField] public string infoOperator = "?";
        [SerializeField] public bool allowCommandPreProcessing = true;
        
        //---- CONSOLE 
        
        [Header("Console")]
        [SerializeField] public int inputCacheSize = 10;
        [SerializeField] public bool bindDebugConsoleToConsole = true;
        [SerializeField] public bool logCommandsLoadedOnStart = true;
        [SerializeField] public bool logConfigurationOnStart = true;
        [SerializeField] public bool logWarnings = true;
        
        [Tooltip("Allow numeric input for boolean parameter in console commands")]
        [SerializeField] public bool allowNumericBoolProcessing = true;
        
        [Space]
        //Tooltip
        [SerializeField] public Visibility visibility = Visibility.ActiveAndVisible;
        [SerializeField] [HideInInspector] public KeyCode ToggleKey = KeyCode.None;
        [SerializeField] [HideInInspector] public KeyCode PreviousInput = KeyCode.UpArrow;
        [SerializeField] [HideInInspector] public KeyCode SubsequentInput = KeyCode.DownArrow;
        
        [Header("Visuals")]
        [SerializeField] public bool allowFrostedGlassEffect = true;
        [Header("Console Color")]
        [SerializeField] public Color consoleBackgroundColor = Color.white;
        [Space]
        [SerializeField] public Color scrollbarColor = Color.gray;
        [SerializeField] public Color scrollbarHandleColor = Color.black;
        [Header("Font Color")]
        [SerializeField] public Color chatColor = new Color(0.89f, 0.87f, 1f);
        [SerializeField] public Color inputColor = Color.white;
        [Header("Validation Color (Input)")]
        [SerializeField] public Color validColor = new Color(0.59f, 1f, 0.62f);
        [SerializeField] public Color optionalParamsLeftColor = new Color(0.53f, 1f, 0.96f);
        [SerializeField] public Color incompleteColor = new Color(1f, 1f, 0.58f);
        [SerializeField] public Color incorrectColor = new Color(1f, 0f, 0.44f);
        [SerializeField] public Color infoColor = new Color(0.93f, 0.71f, 1f);
        [Header("Error & Warning Color")]
        [SerializeField] public Color warningColor = Color.white;
        [SerializeField] public Color errorColor = new Color(0.89f, 0.87f, 1f);
        [Header("FontSize")]
        [SerializeField] [Range(6, 18)] public float inputFontSize = 8;
        [SerializeField] [Range(6, 18)] public float chatFontSize = 8;
        
        #region --- [EVENTS] ---

        public event Action OnGUIChanged;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        public void GUIChanged()
        {
            OnGUIChanged?.Invoke();
            if(Core.Console.Configuration == this)
                CommandHandler.SetConfiguration(this);
        }
    }
}
