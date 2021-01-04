using UnityEngine;

namespace Ganymed.Utils.Handler
{
    public class DestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(this.gameObject);
        }
    }
}
