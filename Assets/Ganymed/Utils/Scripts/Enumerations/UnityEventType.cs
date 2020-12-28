namespace Ganymed.Utils
{
    public enum UnityEventType
    {
        Unknown,
        Awake,
        Start,
        Recompile,
        ApplicationQuit,
        EditorApplicationQuit,
        Update,
        LateUpdate,
        FixedUpdate,
        
        /// <summary>
        /// OnEnable is invoked when the UnityEventCallbacks is enabled. This can result in unpredicted callbacks.
        /// CAUTION!
        /// </summary>
        OnEnable,
        
        /// <summary>
        /// OnEnable is invoked when the UnityEventCallbacks is disabled. This can result in unpredicted callbacks.
        /// CAUTION!
        /// </summary>
        OnDisable,
        
        BuildPlayer,
        ManuallyInvoked,
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [EDITOR] ---

        /// <summary>
        /// Transition between Edit and Playmode 
        /// </summary>
        TransitionEditPlayMode,
        EnteredEditMode,
        ExitingEditMode,
        EnteredPlayMode,
        ExitingPlayMode,
        EditorApplicationStart,

        #endregion
    }
}