using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_AnimationsCurve : TypeDefinition
    {
        protected override string methodParametersPropertyName => null;
        protected override string methodsPropertyName => "animationCurves";
        protected override int methodsCapacity => 2;

        protected override bool Match(SequenceView.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(AnimationCurve);
        }

        protected override VisualElement OnCreateField(
            SequenceView.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new CurveField(parameterInfo.friendlyName);
            field.style.height = EditorGUIUtility.singleLineHeight * 1.5f;
            field.BindProperty(property);
            return field;
        }
    }
}
