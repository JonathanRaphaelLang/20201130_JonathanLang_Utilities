using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringBehaviour))]
    public class MonitorBehaviourInspector : CleanEditor
    {
        private MonitoringBehaviour Target = null;
        private void OnEnable()
        {
            Target = (MonitoringBehaviour) target;
        }

        protected override void OnBeforeDefaultInspector() => EditorGUILayout.Space();

        protected override void OnAfterDefaultInspector()
        {
            if (GUILayout.Button("Edit Monitoring Settings"))
            {
                MonitoringSettings.EditSettings();
            }
        }
    }
}