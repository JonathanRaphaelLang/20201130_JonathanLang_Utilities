using System;
using System.Threading.Tasks;
using Ganymed.Monitoring.Core;
using Ganymed.Utils;
using UnityEngine;

namespace Ganymed.Monitoring.Configuration
{
    [CreateAssetMenu(fileName = "Global_Configuration", menuName = "Monitoring/Configuration", order = 1)]
    public class GlobalConfiguration : Configurable
    {
#pragma warning disable 649
#pragma warning disable 414
        
        #region --- [CONFIGURATION] ---

        //--- SETTINGS ---
        [HideInInspector] [SerializeField] public bool active = false;
        private bool lastActive = false;
        
        [HideInInspector] [SerializeField] public bool automateCanvasState = false;
        [HideInInspector] [SerializeField] public bool openCanvasOnEnterPlay = true;
        [HideInInspector] [SerializeField] public bool closeCanvasOnEdit = true;
        [HideInInspector] [SerializeField] public KeyCode toggleKey = KeyCode.F3;
        [HideInInspector] [SerializeField] public int sortingOrder = 10000;
        
        //--- DEBUG ---
        [HideInInspector] [SerializeField] public bool logSerializationEvents = false; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
       
        #region --- [CANVAS STYLE] ---

        //--- SPACING ---
        [HideInInspector] [SerializeField] public float canvasPadding = 0;
        [HideInInspector] [SerializeField] public float canvasMargin = 10;
        [HideInInspector] [SerializeField] public float elementSpacing = 0;
        [HideInInspector] [SerializeField] public float areaSpacing = 10;
        
        //--- BACKGROUND ---
        [HideInInspector] [SerializeField] public bool showBackground = true;
        [HideInInspector] [SerializeField] public Color colorCanvasBackground = new Color(0.71f, 1f, 0.68f);
        [HideInInspector] [SerializeField] public bool showAreaBackground = true;
        [HideInInspector] [SerializeField] public Color colorTopLeft = new Color(0.43f, 0.99f, 1f);
        [HideInInspector] [SerializeField] public Color colorTopRight = new Color(1f, 0.57f, 0.87f);
        [HideInInspector] [SerializeField] public Color colorBottomLeft = new Color(0.54f, 0.39f, 1f);
        [HideInInspector] [SerializeField] public Color colorBottomRight = new Color(0.69f, 0.64f, 1f);
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [SINGLETON] ---

        public static GlobalConfiguration Instance
        {
            get
            {
                if (instance)
                    return instance;

                var instances = Resources.FindObjectsOfTypeAll<GlobalConfiguration>();
                if (instances.Length > 0)
                {
                    if (instances.Length > 1)
                        Debug.LogWarning("More than one instance of type: GlobalConfiguration found.");
                    instance = instances[0];
                    return instance;
                }
                Debug.LogWarning("No instance found.");
                return null;
            }
        }

        private static GlobalConfiguration instance = null;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EVENTS] ---

        public static event Action<GlobalConfiguration> OnActiveConfigurationChanged;
        public static event Action<Visibility> OnVisiblityChanged;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        public void OnValidate()
        {
            ValidateGlobalConfiguration();
        }

        private async void ValidateGlobalConfiguration()
        {
            await Task.Delay(100);

            try
            {
                if(MonitorBehaviour.Instance.GlobalConfiguration == null || MonitorBehaviour.Instance.GlobalConfiguration != this) return;
            }
            catch
            {
                return;
            }

            if(active != lastActive)
                OnVisiblityChanged?.Invoke(active? Visibility.ActiveAndVisible : Visibility.ActiveAndHidden);
            lastActive = active;
            
            if(MonitorBehaviour.Instance.MonitoringCanvasBehaviour == null) return;
            active = MonitorBehaviour.Instance.MonitoringCanvasBehaviour.VisibilityFlags == Visibility.ActiveAndVisible;
            if (active != lastActive)
                lastActive = active;
            OnActiveConfigurationChanged?.Invoke(this);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SET VALUES] ---

        public void SetValues(Configurable ctx)
        { 
            this.fontSize = ctx.fontSize;
            this.individualFontSize = ctx.individualFontSize;
            this.prefixFontSize = ctx.prefixFontSize;
            this.infixFontSize = ctx.infixFontSize;
            this.suffixFontSize = ctx.suffixFontSize;
        
            this.individualMargins = ctx.individualMargins;
            this.marginsAll = ctx.marginsAll;
            this.marginLeft = ctx.marginLeft;
            this.marginTop = ctx.marginTop;
            this.marginRight = ctx.marginRight;
            this.marginBottom = ctx.marginBottom;
        
            this.autoBrackets = ctx.autoBrackets;
            this.autoSpace = ctx.autoSpace;
        
            this.useTextStyle = ctx.useTextStyle;
            this.prefixTextStyle = ctx.prefixTextStyle;
            this.infixTextStyle = ctx.infixTextStyle;
            this.suffixTextStyle = ctx.suffixTextStyle;

            this.colorBackground = ctx.colorBackground;
            this.prefixColor = ctx.prefixColor;
            this.infixColor = ctx.infixColor;
            this.suffixColor = ctx.suffixColor;

            ValidateGlobalConfiguration();
            Validate();
        }

        #endregion
        
    }
}
