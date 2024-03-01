using System.Collections;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    public class SequenceFlowInterpreter : Interpreter
    {
        public SequenceFlowObject sequenceFlowObject;
        public bool autoAbortSequenceFlow;

        [SerializeField] bool logging;

        public SimpleData parameters = new();

        void Awake()
        {
            if (sequenceFlowObject)
                sequenceFlowObject = Instantiate(sequenceFlowObject);
        }

        protected override void OnStart()
        {
            if (!sequenceFlowObject)
                return;

            sequenceFlowObject.ReadFromData();
        }

        protected override void OnReceive(Transform sender, EventArgs e) =>
            Perform(sender, e);

        protected override void OnAbort()
        {
            sequenceFlowObject.AbortFlow();

            Debug.LogWarning("Sequence aborted");
        }

        public void Perform()
            => Perform(null, new());

        public void Perform(Transform activator)
            => Perform(activator, new());

        public void Perform(string eventType)
            => Perform(null, new(eventType));

        public override void Perform(Transform activator, EventArgs e)
        {
            if (logging)
                Debug.Log("Try to perform sequence flow...");

            if (!HasSequenceFlow())
            {
                Debug.LogWarning("There is no sequence flow to perform");
                return;
            }

            if (IsRunning())
            {
                Debug.LogWarning("Sequence flow is already running");

                if (!autoAbortSequenceFlow)
                    return;

                Abort();
            }

            StartCoroutine(Flow(activator, e));
        }

        bool HasSequenceFlow()
        {
            if (!sequenceFlowObject)
                return false;

            if (sequenceFlowObject.sequenceFlow == null)
                return false;

            return true;
        }

        bool IsRunning()
        {
            return sequenceFlowObject.sequenceFlow.Running;
        }

        IEnumerator Flow(Transform activator, EventArgs e)
        {
            if (logging)
                Debug.Log("Coroutine started");

            yield return sequenceFlowObject.sequenceFlow.Start(activator, this, e, parameters);

            if (logging)
                Debug.Log("Coroutine has finished");
        }
    }
}
