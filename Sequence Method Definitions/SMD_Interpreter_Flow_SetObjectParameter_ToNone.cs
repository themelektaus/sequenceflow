
namespace MT.Packages.SequenceFlow
{
	using Core.Attributes;
	
	[GUID("b1dcb207-f396-4fbd-a83c-221e21d7f185")]
	public class SMD_Interpreter_Flow_SetObjectParameter_ToNone : SMD_Interpreter_Flow_SetParameter
	{
		public override string menuPath => "Interpreter/Flow/Set Object Parameter/To None";
		
		public override void Execute() {
			parameters.SetObject(name, null);
		}
	}
}