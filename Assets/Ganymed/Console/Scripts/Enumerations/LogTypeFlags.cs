using System;

namespace Ganymed.Console
{
    [Flags]
    public enum LogTypeFlags
    {
        Everything = -1,
        None = 0,
    
        /// <summary>
        ///   <para>LogType used for Errors.</para>
        /// </summary>
        Error = 1,
        /// <summary>
        ///   <para>LogType used for Asserts. (These could also indicate an error inside Unity itself.)</para>
        /// </summary>
        Assert = 2,
        /// <summary>
        ///   <para>LogType used for Warnings.</para>
        /// </summary>
        Warning = 4,
        /// <summary>
        ///   <para>LogType used for regular log messages.</para>
        /// </summary>
        Log = 8,
        /// <summary>
        ///   <para>LogType used for Exceptions.</para>
        /// </summary>
        Exception = 16,
        
    }
}
