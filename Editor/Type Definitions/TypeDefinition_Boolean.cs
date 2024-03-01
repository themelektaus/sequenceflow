using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Boolean : TypeDefinition
    {
        protected override string methodParametersPropertyName => "boolParameters";
        protected override string methodsPropertyName => "bools";
        protected override int methodsCapacity => 4;

        protected override bool Match(SequenceView.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(bool);
        }

        protected override VisualElement OnCreateField(
            SequenceView.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new Toggle(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
