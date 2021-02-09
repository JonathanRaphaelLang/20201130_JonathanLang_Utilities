namespace Ganymed.Utils.Editor
{
    public abstract class CleanEditor : UnityEditor.Editor
    {
        private static readonly string[] Exclude = new string[]{"m_Script"};
         
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnBeforeDefaultInspector();
            DrawPropertiesExcluding(serializedObject, Exclude);
            OnAfterDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    
        protected virtual void OnBeforeDefaultInspector()
        {}
 
        protected virtual void OnAfterDefaultInspector()
        {}
    }
}
