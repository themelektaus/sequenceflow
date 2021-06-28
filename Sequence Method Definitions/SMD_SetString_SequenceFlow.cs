
namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;
	
	[GUID("c7e83ed8-2502-4262-b65b-8d2244e8aeae")]
	public class SMD_SetString_SequenceFlow : SMD_SetString
	{
		public override string menuPath => "Set String/Sequence Flow";

		public string value;

		public override void Execute() {
			if (executer.TryGetSequenceFlowObject(out var sequenceFlowObject)) {
				SetString(sequenceFlowObject.sequenceFlow.simpleData, name, value);
			}
		}
	}
}