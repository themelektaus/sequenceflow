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

                    SetupMethodField(layout, serializedCondition.FindPropertyRelative("method"));

                    layout.Q<Button>("Up").SetEnabled(i > 0);
                    layout.Q<Button>("Up").clicked += () =>
                    {
                        var _i = ArrayUtility.IndexOf(statement.conditions, condition);
                        ArrayUtility.Remove(ref statement.conditions, condition);
                        ArrayUtility.Insert(ref statement.conditions, _i - 1, condition);
                        Refresh();
                    };

                    layout.Q<Button>("Down").SetEnabled(i < statement.conditions.Length - 1);
                    layout.Q<Button>("Down").clicked += () =>
                    {
                        var _i = ArrayUtility.IndexOf(statement.conditions, condition);
                        ArrayUtility.Remove(ref statement.conditions, condition);
                        ArrayUtility.Insert(ref statement.conditions, _i + 1, condition);
                        Refresh();
                    };

                    layout.Q<Button>("Delete").clicked += () =>
                    {
                        ArrayUtility.Remove(ref statement.conditions, condition);
                        Refresh();
                    };

                    Add(layout);
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

        void SetupMethodField(TemplateContainer layout, SerializedProperty serializedMethod)
        {
            var methodNameProperty = serializedMethod.FindPropertyRelative("name");

            var bridge = new MethodDataBridge
            {
                methodField = layout.Q<TextField>("Method"),
                parametersElement = layout.Q("Parameters"),
                serializedMethod = serializedMethod,
                methodNameProperty = methodNameProperty
            };

            bridge.methodField.Q("unity-text-input").style.color = new Color(1, .75f, 0);
            bridge.methodField.RegisterCallback<FocusEvent>(e =>
            {
                bridge.methodField.Blur();

                var menu = new GenericMenu();
                var on = bridge.methodNameProperty.stringValue.Equals("");

                menu.AddItem(new GUIContent("(None)"), on, OnMethodItemSelected, (bridge, ""));

                menu.AddSeparator(string.Empty);

                foreach (var definition in StatementMethodDefinition.GetDefaultInstances())
                    AddMethodItem(bridge, menu, "", definition);

                menu.ShowAsContext();
            });

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
    }
}
