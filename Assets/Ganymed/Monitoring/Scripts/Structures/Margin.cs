using Ganymed.Utils;

namespace Ganymed.Monitoring
{
    public readonly struct Margin
    {
        public readonly Alignment MarginAlignment;
        public readonly float MarginValue;

        public Margin(float marginValue, Alignment marginAlignment)
        {
            MarginValue = marginValue;
            MarginAlignment = marginAlignment;
        }
    }
}