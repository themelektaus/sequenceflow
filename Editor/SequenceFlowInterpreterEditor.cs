#if !MT_PACKAGES_PROJECT
using UnityEditor;
using UnityEngine;

namespace MT.Packages.SequenceFlow.Editor
{
	[CustomEditor(typeof(SequenceFlowInterpreter))]
	public class SequenceFlowInterpreterEditor : UnityEditor.Editor
	{
		//readonly SequenceFlowObjectDrawer objectDrawer = new SequenceFlowObjectDrawer();

		public override void OnInspectorGUI() {
			//DrawDefaultInspector();
			DrawCustom();
		}

		void DrawCustom() {
			//var eventType = serializedObject.FindProperty("eventType");
			//if (eventType != null) {
			//	EditorGUILayout.PropertyField(eventType);
			//	if (eventType.stringValue == "Continuous") {
			//		EditorGUILayout.PropertyField(serializedObject.FindProperty("updateInterval"));
			//	}
			//}

			var eventType = serializedObject.FindProperty("eventType");
			if (eventType.stringValue != "None")
				EditorGUILayout.PropertyField(serializedObject.FindProperty("eventType"));

			var eventTypes = serializedObject.FindProperty("eventTypes");
			EditorGUILayout.PropertyField(eventTypes);

			for (int i = 0; i < eventTypes.arraySize; i++)
			{
				if (eventTypes.GetArrayElementAtIndex(i).stringValue != "Continuous")
					continue;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("updateInterval"));
				EditorGUILayout.Space();
				break;
			}

			//if (eventType.stringValue == "None") {
			//	EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerType"), new GUIContent("Trigger Type"));
			//}
			//objectDrawer.Draw(serializedObject, (SequenceFlowInterpreter) target);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sequenceFlowObject"));

			EditorGUILayout.BeginHorizontal();
			var autoAbort = serializedObject.FindProperty("autoAbortSequenceFlow");
			autoAbort.boolValue = EditorGUILayout.Toggle("Auto Abort", autoAbort.boolValue);
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.PropertyField(serializedObject.FindProperty("startSequenceFlowButton"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("parameters"));
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif