using UnityEngine;

namespace MT.Packages.SequenceFlow
{
	using Core;
	using Core.Attributes;

	public abstract class SMD_Interpreter_Flow_SetParameter : SMD
	{
		public override Color color => Color.cyan;

		[DisplayNoneAsExecuter] public Object interpreter;
		public string name;

		protected SimpleData parameters {
			get {
				SequenceFlowInterpreter _interpreter;
				if (interpreter) {
					_interpreter = interpreter as SequenceFlowInterpreter;
				} else {
					_interpreter = monoBehaviour as SequenceFlowInterpreter;
					if (!_interpreter) {
						_interpreter = executer.GetComponent<SequenceFlowInterpreter>();
					}
				}
				return _interpreter.parameters;
			}
		}
	}
}