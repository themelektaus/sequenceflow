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
    public class SequenceView : VisualElement
    {
        struct MethodDataBridge
        {
            public Sequence sequence;
            public int commandIndex;
            public Button methodField;
            public VisualElement parametersElement;
            public SerializedProperty serializedCommand;
            public SerializedProperty serializedMethod;
            public SerializedProperty methodNameProperty;
            public SequenceMethodDefinition sequenceMethodDefinition;

            public bool IsWaitable()
            {
                if (sequenceMethodDefinition is null)
                    return false;

                if (!sequenceMethodDefinition.waitable)
                    return false;

                return true;
            }

            public void UpdateMethodFieldValue()
            {
                Color backgroundColor;

                var container = methodField.parent.parent;

                if (string.IsNullOrWhiteSpace(methodNameProperty.stringValue))
                {
                    backgroundColor = new(1, 1, 1, .08f);
                    methodField.text = "(None)";
                }
                else
                {
                    var definition = SequenceMethodDefinition.GetDefaultInstance(methodNameProperty.stringValue);
                    if (definition is null)
                    {
                        methodField.text = methodNameProperty.stringValue;
                        methodField.style.backgroundColor = new Color(.5f, 0, 0, .375f);

                        backgroundColor = new(1, 1, 1, .08f);
                    }
                    else
                    {
                        var value = definition.displayName ?? definition.menuPath;

                        methodField.text = value.Replace("/", " → ");
                        methodField.style.backgroundColor = new StyleColor(StyleKeyword.Null);

                        backgroundColor = definition.color == Color.clear
                            ? new(1, 1, 1, .08f)
                            : definition.styleColor;
                    }
                }

                var hoverColor = backgroundColor;
                hoverColor.a *= 1.8f;
                container.RegisterCallback<MouseOverEvent>(e => container.style.backgroundColor = hoverColor);
                container.RegisterCallback<MouseOutEvent>(e => container.style.backgroundColor = backgroundColor);
                container.style.backgroundColor = backgroundColor;
            }
        }

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

        public Func<Sequence> getSequence;
        public Func<SerializedProperty> getSerializedCommands;
        public SimpleData parameters;

        readonly VisualTreeAsset methodLayoutAsset;

        public SequenceView()
        {
            methodLayoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/SequenceFlow/Editor/Method.uxml");
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SequenceFlow/Editor/SequenceView.uss"));
        }

        public void Refresh()
        {
            Clear();

            var sequence = getSequence();

            if (sequence.commands is not null)
            {
                var serializedCommands = getSerializedCommands();

                foreach (var command in sequence.commands)
                {
                    var i = ArrayUtility.IndexOf(sequence.commands, command);
                    var serializedCommand = serializedCommands.GetArrayElementAtIndex(i);
                    var layout = methodLayoutAsset.CloneTree();
                    var enabledProperty = serializedCommand.FindPropertyRelative("enabled");
                    var delayProperty = serializedCommand.FindPropertyRelative("delay");
                    var postDelayProperty = serializedCommand.FindPropertyRelative("postDelay");

                    if (!enabledProperty.boolValue)
                        layout.AddToClassList("disabled");

                    var bridge = SetupMethodField(layout, sequence, serializedCommand, i);

                    void OnMouseUp(MouseUpEvent e)
                    {
                        if (e.button != 1)
                            return;

                        e.StopPropagation();

                        var menu = new GenericMenu();
                        AddCommandsToMenu(menu, "Command/", bridge);
                        menu.AddSeparator(string.Empty);
                        AddContextToMenu(menu, bridge);
                        menu.ShowAsContext();
                    }

                    layout.RegisterCallback<MouseUpEvent>(OnMouseUp);
                    bridge.methodField.RegisterCallback<MouseUpEvent>(OnMouseUp);

                    if (delayProperty.floatValue > 0)
                        AddDelayElement("Pre-Delay", "pre-delay", delayProperty);

                    Add(layout);

                    var executerIsActivator = serializedCommand.FindPropertyRelative("executer").enumValueIndex == 1;
                    var flow = serializedCommand.FindPropertyRelative("flow").enumValueIndex;
                    var isAsync = bridge.IsWaitable() && flow > 0;

                    if (executerIsActivator || isAsync)
                    {
                        var executerElement = new VisualElement();
                        executerElement.AddToClassList("attributes");

                        if (executerIsActivator)
                        {
                            var label = new Label("Activator");
                            label.AddToClassList("activator");
                            executerElement.Add(label);
                        }

                        if (isAsync)
                        {
                            var label = new Label("Async");
                            executerElement.Add(label);

                            if (flow == 2)
                            {
                                label.AddToClassList("forced");

                                label = new Label("Forced");
                                label.AddToClassList("forced-suffix");
                                executerElement.Add(label);
                            }
                        }

                        layout.Insert(0, executerElement);
                    }

                    if (postDelayProperty.floatValue > 0)
                        AddDelayElement("Post-Delay", "post-delay", postDelayProperty);
                }
            }

            var button = new Button { text = "↳ Command" };
            button.AddToClassList("AddButton");
            button.clicked += () => AddCommand(sequence);
            Add(button);
        }

        void AddDelayElement(string menuText, string className, SerializedProperty delayProperty)
        {
            var delayLabel = new Label("Wait " + delayProperty.floatValue.ToString().Replace(',', '.') + " sec");
            delayLabel.AddToClassList(className);
            delayLabel.RegisterCallback<MouseUpEvent>(e =>
            {
                if (e.button != 1)
                    return;

                e.StopPropagation();

                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Remove " + menuText), false, () =>
                {
                    delayProperty.floatValue = 0;
                    delayProperty.serializedObject.ApplyModifiedProperties();
                    Refresh();
                });

                menu.ShowAsContext();
            });

            Add(delayLabel);
        }

        void AddCommand(Sequence sequence)
        {
            var command = new SequenceCommand();

            if (sequence.commands is null)
                sequence.commands = new[] { command };
            else
                ArrayUtility.Add(ref sequence.commands, command);

            Refresh();
        }

        void MoveUp(Sequence sequence, SequenceCommand command)
        {
            var i = ArrayUtility.IndexOf(sequence.commands, command);
            ArrayUtility.Remove(ref sequence.commands, command);
            ArrayUtility.Insert(ref sequence.commands, i - 1, command);
            Refresh();
        }

        void MoveDown(Sequence sequence, SequenceCommand command)
        {
            var i = ArrayUtility.IndexOf(sequence.commands, command);
            ArrayUtility.Remove(ref sequence.commands, command);
            ArrayUtility.Insert(ref sequence.commands, i + 1, command);
            Refresh();
        }

        void Delete(Sequence sequence, SequenceCommand command)
        {
            ArrayUtility.Remove(ref sequence.commands, command);
            Refresh();
        }

        MethodDataBridge SetupMethodField(
            TemplateContainer layout,
            Sequence sequence,
            SerializedProperty serializedCommand,
            int commandIndex
        )
        {
            var bridge = new MethodDataBridge
            {
                sequence = sequence,
                commandIndex = commandIndex,
                methodField = layout.Q<Button>("Method"),
                parametersElement = layout.Q("Parameters"),
                serializedCommand = serializedCommand,
                serializedMethod = serializedCommand.FindPropertyRelative("method")
            };

            bridge.methodNameProperty = bridge.serializedMethod.FindPropertyRelative("name");
            bridge.methodField.style.color = new Color(1, .75f, 0);
            bridge.methodField.focusable = false;

            bridge.methodField.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    SearchWindowProvider.Open(
                        e.mousePosition,
                        x => OnMethodItemSelected((bridge, x?.ToString() ?? string.Empty))
                    );
                }
            }, TrickleDown.TrickleDown);

            SetMethodFieldValue(ref bridge);

            return bridge;
        }

        void AddCommandsToMenu(GenericMenu menu, string rootPath, MethodDataBridge bridge)
        {
            var on = bridge.methodNameProperty.stringValue.Equals(string.Empty);

            menu.AddItem(new GUIContent(rootPath + "(None)"), on, OnMethodItemSelected, (bridge, string.Empty));
            menu.AddSeparator(rootPath);

            foreach (var definition in SequenceMethodDefinition.GetDefaultInstances())
                AddMethodItem(bridge, menu, rootPath, definition);
        }

        void AddContextToMenu(GenericMenu menu, MethodDataBridge bridge)
        {
            var command = bridge.sequence.commands[bridge.commandIndex];
            var enabledProperty = bridge.serializedCommand.FindPropertyRelative("enabled");
            var executerProperty = bridge.serializedCommand.FindPropertyRelative("executer");
            var flowProperty = bridge.serializedCommand.FindPropertyRelative("flow");

            menu.AddItem(new("Enabled"), enabledProperty.boolValue, () =>
            {
                enabledProperty.boolValue = !enabledProperty.boolValue;
                enabledProperty.serializedObject.ApplyModifiedProperties();
                Refresh();
            });

            var executerIsActivator = executerProperty.enumValueIndex == 1;
            menu.AddItem(new("Execute as Activator"), executerIsActivator, () =>
            {
                executerProperty.enumValueIndex = executerIsActivator ? 0 : 1;
                executerProperty.serializedObject.ApplyModifiedProperties();
                Refresh();
            });

            if (bridge.IsWaitable())
            {
                menu.AddItem(new("Async"), flowProperty.enumValueIndex == 1, () =>
                {
                    flowProperty.enumValueIndex = flowProperty.enumValueIndex == 1 ? 0 : 1;
                    executerProperty.serializedObject.ApplyModifiedProperties();
                    Refresh();
                });

                menu.AddItem(new("Async (Forced)"), flowProperty.enumValueIndex == 2, () =>
                {
                    flowProperty.enumValueIndex = flowProperty.enumValueIndex == 2 ? 0 : 2;
                    executerProperty.serializedObject.ApplyModifiedProperties();
                    Refresh();
                });
            }

            menu.AddSeparator(string.Empty);

            AddContextToMenu_Delay(menu, bridge, "Pre-Delay", "delay");
            AddContextToMenu_Delay(menu, bridge, "Post-Delay", "postDelay");

            menu.AddSeparator(string.Empty);

            menu.AddItem(new("Move Up"), false, bridge.commandIndex > 0 ? new GenericMenu.MenuFunction(() => MoveUp(bridge.sequence, command)) : null);
            menu.AddItem(new("Move Down"), false, bridge.commandIndex < bridge.sequence.commands.Length - 1 ? new GenericMenu.MenuFunction(() => MoveDown(bridge.sequence, command)) : null);

            menu.AddSeparator(string.Empty);

            menu.AddItem(new("Delete"), false, () => Delete(bridge.sequence, command));
        }

        void AddContextToMenu_Delay(GenericMenu menu, MethodDataBridge bridge, string menuText, string relativePropertyPath)
        {
            var delayProperty = bridge.serializedCommand.FindPropertyRelative(relativePropertyPath);

            foreach (var delay in new[] { .05f, .1f, .15f, .2f, .25f, .5f, .75f, 1, 2, 3, 5, 8 })
            {
                var sec = delay.ToString("0.00").Replace(',', '.');
                menu.AddItem(new GUIContent($"{menuText}/{sec} sec"), Mathf.Approximately(delayProperty.floatValue, delay), () =>
                {
                    delayProperty.floatValue = delay;
                    delayProperty.serializedObject.ApplyModifiedProperties();
                    Refresh();
                });
            }
        }

        void SetMethodFieldValue(ref MethodDataBridge bridge)
        {
            bridge.sequenceMethodDefinition = SequenceMethodDefinition.GetDefaultInstance(bridge.methodNameProperty.stringValue);
            if (bridge.sequenceMethodDefinition is null)
                SetMethodFieldValue(bridge, Enumerable.Empty<ParameterInfo>());
            else
                SetMethodFieldValue(bridge, GetMethodParameters(bridge.sequenceMethodDefinition.fieldInfos));
        }

        void SetMethodFieldValue(MethodDataBridge methodDataBridge, IEnumerable<ParameterInfo> parameterInfos)
        {
            var parametersElement = methodDataBridge.parametersElement;
            var serializedMethod = methodDataBridge.serializedMethod;

            methodDataBridge.UpdateMethodFieldValue();

            parametersElement.Clear();

            var enumerators = new TypeDefinition.EnumeratorCollection(serializedMethod);
            if (enumerators.dirty)
                serializedMethod.serializedObject.ApplyModifiedProperties();

            foreach (var parameterInfo in parameterInfos)
            {
                var enumerator = enumerators.Get(parameterInfo);
                if (enumerator is null)
                {
                    Debug.LogError($"parameter type {parameterInfo.type.Name} not supported");
                    continue;
                }

                SerializedProperty parameterNameProperty = null;

                var field = enumerator.CreateField(parameterInfo, out var parameterProperty);
                if (field is null)
                {
                    parameterNameProperty = parameterProperty.FindPropertyRelative("name");
                    field = CreateParameterField(
                        parameterInfo.friendlyName,
                        parameterNameProperty
                    );
                }

                parametersElement.Add(field);

                if (!enumerator.typeDefinition.isLinkable)
                    continue;

                SetLinkable(
                    methodDataBridge,
                    field,
                    enumerator.methodParametersProperty,
                    enumerator.currentMethodPropertyIndex,
                    parameterProperty,
                    parameterInfo.type
                );

                if (parameterNameProperty is null)
                    continue;

                var textInput = field.Q("unity-text-input");

                var typeName = parameterInfo.type.FullName switch
                {
                    "System.Int32" => "Integer",
                    "System.Single" => "Float",
                    "UnityEngine.Object" => "Object",
                    _ => parameterInfo.type.Name
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
        }

        void RefreshPlaceholder(VisualElement field, string text)
        {
            field.Q("placeholder").style.display
                = string.IsNullOrEmpty(text)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        static VisualElement CreateParameterField(string parameterName, SerializedProperty parameterNameProperty)
        {
            var field = new TextField(parameterName);
            field.AddToClassList("linked");
            field.BindProperty(parameterNameProperty);
            return field;
        }

        void SetLinkable(
            MethodDataBridge bridge,
            VisualElement field,
            SerializedProperty methodParameters,
            int index,
            SerializedProperty parameterProperty,
            Type parameterType
        )
        {
            field.RegisterCallback<MouseUpEvent>(e =>
            {
                if (e.button != 1)
                    return;

                e.StopPropagation();

                var menu = new GenericMenu();

                menu.AddItem(
                    new GUIContent("Link Parameter"),
                    parameterProperty is not null,
                    () =>
                    {
                        ToggleLink(methodParameters, index, parameterProperty);
                        Refresh();
                    }
                );
                menu.AddSeparator(string.Empty);

                if (parameterProperty is not null && parameters is not null)
                {
                    var nameProperty = parameterProperty.FindPropertyRelative("name");
                    foreach (var parameter in parameters)
                    {
                        if (!parameter.GetValueType().IsAssignableFrom(parameterType))
                            continue;

                        var name = parameter.GetName();

                        menu.AddItem(
                            new GUIContent($"Linkable Parameters/{name} ({parameterType.Name})"),
                            nameProperty.stringValue == name,
                            () =>
                            {
                                nameProperty.stringValue = name;
                                nameProperty.serializedObject.ApplyModifiedProperties();

                                RefreshPlaceholder(field, name);
                            }
                        );
                    }
                }

                AddCommandsToMenu(menu, "Command/", bridge);

                menu.AddSeparator(string.Empty);

                AddContextToMenu(menu, bridge);

                menu.ShowAsContext();

            }, TrickleDown.TrickleDown);
        }

        static void ToggleLink(SerializedProperty methodParameters, int index, SerializedProperty parameterProperty)
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

        static IEnumerable<ParameterInfo> GetMethodParameters(FieldInfo[] fieldInfos)
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

        void AddMethodItem(MethodDataBridge methodDataBridge, GenericMenu menu, string rootPath, SequenceMethodDefinition definition)
        {
            var on = methodDataBridge.methodNameProperty.stringValue.Equals(definition.ToString());
            menu.AddItem(new(rootPath + definition.menuPath), on, OnMethodItemSelected, (methodDataBridge, definition.ToString()));
        }

        void OnMethodItemSelected(object x)
        {
            var y = ((MethodDataBridge, string)) x;

            var identifier = y.Item2;
            y.Item1.methodNameProperty.stringValue = identifier;

            var definition = SequenceMethodDefinition.GetDefaultInstance(identifier);

            if (definition is not null)
            {
                var instance = Activator.CreateInstance(definition.GetType()) as SequenceMethodDefinition;

                var method = y.Item1.serializedMethod;

                var boolsIndex = 0;
                var intsIndex = 0;
                var floatsIndex = 0;
                var stringsIndex = 0;

                foreach (var fieldInfo in definition.fieldInfos)
                {
                    var value = fieldInfo.GetValue(instance);
                    if (value is null)
                        continue;

                    SerializedProperty arrayProperty;

                    if (value is bool boolValue)
                    {
                        arrayProperty = method.FindPropertyRelative("bools");
                        arrayProperty.GetArrayElementAtIndex(boolsIndex++).boolValue = boolValue;
                        continue;
                    }

                    if (value is int intValue)
                    {
                        arrayProperty = method.FindPropertyRelative("ints");
                        arrayProperty.GetArrayElementAtIndex(intsIndex++).intValue = intValue;
                        continue;
                    }

                    if (value is float floatValue)
                    {
                        arrayProperty = method.FindPropertyRelative("floats");
                        arrayProperty.GetArrayElementAtIndex(floatsIndex++).floatValue = floatValue;
                        continue;
                    }

                    if (value is string stringValue)
                    {
                        arrayProperty = method.FindPropertyRelative("strings");
                        arrayProperty.GetArrayElementAtIndex(stringsIndex++).stringValue = stringValue;
                        continue;
                    }
                }
            }

            y.Item1.methodNameProperty.serializedObject.ApplyModifiedProperties();

            Refresh();
        }
    }
}
