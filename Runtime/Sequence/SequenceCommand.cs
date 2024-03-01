
namespace Prototype.SequenceFlow
{
    [System.Serializable]
    public class SequenceCommand : Command
    {
        public float delay;
        public float postDelay;
        public Flow flow;
        public SequenceMethod method;

        public SequenceCommand() : base()
        {
            delay = 0;
            postDelay = 0;
            flow = Flow.Sync;
            method = new SequenceMethod();
        }

        public SequenceCommand(string methodName, float delay, params object[] methodParameters)
        {
            this.delay = delay;
            postDelay = 0;
            flow = Flow.Sync;
            method = new SequenceMethod(methodName, methodParameters);
        }
    }
}
