
namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;
	
	[GUID("66445167-5528-45c2-85e9-08e64db2b159")]
	public class SMD_SetObject_SequenceFlow : SMD_SetObject
	{
		public override string menuPath => "Set Object/Sequence Flow";

		public UnityEngine.Object value;

		public override void Execute() {
			if (executer.TryGetSequenceFlowObject(out var sequenceFlowObject)) {
				SetObject(sequenceFlowObject.sequenceFlow.simpleData, name, value);
			}
		}
	}
}