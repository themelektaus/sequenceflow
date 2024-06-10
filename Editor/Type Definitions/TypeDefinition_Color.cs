using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Color : TypeDefinition
    {
        protected override string methodParametersPropertyName => null;
        protected override string methodsPropertyName => "colors";
        protected override int methodsCapacity => 2;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(Color);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new ColorField(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
