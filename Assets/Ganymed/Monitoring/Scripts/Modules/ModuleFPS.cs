using System;
using System.Globalization;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_FPS", menuName = "Monitoring/Modules/FPS")]
    public sealed class ModuleFPS : Module<float>
    {
        #region --- [INSPECTOR] ---
#pragma warning disable 649
        
        [Header("Settings")]
        [SerializeField] [Range(1,4)] private int preDecimalPlaces = 2;
        [SerializeField] [Range(0,10)] private int postDecimalPlaces = 2;
        [SerializeField] [Range(.1f,1)] private float measurePeriod = 0.5f;

        [Header("Color")]
        [SerializeField] private Color colorMin = new Color(1,1,1,1);
        [SerializeField] [Range(0,300)] private int thresholdOne = 30;
        [SerializeField] private Color colorMid = new Color(1,1,1,1);
        [SerializeField] [Range(0,300)]private int thresholdTwo = 60;
        [SerializeField] private Color colorMax = new Color(1,1,1,1);
        
        #endregion
        
        #region --- [FIELDS] ---

        private int thresholdOneCache;
        private int thresholdTwoCache;
        private string colorMinMarkup = "<color=#FFF>";
        private string colorMidMarkup = "<color=#FFF>";
        private string cMax = "<color=#FFF>";
        private int fps = 0;
        private float timer = 0;
        private int lastFPS;

        private readonly CultureInfo culture = CultureInfo.InvariantCulture;

        #endregion
        
        #region --- [PROPERTIES] ---
        
        
        public static float LastMeasuredFps { get; private set; } = 0;
        public static float MeasurePeriod { get; private set; } = 0;
        

        #endregion
        
        #region --- [EVENTS] ---

        public static event ModuleUpdateDelegate OnValueChanged;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [MODULE] ---
        
        protected override string ParseToString(float currentValue)
        { return
                $"{(currentValue >= thresholdTwo ? cMax : currentValue >= thresholdOne ? colorMidMarkup : colorMinMarkup)}" +
                $"{currentValue.ToString($"{preDecimalPlaces.Repeat("0")}.{postDecimalPlaces.Repeat("0")}", culture)}";
        }
        
        protected override void OnInitialize()
        { 
            InitializeUpdateEvent(ref OnValueChanged);
            InitializeValue();
            MeasurePeriod = measurePeriod;
            ResetValues();
            CompileMarkup();
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VALIDATE & FORMATTING] ---

        private void ResetValues()
        {
            fps = 0;
            timer = 0;
            lastFPS = 0;
            LastMeasuredFps = 0;
        }

        protected override void OnValidate()
        {
            MeasurePeriod = measurePeriod;
            CompileMarkup();
            base.OnValidate();
        }

        private void CompileMarkup()
        {
            colorMinMarkup = colorMin.AsRichText();
            colorMidMarkup = colorMid.AsRichText();
            cMax = colorMax.AsRichText();

            if (thresholdOne > thresholdOneCache)
            {
                if (thresholdTwo < thresholdOne)
                    thresholdTwo = thresholdOne;
            }
            else if (thresholdTwo < thresholdTwoCache)
            {
                if (thresholdOne > thresholdTwo)
                    thresholdOne = thresholdTwo;
            }
            thresholdOneCache = thresholdOne;
            thresholdTwoCache = thresholdTwo;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FPS CALCULATION] ---
        
        protected override void Tick()
        {
            fps++;
            timer += Time.deltaTime;

            if (timer < measurePeriod) return;

            LastMeasuredFps = (fps / timer);
           
            if (Math.Abs(LastMeasuredFps - lastFPS) > .1f)
                OnValueChanged?.Invoke(LastMeasuredFps);

            lastFPS = fps;
            fps = 0;

            var rest = measurePeriod - timer;
            timer = rest;
        }

        #endregion

    }
}
