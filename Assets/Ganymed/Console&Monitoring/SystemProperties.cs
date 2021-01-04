using System;
using Ganymed.Console.Attributes;
using Ganymed.Monitoring.Modules;
using UnityEngine;

public static class SystemProperties
{
    #region --- [PROPERTIES] ---

    [Getter][NativeCommand]
    public static float FPS => ModuleFPS.CurrentFps;
        
    [Getter][NativeCommand]
    public static DateTime DateTime => DateTime.Now;
    
    [Getter][NativeCommand]
    public static string CPU => SystemInfo.processorType;
    
    #endregion
}
