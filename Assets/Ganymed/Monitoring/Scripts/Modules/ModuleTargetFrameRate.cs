using System;
using System.Threading.Tasks;
using Ganymed.Monitoring.Core;
using Ganymed.Utils;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_TargetFrameRate", menuName = "Monitoring/Modules/TargetFrameRate")]
    public sealed class ModuleTargetFrameRate : Module<int>
    {
        #region --- [INSPECTOR] ---

        [Header("TFR Configuration")]
        [SerializeField] private bool controlTargetFrameRate = false;
        [SerializeField] private FPSLimitations fpsLimitations = FPSLimitations.DontLimit;

        #endregion

        #region --- [FIELDS] ---
 
        private FPSLimitations lastFPSLimitations;

        #endregion

        #region --- [EVENTS] ---

        private static event Action<int> OnValueChanged; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE] ---

        protected override string DynamicValue(int value)
        {
            return value == int.MaxValue || value <= 0 ? "none" : value.ToString();
        }

        protected override void OnInitialize()
        {
            SetValueDelegate(ref OnValueChanged);
            OverrideDefaultValue(int.MaxValue);
            SetTargetFPS(true);
        }        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [AUTO INSPECTION] ---
        
        protected override void OnInspection()
        {
            if (Application.targetFrameRate != CurrentValue)
            {
                OnValueChanged?.Invoke(Application.targetFrameRate);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        protected override void OnValidate()
        {
            base.OnValidate();
            SetTargetFPS();
        }        

        #endregion

        #region --- [EXCECUTION] ---

        private void SetTargetFPS(bool force = false)
        {
            if (fpsLimitations == lastFPSLimitations && !force || !controlTargetFrameRate) return;
            Application.targetFrameRate = (int)fpsLimitations;
            lastFPSLimitations = fpsLimitations;
            OnValueChanged?.Invoke((int)fpsLimitations);
        }

        #endregion
    }
}