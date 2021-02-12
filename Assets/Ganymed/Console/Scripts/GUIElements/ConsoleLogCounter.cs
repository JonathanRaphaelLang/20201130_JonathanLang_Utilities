using TMPro;
using UnityEngine;

namespace Ganymed.Console.GUIElements
{
    internal sealed class ConsoleLogCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textField = null;

        private void OnEnable() => Console.Core.Console.OnConsoleLogCountUpdateCallback += ConsoleOnOnLogCacheUpdate;
        private void OnDisable() => Console.Core.Console.OnConsoleLogCountUpdateCallback -= ConsoleOnOnLogCacheUpdate;
        private void OnDestroy() => Console.Core.Console.OnConsoleLogCountUpdateCallback -= ConsoleOnOnLogCacheUpdate;

        private void ConsoleOnOnLogCacheUpdate(int current, int max)
            => textField.text = $"Log: {current} / {max}";
    }
}
