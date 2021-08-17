using UnityEditor;

namespace MT.Packages.SequenceFlow.Editor
{
	[CustomEditor(typeof(SequenceFlowInterpreter))]
	public class SequenceFlowInterpreterEditor : UnityEditor.Editor
	{
		readonly SequenceFlowObjectDrawer objectDrawer = new SequenceFlowObjectDrawer();

		public override void OnInspectorGUI() {
			// DrawDefaultInspector();
			DrawCustom();
		}

		void DrawCustom() {
			var eventType = serializedObject.FindProperty("eventType");
			if (eventType != null) {
				EditorGUILayout.PropertyField(eventType);
				if (eventType.stringValue == "Continuous") {
					EditorGUILayout.PropertyField(serializedObject.FindProperty("updateInterval"));
				}
			}
			//if (eventType.stringValue == "None") {
			//	EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerType"), new GUIContent("Trigger Type"));
			//}
			//objectDrawer.Draw(serializedObject, (SequenceFlowInterpreter) target);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sequenceFlowObject"));
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("startSequenceFlowButton"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("parameters"));
			serializedObject.ApplyModifiedProperties();
		}
	}
}