using System;

namespace Ganymed.Console
{
    [Flags]
    public enum LogOptions
    {
        None = 0,
        IgnoreFormatting = 1,  
        DontBreak = 2,            
        IsInput = 4,
        EndLine = 8,
        Tab = 16,
        Cross = 32,
        Bold = 64,
    }
}
