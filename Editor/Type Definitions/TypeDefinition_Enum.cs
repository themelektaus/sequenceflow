using System;

using UnityEditor;

using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class TypeDefinition_Enum : TypeDefinition
    {
        protected override string methodParametersPropertyName => null;
        protected override string methodsPropertyName => "enums";
        protected override int methodsCapacity => 2;

        protected override bool Match(View.ParameterInfo parameterInfo)
        {
            return parameterInfo.type.IsEnum;
        }

        protected override VisualElement OnCreateField(
            View.ParameterInfo parameterInfo,
            SerializedProperty property
        )
        {
            var values = Enum.GetValues(parameterInfo.type) as int[];
            var names = Enum.GetNames(parameterInfo.type);
            var choices = new System.Collections.Generic.List<PopupItem>();

            for (var i = 0; i < values.Length; i++)
                choices.Add(new PopupItem { value = values[i], name = names[i] });

            var field = new PopupField<PopupItem>(
                parameterInfo.friendlyName,
                choices,
                property.intValue
            );

            field.RegisterValueChangedCallback(choice =>
            {
                property.intValue = choice.newValue.value;
                property.serializedObject.ApplyModifiedProperties();
            });

            return field;
        }

        struct PopupItem
        {
            public int value;
            public string name;

            public override string ToString() => name;
        }
    }
}
