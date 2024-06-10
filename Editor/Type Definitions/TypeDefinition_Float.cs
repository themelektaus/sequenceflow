using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Float : TypeDefinition
    {
        protected override string methodParametersPropertyName => "floatParameters";
        protected override string methodsPropertyName => "floats";
        protected override int methodsCapacity => 4;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(float);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new FloatField(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
