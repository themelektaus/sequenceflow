using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Vector3 : TypeDefinition
    {
        protected override string methodParametersPropertyName => null;
        protected override string methodsPropertyName => "vector3s";
        protected override int methodsCapacity => 2;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(Vector3);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new Vector3Field(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
