using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public abstract class View : VisualElement
    {
        public struct ParameterInfo
        {
            public string friendlyName;
            public Type type;
            public Attribute[] attributes;

            public T GetAttribute<T>() where T : Attribute
            {
                return attributes.Where(x => x is T).FirstOrDefault() as T;
            }
        }

        protected static void SetupPlaceholder(
            SerializedProperty parameterNameProperty,
            Type parameterType,
            VisualElement field
        )
        {
            if (parameterNameProperty is null)
                return;

            var textInput = field.Q("unity-text-input");

            var typeName = parameterType.FullName switch
            {
                "System.Int32" => "Integer",
                "System.Single" => "Float",
                "UnityEngine.Object" => "Object",
                _ => parameterType.Name
            };

            var placeholder = new Label(typeName) { name = "placeholder" };
            placeholder.style.position = Position.Absolute;
            placeholder.style.left = 3;
            placeholder.style.minWidth = StyleKeyword.None;
            placeholder.style.color = Color.gray;
            textInput.Add(placeholder);

            RefreshPlaceholder(field, parameterNameProperty.stringValue);

            var textInputText = textInput.GetType().GetProperty("text");
            textInput.RegisterCallback<InputEvent>(
                e => RefreshPlaceholder(
                    field,
                    textInputText.GetValue(textInput) as string
                )
            );
        }

        protected static void RefreshPlaceholder(VisualElement field, string text)
        {
            field.Q("placeholder").style.display
                = string.IsNullOrEmpty(text)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        protected static VisualElement CreateParameterField(
            string parameterName,
            SerializedProperty parameterNameProperty
        )
        {
            var field = new TextField(parameterName);
            field.AddToClassList("linked");
            field.BindProperty(parameterNameProperty);
            return field;
        }

        protected static void ToggleLink(SerializedProperty methodParameters, int index, SerializedProperty parameterProperty)
        {
            if (parameterProperty is null)
            {
                methodParameters.InsertArrayElementAtIndex(methodParameters.arraySize);

                var newParameter = methodParameters.GetArrayElementAtIndex(methodParameters.arraySize - 1);
                newParameter.FindPropertyRelative("index").intValue = index;
            }
            else
            {
                for (int i = 0; i < methodParameters.arraySize; i++)
                {
                    if (methodParameters.GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue == index)
                    {
                        methodParameters.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }
            methodParameters.serializedObject.ApplyModifiedProperties();
        }

        protected static IEnumerable<ParameterInfo> GetMethodParameters(FieldInfo[] fieldInfos)
        {
            foreach (var fieldInfo in fieldInfos)
            {
                yield return new()
                {
                    friendlyName = Utils.StringToCamelCase(fieldInfo.Name),
                    type = fieldInfo.FieldType,
                    attributes = fieldInfo.GetCustomAttributes().ToArray()
                };
            }
        }
    }
}
