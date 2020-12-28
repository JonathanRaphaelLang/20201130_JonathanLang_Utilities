using System;
using Ganymed.Monitoring.Core;
using Ganymed.Utils;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_VSync", menuName = "Monitoring/Modules/VSync")]
    public class ModuleVsync : Module<VSyncCount>
    {
        #region --- [INSPECTOR] ---

        [SerializeField] private bool setVsync = false;
        [SerializeField] private VSyncCount VSync = VSyncCount.DontSync;

        #endregion

        #region --- [FIELDS] ---

        private VSyncCount lastVSync = VSyncCount.DontSync;

        #endregion

        #region --- [EVENTS] ---

        public static event Action<VSyncCount> OnValueChanged; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE] ---

        protected override string DynamicValue(VSyncCount value)
        {
            return (value > 0) ? value.ToString() : "none";
        } 

        protected override void OnInitialize()
        {
            SetValueDelegate(ref OnValueChanged);
            
            if (setVsync) {
                QualitySettings.vSyncCount = (int) VSync;
            }
            defaultValue = (VSyncCount)QualitySettings.vSyncCount;
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!setVsync || lastVSync == VSync) return;
            SetVSync(VSync);
            lastVSync = VSync;
        }

        protected override void OnInspection()
        {
            if((int)CurrentValue != QualitySettings.vSyncCount)
                OnValueChanged?.Invoke((VSyncCount)QualitySettings.vSyncCount);
        }
        
        #endregion

        #region --- [VSYNC] ---

        private static void SetVSync(VSyncCount vSyncCount)
        {
            QualitySettings.vSyncCount = (int)vSyncCount;
            OnValueChanged?.Invoke(vSyncCount);
        } 

        #endregion
    }
}
