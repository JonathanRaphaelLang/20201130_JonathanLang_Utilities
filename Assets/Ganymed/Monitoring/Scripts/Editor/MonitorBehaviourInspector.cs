﻿using Ganymed.Monitoring.Configuration;
using Ganymed.Monitoring.Core;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    [CustomEditor(typeof(MonitoringBehaviour))]
    public class MonitorBehaviourInspector : UnityEditor.Editor
    {
        private MonitoringBehaviour Target = null;
        private void OnEnable()
        {
            Target = (MonitoringBehaviour) target;
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