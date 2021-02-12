using System;
using Ganymed.Monitoring.Core;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Monitoring.Editor
{
    public class CreateMonitorInstance : MonoBehaviour
    {
        private const string AssetName = "Monitoring";
        
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

                var success = false;
                
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadMainAssetAtPath(path);

                    if (prefab.name != AssetName) continue;

                    PrefabUtility.InstantiatePrefab(prefab);
                    Debug.Log("Instantiated Monitoring Prefab");
                    success = true;
                    break;
                }

                if (!success) throw new Exception();
            }
            catch
            {
                Debug.LogWarning($"Failed to instantiate Monitoring Prefab!Make sure that the corresponding prefab" +
                                 $"{AssetName} can be found within the project.");
            }
        }
    }
}