using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Console.Transmissions
{
    public readonly struct Message
    {
        public readonly string Content;
        public readonly MessageOptions Options;
        [CanBeNull] public readonly string Color;

        public Message(object message, MessageOptions options = MessageOptions.None)
        {
            Content = message.ToString();
            Options = options;
            Color = null;
        }
        public Message(object message, Color color, MessageOptions options = MessageOptions.None)
        {
            Content = message.ToString();
            Options = options;
            Color = color.AsRichText();
        }

    }
}