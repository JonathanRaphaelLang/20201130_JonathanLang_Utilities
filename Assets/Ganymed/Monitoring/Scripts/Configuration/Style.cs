using System;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.Helper;
using TMPro;
using UnityEngine;

namespace Ganymed.Monitoring.Configuration
{
    [CreateAssetMenu(fileName = "Style", menuName = "Monitoring/Style")]
    public class Style : ScriptableObject
    {
        #region --- [STYLE] ---
        
        [HideInInspector] [SerializeField] public float fontSize = 11;
        [HideInInspector] [SerializeField] public bool individualFontSize = false;
        [HideInInspector] [SerializeField] public float prefixFontSize = 11;
        [HideInInspector] [SerializeField] public float infixFontSize = 11;
        [HideInInspector] [SerializeField] public float suffixFontSize = 11;
        
        [HideInInspector] [SerializeField] public bool individualMargins = false;
        [HideInInspector] [SerializeField] public float marginsAll = 5;
        [HideInInspector] [SerializeField] public float marginLeft;
        [HideInInspector] [SerializeField] public float marginTop;
        [HideInInspector] [SerializeField] public float marginRight;
        [HideInInspector] [SerializeField] public float marginBottom;

        [HideInInspector] [SerializeField] public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
        
        [Tooltip("If enabled, the main content will be wrapped in brackets.")]
        [HideInInspector] [SerializeField] public bool autoBrackets = true;
        
        [Tooltip("If enabled, spaces will be added between prefix, infix and suffix.")]
        [HideInInspector] [SerializeField] public bool autoSpace = true;
        
        [Tooltip("If enabled, you can add custom rich-text for individual aspects of the text.")]
        [HideInInspector] [SerializeField] public bool useCustomRichText = false;
        [HideInInspector] [SerializeField] public string prefixTextStyle;
        [HideInInspector] [SerializeField] public string infixTextStyle;
        [HideInInspector] [SerializeField] public string suffixTextStyle;

        [HideInInspector] [SerializeField] public Color colorBackground = new Color(0.05f, 0.05f, 0.11f);
        [HideInInspector] [SerializeField] public Color prefixColor = new Color(0.52f, 0.53f, 0.84f);
        [HideInInspector] [SerializeField] public Color infixColor = new Color(1f,1f,1f,1f);
        [HideInInspector] [SerializeField] public Color suffixColor = new Color(0.52f, 0.53f, 0.84f);

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PROPERTIES] ---
        public Vector4 margins => individualMargins
            ? new Vector4(marginLeft, marginTop, marginRight, marginBottom)
            : Vector4.one * marginsAll;
      
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTS] ---

        public event Action<InvokeOrigin>OnValuesChanged;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONST] ---

        public const int MaxFontSize = 72;
        public const int MinFontSize = 4;

        public const float MaxMargin = 500f;
        public const float MinMargin = 0f;
     
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        protected Style() 
            => UnityEventCallbacks.AddEventListener(Validate, true, UnityEventType.Start, UnityEventType.Recompile);

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        public void Validate(UnityEventType eventType = UnityEventType.Recompile)
        {
            OnValuesChanged?.Invoke(eventType.ToOrigin());
        } 

        #endregion
    }
}