using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_String : TypeDefinition
    {
        protected override string methodParametersPropertyName => "stringParameters";
        protected override string methodsPropertyName => "strings";
        protected override int methodsCapacity => 4;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type == typeof(string);
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var attribute = parameterInfo.GetAttribute<GlobalStringAttribute>();
            if (attribute is not null)
            {
                return Utils.CreatePropertyField(parameterInfo.friendlyName, () =>
                {
                    var height = GlobalStringPropertyDrawer.CalcPropertyHeight(property, new(parameterInfo.friendlyName));
                    var rect = new Rect(0, 0, 200, height);

                    if (GlobalStringPropertyDrawer.OnGUI(rect, property, null, attribute))
                        property.serializedObject.ApplyModifiedProperties();

                    return rect;
                });
            }

            var field = new TextField(parameterInfo.friendlyName);
            var multilineAttribute = parameterInfo.GetAttribute<MultilineAttribute>();
            if (multilineAttribute is not null)
            {
                field.multiline = true;
                field.style.maxHeight = int.MaxValue;
                field.AddToClassList("multiline");
            }
            field.BindProperty(property);
            return field;
        }
    }
}
