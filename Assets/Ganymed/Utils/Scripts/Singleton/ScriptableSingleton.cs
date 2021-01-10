using UnityEngine;

namespace Ganymed.Utils.Singleton
{
    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created on the editor, or null if there is none
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        
    }
}
