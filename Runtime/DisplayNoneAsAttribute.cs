using System;

namespace Prototype.SequenceFlow
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field)]
    public class DisplayNoneAsAttribute : Attribute
    {
        public readonly string text;

        public DisplayNoneAsAttribute(string text)
        {
            this.text = text;
        }
    }
}
