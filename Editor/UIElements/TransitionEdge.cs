using UnityEditor.Experimental.GraphView;

namespace MT.Packages.SequenceFlow.Editor.UIElements
{
	public class TransitionEdge : Edge
	{
		public Transition transition;

		public TransitionEdge(Transition transition) {
			this.transition = transition;
		}
	}
}