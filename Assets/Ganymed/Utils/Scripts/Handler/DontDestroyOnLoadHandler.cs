using System.Collections.Generic;
using UnityEngine;

public static class DontDestroyOnLoadHandler
{
    public static readonly List<GameObject> DontDestroyOnLoadObjects = new List<GameObject>();

    public static void DontDestroyOnLoad(this GameObject go) {
        Object.DontDestroyOnLoad(go);
        DontDestroyOnLoadObjects.Add(go);
    }

    public static void DestroyAll() {
        foreach(var go in DontDestroyOnLoadObjects)
            if(go != null)
                Object.Destroy(go);

        DontDestroyOnLoadObjects.Clear();
    }
}