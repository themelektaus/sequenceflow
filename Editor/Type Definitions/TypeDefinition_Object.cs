using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Object : TypeDefinition
    {
        static ObjectField lastObjectField = null;

        protected override string methodParametersPropertyName => "objectParameters";
        protected override string methodsPropertyName => "objects";
        protected override int methodsCapacity => 3;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(parameterInfo.type);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            lastObjectField = new(parameterInfo.friendlyName)
            {
                objectType = parameterInfo.type
            };
            lastObjectField.BindProperty(property);
            if (!property.objectReferenceValue)
            {
                var displayAttribute = parameterInfo.GetAttribute<DisplayNoneAsAttribute>(); ;
                if (displayAttribute is not null)
                {
                    var input = lastObjectField.Q(className: "unity-object-field__input");
                    input.style.display = DisplayStyle.None;
                    var executerButton = new Button { text = displayAttribute.text };
                    executerButton.AddToClassList("ExecuterButton");
                    executerButton.clicked += () =>
                    {
                        executerButton.style.display = DisplayStyle.None;
                        input.style.display = DisplayStyle.Flex;
                    };
                    lastObjectField.Add(executerButton);
                }
            }
            return lastObjectField;
        }
    }
}
