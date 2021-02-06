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

        public static event ModuleUpdateDelegate OnValueChanged; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE] ---

        protected override string ParseToString(VSyncCount currentValue)
        {
            return (currentValue > 0) ? currentValue.ToString() : "none";
        } 

        protected override void OnInitialize()
        {
            InitializeUpdateEvent(ref OnValueChanged);
            
            if (setVsync)
            {
                QualitySettings.vSyncCount = (int) VSync;
            }
            
            InitializeValue((VSyncCount)QualitySettings.vSyncCount);
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
            if((int)Value != QualitySettings.vSyncCount)
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
