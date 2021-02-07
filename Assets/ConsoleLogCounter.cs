using TMPro;
using UnityEngine;
using Console = Ganymed.Console.Core.Console;

internal sealed class ConsoleLogCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textField = null;

    private void OnEnable() => Console.OnConsoleLogCountUpdate += ConsoleOnOnLogCacheUpdate;
    private void OnDisable() => Console.OnConsoleLogCountUpdate -= ConsoleOnOnLogCacheUpdate;
    private void OnDestroy() => Console.OnConsoleLogCountUpdate -= ConsoleOnOnLogCacheUpdate;

    private void ConsoleOnOnLogCacheUpdate(int current, int max)
        => textField.text = $"Log: {current} / {max}";
}
