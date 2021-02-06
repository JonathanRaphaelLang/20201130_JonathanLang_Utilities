
//#define ENABLE_EXAMPLES // => toggle to activate / deactivate examples.

using Ganymed.Console;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using UnityEngine;

#if ENABLE_EXAMPLES

public static class TransmissionExample
{
    [ConsoleCommand("Transmission")]
    public static void Example(int num = 10, bool enumeration = true, bool async = false)
    {
        // Preparing and starting a new transmission
        var options = enumeration ? TransmissionOptions.Enumeration : TransmissionOptions.None;
        Transmission.Start(options, nameof(TransmissionExample));



        #region --- [ADDING LINES TO THE TRANSMISSION] ---

        Transmission.AddTitle("Title");
        
        Transmission.AddLine("Line 1:", "column one", "column two", "column three"); // adding 4 columns.
        
        Transmission.AddLine("Line 2:",
            new MessageFormat("column one", MessageOptions.Bold),
            new MessageFormat("column two", MessageOptions.Brackets | MessageOptions.Underline),
            new MessageFormat("column three", Color.green, MessageOptions.Smallcaps));
        
        Transmission.AddLine("Line 3:", new MessageFormat("column one", Color.red, MessageOptions.UpperCase));
        Transmission.AddLine("Line 4:", "column one", "column two", "column three");
        
        Transmission.AddBreak();
        
        for (var i = 0; i < num; i++)
        {
            Transmission.AddLine($"Line {5+i}:", "column one", "column two", "column three");
        }
        
        Transmission.AddTitle("Title", TitlePreset.Sub);
        Transmission.AddLine("Last Line", "column one", "column two", "column three");

        #endregion
        
        
        // Releasing the collected messages
        if(async)
            Transmission.ReleaseAsync();
        else
            Transmission.Release();
    }
    
    
    //--------------------------------------------------------------------------------------------------------------
        
        
    #region --- [MISC] ---

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    private static void LogState() => Debug.Log("Note: TransmissionExample is enabled\n");

    #endregion
}

#endif