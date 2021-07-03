
namespace MT.Packages.SequenceFlow
{
	using Core.Attributes;

	[GUID("8f57df0e-e3b4-413d-b542-3b6631fcca33")]
	public class SMD_Interpreter_Flow_SetBoolParameter_ByValue : SMD_Interpreter_Flow_SetParameter
	{
		public override string menuPath => "Interpreter/Flow/Set Bool Parameter/By Value";

		public bool value;

		public override void Execute() {
			parameters.SetBool(name, value);
		}
	}
}