using TMPro;
using UnityEngine;

namespace Ganymed.Console.GUIElements
{
    [ExecuteAlways]
    public class SizeButtons : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI sizeDisplay = null;
        private void Awake()
        {
            sizeDisplay.text = $"{Core.ConsoleSettings.Instance.fontSize} pt";
        }


        public void IncrementFontSize()
        {
            Core.Console.FontSize += 1;
            Core.Console.FontSizeInput += 1;
            sizeDisplay.text = $"{Core.ConsoleSettings.Instance.fontSize} pt";
        }

        public void DecrementFontSize()
        {
            Core.Console.FontSize -= 1;
            Core.Console.FontSizeInput -= 1;
            sizeDisplay.text = $"{Core.ConsoleSettings.Instance.fontSize} pt";
        }
    }
}
