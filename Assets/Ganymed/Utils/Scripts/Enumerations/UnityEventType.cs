namespace Ganymed.Utils
{
    public enum UnityEventType
    {
        Awake,
        Start,
        Recompile,
        ApplicationQuit,
        EditorApplicationQuit,
        Update,
        LateUpdate,
        FixedUpdate,
        OnEnable,
        OnDisable,
        BuildPlayer,
        ManuallyInvoked,
        
        //--------------------------------------------------------------------------------------------------------------

        TransitionEditPlayMode,
        EnteredEditMode,
        ExitingEditMode,
        EnteredPlayMode,
        ExitingPlayMode,
        EditorApplicationStart,
        BeforeSceneLoad,
        AfterSceneLoad,
        AfterAssembliesLoaded,
        InspectorUpdate,
        SubsystemRegistration,
        BeforeSplashScreen
    }
}