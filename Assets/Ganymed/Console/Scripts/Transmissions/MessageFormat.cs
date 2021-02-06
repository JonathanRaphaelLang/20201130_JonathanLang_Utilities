using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Transmissions
{
    /// <summary>
    /// Wrapper struct to format message strings for transmissions
    /// </summary>
    public readonly struct MessageFormat
    {
        public readonly string ContainedMessage;
        public readonly MessageOptions Options;
        public readonly string Color;

        public MessageFormat(object message, MessageOptions options = MessageOptions.None)
        {
            ContainedMessage = message.ToString();
            Options = options;
            Color = null;
        }
        
        public MessageFormat(object message, Color color, MessageOptions options = MessageOptions.None)
        {
            ContainedMessage = message.ToString();
            Options = options;
            Color = color.AsRichText();
        }
    }
}