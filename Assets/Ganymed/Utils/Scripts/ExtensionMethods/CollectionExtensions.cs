using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionExtensions
{
    /// <summary>
    /// Count - 1
    /// </summary>
    /// <param name="inspected"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static int Cut<T>(this List<T> inspected)
    {
        return inspected.Count - 1;
    }
}
