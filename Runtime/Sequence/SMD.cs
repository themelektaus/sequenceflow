namespace Prototype.SequenceFlow
{
    public abstract class SMD : SequenceMethodDefinition
    {
        public sealed override bool waitable => false;

        public void Invoke(Sequence sequence, object[] parameters)
        {
            Prepare(sequence, parameters);
            Execute();
        }

        public abstract void Execute();
    }
}
