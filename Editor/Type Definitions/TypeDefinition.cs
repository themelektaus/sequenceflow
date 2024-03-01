using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Prototype.SequenceFlow.Editor
{
    public abstract class TypeDefinition
    {
        protected abstract string methodParametersPropertyName { get; }
        protected abstract string methodsPropertyName { get; }
        protected abstract int methodsCapacity { get; }

        public bool isLinkable => methodParametersPropertyName is not null;

        protected abstract bool Match(
            SequenceView.ParameterInfo parameterInfo
        );

        protected abstract VisualElement OnCreateField(
            SequenceView.ParameterInfo parameterInfo,
            SerializedProperty property
        );

        static HashSet<TypeDefinition> instances;
        public static HashSet<TypeDefinition> GetInstances()
            => instances ??= Utils
                .GetAll<TypeDefinition>()
                .Select(x => System.Activator.CreateInstance(x) as TypeDefinition)
                .ToHashSet();

        public class EnumeratorCollection
        {
            public readonly SerializedProperty serializedMethod;

            public bool dirty { get; private set; }

            readonly List<Enumerator> items = new();

            public Enumerator Get(SequenceView.ParameterInfo parameterInfo)
            {
                return items.FirstOrDefault(x => x.typeDefinition.Match(parameterInfo));
            }

            public EnumeratorCollection(SerializedProperty serializedMethod)
            {
                this.serializedMethod = serializedMethod;
                foreach (var x in GetInstances())
                {
                    string name;

                    name = x.methodParametersPropertyName;

                    var methodParametersProperty = name is null
                        ? null
                        : serializedMethod.FindPropertyRelative(name);

                    name = x.methodsPropertyName;

                    var temp = serializedMethod.FindPropertyRelative(name);
                    if (temp.arraySize != x.methodsCapacity)
                    {
                        temp.arraySize = x.methodsCapacity;
                        dirty = true;
                    }

                    var methodsProperty = new SerializedProperty[temp.arraySize];
                    for (int i = 0; i < temp.arraySize; i++)
                        methodsProperty[i] = temp.GetArrayElementAtIndex(i);

                    items.Add(new(x, methodParametersProperty, methodsProperty));
                }
            }
        }

        public class Enumerator
        {
            public readonly TypeDefinition typeDefinition;
            public readonly SerializedProperty methodParametersProperty;

            readonly SerializedProperty[] methodsProperty;

            int nextMethodsPropertyIndex;
            public int currentMethodPropertyIndex => nextMethodsPropertyIndex - 1;

            public Enumerator(
                TypeDefinition typeDefinition,
                SerializedProperty methodParametersProperty,
                SerializedProperty[] methodsProperty
            )
            {
                this.typeDefinition = typeDefinition;
                this.methodParametersProperty = methodParametersProperty;
                this.methodsProperty = methodsProperty;
            }

            public VisualElement CreateField(
                SequenceView.ParameterInfo parameterInfo,
                out SerializedProperty parameterProperty
            )
            {
                var property = methodsProperty[nextMethodsPropertyIndex++];

                parameterProperty = GetParameterProperty();
                if (parameterProperty is not null)
                    return null;

                return typeDefinition.OnCreateField(parameterInfo, property);
            }

            SerializedProperty GetParameterProperty()
            {
                if (methodParametersProperty is not null)
                {
                    for (int i = 0; i < methodParametersProperty.arraySize; i++)
                    {
                        var methodParameter = methodParametersProperty.GetArrayElementAtIndex(i);
                        if (methodParameter.FindPropertyRelative("index").intValue == currentMethodPropertyIndex)
                            return methodParameter;
                    }
                }

                return null;
            }
        }
    }
}
