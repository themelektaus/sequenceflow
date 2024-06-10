using System.Collections;

namespace Prototype.SequenceFlow
{
    public abstract class AsyncSMD : SequenceMethodDefinition
    {
        public sealed override bool waitable => true;

        public IEnumerator Invoke(Sequence sequence, object[] parameters)
        {
            Prepare(sequence, parameters);
            return Execute();
        }

        public abstract IEnumerator Execute();
    }
}
