using System;

namespace Prototype.SequenceFlow
{
    public abstract class Guid
    {
        [NonSerialized]
        public SequenceFlow sequenceFlow;

        public string guid;

        public void Setup(SequenceFlow sequenceFlow)
        {
            this.sequenceFlow = sequenceFlow;

            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();
        }
    }
}
