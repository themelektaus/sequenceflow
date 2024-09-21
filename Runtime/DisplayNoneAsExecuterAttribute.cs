using System;

namespace Prototype.SequenceFlow
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field)]
    public class DisplayNoneAsExecuterAttribute : DisplayNoneAsAttribute
    {
        public DisplayNoneAsExecuterAttribute() : base("EXECUTER") { }
    }
}
