using System;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class SequenceFlowWindow : EditorWindow
    {
        public static UnityEngine.SceneManagement.Scene scene;
        public static SimpleData parameters;

        static SequenceFlowWindow _window;
        static SequenceFlowGraphView _view;
        public static float viewHeight => _view.contentRect.height;

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
                scene = default;
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

            if (sequenceFlowObject && sequenceFlowObject.WriteToData(scene))
                Debug.Log("Sequence Flow has been saved");
        }

        void OnEnable()
        {
            RefreshLayout();
        }

        void RefreshLayout()
        {
            rootVisualElement.Clear();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/SequenceFlow/Editor/SequenceFlowWindow.uss"
            );

            rootVisualElement.styleSheets.Add(styleSheet);

            CreateAndAddView(styleSheet);

            var toolbar = new Toolbar();

            toolbar.Add(
                new ToolbarButton(() =>
                {
                    var field = GetSequenceFlowObjectField();

                    var menu = new GenericMenu();

                    var assets = AssetDatabase.FindAssets($"t:{typeof(SequenceFlowObject).FullName}")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Select(AssetDatabase.LoadAssetAtPath<SequenceFlowObject>);

                    menu.AddItem(new("Refresh"), false, () => Open(sequenceFlowObject));
                    menu.AddItem(new(""), false, null);

                    menu.AddItem(new("Load Assets"), false, null);
                    foreach (var x in assets)
                    {
                        menu.AddItem(
                            new($"{x.name}"),
                            field.value == x,
                            x =>
                            {
                                var y = x as SequenceFlowObject;
                                Selection.activeObject = y;
                                EditorGUIUtility.PingObject(y);
                                Open(y);
                            },
                            x
                        );
                    }

                    var interpreters = FindObjectsByType<SequenceFlowInterpreter>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None
                    );

                    menu.AddItem(new(""), false, null);
                    menu.AddItem(new("Load From Scene"), false, null);

                    foreach (var x in interpreters)
                    {
                        if (!x.sequenceFlowObject)
                            continue;

                        if (x.sequenceFlowObject.embeddedInScene)
                            continue;

                        menu.AddItem(
                            new($"{x.gameObject.scene.name}/{x.name}"),
                            field.value == x.sequenceFlowObject,
                            x =>
                            {
                                var y = x as SequenceFlowInterpreter;
                                Selection.activeGameObject = y.gameObject;
                                EditorGUIUtility.PingObject(y);
                                SequenceFlowObjectDrawer.Edit(y);
                            },
                            x
                        );
                    }

                    menu.AddItem(new(""), false, null);
                    menu.AddItem(new("Load Embedded"), false, null);

                    foreach (var x in interpreters)
                    {
                        if (!x.sequenceFlowObject)
                            continue;

                        if (!x.sequenceFlowObject.embeddedInScene)
                            continue;

                        menu.AddItem(
                            new($" {x.gameObject.scene.name}/{x.name}"),
                            field.value == x.sequenceFlowObject,
                            x =>
                            {
                                var y = x as SequenceFlowInterpreter;
                                Selection.activeGameObject = y.gameObject;
                                EditorGUIUtility.PingObject(y);
                                SequenceFlowObjectDrawer.Edit(y);
                            },
                            x
                        );
                    }

                    menu.ShowAsContext();
                })
                {
                    text = "File",
                    focusable = false
                }
            );

            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1 } });

            toolbar.Add(new ObjectField
            {
                name = "SequenceFlowObject",
                allowSceneObjects = false
            });

            rootVisualElement.Add(toolbar);

            var field = GetSequenceFlowObjectField();
            if (field is null)
                return;

            void OnChange(UnityEngine.Object value)
            {
                var sequenceFlowObject = value as SequenceFlowObject;
                Load(sequenceFlowObject);
                CreateAndAddView(styleSheet);
                _view.Refresh(sequenceFlowObject ? sequenceFlowObject.sequenceFlow : null);
            }

            field.RegisterValueChangedCallback(e => OnChange(e.newValue));

            OnChange(null);
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
