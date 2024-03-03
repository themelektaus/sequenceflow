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
    using ParameterInfo = SequenceView.ParameterInfo;

    public class StatementView : VisualElement
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

        struct PopupItem
        {
            public int value;
            public string name;

            public override string ToString() => name;
        }

        static readonly VisualTreeAsset statementLayoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/SequenceFlow/Editor/Statement.uxml"
        );

        public Func<Statement> getStatement;
        public Func<SerializedProperty> getSerializedStatement;

        public StatementView()
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SequenceFlow/Editor/StatementView.uss"));
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

            if (statement.conditions is not null)
            {
                var serializedConditions = serializedStatement.FindPropertyRelative("conditions");
                foreach (var condition in statement.conditions)
                {
                    var i = ArrayUtility.IndexOf(statement.conditions, condition);
                    var serializedCondition = serializedConditions.GetArrayElementAtIndex(i);
                    var layout = statementLayoutAsset.CloneTree();

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

                    SetupMethodField(layout, statement, serializedCondition.FindPropertyRelative("method"), i);

                    scrollView.Add(layout);
                }
            }

            var button = new Button { text = "+ Condition" };
            button.AddToClassList("AddButton");
            button.clicked += () =>
            {
                if (statement.conditions is null)
                    statement.conditions = new[] { new StatementCondition() };
                else
                    ArrayUtility.Add(ref statement.conditions, new StatementCondition());

                Refresh();
            };

            Add(button);
        }

        void SetupMethodField(
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

            layout.RegisterCallback<MouseUpEvent>(e =>
            {
                if (e.button != 1)
                    return;

                e.StopPropagation();
            }, TrickleDown.TrickleDown);

            layout.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button != 1)
                    return;

                e.StopPropagation();

                var menu = new GenericMenu();
                var on = bridge.methodNameProperty.stringValue.Equals("");

                menu.AddItem(new GUIContent("Statement/(None)"), on, OnMethodItemSelected, (bridge, ""));

                menu.AddSeparator(string.Empty);

                foreach (var definition in StatementMethodDefinition.GetDefaultInstances())
                    AddMethodItem(bridge, menu, "Statement/", definition);

                menu.AddSeparator(string.Empty);

                var condition = bridge.statement.conditions[bridge.conditionIndex];

                menu.AddItem(new("Move Up"), false, bridge.conditionIndex > 0 ? new GenericMenu.MenuFunction(() => MoveUp(bridge.statement, condition)) : null);
                menu.AddItem(new("Move Down"), false, bridge.conditionIndex < bridge.statement.conditions.Length - 1 ? new GenericMenu.MenuFunction(() => MoveDown(bridge.statement, condition)) : null);

                menu.AddSeparator(string.Empty);

                menu.AddItem(new("Delete"), false, () => Delete(bridge.statement, condition));
                menu.ShowAsContext();
            });

            bridge.methodField.Q("unity-text-input").style.color = new Color(1, .75f, 0);

            SetMethodFieldValue(ref bridge);
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

                var field = enumerator.CreateField(parameterInfo, out _);
                parametersElement.Add(field);
            }
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

        void AddMethodItem(MethodDataBridge methodDataBridge, GenericMenu menu, string rootPath, StatementMethodDefinition definition)
        {
            var on = methodDataBridge.methodNameProperty.stringValue.Equals(definition.ToString());
            menu.AddItem(new(rootPath + definition.menuPath), on, OnMethodItemSelected, (methodDataBridge, definition.ToString()));
        }

        void OnMethodItemSelected(object x)
        {
            var y = ((MethodDataBridge, string)) x;
            y.Item1.methodNameProperty.stringValue = y.Item2;
            y.Item1.methodNameProperty.serializedObject.ApplyModifiedProperties();
            Refresh();
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

    }
}
