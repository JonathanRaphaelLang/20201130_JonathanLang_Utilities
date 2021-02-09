using UnityEditor;
using UnityEngine;

namespace Ganymed.Utils.Singleton
{
    public class ScriptableSettings : ScriptableObject
    {
        public virtual string FilePath() => "Assets/Settings";

        #region --- [EDITOR SELECTION] ---

#if UNITY_EDITOR
        protected internal static void SelectObject(Object target)
        {
            Selection.activeObject = target;
#if UNITY_2018_2_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
#else
            UnityEditor.EditorApplication.ExecuteMenuItem("Window/Inspector");
#endif
        }
#endif

        #endregion
    }
}