using Ganymed.Monitoring.Core;
using Ganymed.Utils.ExtensionMethods;
using TMPro;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_System", menuName = "Monitoring/Modules/System")]
    public sealed class ModuleSystem : Module<string>
    {
        #region --- [INSPECTOR] ---

        [Header("System Information")]
        [SerializeField] private bool selectAll = true;
        [Space]
        [SerializeField] private bool operatingSystem = true;
        [SerializeField] private bool processorType = true;
        [SerializeField] private bool processorCount = true;
        [SerializeField] private bool systemMemorySize = true;
        [SerializeField] private bool graphicsDeviceName = true;
        [SerializeField] private bool graphicsMemorySize = true;
        [SerializeField] private bool graphicsDeviceType = true;
        [SerializeField] private bool graphicsMultiThreaded = true;
        [SerializeField] private bool renderingThreadingMode = true;
        [SerializeField] private bool batteryLevel = true;
        [SerializeField] private bool batteryStatus = true;
        [SerializeField] private bool deviceModel = true;
        [SerializeField] private bool deviceType = true;
        [Space]
        [SerializeField] private bool deviceUniqueIdentifier = false;

        #endregion

        #region --- [FIELDS] ---

        private bool cachedAll = true;

        #endregion

        #region --- [EVENTS] ---

        private event ModuleUpdateDelegate OnValueChanged; 
        private event ModuleActivationDelegate OnToggle; 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE] ---

        protected override void OnValidate()
        {
            base.OnValidate();

            #region --- [FIELD VALIDATION] ---
            
            if (selectAll != cachedAll)
            {
                operatingSystem = selectAll;
                processorType = selectAll;
                processorCount = selectAll;
                systemMemorySize = selectAll;
                graphicsDeviceName = selectAll;
                graphicsMemorySize = selectAll;
                graphicsDeviceType = selectAll;
                graphicsMultiThreaded = selectAll;
                renderingThreadingMode = selectAll;
                batteryLevel = selectAll;
                batteryStatus = selectAll;
                deviceModel = selectAll;
                deviceType = selectAll;
            }
            
            //if any field is true 
            if (operatingSystem || processorType || processorCount || systemMemorySize || graphicsDeviceName
                || graphicsMemorySize || graphicsDeviceType || graphicsMultiThreaded || renderingThreadingMode
                || batteryLevel || batteryStatus || deviceModel || deviceType 
                && cachedAll == selectAll) {
                selectAll = true; //flip the select all toggle to true
            }
            //if every field is false 
            else if (!operatingSystem && !processorType && !processorCount && !systemMemorySize && !graphicsDeviceName 
                     && !graphicsMemorySize && !graphicsDeviceType && graphicsMultiThreaded && renderingThreadingMode
                     && batteryLevel && batteryStatus && deviceModel && deviceType
                     && cachedAll == selectAll) {
                selectAll = false; //flip the select all toggle to false
            }

            if(cachedAll != selectAll)
                OnToggle?.Invoke(selectAll);
            
            cachedAll = selectAll;
            #endregion
            
            OnValueChanged?.Invoke(Default);
        }

        protected override string ParseToString(string currentValue)
        {
            return base.ParseToString(GenerateText(Style.alignment));
        }

        protected override void OnInitialize()
        {
            InitializeValue(GenerateText(Style.alignment));
            InitializeUpdateEvent(ref OnValueChanged);
            
            InitializeActivationEvent(ref OnToggle);
        }

        #endregion

        #region --- [TEXT GENERATION] ---

        private string GenerateText(TextAlignmentOptions alignmentOptions)
        {
            switch (alignmentOptions)
            {
                case TextAlignmentOptions.TopRight:
                case TextAlignmentOptions.Right:
                case TextAlignmentOptions.BottomRight:
                case TextAlignmentOptions.BaselineRight:
                case TextAlignmentOptions.MidlineRight:
                case TextAlignmentOptions.CaplineRight:
                    return
                        $"{Style.prefixColor.AsRichText()}Stat       System</color>\n" +
                        $"{(operatingSystem ?        $"{SystemInfo.operatingSystem}           OS\n" : string.Empty)}" +
                        $"{(processorType ?          $"{SystemInfo.processorType}     CPU-TYPE\n" : string.Empty)}" +
                        $"{(processorCount ?         $"{SystemInfo.processorCount}    CPU-CORES\n" : string.Empty)}" +
                        $"{(systemMemorySize ?       $"{SystemInfo.systemMemorySize}       MEMORY\n" : string.Empty)}" +
                        $"{(graphicsDeviceName ?     $"{SystemInfo.graphicsDeviceName}          GPU\n" : string.Empty)}" +
                        $"{(graphicsMemorySize ?     $"{SystemInfo.graphicsMemorySize}   GPU-MEMORY\n" : string.Empty)}" +
                        $"{(graphicsDeviceType ?     $"{SystemInfo.graphicsDeviceType}     GPU-TYPE\n" : string.Empty)}" +
                        $"{(graphicsMultiThreaded ?  $"{SystemInfo.graphicsMultiThreaded}   GPU-THREAD\n" : string.Empty)}" +
                        $"{(renderingThreadingMode ? $"{SystemInfo.renderingThreadingMode}     GPU-TYPE\n" : string.Empty)}" +
                        $"{(batteryLevel ?           $"{SystemInfo.batteryLevel}      Battery\n" : string.Empty)}" +
                        $"{(batteryStatus ?          $"{SystemInfo.batteryStatus}      Battery\n" : string.Empty)}" +
                        $"{(deviceModel ?            $"{SystemInfo.deviceModel}        Model\n" : string.Empty)}" +
                        $"{(deviceType ?             $"{SystemInfo.deviceType}         Type\n" : string.Empty)}" +
                        $"{(deviceUniqueIdentifier ? $"{SystemInfo.deviceUniqueIdentifier}       : GUID\n" : string.Empty)}";
            }
            
            return 
                $"{Style.prefixColor.AsRichText()}System     : Stat</color>\n" +
                   $"{(operatingSystem ?        $"OS         : {SystemInfo.operatingSystem}\n" : string.Empty)}" +
                   $"{(processorType ?          $"CPU-TYPE   : {SystemInfo.processorType}\n" : string.Empty)}" +
                   $"{(processorCount ?         $"CPU-CORES  : {SystemInfo.processorCount}\n" : string.Empty)}" +
                   $"{(systemMemorySize ?       $"MEMORY     : {SystemInfo.systemMemorySize}\n" : string.Empty)}" +
                   $"{(graphicsDeviceName ?     $"GPU        : {SystemInfo.graphicsDeviceName}\n" : string.Empty)}" +
                   $"{(graphicsMemorySize ?     $"GPU-MEMORY : {SystemInfo.graphicsMemorySize}\n" : string.Empty)}" +
                   $"{(graphicsDeviceType ?     $"GPU-TYPE   : {SystemInfo.graphicsDeviceType}\n" : string.Empty)}" +
                   $"{(graphicsMultiThreaded ?  $"GPU-THREAD : {SystemInfo.graphicsMultiThreaded}\n" : string.Empty)}" +
                   $"{(renderingThreadingMode ? $"GPU-TYPE   : {SystemInfo.renderingThreadingMode}\n" : string.Empty)}" +
                   $"{(batteryLevel ?           $"Battery    : {SystemInfo.batteryLevel}\n" : string.Empty)}" +
                   $"{(batteryStatus ?          $"Battery    : {SystemInfo.batteryStatus}\n" : string.Empty)}" +
                   $"{(deviceModel ?            $"Model      : {SystemInfo.deviceModel}\n" : string.Empty)}" +
                   $"{(deviceType ?             $"Type       : {SystemInfo.deviceType}\n" : string.Empty)}" +
                   $"{(deviceUniqueIdentifier ? $"GUID       : {SystemInfo.deviceUniqueIdentifier}\n" : string.Empty)}";
        }

        #endregion
    }
}