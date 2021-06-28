using MT.Packages.TenYears;
using UnityEngine;

namespace MT.Packages.Core
{
	using Attributes;

	public partial class Statement
	{
		// TODO: Rename StateMachine() to SequenceFlow()

		[Method(null, "Get Bool/Sequence Flow")]
		public bool StateMachineBool(string name, bool value) {
			var interpreter = executer.GetComponent<SequenceFlow.SequenceFlowInterpreter>();
			if (interpreter) {
				var @object = interpreter.sequenceFlowObject;
				if (@object) {
					return @object.sequenceFlow.simpleData.GetBool(name) == value;
				}
			}
			return false;
		}

		[Method(null, "Get Integer/Sequence Flow")]
		public bool StateMachineInteger(string name, Utility.StatementType type, int value) {
			var interpreter = executer.GetComponent<SequenceFlow.SequenceFlowInterpreter>();
			if (interpreter) {
				var @object = interpreter.sequenceFlowObject;
				if (@object) {
					if (Utility.Equals(@object.sequenceFlow.simpleData.GetInteger(name), type, value)) {
						return true;
					}
				}
			}
			return false;
		}

		[Method(null, "Get Float/Sequence Flow")]
		public bool StateMachineFloat(string name, Utility.StatementType type, float value) {
			var interpreter = executer.GetComponent<SequenceFlow.SequenceFlowInterpreter>();
			if (interpreter) {
				var @object = interpreter.sequenceFlowObject;
				if (@object) {
					if (Utility.Equals(@object.sequenceFlow.simpleData.GetFloat(name), type, value)) {
						return true;
					}
				}
			}
			return false;
		}

		[Method(null, "Get String/Sequence Flow")]
		public bool StateMachineString(string name, Utility.StringStatementType type, string value) {
			var interpreter = executer.GetComponent<SequenceFlow.SequenceFlowInterpreter>();
			if (interpreter) {
				var @object = interpreter.sequenceFlowObject;
				if (@object) {
					if (Utility.Equals(@object.sequenceFlow.simpleData.GetString(name), type, value)) {
						return true;
					}
				}
			}
			return false;
		}

		[Method(null, "Get Object/Sequence Flow")]
		public bool StateMachineObject(string name, Utility.ObjectStatementType type, Object value) {
			var interpreter = executer.GetComponent<SequenceFlow.SequenceFlowInterpreter>();
			if (interpreter) {
				var @object = interpreter.sequenceFlowObject;
				if (@object) {
					if (Utility.Equals(@object.sequenceFlow.simpleData.GetObject(name), type, value)) {
						return true;
					}
				}
			}
			return false;
		}



		SimpleData _GetSequenceFlowInterpreterParameters() {
			SequenceFlow.SequenceFlowInterpreter _interpreter;
			_interpreter = _self as SequenceFlow.SequenceFlowInterpreter;
			if (!_interpreter) {
				_interpreter = executer.GetComponent<SequenceFlow.SequenceFlowInterpreter>();
			}
			return _interpreter.parameters;
		}

		[Method("#00FFFF", "Interpreter/Flow/Get Bool Parameter")]
		public bool Interpreter_Flow_GetBoolParameter(string name, bool value) {
			return _GetSequenceFlowInterpreterParameters().GetBool(name) == value;
		}

		[Method("#00FFFF", "Interpreter/Flow/Get Integer Parameter")]
		public bool Interpreter_Flow_GetIntegerParameter(string name, Utility.StatementType type, int value) {
			return Utility.Equals(_GetSequenceFlowInterpreterParameters().GetInteger(name), type, value);
		}

		[Method("#00FFFF", "Interpreter/Flow/Get Float Parameter")]
		public bool Interpreter_Flow_GetFloatParameter(string name, Utility.StatementType type, float value) {
			return Utility.Equals(_GetSequenceFlowInterpreterParameters().GetFloat(name), type, value);
		}

