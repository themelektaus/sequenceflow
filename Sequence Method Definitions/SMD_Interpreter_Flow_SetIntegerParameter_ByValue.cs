
namespace MT.Packages.SequenceFlow
{
	using Core.Attributes;

	[GUID("ef0968ab-697b-455b-89e7-ae4c126c81e3")]
	public class SMD_Interpreter_Flow_SetIntegerParameter_ByValue : SMD_Interpreter_Flow_SetParameter
	{
		public override string menuPath => "Interpreter/Flow/Set Integer Parameter/By Value";

		public int value;

		public override void Execute() {
			parameters.SetInteger(name, value);
		}
	}
}