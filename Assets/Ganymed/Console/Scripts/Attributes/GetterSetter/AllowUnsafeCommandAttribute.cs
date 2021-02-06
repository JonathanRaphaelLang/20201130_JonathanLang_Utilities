using System;
using Ganymed.Utils.Attributes;

namespace Ganymed.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    [RequiresAdditionalAttributes(typeof(ConsoleCommandAttribute), Inherited = true)]
    public class AllowUnsafeCommandAttribute : AllowUnsafeAttribute
    {
        /// <summary>
        /// Suppress exceptions on "unsafe" Types that would otherwise be marked as invalid.
        /// </summary>
        public AllowUnsafeCommandAttribute() { }
    }
}