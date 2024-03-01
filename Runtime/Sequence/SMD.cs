using System.Collections.Generic;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    public abstract class SMD : SequenceMethodDefinition
    {
        static readonly Dictionary<MonoBehaviour, object> dataCache = new();

        public sealed override bool waitable => false;

        public void Invoke(Sequence sequence, object[] parameters)
        {
            Prepare(sequence, parameters);
            Execute();
        }

        public abstract void Execute();
    }
}
