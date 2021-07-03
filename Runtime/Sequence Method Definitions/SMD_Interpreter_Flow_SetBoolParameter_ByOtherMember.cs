
namespace MT.Packages.SequenceFlow
{
	using Core.Attributes;

	[GUID("f40432b2-405f-457b-b016-8837f7cd1fb7")]
	public class SMD_Interpreter_Flow_SetBoolParameter_ByOtherMember : SMD_Interpreter_Flow_SetParameter
	{
		public override string menuPath => "Interpreter/Flow/Set Bool Parameter/By Other Member";

		public UnityEngine.Object memberOwner;
		public Core.Field member;

		public override void Execute() {
			parameters.SetBool(name, member.Of(memberOwner).Get<bool>());
		}
	}
}