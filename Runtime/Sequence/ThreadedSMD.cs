using System.Collections;
using System.Threading;

namespace Prototype.SequenceFlow
{
    public abstract class ThreadedSMD : AsyncSMD
    {
        protected abstract void Task();

        protected virtual ThreadPriority priority => ThreadPriority.Normal;

        public sealed override IEnumerator Execute()
        {
            yield return new WaitForThreadedTask(Task, priority);
        }
    }
}