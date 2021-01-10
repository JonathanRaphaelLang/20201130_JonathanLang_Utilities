using System.Reflection;

namespace Ganymed.Console.Processor
{
    internal sealed class Signature
    {
        public readonly MethodInfo methodInfo;
        public readonly string description;
        public readonly bool disableNBP;
        public readonly int priority;

        public Signature(MethodInfo methodInfo, int priority, string description, bool disableNbp)
        {
            this.methodInfo = methodInfo;
            this.priority = priority;
            this.description = description;
            this.disableNBP = disableNbp;
        }
    }
}