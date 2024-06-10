using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Vector2 : TypeDefinition
    {
        protected override string methodParametersPropertyName => null;
        protected override string methodsPropertyName => "vector2s";
        protected override int methodsCapacity => 2;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(Vector2);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new Vector2Field(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
