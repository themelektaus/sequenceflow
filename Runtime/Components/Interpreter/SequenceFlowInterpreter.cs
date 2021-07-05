#if !MT_PACKAGES_PROJECT
using System.Collections;
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

		//[Space, InspectorButton] public bool startSequenceFlowButton;

		[ReadOnly(onlyDuringPlayMode = true)] public float updateInterval = 1;
		public SimpleData parameters = new SimpleData();

		Coroutine coroutine;
		Timer updateTimer;

		//void StartSequenceFlowButton_Click() {
		//	Perform(transform, new EventArgs("Editor"));
		//}

		public void StartSequenceFlow(Transform activator) {
			Perform(activator, new EventArgs("Runtime"));
		}

		void Awake() {
			updateTimer = updateInterval;
		}

		protected override void OnStart() {
			if (!sequenceFlowObject) {
				return;
			}
			sequenceFlowObject.ReadFromData();
			if (eventType == "Automatic") {
				Perform(transform, new EventArgs(eventType));
			}
		}

		protected override void OnReceive(Transform sender, EventArgs e) {
			Perform(sender, e);
		}

		protected virtual IEnumerator BeforeCoroutine(Transform activator) {
			yield break;
		}

		protected virtual IEnumerator AfterCoroutine() {
			yield break;
		}

		public void Perform(Transform activator, EventArgs e) {
			this.Log("Try to perform sequence flow...");
			if (CanPerform()) {
				coroutine = StartCoroutine(Flow(activator, e));
			}
		}

		bool CanPerform() {
			if (!sequenceFlowObject) return false;
			if (sequenceFlowObject.sequenceFlow == null) return false;
			if (sequenceFlowObject.sequenceFlow.Running) return false;
			return true;
		}

		IEnumerator Flow(Transform activator, EventArgs e) {
			this.Log("Coroutine started");
			yield return BeforeCoroutine(activator); ;
			yield return sequenceFlowObject.sequenceFlow.Start(activator, this, parameters, e);
			yield return AfterCoroutine();
			coroutine = null;
		}

		void OnEnable() {
#if PACKAGE_MT_TEN_YEARS_EXISTS
			MT.Packages.TenYears.Game.instance.Register(transform);
#endif
		}

		void OnDisable() {
#if PACKAGE_MT_TEN_YEARS_EXISTS
			Game.instance.Unregister(transform);
#endif
			if (coroutine != null) {
				AfterCoroutine();
				coroutine = null;
			}
		}

		void Update() {
			if (eventType != "Continuous") {
				return;
			}
			if (!CanPerform()) {
				return;
			}
			if (!updateTimer.Update()) {
				return;
			}
			coroutine = StartCoroutine(Flow(transform, new EventArgs(eventType)));
		}
	}
}
#endif