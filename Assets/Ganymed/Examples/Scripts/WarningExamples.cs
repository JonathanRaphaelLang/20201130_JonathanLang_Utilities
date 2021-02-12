
//#define ENABLE_EXAMPLES // => toggle to activate / deactivate examples.

// ------------------------
// !!! Work In Progress !!!
// ------------------------

namespace Ganymed.Examples.Scripts
{
#pragma warning disable 414
    
    /// <summary>
    /// Class containing examples for the origin/cause of custom warnings.
    /// 
    /// </summary>
    public class WarningExamples
    {
        #region --- [WARNING MESSAGES] ---
        
        // This region is deactivated by default because it will cause warning messages. 

#if ENABLE_EXAMPLES
        // This field will cause warning 300
        [Getter]
        private int NonStaticMember = 20;
        
        // This field will cause warning 200
        [AllowUnsafeSetter]
        private int NoSetterMember = 20;

        [Setter]
        private int NoWriteAccess => NoSetterMember;
        
        
        [AttributeTarget(typeof(Attribute))]
        private class ThisIsNotAnAttribute
        {
            
        }
        
            
        #region [MISC]

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        private static void LogState() => Debug.LogWarning("Warning: example Warnings are enabled\n");

        #endregion
#endif

        #endregion
    }
}
