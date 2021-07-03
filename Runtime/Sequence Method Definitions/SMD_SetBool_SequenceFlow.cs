
namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;
	
	[GUID("62798d0f-5439-4913-b1b5-898be1535b68")]
	public class SMD_SetBool_SequenceFlow : SMD
	{
		public override string menuPath => "Set Bool/Sequence Flow";

		public string name;
		public bool value;

		public override void Execute() {
			if (executer.TryGetSequenceFlowObject(out var sequenceFlowObject)) {
				sequenceFlowObject.sequenceFlow.simpleData.SetBool(name, value);
			}
		}
	}
}