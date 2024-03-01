using UnityEditor.Experimental.GraphView;

namespace Prototype.SequenceFlow.Editor
{
    public class TransitionEdge : Edge
    {
        public Transition transition;

        public TransitionEdge(Transition transition)
        {
            this.transition = transition;
        }
    }
}
