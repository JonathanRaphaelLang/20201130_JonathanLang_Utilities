using UnityEditor;
using UnityEngine;

namespace Ganymed.Console.Scripts.Editor
{
    public class CreateConsoleInstance : MonoBehaviour
    {
        [MenuItem("GameObject/Ganymed/Console", false, 12)]
        [MenuItem("Ganymed/Create Console", priority = 1)]
        public static void CreateGameObjectInstance()
        {
            if (Core.Console.Instance != null)
            {
                Debug.Log("An instance of the console already exists!");
                Selection.activeGameObject = Core.Console.Instance.gameObject;
                return;
            }

            try
            {
                var guids = AssetDatabase.FindAssets("t:prefab", new[] {"Assets"});

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadMainAssetAtPath(path);

                    if (prefab.name != "Console") continue;

                    PrefabUtility.InstantiatePrefab(prefab);
                    Core.Console.Instance.OnInstantiation();
                    Debug.Log("Instantiated Console Prefab");
                    break;
                }
            }
            catch
            {
                Debug.LogWarning("Failed to instantiate Console! Make sure that the corresponding prefab" +
                                 "[Console] is present within the project.");
            }
        }
    }
}