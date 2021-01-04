using System;
using System.Globalization;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_FPS", menuName = "Monitoring/ModuleDictionary/FPS")]
    public sealed class ModuleFPS : Module<float>
    {
        #region --- [INSPECTOR] ---
#pragma warning disable 649
        
        [Header("Configuration")]
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

        private int lastThresholdOne;
        private int lastThresholdTwo;
        private string colorMinMarkup = "<color=#FFF>";
        private string colorMidMarkup = "<color=#FFF>";
        private string cMax = "<color=#FFF>";
        private int fps = 0;
        private float timer = 0;
        private int lastFPS;

        private readonly CultureInfo culture = CultureInfo.InvariantCulture;

        #endregion
        
        #region --- [PROPERTIES] ---
        
        
        public static float CurrentFps { get; private set; } = 0;
        public static float MeasurePeriod { get; private set; } = 0;
        

        #endregion
        
        #region --- [EVENTS] ---

        public static event Action<float> OnValueChanged;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [MODULE] ---

        protected override string ValueToString(float currentValue)
        { return
                $"{(currentValue >= thresholdTwo ? cMax : currentValue >= thresholdOne ? colorMidMarkup : colorMinMarkup)}" +
                $"{currentValue.ToString($"{preDecimalPlaces.Repeat("0")}.{postDecimalPlaces.Repeat("0")}", culture)}";
        }
        
        protected override void OnInitialize()
        {
            MeasurePeriod = measurePeriod;
            SetUpdateDelegate(ref OnValueChanged);
            ResetValues();
            CompileMarkup();
            OnValueChanged?.Invoke(CurrentFps);
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VALIDATE & FORMATTING] ---

        private void ResetValues()
        {
            fps = 0;
            timer = 0;
            lastFPS = 0;
            CurrentFps = 0;
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

            if (thresholdOne > lastThresholdOne)
            {
                if (thresholdTwo < thresholdOne)
                    thresholdTwo = thresholdOne;
            }
            else if (thresholdTwo < lastThresholdTwo)
            {
                if (thresholdOne > thresholdTwo)
                    thresholdOne = thresholdTwo;
            }
            lastThresholdOne = thresholdOne;
            lastThresholdTwo = thresholdTwo;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FPS CALCULATION] ---
        
        protected override void Tick()
        {
            fps++;
            timer += Time.deltaTime;

            if (timer < measurePeriod) return;

            CurrentFps = (fps / timer);
           
            if (Math.Abs(CurrentFps - lastFPS) > .1f)
                OnValueChanged?.Invoke(CurrentFps);

            lastFPS = fps;
            fps = 0;

            var rest = measurePeriod - timer;
            timer = rest;
        }

        #endregion

    }
}
