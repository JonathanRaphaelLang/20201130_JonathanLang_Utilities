using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Transmissions
{
    public readonly struct Message
    {
        public readonly string Content;
        public readonly MessageOptions Options;
        public readonly string Color;
        public readonly string Size;

        public Message(object message, MessageOptions options = MessageOptions.None)
        {
            Content = message.ToString();
            Options = options;
            Color = null;
            Size = null;
        }
        public Message(object message, Color color, MessageOptions options = MessageOptions.None)
        {
            Content = message.ToString();
            Options = options;
            Color = color.AsRichText();
            Size = null;
        }
        
        public Message(object message, float size, MessageOptions options = MessageOptions.None)
        {
            Content = message.ToString();
            Options = options;
            Color = null;
            Size = size.ToFontSize(Core.Console.Configuration.fontSize);
        }
        
        public Message(object message, float size, Color color, MessageOptions options = MessageOptions.None)
        {
            Content = message.ToString();
            Options = options;
            Color = color.AsRichText();
            Size = size.ToFontSize(Core.Console.Configuration.fontSize);
        }
    }
}