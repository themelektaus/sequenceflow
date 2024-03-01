using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_UnityEvent : TypeDefinition
    {
        protected override string methodParametersPropertyName => null;
        protected override string methodsPropertyName => "unityEvents";
        protected override int methodsCapacity => 2;

        protected override bool Match(SequenceView.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(UnityEngine.Events.UnityEvent);
        }

        protected override VisualElement OnCreateField(
            SequenceView.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            return Utils.CreatePropertyField(parameterInfo.friendlyName, () =>
            {
                var rect = new Rect(0, 0, 200, EditorGUI.GetPropertyHeight(property));
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, property, new(string.Empty), false);
                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
                return rect;
            });
        }
    }
}
