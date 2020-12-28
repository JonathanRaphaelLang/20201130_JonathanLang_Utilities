using System;

namespace Ganymed.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ScriptOrder : Attribute
    {
        public readonly int order;
        public ScriptOrder(int order)
        {
            this.order = order;
        }
    }
}