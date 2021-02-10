using Ganymed.Monitoring.Configuration;
using UnityEngine;

namespace Ganymed.Monitoring.Core
{
    public class MonitoringBehaviourInput : MonoBehaviour
    {
        private MonitoringBehaviour Target;
        
        private void Awake()
        {
            Target = GetComponent<MonitoringBehaviour>();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(MonitoringSettings.Instance.toggleKey)) return;
            Target.Toggle();
        }
    }
}