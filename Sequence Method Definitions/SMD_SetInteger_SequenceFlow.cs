
namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;
	
	[GUID("124f74d0-2e13-4237-8584-5fb64b73c35e")]
	public class SMD_SetInteger_SequenceFlow : SMD_SetInteger
	{
		public override string menuPath => "Set Integer/Sequence Flow";

		public int value;

		public override void Execute() {
			if (executer.TryGetSequenceFlowObject(out var sequenceFlowObject)) {
				SetInteger(sequenceFlowObject.sequenceFlow.simpleData, name, value);
			}
		}
	}
}