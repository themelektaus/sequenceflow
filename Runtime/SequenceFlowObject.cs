#if !MT_PACKAGES_PROJECT
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OldName = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace MT.Packages.SequenceFlow
{
	using EventSystem;

	[UnityEngine.CreateAssetMenu(menuName = Core.Utility.NEW_ASSET_MENU + "Sequence Flow/Sequence Flow Object")]
	public class SequenceFlowObject : UnityEngine.ScriptableObject
	{
		public bool compress = true;
		public bool embeddedInScene;
		public GameObject embeddedInPrefab;
		[TextArea(8, 8)] public string data;

		[OldName("stateMachine")] public SequenceFlow sequenceFlow = new SequenceFlow();
		public List<Core.Sequence> stateSequences = new List<Core.Sequence>();
		public List<Core.Statement> transitionStatements = new List<Core.Statement>();

		public SequenceFlowObject() {
			sequenceFlow.sequenceFlowObject = this;
		}

		public void ReadFromData() {
			if (!string.IsNullOrEmpty(data)) {
				var data = this.data.StartsWith("{") ? this.data : Core.Utility.Decompress(this.data);
				sequenceFlow = JsonUtility.FromJson<SequenceFlow>(data);
				sequenceFlow.startState = sequenceFlow.states.Where(x => x.guid == sequenceFlow.startStateGUID).FirstOrDefault();
				foreach (var state in sequenceFlow.states) {
					state.sequenceFlow = sequenceFlow;
				}
				foreach (var transition in sequenceFlow.transitions) {
					transition.sequenceFlow = sequenceFlow;
					transition.source = sequenceFlow.states.Where(x => x.guid == transition.sourceGUID).FirstOrDefault();
					transition.destination = sequenceFlow.states.Where(x => x.guid == transition.destinationGUID).FirstOrDefault();
				}
			}
			sequenceFlow.sequenceFlowObject = this;
		}

		public Coroutine StartFlow(Transform activator, MonoBehaviour _self, Core.SimpleData parameters, string eventType) {
			var e = new EventArgs(eventType);
			return StartFlow(activator, _self, parameters, e);
		}

		public Coroutine StartFlow(Transform activator, MonoBehaviour _self, Core.SimpleData parameters, EventArgs e) {
			var routine = sequenceFlow.Start(activator, _self, parameters, e);
			return _self.StartCoroutine(routine);
		}

		public void AbortFlow() {
			sequenceFlow.Abort();
		}

#if UNITY_EDITOR
		public bool WriteToData(bool saveAssets = false) {
			if (sequenceFlow.states.Any(x => x.sequenceFlow == null))
				return false;
			var newData = GetSerializedData();
			if (data != newData) {
				data = newData;
				UnityEditor.EditorUtility.SetDirty(this);
				if (saveAssets) {
					UnityEditor.AssetDatabase.SaveAssets();
				}
				return true;
			}
			return false;
		}

		string GetSerializedData() {
			if (sequenceFlow.startState != null) {
				sequenceFlow.startStateGUID = sequenceFlow.startState.guid;
			}
			foreach (var transition in sequenceFlow.transitions) {
				transition.sourceGUID = transition.source.guid;
				transition.destinationGUID = transition.destination.guid;
			}
			var result = JsonUtility.ToJson(sequenceFlow);
			if (compress) {
				result = Core.Utility.Compress(result);
			}
			return result;
		}
#endif
	}
}
#endif