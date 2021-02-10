using System;
using System.IO;
using UnityEngine;

namespace Ganymed.Utils.Singleton
{
    /// <summary>
    /// Generic base class for singleton instances of setting files.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Settings<T> : ScriptableSettings where T : ScriptableSettings
    {
        private static string SettingsAssetName => typeof(T).Name;
        
        // private backfield cache of the instance.
        private static T instance = null;                   
        // ReSharper disable once StaticMemberInGenericType This field is required for multithreading environments. 
        private static bool isInitializing = false;           

        /// <summary>
        /// The singleton instance of the related type. Will create an instance of the type if none is present. 
        /// </summary>
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
                            UnityEditor.AssetDatabase.CreateFolder(instance.FilePath(), "Resources");
                        }
                        
                        UnityEditor.AssetDatabase.CreateAsset(instance, $"{instance.FilePath()}/Resources/{SettingsAssetName}.asset");
#endif
                    }
                    isInitializing = false;
                }
                return instance;
            }
        }
    }
}
