using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Core;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitorBehaviour))]
    public class MonitorBehaviourInspector : UnityEditor.Editor
    {
        private MonitorBehaviour Target = null;
        private void OnEnable()
        {
            Target = (MonitorBehaviour) target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit Settings"))
            {
                MonitoringSettings.EditSettings();
            }
        }
    }
}