using Action = System.Action;
using Thread = System.Threading.Thread;
using Priority = System.Threading.ThreadPriority;

namespace Prototype.SequenceFlow
{
	public class WaitForThreadedTask : UnityEngine.CustomYieldInstruction
	{
		public override bool keepWaiting => !done;

		bool done;

		public WaitForThreadedTask(Action task) : this(task, Priority.Normal) {

		}

		public WaitForThreadedTask(Action task, Priority priority) : base() {
			done = false;
			new Thread(() => {
				task();
				done = true;
			}) {
				Priority = priority
			}.Start();
		}
	}
}