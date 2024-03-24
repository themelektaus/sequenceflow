using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class StatementView : View
    {
        struct MethodDataBridge
        {
            public Statement statement;
            public int conditionIndex;
            public TextField methodField;
            public VisualElement parametersElement;
            public SerializedProperty serializedMethod;
            public SerializedProperty methodNameProperty;
            public StatementMethodDefinition statementMethodDefinition;

            public void UpdateMethodFieldValue()
            {
                if (string.IsNullOrWhiteSpace(methodNameProperty.stringValue))
                {
                    methodField.value = "(None)";
                }
                else
                {
                    methodField.value = statementMethodDefinition.menuPath.Replace("/", " → ");
                    methodField.Q("unity-text-input").style.backgroundColor = new StyleColor(StyleKeyword.Null);
                    methodField.parent.parent.style.backgroundColor =
                        statementMethodDefinition.color == Color.clear ?
                        new Color(1, 1, 1, .125f) :
                        statementMethodDefinition.styleColor;
                }
            }
        }

        static readonly VisualTreeAsset conditionLayoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/SequenceFlow/Editor/StatementViewCondition.uxml"
        );

        public Func<Statement> getStatement;
        public Func<SerializedProperty> getSerializedStatement;
        public SimpleData parameters;

        public StatementView()
        {
            styleSheets.Add(
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/SequenceFlow/Editor/StatementView.uss"
                )
            );
        }

        public void Refresh()
        {
            Clear();

            var statement = getStatement();
            var serializedStatement = getSerializedStatement();

            var logicGate = new EnumField("Logic Gate");
            logicGate.BindProperty(serializedStatement.FindPropertyRelative("logicGate"));
            Add(logicGate);

            var scrollView = new ScrollView();
            Add(scrollView);

            scrollView.style.maxHeight = SequenceFlowWindow.viewHeight - 118;

            if (statement.conditions is not null)
            {
                var serializedConditions = serializedStatement.FindPropertyRelative("conditions");
                foreach (var condition in statement.conditions)
                {
                    var i = ArrayUtility.IndexOf(statement.conditions, condition);
                    var serializedCondition = serializedConditions.GetArrayElementAtIndex(i);
                    var layout = conditionLayoutAsset.CloneTree();

                    var enabledProperty = serializedCondition.FindPropertyRelative("enabled");
                    if (!enabledProperty.boolValue)
                        layout.AddToClassList("disabled");

                    layout.Q<Toggle>("Enabled").BindProperty(enabledProperty);
                    layout.Q<Toggle>("Enabled").RegisterValueChangedCallback(e =>
                    {
                        if (e.newValue)
                            layout.RemoveFromClassList("disabled");
                        else
                            layout.AddToClassList("disabled");
                    });

                    layout.Q<EnumField>("Executer").BindProperty(serializedCondition.FindPropertyRelative("executer"));

                    var bridge = SetupMethodField(layout, statement, serializedCondition.FindPropertyRelative("method"), i);

                    void OnMouseUp(MouseUpEvent e)
                    {
                        if (e.button != 1)
                            return;

                        e.StopPropagation();

                        var menu = new GenericMenu();
                        AddMethodsToMenu(menu, "Statement/", bridge);
                        menu.AddSeparator(string.Empty);
                        AddContextToMenu(menu, bridge);
                        menu.ShowAsContext();
                    }

                    layout.RegisterCallback<MouseUpEvent>(OnMouseUp);
                    bridge.methodField.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);

                    bridge.methodField.Q("unity-text-input").style.color = new Color(1, .75f, 0);

                    scrollView.Add(layout);
                }
            }

            var button = new Button { text = "+ Condition" };
            button.AddToClassList("AddButton");
            button.clicked += () =>
            {
                var condition = new StatementCondition();

                if (statement.conditions is null)
                    statement.conditions = new[] { condition };
                else
                    ArrayUtility.Add(ref statement.conditions, condition);

                Refresh();
            };

            Add(button);
        }

        void MoveUp(Statement statement, StatementCondition condition)
        {
            var i = ArrayUtility.IndexOf(statement.conditions, condition);
            ArrayUtility.Remove(ref statement.conditions, condition);
            ArrayUtility.Insert(ref statement.conditions, i - 1, condition);
            Refresh();
        }

        void MoveDown(Statement statement, StatementCondition condition)
        {
            var i = ArrayUtility.IndexOf(statement.conditions, condition);
            ArrayUtility.Remove(ref statement.conditions, condition);
            ArrayUtility.Insert(ref statement.conditions, i + 1, condition);
            Refresh();
        }

        void Delete(Statement statement, StatementCondition condition)
        {
            ArrayUtility.Remove(ref statement.conditions, condition);
            Refresh();
        }

        MethodDataBridge SetupMethodField(
            TemplateContainer layout,
            Statement statement,
            SerializedProperty serializedMethod,
            int conditionIndex
        )
        {
            var methodNameProperty = serializedMethod.FindPropertyRelative("name");

            var bridge = new MethodDataBridge
            {
                statement = statement,
                conditionIndex = conditionIndex,
                methodField = layout.Q<TextField>("Method"),
                parametersElement = layout.Q("Parameters"),
                serializedMethod = serializedMethod,
                methodNameProperty = methodNameProperty
            };

            SetMethodFieldValue(ref bridge);

            return bridge;
        }

        void SetMethodFieldValue(ref MethodDataBridge bridge)
        {
            bridge.statementMethodDefinition = StatementMethodDefinition.GetDefaultInstance(bridge.methodNameProperty.stringValue);
            if (bridge.statementMethodDefinition is null)
                SetMethodFieldValue(bridge, Enumerable.Empty<ParameterInfo>());
            else
                SetMethodFieldValue(bridge, GetMethodParameters(bridge.statementMethodDefinition.fieldInfos));
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

                SetupPlaceholder(
                    parameterNameProperty,
                    parameterInfo.type,
                    field
                );
            }
        }

        void AddMethodsToMenu(GenericMenu menu, string rootPath, MethodDataBridge bridge)
        {
            menu.AddItem(
                new($"{rootPath}(None)"),
                bridge.methodNameProperty.stringValue.Equals(string.Empty),
                OnMethodItemSelected,
                (bridge, string.Empty)
            );

            menu.AddSeparator(rootPath);

            foreach (var definition in StatementMethodDefinition.GetDefaultInstances())
                AddMethodItem(bridge, menu, rootPath, definition);
        }

        void AddContextToMenu(GenericMenu menu, MethodDataBridge bridge)
        {
            var condition = bridge.statement.conditions[bridge.conditionIndex];

            menu.AddItem(
                new("Move Up"),
                false,
                bridge.conditionIndex > 0
                    ? new(() => MoveUp(bridge.statement, condition))
                    : null
            );

            menu.AddItem(
                new("Move Down"),
                false,
                bridge.conditionIndex < bridge.statement.conditions.Length - 1
                    ? new(() => MoveDown(bridge.statement, condition))
                    : null
            );

            menu.AddSeparator(string.Empty);

            menu.AddItem(
                new("Delete"),
                false,
                () => Delete(bridge.statement, condition)
            );
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
                    new("Link Parameter"),
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
                        var valueType = parameter.GetValueType();

                        if (!parameterType.IsAssignableFrom(valueType))
                            continue;

                        var name = parameter.GetName();

                        var itemName = $"[{valueType.Name}]";
                        if (name != string.Empty)
                            itemName += $" {name}";

                        menu.AddItem(
                            new($"Linkable Parameters/{itemName}"),
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

                AddMethodsToMenu(menu, "Statement/", bridge);

                menu.AddSeparator(string.Empty);

                AddContextToMenu(menu, bridge);

                menu.ShowAsContext();

            }, TrickleDown.TrickleDown);
        }

        void AddMethodItem(
            MethodDataBridge methodDataBridge,
            GenericMenu menu,
            string rootPath,
            StatementMethodDefinition definition
        )
        {
            menu.AddItem(
                new(rootPath + definition.menuPath),
                methodDataBridge.methodNameProperty.stringValue.Equals(
                    definition.ToString()
                ),
                OnMethodItemSelected,
                (methodDataBridge, definition.ToString())
            );
        }

        void OnMethodItemSelected(object x)
        {
            var y = ((MethodDataBridge, string)) x;
            y.Item1.methodNameProperty.stringValue = y.Item2;
            y.Item1.methodNameProperty.serializedObject.ApplyModifiedProperties();
            Refresh();
        }
    }
}
