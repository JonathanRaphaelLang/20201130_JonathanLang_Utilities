using UnityEngine;

namespace Ganymed.Console.Utils
{
    public class ReverseScrollPlacer : MonoBehaviour
    {
        private void Start()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.AddComponent<ReverseScroll>();
            }
        }

    }
}