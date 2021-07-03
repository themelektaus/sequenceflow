using UnityEngine;

namespace MT.Packages.SequenceFlow
{
	public static class ExtensionMethods
	{
		public static bool TryGetSequenceFlowObject(this Transform @this, out SequenceFlowObject sequenceFlowObject) {
			var interpreter = @this.GetComponentInParent<SequenceFlowInterpreter>();
			if (interpreter) {
				sequenceFlowObject = interpreter.sequenceFlowObject;
				if (sequenceFlowObject) {
					return true;
				}
			}
			sequenceFlowObject = null;
			return false;
		}
	}
}