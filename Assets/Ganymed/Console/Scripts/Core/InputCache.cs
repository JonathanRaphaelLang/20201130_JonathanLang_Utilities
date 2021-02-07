using UnityEngine;

namespace Ganymed.Console.Core
{
    public readonly struct InputCache
    {
        public readonly string text;
        public readonly Color color;

        public InputCache(string text, Color color)
        {
            this.text = text;
            this.color = color;
        }
    }
}