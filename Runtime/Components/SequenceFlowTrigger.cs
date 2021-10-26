using System.Collections.Generic;
using UnityEngine;

namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;
	using MoreTags;

	[AddComponentMenu(Core.Utility.ASSET_NAME + "/Sequence Flow/Sequence Flow Trigger (Deprecated)")]
	public class SequenceFlowTrigger : MonoBehaviour
	{
		[Tag] public string[] requiredTags;
#if !MT_PACKAGES_PROJECT
		public SequenceFlowInterpreter onEnterInterpreter;
		public SequenceFlowInterpreter onExitInterpreter;
#endif
		public SimpleData parameters = new SimpleData();
		
		readonly List<Collider> activators = new List<Collider>();

		void Awake() {
			Debug.LogWarning($"{gameObject.name}: {GetType().Name} is deprecated");
		}

		void OnDisable() {
			activators.Clear();
		}

		void OnTriggerEnter(Collider collision) {
#if !MT_PACKAGES_PROJECT
			if (!onEnterInterpreter || !enabled) {
				return;
			}
			foreach (var tag in requiredTags) {
				if (!collision.gameObject.HasTag(tag, true)) {
					return;
				}
			}
			if (activators.Count == 0) {
				foreach (var parameter in parameters) {
					onEnterInterpreter.parameters.Set(parameter);
				}
				onEnterInterpreter.Perform(collision.transform, null);
			}
#endif
			if (!activators.Contains(collision)) {
				activators.Add(collision);
			}
		}

		void OnTriggerExit(Collider collision) {
			foreach (var tag in requiredTags) {
				if (!collision.gameObject.HasTag(tag, true)) {
					return;
				}
			}
			if (activators.Contains(collision)) {
				activators.Remove(collision);
			}
#if !MT_PACKAGES_PROJECT
			if (!onExitInterpreter || !enabled) {
				return;
			}
			if (activators.Count == 0) {
				foreach (var parameter in parameters) {
					onExitInterpreter.parameters.Set(parameter);
				}
				onExitInterpreter.Perform(collision.transform, null);
			}
#endif
		}
	}
}