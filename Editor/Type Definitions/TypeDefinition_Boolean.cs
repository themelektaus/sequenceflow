using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Boolean : TypeDefinition
    {
        protected override string methodParametersPropertyName => "boolParameters";
        protected override string methodsPropertyName => "bools";
        protected override int methodsCapacity => 4;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(bool);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var field = new Toggle(parameterInfo.friendlyName);
            field.BindProperty(property);
            return field;
        }
    }
}
