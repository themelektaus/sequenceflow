using System;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public class Transition : Guid
    {
        [NonSerialized] public State source;
        [NonSerialized] public State destination;

        public const int ARROW_SIZE = 12;

        public int priority;
        public string sourceGuid;
        public string destinationGuid;

        public int GetStatementIndex()
        {
            return sequenceFlow.transitions.IndexOf(this);
        }

        public Statement GetStatement()
        {
            return sequenceFlow.sequenceFlowObject.transitionStatements[GetStatementIndex()];
        }
    }
}