		[Method("#00FFFF", "Interpreter/Flow/Get String Parameter")]
		public bool Interpreter_Flow_GetStringParameter(string name, Utility.StringStatementType type, string value) {
			return Utility.Equals(_GetSequenceFlowInterpreterParameters().GetString(name), type, value);
		}

		[Method("#00FFFF", "Interpreter/Flow/Get Object Parameter")]
		public bool Interpreter_Flow_GetObjectParameter(string name, Utility.ObjectStatementType type, Object value) {
			return Utility.Equals(_GetSequenceFlowInterpreterParameters().GetObject(name), type, value);
		}



		[Method(null, "Object/By Name/Get Bool/Is Value")]
		public bool Object_GetBool(string memberOwnerName, Field member, bool value) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			return member.Get<bool>() == value;
		}

		[Method(null, "Object/By Name/Get Bool/Is Field")]
		public bool Object_GetBool_ByField(string memberOwnerName, Field member, bool @not, string sourceFieldObjectName, Field sourceField) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			sourceField.@object = _GetSequenceFlowInterpreterParameters().GetObject(sourceFieldObjectName);
			var result = member.Get<bool>() == sourceField.Get<bool>();
			return @not ? !result : result;
		}

		[Method(null, "Object/By Name/Get Integer/Is Value")]
		public bool Object_GetInteger(string memberOwnerName, Field member, Utility.StatementType type, int value) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			return Utility.Equals(member.Get<int>(), type, value);
		}

		[Method(null, "Object/By Name/Get Integer/Is Field")]
		public bool Object_GetInteger_ByField(string memberOwnerName, Field member, Utility.StatementType type, string sourceFieldObjectName, Field sourceField) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			sourceField.@object = _GetSequenceFlowInterpreterParameters().GetObject(sourceFieldObjectName);
			return Utility.Equals(member.Get<int>(), type, sourceField.Get<int>());
		}

		[Method(null, "Object/By Name/Get Float/Is Value")]
		public bool Object_GetFloat(string memberOwnerName, Field member, Utility.StatementType type, float value) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			return Utility.Equals(member.Get<float>(), type, value);
		}

		[Method(null, "Object/By Name/Get Float/Is Field")]
		public bool Object_GetFloat_ByField(string memberOwnerName, Field member, Utility.StatementType type, string sourceFieldObjectName, Field sourceField) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			sourceField.@object = _GetSequenceFlowInterpreterParameters().GetObject(sourceFieldObjectName);
			return Utility.Equals(member.Get<float>(), type, sourceField.Get<float>());
		}

		[Method(null, "Object/By Name/Get String/Is Value")]
		public bool Object_GetString(string memberOwnerName, Field member, Utility.StringStatementType type, string value) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			return Utility.Equals(member.Get<string>(), type, value);
		}

		[Method(null, "Object/By Name/Get String/Is Field")]
		public bool Object_GetString_ByField(string memberOwnerName, Field member, Utility.StringStatementType type, string sourceFieldObjectName, Field sourceField) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			sourceField.@object = _GetSequenceFlowInterpreterParameters().GetObject(sourceFieldObjectName);
			return Utility.Equals(member.Get<string>(), type, sourceField.Get<string>());
		}

		[Method(null, "Object/By Name/Get Object/Is Value")]
		public bool Object_GetObject(string memberOwnerName, Field member, Utility.ObjectStatementType type, Object value) {
			member.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			return Utility.Equals(member.Get<Object>(), type, value);
		}

		[Method(null, "Object/By Name/Get Object/Is Field")]
		public bool Object_GetObject_ByField(string memberOwnerName, Field field, Utility.ObjectStatementType type, string sourceFieldObjectName, Field sourceField) {
			field.@object = _GetSequenceFlowInterpreterParameters().GetObject(memberOwnerName);
			sourceField.@object = _GetSequenceFlowInterpreterParameters().GetObject(sourceFieldObjectName);
			return Utility.Equals(field.Get<Object>(), type, sourceField.Get<Object>());
		}



		[Method(null, "Input")]
		public bool Input(KeyCode key) {
			if (e is InputEventArgs input) {
				return input.key == key;
			}
			return false;
		}
	}
}