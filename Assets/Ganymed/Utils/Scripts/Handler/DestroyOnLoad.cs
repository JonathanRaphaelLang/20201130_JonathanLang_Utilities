using UnityEngine;

public class DestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        Destroy(this.gameObject);
    }
}
