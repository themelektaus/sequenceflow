using System;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public class StatementCondition : Command
    {
        public StatementMethod method;

        public StatementCondition() : base()
        {

        }
    }
}
