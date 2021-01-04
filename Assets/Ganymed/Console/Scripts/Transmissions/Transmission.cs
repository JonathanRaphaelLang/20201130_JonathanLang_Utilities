using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Console.Transmissions
{
    public static class Transmission
    {
        #region --- [FILEDS] ---

        private static bool isTransmitting = false;
        private static bool isEnumeration = false;
        private static bool isReleasing = false;
        
        private static readonly string EvenColor = new Color(0.72f, 0.7f, 0.85f).AsRichText();
        
        private static readonly List<List<string>> transmissionMessages = new List<List<string>>();
        private static readonly List<List<string>> transmissionPrefix = new List<List<string>>();
        private static readonly List<List<string>> transmissionSuffix = new List<List<string>>();
        
        private static readonly List<List<string>> transmissionExtraPrefix = new List<List<string>>();
        private static readonly List<List<string>> transmissionExtraSuffix = new List<List<string>>();
        
        private static readonly List<List<MessageOptions>> transmissionOptions = new List<List<MessageOptions>>();
        private static readonly List<int> transmissionLengths = new List<int>();

        private static string message = string.Empty;

        private static object Sender = null;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [START] ---
        
        public static void Start(TransmissionOptions options = TransmissionOptions.None, object sender = null)
        {
            isTransmitting = true;
            
            transmissionPrefix.Clear();
            transmissionMessages.Clear();
            transmissionSuffix.Clear();
            
            transmissionExtraPrefix.Clear();
            transmissionExtraSuffix.Clear();
            
            transmissionOptions.Clear();
            transmissionLengths.Clear();

            Sender = sender;
            
            isEnumeration = options.HasFlag(TransmissionOptions.Enumeration);
        }
        

        #endregion

        #region --- [RELEASE] ---

        /// <summary>
        /// End transmission and log collected messages to the console.
        /// </summary>
        public static void ReleaseAsync(Action callback = null)
        {
            if (!isTransmitting || isReleasing)
            {
                Debug.LogWarning("You are transmitting a message while no transmission has been started!");
                return;
            }
            
            if(Sender != null)
                Core.Console.Log(Sender, Core.Console.colorSender, LogOptions.IsInput | LogOptions.EndLine);
               
            isReleasing = true;
            Task.Run(CompileTransmissionMessage).Then(
                delegate{
                    Core.Console.Log(message);
                    isTransmitting = false;
                    isReleasing = false;
                    callback?.Invoke();});
        }


        /// <summary>
        /// End transmission and log collected messages to the console.
        /// </summary>
        public static void Release()
        {
            if (!isTransmitting || isReleasing)
            {
                Debug.LogWarning("You are transmitting a message while no transmission has been started!");
                return;
            }
            
            if(Sender != null)
                Core.Console.Log(Sender, Core.Console.colorSender, LogOptions.IsInput | LogOptions.EndLine);

            isReleasing = true;
            CompileTransmissionMessage();
            Core.Console.Log(message);
            isTransmitting = false;
            isReleasing = false;
        }

        #endregion

        #region --- [COMPILE TRANSMISSION] ---

        private static void CompileTransmissionMessage()
        {
            message = string.Empty;
            //--- Prefix / Suffix
            
            for (var i = 0; i < transmissionOptions.Count; i++)
            {
                for (var j = 0; j < transmissionOptions[i].Count; j++)
                {
                    var prefix = string.Empty;
                    var suffix = string.Empty;

                    foreach (MessageOptions value in Enum.GetValues(typeof(MessageOptions)))
                    {
                        if (transmissionOptions[i][j].HasFlag(value))
                        {
                            switch (value)
                            {
                                case MessageOptions.None:
                                    continue;

                                case MessageOptions.Bold:
                                    prefix += "<b>";
                                    suffix += "</b>";
                                    continue;

                                case MessageOptions.Italics:
                                    prefix += "<i>";
                                    suffix += "</i>";
                                    continue;

                                case MessageOptions.Strike:
                                    prefix += "<s>";
                                    suffix += "</s>";
                                    continue;

                                case MessageOptions.Underline:
                                    prefix += "<u>";
                                    suffix += "</u>";
                                    continue;

                                case MessageOptions.Uppercase:
                                    prefix += "<uppercase>";
                                    suffix += "</uppercase>";
                                    continue;

                                case MessageOptions.Lowercase:
                                    prefix += "<lowercase>";
                                    suffix += "</lowercase>";
                                    continue;

                                case MessageOptions.Smallcaps:
                                    prefix += "<smallcaps>";
                                    suffix += "</smallcaps>";
                                    continue;

                                case MessageOptions.Mark:
                                    prefix += Core.Console.Configuration.colorMarker;
                                    suffix += "</mark>";
                                    continue;

                                case MessageOptions.Brackets:
                                    transmissionMessages[i][j] = $"[{transmissionMessages[i][j]}]";
                                    continue;

                                default:
                                    continue;
                            }
                        }
                    }

                    if (j == 0)
                    {
                        transmissionPrefix.Add(new List<string>() {prefix});
                        transmissionSuffix.Add(new List<string>() {suffix});
                    }
                    else
                    {
                        transmissionPrefix[transmissionPrefix.Cut()].Add(prefix);
                        transmissionSuffix[transmissionSuffix.Cut()].Add(suffix);
                    }
                }
            }


            foreach (var transmission in transmissionMessages)
            {
                for (var j = 0; j < transmission.Count; j++)
                {
                    if (transmissionLengths.Cut() < j)
                        transmissionLengths.Add(0);

                    if (transmission[j].Length > transmissionLengths[j])
                        transmissionLengths[j] = transmission[j].Length;
                }
            }
            

            // foreach row
            for (var i = 0; i < transmissionMessages.Count; i++)
            {
                // foreach column
                for (var j = 0; j < transmissionMessages[i].Count; j++)
                {
                    message +=
                        $"{(j == 0 && i != 0 ? $"\n{100.AsLineHeight()}" : string.Empty)}" +
                        $"{(isEnumeration && j == 0 ? i.IsEven() ? EvenColor : "</color>" : string.Empty)}" +
                        //------------------------
                        $"{transmissionExtraPrefix[i][j]}" +
                        $"{transmissionPrefix[i][j]}" +
                        $"{transmissionMessages[i][j]}" + // --- Here are the messages ---
                        $"{transmissionSuffix[i][j]}" +
                        $"{transmissionExtraSuffix[i][j]}" +
                        //------------------------
                        $"{(j < transmissionMessages[i].Cut() ? (transmissionLengths[j] - transmissionMessages[i][j].Length + 1).Repeat() : string.Empty)}" +
                        $"{(i == transmissionMessages.Cut() ? 150.AsLineHeight() : string.Empty)}";
                }
                // End of row
                
                //message += AsLineHeight
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [LINE] ---

        public static void AddLine(params object[] messages)
        {
            if(!isTransmitting)
                return;
            
            // --- Iterate through every message
            for (var i = 0; i < messages.Length; i++)
            {
                messages[i] = messages[i] ?? string.Empty;
                
                if (messages[i] is Message)
                {
                    if (i == 0)
                    {
                        var m = (Message)messages[i];
                        transmissionMessages.Add(new List<string>(){m.Content});
                        transmissionOptions.Add(new List<MessageOptions>(){m.Options});
                    
                        transmissionExtraPrefix.Add(new List<string>(){$"{m.Size}{m.Color}"});
                        transmissionExtraSuffix.Add(new List<string>(){$"{(m.Size == string.Empty? string.Empty : "</size>")}" +
                                                                       $"{(m.Color == string.Empty? string.Empty : "</color>")}"});
                    }
                    else
                    {
                        var m = (Message)messages[i];
                        var index = transmissionMessages.Cut();
                    
                        transmissionMessages[index].Add(m.Content);
                        transmissionOptions[index].Add(m.Options);
                    
                        transmissionExtraPrefix[index].Add($"{m.Size}{m.Color}");
                        transmissionExtraSuffix[index].Add($"{(m.Size == string.Empty? string.Empty : "</size>")}" +
                                                           $"{(m.Color == string.Empty? string.Empty : "</color>")}");
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        transmissionMessages.Add(new List<string>(){messages[i].ToString()});
                        transmissionOptions.Add(new List<MessageOptions>(){0});
                        transmissionExtraPrefix.Add(new List<string>(){string.Empty});
                        transmissionExtraSuffix.Add(new List<string>(){string.Empty});
                    }
                    else
                    {
                        var index = transmissionMessages.Cut();
                        transmissionMessages[index].Add(messages[i].ToString());
                        transmissionOptions[index].Add(0);
                        transmissionExtraPrefix[index].Add(string.Empty);
                        transmissionExtraSuffix[index].Add(string.Empty);
                    }    
                }
            }
        }
        public static void AddLine(params Message[] messages)
        {
            if (!isTransmitting)
                return;

            // --- Iterate through every message
            for (var i = 0; i < messages.Length; i++)
            {
                if (i == 0)
                {
                    var m = messages[i];
                    transmissionMessages.Add(new List<string>(){messages[i].Content});
                    transmissionOptions.Add(new List<MessageOptions>(){messages[i].Options});
                    
                    transmissionExtraPrefix.Add(new List<string>(){$"{m.Size}{m.Color}"});
                    transmissionExtraSuffix.Add(new List<string>(){$"{(m.Size == string.Empty? string.Empty : "</size>")}" +
                                                                   $"{(m.Color == string.Empty? string.Empty : "</color>")}"});
                }
                else
                {
                    var m = messages[i];
                    var index = transmissionMessages.Cut();
                    
                    transmissionMessages[index].Add(m.Content);
                    transmissionOptions[index].Add(m.Options);
                    
                    transmissionExtraPrefix[index].Add($"{m.Size}{m.Color}");
                    transmissionExtraSuffix[index].Add($"{(m.Size == string.Empty? string.Empty : "</size>")}" +
                                                       $"{(m.Color == string.Empty? string.Empty : "</color>")}");
                }
            }
        }        

        #endregion

        #region --- [BREAK] ---

        public static void AddBreak(int lineHeight = 130)
        {
            try
            {
                transmissionExtraSuffix[transmissionExtraSuffix.Cut()][transmissionExtraSuffix[transmissionExtraSuffix.Cut()].Cut()] +=
                    lineHeight.AsLineHeight();
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region --- [TITLE] ---

        public static void AddTitle(string title, int size = 110, int space = 110, MessageOptions options = MessageOptions.Bold)
        {
            AddLine(new Message($"> {title}", size, Core.Console.colorTitles, options));
            AddBreak(space);
        }

        #endregion
    }
}
