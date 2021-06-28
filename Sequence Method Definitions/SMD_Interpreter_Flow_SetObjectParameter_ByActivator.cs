
namespace MT.Packages.SequenceFlow
{
	using Core.Attributes;

	[GUID("98786cf8-ff11-45dc-89d6-7d4b900dad0d")]
	public class SMD_Interpreter_Flow_SetObjectParameter_ByActivator : SMD_Interpreter_Flow_SetParameter
	{
		public override string menuPath => "Interpreter/Flow/Set Object Parameter/By Activator";
		
		public override void Execute() {
			parameters.SetObject(name, activator);
		}
	}
}