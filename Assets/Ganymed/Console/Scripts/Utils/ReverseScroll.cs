using UnityEngine;

namespace Ganymed.Console.Utils
{
    public class ReverseScroll : MonoBehaviour
    {
        private RectTransform rectTransform;
    
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            rectTransform.anchoredPosition = new Vector2(0, 0 - Mathf.Abs(rectTransform.anchoredPosition.y));
        }
    }
}
