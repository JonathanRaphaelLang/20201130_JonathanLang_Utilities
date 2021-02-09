using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ganymed.Console.Core;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Transmissions
{
    public static class Transmission
    {
        #region --- [FILEDS] ---

        private static bool isTransmitting = false;
        private static bool isEnumeration = false;
        private static bool isReleasing = false;
        
        private static readonly string EvenColor = new Color(0.65f, 0.84f, 1f).AsRichText();
        
        private static readonly List<List<string>> transmissionMessages = new List<List<string>>();
        private static readonly List<List<string>> transmissionPrefix = new List<List<string>>();
        private static readonly List<List<string>> transmissionSuffix = new List<List<string>>();
        
        private static readonly List<List<string>> transmissionExtraPrefix = new List<List<string>>();
        private static readonly List<List<string>> transmissionExtraSuffix = new List<List<string>>();
        
        private static readonly List<List<MessageOptions>> transmissionOptions = new List<List<MessageOptions>>();
        private static readonly List<int> transmissionLengths = new List<int>();

        private static string message = string.Empty;

        private static object Sender = null;

        private static int BreakLineHeight => (int) (breakLineHeight ?? (breakLineHeight = Core.ConsoleSettings.Instance.breakLineHeight));
        private static int? breakLineHeight = null;
        
        private static volatile CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static CancellationToken ct;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---

        static Transmission()
        {
            UnityEventCallbacks.AddEventListener(CancelRelease, UnityEventType.ApplicationQuit);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [START] ---
        
        /// <summary>
        /// Begin a transmission.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="sender"></param>
        public static bool Start(TransmissionOptions options = TransmissionOptions.None, object sender = null)
        {
            if (!Core.Console.IsInitialized) return false;
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
            return true;
        }
        

        #endregion

        #region --- [RELEASE] ---


        #region --- [TASK HELPER] ---

        private static void ResetCancellationToken()
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
        }

        private static void CancelRelease()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion
        
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
                Core.Console.Log(Sender, ConsoleSettings.ColorEmphasize, LogOptions.IsInput | LogOptions.EndLine);

            isReleasing = true;

            ct = cancellationTokenSource.Token;
            ct.ThrowIfCancellationRequested();

            try
            {
                Task.Run(CompileTransmissionMessage, ct).Then(delegate
                {
                    Core.Console.Log(message);
                    isTransmitting = false;
                    isReleasing = false;
                    callback?.Invoke();
                });
            }
            catch
            {
                ResetCancellationToken();
            }
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
                Core.Console.Log(Sender, ConsoleSettings.ColorEmphasize, LogOptions.IsInput | LogOptions.EndLine);

            isReleasing = true;
            CompileTransmissionMessage();
            Core.Console.Log(message);
            isTransmitting = false;
            isReleasing = false;
        }

        #endregion

        #region --- [COMPILE TRANSMISSION] ---

        private static Task CompileTransmissionMessage()
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

                                case MessageOptions.UpperCase:
                                    prefix += "<uppercase>";
                                    suffix += "</uppercase>";
                                    continue;

                                case MessageOptions.LowerCase:
                                    prefix += "<lowercase>";
                                    suffix += "</lowercase>";
                                    continue;

                                case MessageOptions.Smallcaps:
                                    prefix += "<smallcaps>";
                                    suffix += "</smallcaps>";
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
                        transmissionPrefix[transmissionPrefix.Indices()].Add(prefix);
                        transmissionSuffix[transmissionSuffix.Indices()].Add(suffix);
                    }
                }
            }


            foreach (var transmission in transmissionMessages)
            {
                for (var j = 0; j < transmission.Count; j++)
                {
                    if (transmissionLengths.Indices() < j)
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
                        $"{(j < transmissionMessages[i].Indices() ? (transmissionLengths[j] - transmissionMessages[i][j].Length + 1).Repeat() : string.Empty)}" +
                        $"{(i == transmissionMessages.Indices() ? 150.AsLineHeight() : string.Empty)}";
                }
            }
            return Task.CompletedTask;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [LINE] ---

        /// <summary>
        /// Add multiple columns as text. Each column can be transmitted as a Message (struct) to add formatting options.
        /// </summary>
        /// <param name="messages"></param>
        public static void AddLine(params object[] messages)
        {
            if(!isTransmitting)
                return;
            
            // --- Iterate through every message
            for (var i = 0; i < messages.Length; i++)
            {
                messages[i] = messages[i] ?? string.Empty;
                
                if (messages[i] is MessageFormat)
                {
                    if (i == 0)
                    {
                        var m = (MessageFormat)messages[i];
                        transmissionMessages.Add(new List<string>(){m.ContainedMessage});
                        transmissionOptions.Add(new List<MessageOptions>(){m.Options});
                    
                        transmissionExtraPrefix.Add(new List<string>(){$"{m.Color}"});
                        transmissionExtraSuffix.Add(new List<string>(){$"{(m.Color == string.Empty? string.Empty : "</color>")}"});
                    }
                    else
                    {
                        var m = (MessageFormat)messages[i];
                        var index = transmissionMessages.Indices();
                    
                        transmissionMessages[index].Add(m.ContainedMessage);
                        transmissionOptions[index].Add(m.Options);
                    
                        transmissionExtraPrefix[index].Add($"{m.Color}");
                        transmissionExtraSuffix[index].Add($"{(m.Color == string.Empty? string.Empty : "</color>")}");
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
                        var index = transmissionMessages.Indices();
                        transmissionMessages[index].Add(messages[i].ToString());
                        transmissionOptions[index].Add(0);
                        transmissionExtraPrefix[index].Add(string.Empty);
                        transmissionExtraSuffix[index].Add(string.Empty);
                    }    
                }
            }
        }
        
        /// <summary>
        /// Add multiple columns as text. Each column can be transmitted as a Message (struct) to add formatting options.
        /// </summary>
        public static void AddLine(params MessageFormat[] messages)
        {
            if (!isTransmitting)
                return;

            // --- Iterate through every message
            for (var i = 0; i < messages.Length; i++)
            {
                if (i == 0)
                {
                    var m = messages[i];
                    transmissionMessages.Add(new List<string>(){messages[i].ContainedMessage});
                    transmissionOptions.Add(new List<MessageOptions>(){messages[i].Options});
                    
                    transmissionExtraPrefix.Add(new List<string>(){$"{m.Color}"});
                    transmissionExtraSuffix.Add(new List<string>(){$"{(m.Color == null? string.Empty : "</color>")}"});
                }
                else
                {
                    var m = messages[i];
                    var index = transmissionMessages.Indices();
                    
                    transmissionMessages[index].Add(m.ContainedMessage);
                    transmissionOptions[index].Add(m.Options);
                    
                    transmissionExtraPrefix[index].Add($"{m.Color}");
                    transmissionExtraSuffix[index].Add($"{(m.Color == null? string.Empty : "</color>")}");
                }
            }
        }        

        #endregion

        #region --- [BREAK] ---

        /// <summary>
        /// Add a break after the last transmitted line.
        /// </summary>
        /// <param name="lineHeight"></param>
        public static void AddBreak(int? lineHeight = null)
        {
            try
            {
                transmissionExtraSuffix[transmissionExtraSuffix.Indices()][transmissionExtraSuffix[transmissionExtraSuffix.Indices()].Indices()] +=
                    (lineHeight ?? BreakLineHeight).AsLineHeight();
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region --- [TITLE] ---

        /// <summary>
        /// Add a single column line that will be formatted like a title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="preset"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void AddTitle(string title, TitlePreset preset = TitlePreset.Main)
        {
            switch (preset)
            {
                case TitlePreset.Main:
                    AddTitle(title, 150, ConsoleSettings.ColorTitleMain, MessageOptions.Bold | MessageOptions.Brackets);
                    break;
                case TitlePreset.Sub:
                    AddTitle(title, 130, ConsoleSettings.ColorTitleSub,  MessageOptions.Brackets);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }
        
        
        
        /// <summary>
        /// Add a single column line that will be formatted like a title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="spacing"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        private static void AddTitle(string title, int spacing, Color color, MessageOptions options)
        {
            AddLine(new MessageFormat($"> {title}", color, options));
            AddBreak(spacing);
        }

        #endregion
    }
}
