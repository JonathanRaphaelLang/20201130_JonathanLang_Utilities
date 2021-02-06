using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ganymed.Utils.Helper
{
    /// <summary>
    /// Class handling DontDestroyOnLoad objects. 
    /// </summary>
    public static class DontDestroyOnLoadHandler
    {
        public static readonly List<GameObject> DontDestroyOnLoadObjects = new List<GameObject>();

        /// <summary>
        /// Set an object as DontDestroyOnLoad.
        /// </summary>
        /// <param name="go"></param>
        public static void DontDestroyOnLoad(this GameObject go)
        {
            Object.DontDestroyOnLoad(go);
            DontDestroyOnLoadObjects.Add(go);
        }

        /// <summary>
        /// Destroy every object set as DontDestroyOnLoad.
        /// </summary>
        public static void DestroyAll()
        {
            foreach (var go in DontDestroyOnLoadObjects.Where(go => go != null))
            {
                Object.Destroy(go);
            }

            DontDestroyOnLoadObjects.Clear();
        }
    }
}