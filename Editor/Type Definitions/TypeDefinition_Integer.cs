using System;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Integer : TypeDefinition
    {
        protected override string methodParametersPropertyName => "intParameters";
        protected override string methodsPropertyName => "ints";
        protected override int methodsCapacity => 4;

        protected override bool Match(SequenceView.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(int);
        }

        protected override VisualElement OnCreateField(
            SequenceView.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new IntegerField(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
