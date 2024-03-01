using UnityEngine;

namespace Prototype.SequenceFlow
{
	public interface IReceiver
	{
		bool OnReceive(Transform sender, Transform receiver, EventArgs e);
	}
}
