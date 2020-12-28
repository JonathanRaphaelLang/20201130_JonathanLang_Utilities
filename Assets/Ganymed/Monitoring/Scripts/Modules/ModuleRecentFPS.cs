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
        [SerializeField] [Range(6,72)] private int fontSize = 8;

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

        private event Action<Vector2> OnValueChanged;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        protected override string DynamicValue(Vector2 value)
        {
            return
                $"Max:{fontSize.ToFontSize()}[{(recentMaximum >= thresholdTwo ? colorMax.ToRichTextMarkup() : recentMaximum >= thresholdOne ? colorMid.ToRichTextMarkup() : colorMin.ToRichTextMarkup())}{recentMaximum}{Config.infixColor.ToRichTextMarkup()}]{Config.infixFontSize.ToFontSize()} | " +
                $"Min:{fontSize.ToFontSize()}[{(recentMinimum >= thresholdTwo ? colorMax.ToRichTextMarkup() : recentMinimum >= thresholdOne ? colorMid.ToRichTextMarkup() : colorMin.ToRichTextMarkup())}{recentMinimum}{Config.infixColor.ToRichTextMarkup()}]{Config.infixFontSize.ToFontSize()}" +
                $"{(showFPSMeasurePeriod? $" |{(abbreviations? " MP:" : " Measure Period:")} [{numColor.ToRichTextMarkup()}{ModuleFPS.MeasurePeriod:0.0}{Config.infixColor.ToRichTextMarkup()}]" : string.Empty )}" +
                $"{(showCacheDuration? $" |{(abbreviations? " CD:" : " Cache Duration:")} [{numColor.ToRichTextMarkup()}{cacheDuration}{Config.infixColor.ToRichTextMarkup()}]" : string.Empty )}";
        }

        
        protected override void OnInitialize()
        {
            SetValueDelegate(ref OnValueChanged);
            
            ModuleFPS.OnValueChanged -= CalculateRecentBorderValues;
            ModuleFPS.OnValueChanged += CalculateRecentBorderValues;
        }

        private async void CalculateRecentBorderValues(float recent)
        {
            if(recent == 0 || !showRecentValues) return;
            
            await Task.Run(delegate
            {
                recentValues.Enqueue((int)recent);
                if (recentValues.Count > cacheDuration / ModuleFPS.MeasurePeriod) {
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
                
            });
            OnValueChanged?.Invoke(new Vector2(recentMinimum, recentMaximum));
        }
    }
}
