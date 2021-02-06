using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_RecentFPS", menuName = "Monitoring/Modules/RecentFPS")]
    public sealed class ModuleRecentFPS : Module<Vector2>
    {
        #region --- [INSPECTOR] ---

        [Header("Config")]
        [SerializeField] private bool showRecentValues = false;
        [SerializeField] [Range(0.1f, 60f)] private float cacheDuration = 10f;
        [SerializeField] private bool showFPSMeasurePeriod = false;
        [SerializeField] private bool showCacheDuration = false;
        [SerializeField] private bool abbreviations = false;
        [Space]
        [SerializeField] [Range(6,72)] private float fontSize = 8;

        [Header("Color")]
        [SerializeField] private Color colorMin = new Color(1,1,1,1);
        [SerializeField] [Range(0,300)] private int thresholdOne = 30;
        [SerializeField] private Color colorMid = new Color(1,1,1,1);
        [SerializeField] [Range(0,300)]private int thresholdTwo = 60;
        [SerializeField] private Color colorMax = new Color(1,1,1,1);
        [Space]
        [SerializeField] private Color numColor = Color.cyan;
        
        private int recentMaximum = 0; 
        private int recentMinimum = 0;
        
        private readonly Queue<int> recentValues = new Queue<int>();

        #endregion

        #region --- [EVENTS] ---

        private event ModuleUpdateDelegate OnValueChanged;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        protected override string ParseToString(Vector2 currentValue)
        {
            return
                $"Max:{fontSize.ToFontSize()}[{(currentValue.y >= thresholdTwo ? colorMax.AsRichText() : currentValue.y >= thresholdOne ? colorMid.AsRichText() : colorMin.AsRichText())}{currentValue.y}{Configuration.infixColor.AsRichText()}]{Configuration.infixFontSize.ToFontSize()} | " +
                $"Min:{fontSize.ToFontSize()}[{(currentValue.x >= thresholdTwo ? colorMax.AsRichText() : currentValue.x >= thresholdOne ? colorMid.AsRichText() : colorMin.AsRichText())}{currentValue.x}{Configuration.infixColor.AsRichText()}]{Configuration.infixFontSize.ToFontSize()}" +
                $"{(showFPSMeasurePeriod? $" |{(abbreviations? " MP:" : " Measure Period:")} [{numColor.AsRichText()}{ModuleFPS.MeasurePeriod:0.0}{Configuration.infixColor.AsRichText()}]" : string.Empty )}" +
                $"{(showCacheDuration? $" |{(abbreviations? " CD:" : " Cache Duration:")} [{numColor.AsRichText()}{cacheDuration}{Configuration.infixColor.AsRichText()}]" : string.Empty )}";
        }

        protected override void OnInitialize()
        {
            InitializeUpdateEvent(ref OnValueChanged);
            
            ModuleFPS.OnValueChanged -= CalculateRecentBorderValues;
            ModuleFPS.OnValueChanged += CalculateRecentBorderValues;
        }

        private void CalculateRecentBorderValues(float recent)
        {
            if(recent == 0 || !showRecentValues) return;

            Task.Run(delegate
            {
                recentValues.Enqueue((int) recent);
                if (recentValues.Count > cacheDuration / ModuleFPS.MeasurePeriod)
                {
                    recentValues.Dequeue();
                }

                recentMaximum = int.MinValue;
                recentMinimum = int.MaxValue;

                foreach (var value in recentValues)
                {
                    if (value > recentMaximum)
                        recentMaximum = value;
                    if (value < recentMinimum)
                        recentMinimum = value;
                }

            }).Then(delegate{ OnValueChanged?.Invoke(new Vector2(recentMinimum, recentMaximum)); });
        }
    }
}
