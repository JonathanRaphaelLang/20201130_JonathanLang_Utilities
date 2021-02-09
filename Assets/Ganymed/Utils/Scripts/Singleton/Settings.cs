using System.IO;
using Ganymed.Utils.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace Ganymed.Utils.Singleton
{

    public abstract class Settings<T> : ScriptableSettings where T : ScriptableSettings
    {
        private static string SettingsAssetName => typeof(T).Name;
        
        private static T instance = null;
        // ReSharper disable once StaticMemberInGenericType
        private static bool isInitializing = false;

        public static T Instance
        {
            get
            {
                if (isInitializing)
                {
                    return null;
                }

                if (instance == null)
                {
                    isInitializing = true;
                    instance = Resources.Load(SettingsAssetName) as T;
                    if (instance == null)
                    {
                        Debug.LogWarning($"Cannot find {typeof(T).Name} file, creating default settings");
                        
                        instance = CreateInstance<T>();
                        instance.name = typeof(T).ToString();

#if UNITY_EDITOR
                        if (!Directory.Exists($"{instance.FilePath()}/Resources"))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName($"{instance.FilePath()}/Resources") ?? "Assets/Resources");
                            AssetDatabase.CreateFolder(instance.FilePath(), "Resources");
                        }
                        
                        AssetDatabase.CreateAsset(instance, $"{instance.FilePath()}/Resources/{SettingsAssetName}.asset");
                        // AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(instance), instance.FilePath());
#endif
                    }
                    isInitializing = false;
                }
                return instance;
            }
        }
    }
}
