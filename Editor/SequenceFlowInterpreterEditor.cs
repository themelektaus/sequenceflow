using UnityEditor;
// using UnityEngine;

namespace MT.Packages.SequenceFlow.Editor
{
	[CustomEditor(typeof(SequenceFlowInterpreter))]
	public class SequenceFlowInterpreterEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI() {
			// DrawDefaultInspector();
			DrawCustom();
		}

		void DrawCustom() {
			var _this = (SequenceFlowInterpreter) target;
			var eventType = serializedObject.FindProperty("eventType");
			EditorGUILayout.PropertyField(eventType);
			if (eventType.stringValue == nameof(EventTypesEnum.Continuous)) {
				EditorGUILayout.PropertyField(serializedObject.FindProperty("updateInterval"));
			}
			// if (eventType.stringValue == "None") {
			// 	EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerType"), new GUIContent("Trigger Type"));
			// }
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sequenceFlowObject"));
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("startSequenceFlowButton"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("parameters"));
			serializedObject.ApplyModifiedProperties();
		}
	}
}