using System;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.Handler;
using UnityEngine;

namespace Ganymed.Monitoring.Configuration
{
    public abstract class Configurable : ScriptableObject
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
        
        [HideInInspector] [SerializeField] public bool autoBrackets = true;
        [HideInInspector] [SerializeField] public bool autoSpace = true;
        
        [HideInInspector] [SerializeField] public bool useTextStyle = false;
        [HideInInspector] [SerializeField] public string prefixTextStyle;
        [HideInInspector] [SerializeField] public string infixTextStyle;
        [HideInInspector] [SerializeField] public string suffixTextStyle;

        [HideInInspector] [SerializeField] public Color colorBackground = new Color(0.05f, 0.05f, 0.11f);
        [HideInInspector] [SerializeField] public Color prefixColor = new Color(0.52f, 0.53f, 0.84f);
        [HideInInspector] [SerializeField] public Color infixColor = new Color(1f,1f,1f,1f);
        [HideInInspector] [SerializeField] public Color suffixColor = new Color(0.52f, 0.53f, 0.84f);

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTS] ---

        public event Action<InvokeOrigin>OnValuesChanged;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PROPERTIES] ---

        public float FontSize => fontSize;
        public bool IndividualFontSize => individualFontSize;
        public float PrefixFontSize => prefixFontSize;
        public float InfixFontSize => infixFontSize;
        public float SuffixFontSize => suffixFontSize;
        public Color ColorBackground => colorBackground;
        public bool IndividualMargins => individualMargins;
        public float MarginsAll => marginsAll;
        public float MarginLeft => marginLeft;
        public float MarginTop => marginTop;
        public float MarginRight => marginRight;
        public float MarginBottom => marginBottom;
        public bool AutoBrackets => autoBrackets;
        public bool AutoSpace => autoSpace;
        public bool UseTextStyle => useTextStyle;
        public string PrefixTextStyle => prefixTextStyle;
        public string InfixTextStyle => infixTextStyle;
        public string SuffixTextStyle => suffixTextStyle;
        public Color PrefixColor => prefixColor;
        public Color InfixColor => infixColor;
        public Color SuffixColor => suffixColor;

        public Vector4 Margins => individualMargins
            ? new Vector4(marginLeft, marginTop, marginRight, marginBottom)
            : Vector4.one * marginsAll;
      
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [CONST] ---

        public const int MAXFONTSIZE = 72;
        public const int MINFONTSIZE = 4;

        public const float MAXMARGIN = 500f;
        public const float MINMARGIN = 0f;
     
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        protected Configurable()
        {
            UnityEventCallbacks.AddEventListener(Validate, true, UnityEventType.Start, UnityEventType.Recompile);
        }

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