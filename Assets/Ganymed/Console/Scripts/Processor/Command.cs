using System.Collections.Generic;

namespace Ganymed.Console.Processor
{
    internal sealed class Command
    {
        public readonly string Key;
        public readonly List<Signature> Signatures = new List<Signature>();
        public int Priority { get; private set; }
        public readonly bool hasNativeAttribute;

        public Command(Signature signature, string key, bool hasNativeAttribute)
        {
            Key = key;
            this.hasNativeAttribute = hasNativeAttribute;
            Signatures.Add(signature);
            Priority = signature.priority;
        }

        public void AddOverload(Signature overload)
        {
            Signatures.Add(overload);
            if (overload.priority > Priority)
                Priority = overload.priority;
        }
    }
}