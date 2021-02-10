using Ganymed.Monitoring.Core;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    public class CreateMonitorInstance : MonoBehaviour
    {
        [MenuItem("Ganymed/Create Monitor", priority = 21)]
        [MenuItem("GameObject/Ganymed/Monitoring", false, 11)]
        private static void CreateGameObjectInstance()
        {
            try
            {
                if (MonitoringBehaviour.Instance != null)
                {
                    Debug.Log("An instance of the monitor behaviour object already exists!");
                    Selection.activeGameObject = MonitoringBehaviour.Instance.gameObject;
                    return;
                }

                var guids = AssetDatabase.FindAssets("t:prefab", new[] {"Assets"});

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadMainAssetAtPath(path);

                    if (prefab.name != "MonitoringBehaviour") continue;

                    PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("Instantiated Monitoring Prefab");
                    break;
                }
            }
            catch
            {
                Debug.LogWarning("Failed to instantiate Monitoring Prefab!Make sure that the corresponding prefab" +
                                 "[MonitoringBehaviour] can be found within the project.");
            }
        }
    }
}