using System.Reflection;

namespace Ganymed.Console.Processor
{
    internal sealed class Signature
    {
        public readonly MethodInfo methodInfo;
        public readonly string description;
        public readonly bool disableNBP;
        public readonly int priority;
        public readonly int hiddenPriority;
        public readonly bool disableListings = false;
        public readonly bool disableAutoCompletion = false;

        public Signature(MethodInfo methodInfo, int priority, int hiddenPriority, string description,
            bool disableNBP, bool disableListings, bool disableAutoCompletion)
        {
            this.methodInfo = methodInfo;
            this.priority = priority;
            this.hiddenPriority = hiddenPriority;
            this.description = description;
            this.disableNBP = disableNBP;
            this.disableListings = disableListings;
            this.disableAutoCompletion = disableAutoCompletion;
        }
    }
}