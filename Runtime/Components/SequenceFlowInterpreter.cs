#if !MT_PACKAGES_PROJECT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventArgs = MT.Packages.EventSystem.EventArgs;
using OldName = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace MT.Packages.SequenceFlow
{
    using Core;
    using Core.Attributes;

    [AddComponentMenu(Utility.ASSET_NAME + "/Sequence Flow/Sequence Flow Interpreter")]
    public class SequenceFlowInterpreter : EventSystem.Interpreter
    {
        [OldName("stateMachineObject")] public SequenceFlowObject sequenceFlowObject;
        public bool autoAbortSequenceFlow;

        [ReadOnly(onlyDuringPlayMode = true)] public float updateInterval = 1;
        public SimpleData parameters = new SimpleData();

        Coroutine coroutine;
        Timer updateTimer;

        [HideInInspector]
        public readonly List<System.Func<Transform, IEnumerator>> beforeCoroutines = new List<System.Func<Transform, IEnumerator>>();

        [HideInInspector]
        public readonly List<System.Func<IEnumerator>> afterCoroutines = new List<System.Func<IEnumerator>>();

        readonly List<Collider> activators = new List<Collider>();

        public void StartSequenceFlow(Transform activator) =>
            Perform(activator, new EventArgs("Runtime"));

        void Awake()
        {
            updateTimer = updateInterval;
            if (sequenceFlowObject)
                sequenceFlowObject = Instantiate(sequenceFlowObject);
        }

        protected override void OnStart()
        {
            if (!sequenceFlowObject)
                return;
            sequenceFlowObject.ReadFromData();
            if (Contains("Automatic"))
                Perform(transform, new EventArgs("Automatic"));
        }

        protected override void OnReceive(Transform sender, EventArgs e) =>
            Perform(sender, e);

        protected override void OnAbort()
        {
            sequenceFlowObject.AbortFlow();
        }

        protected virtual IEnumerator BeforeCoroutine(Transform activator)
        {
            yield break;
        }

        protected virtual IEnumerator AfterCoroutine()
        {
            yield break;
        }

        public override void Perform(Transform activator, EventArgs e)
        {
            this.Log("Try to perform sequence flow...");
            if (CanPerform())
                coroutine = StartCoroutine(Flow(activator, e));
        }

        bool CanPerform()
        {
            if (!sequenceFlowObject) return false;
            if (sequenceFlowObject.sequenceFlow == null) return false;
            if (sequenceFlowObject.sequenceFlow.Running) return false;
            return true;
        }

        IEnumerator GetBeforeRoutines(Transform activator)
        {
            foreach (var beforeCoroutine in beforeCoroutines)
                yield return beforeCoroutine(activator);
            yield return BeforeCoroutine(activator);
        }

        IEnumerator GetAfterRoutines()
        {
            yield return AfterCoroutine();
            foreach (var afterCoroutine in afterCoroutines)
                yield return afterCoroutine();
        }

        IEnumerator Flow(Transform activator, EventArgs e)
        {
            this.Log("Coroutine started");
            yield return GetBeforeRoutines(activator);
            yield return sequenceFlowObject.sequenceFlow.Start(activator, this, parameters, e);
            yield return GetAfterRoutines();
            coroutine = null;
        }

        protected virtual void OnEnable()
        {
            
        }

        protected virtual void OnDisable()
        {
            if (coroutine != null)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine(GetAfterRoutines());
                coroutine = null;
            }
        }

        void Update()
        {
            if (!CanPerform()) return;
            if (!Contains("Continuous")) return;
            if (!updateTimer.Update()) return;
            coroutine = StartCoroutine(Flow(transform, new EventArgs("Continuous")));
        }

        void OnTriggerEnter(Collider other)
        {
            if (!enabled)
                return;

            activators.RemoveAll(x => !x);
            if (activators.Count == 0 && sequenceFlowObject && Contains("TriggerEnter"))
            {
                if (autoAbortSequenceFlow)
                    OnAbort();
                Perform(other.transform, new EventArgs("TriggerEnter"));
            }

            if (!activators.Contains(other))
                activators.Add(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (activators.Contains(other))
                activators.Remove(other);

            activators.RemoveAll(x => !x);
            if (enabled && activators.Count == 0 && Contains("TriggerExit"))
            {
                if (autoAbortSequenceFlow)
                    OnAbort();
                Perform(other.transform, new EventArgs("TriggerExit"));
            }
        }
    }
}
#endif