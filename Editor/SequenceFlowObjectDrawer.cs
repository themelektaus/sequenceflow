using UnityEditor;

using UnityEngine;

namespace Prototype.SequenceFlow.Editor
{
    [CustomPropertyDrawer(typeof(SequenceFlowObject))]
    public class SequenceFlowObjectDrawer : PropertyDrawer
    {
        UnityEditor.SceneManagement.PrefabStage stage;
        string stageAssetPath;
        GameObject stageAsset;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sequenceFlowObject = property.objectReferenceValue as SequenceFlowObject;

            var p = position;

            EditorGUI.BeginDisabledGroup(sequenceFlowObject);
            p.width -= 65;
            EditorGUI.PropertyField(p, property);
            EditorGUI.EndDisabledGroup();

            p.x += p.width + 3;

            if (sequenceFlowObject)
            {
                p.width = 40;
                var p1 = p;
                var p2 = p;
                p2.x += p.width + 2;
                p2.width = 20;

                if (GUI.Button(p1, "Edit"))
                {
                    var target = property.serializedObject.targetObject;
                    var parameters = target.GetType().GetField("parameters");

                    if (target is Interpreter interpreter)
                        SequenceFlowWindow.scene = interpreter.gameObject.scene;

                    if (parameters.GetValue(target) is SimpleData simpleData)
                        SequenceFlowWindow.parameters = simpleData;

                    SequenceFlowWindow.Open(sequenceFlowObject);
                }

                if (sequenceFlowObject.embeddedInPrefab)
                {
                    EditorGUI.BeginDisabledGroup(!UpdateAndCheckPrefabStage() && sequenceFlowObject.embeddedInPrefab != stageAsset);
                    if (GUI.Button(p2, "X"))
                    {
                        AssetDatabase.RemoveObjectFromAsset(property.objectReferenceValue);
                        property.objectReferenceValue = null;
                        Object.DestroyImmediate(sequenceFlowObject);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else if (sequenceFlowObject.embeddedInScene)
                {
                    if (GUI.Button(p2, "X"))
                    {
                        property.objectReferenceValue = null;
                        Object.DestroyImmediate(sequenceFlowObject);
                    }
                }
                else
                {
                    if (GUI.Button(p2, "X"))
                    {
                        property.objectReferenceValue = null;
                    }
                }
            }
            else
            {
                p.width = 60;

                if (GUI.Button(p, "Create"))
                {
                    sequenceFlowObject = ScriptableObject.CreateInstance<SequenceFlowObject>();

                    if (UpdateAndCheckPrefabStage())
                    {
                        sequenceFlowObject.name = "Embedded in Prefab";
                        sequenceFlowObject.embeddedInPrefab = stageAsset;
                        AssetDatabase.AddObjectToAsset(sequenceFlowObject, stageAssetPath);
                    }
                    else
                    {
                        sequenceFlowObject.name = "Embedded in Scene";
                        sequenceFlowObject.embeddedInScene = true;
                    }

                    property.objectReferenceValue = sequenceFlowObject;
                }
            }
        }

        bool UpdateAndCheckPrefabStage()
        {
            stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

            if (!stage)
            {
                stageAssetPath = null;
                stageAsset = null;
                return false;
            }

            if (stageAssetPath != stage.assetPath)
            {
                stageAssetPath = stage.assetPath;
                stageAsset = AssetDatabase.LoadAssetAtPath<GameObject>(stageAssetPath);
            }

            return true;
        }
    }
}
