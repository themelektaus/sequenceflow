using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class SequenceFlowWindow : EditorWindow
    {
        const string STYLESHEET_PATH = "Assets/SequenceFlow/Editor/SequenceFlowWindow.uss";

        public static SimpleData parameters;

        static SequenceFlowWindow _window;
        static SequenceFlowGraphView _view;

        [MenuItem("Tools/Sequence Flow")]
        static void Init()
        {
            _window = GetWindow<SequenceFlowWindow>(typeof(SceneView));
            _window.titleContent = new GUIContent("Sequence Flow");
            _window.Focus();
        }

        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var @object = EditorUtility.InstanceIDToObject(instanceID);

            if (@object is SequenceFlowObject sequenceFlowObject)
            {
                parameters = null;
                Open(sequenceFlowObject);
            }

            return false;
        }

        public static void Open(SequenceFlowObject sequenceFlowObject)
        {
            Init();

            var field = _window.GetSequenceFlowObjectField();
            if (field is null)
                return;

            field.value = null;

            if (!sequenceFlowObject)
                return;

            if (sequenceFlowObject.sequenceFlow is null)
                return;

            field.value = sequenceFlowObject;
        }

        SequenceFlowObject sequenceFlowObject;

        DateTime lastInspectorUpdate = DateTime.MinValue;

        ObjectField GetSequenceFlowObjectField()
            => rootVisualElement.Q<ObjectField>("SequenceFlowObject");

        void Load(SequenceFlowObject newSequenceFlowObject)
        {
            sequenceFlowObject = newSequenceFlowObject;

            if (sequenceFlowObject)
                sequenceFlowObject.ReadFromData();
        }

        void OnInspectorUpdate()
        {
            if ((DateTime.Now - lastInspectorUpdate).TotalSeconds < 1)
                return;

            lastInspectorUpdate = DateTime.Now;

            if (sequenceFlowObject && sequenceFlowObject.WriteToData())
                Debug.Log("Sequence Flow has been saved");
        }

        public void OnEnable()
        {
            RefreshLayout();
        }

        void RefreshLayout()
        {
            rootVisualElement.Clear();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLESHEET_PATH);
            rootVisualElement.styleSheets.Add(styleSheet);

            CreateAndAddView(styleSheet);

            var toolbar = new Toolbar();

            toolbar.Add(new ObjectField
            {
                name = "SequenceFlowObject",
                allowSceneObjects = false
            });

            toolbar.Add(
                new Button(() => Open(sequenceFlowObject))
                {
                    text = "Refresh",
                    focusable = false
                }
            );

            rootVisualElement.Add(toolbar);

            var field = GetSequenceFlowObjectField();
            if (field is null)
                return;

            field.value = sequenceFlowObject;
            field.RegisterValueChangedCallback(e =>
            {
                Load(e.newValue as SequenceFlowObject);
                CreateAndAddView(styleSheet);
                _view.Refresh(sequenceFlowObject ? sequenceFlowObject.sequenceFlow : null);
            });
        }

        void CreateAndAddView(StyleSheet styleSheet)
        {
            var oldView = _view;
            var oldIndex = -1;

            if (oldView is not null)
            {
                oldView.RemoveTransitionSettings();
                oldIndex = rootVisualElement.IndexOf(oldView);
            }

            _view = new() { name = "SequenceFlowView" };
            _view.styleSheets.Add(styleSheet);
            _view.StretchToParentSize();

            if (oldIndex > -1)
            {
                rootVisualElement.Remove(oldView);
                rootVisualElement.Insert(oldIndex, _view);
            }
            else
            {
                rootVisualElement.Add(_view);
            }

            _view.AddTransitionSettingsViewTo(rootVisualElement);
        }
    }
}
