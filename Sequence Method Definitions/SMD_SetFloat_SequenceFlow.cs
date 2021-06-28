
namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;
	
	[GUID("e8fa27c1-2cad-4e54-a74a-c7725c312d1e")]
	public class SMD_SetFloat_SequenceFlow : SMD_SetFloat
	{
		public override string menuPath => "Set Float/Sequence Flow";

		public float value;

		public override void Execute() {
			if (executer.TryGetSequenceFlowObject(out var sequenceFlowObject)) {
				SetFloat(sequenceFlowObject.sequenceFlow.simpleData, name, value);
			}
		}
	}
}