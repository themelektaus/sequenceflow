#if !MT_PACKAGES_PROJECT
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MT.Packages.SequenceFlow.Editor.UIElements
{
	public class SequenceFlowWindow : EditorWindow
	{
		const string LAYOUT_PATH = "Assets/MT Packages/Sequence Flow/Editor/UIElements/SequenceFlowWindow.uxml";
		const string STYLESHEET_PATH = "Assets/MT Packages/Sequence Flow/Editor/UIElements/SequenceFlowWindow.uss";

		static readonly Dictionary<string, Type> _typeList = new Dictionary<string, Type>() {
			{ "SequenceFlowObject", typeof(SequenceFlowObject) }
		};

		static SequenceFlowWindow _window;
		static SequenceFlowGraphView _view;

#pragma warning disable 51
		static bool _ReadFromData = false;

		[UnityEditor.Callbacks.DidReloadScripts]
		static void OnReloadScripts() {
			_ReadFromData = true;
		}

		[MenuItem("Tools/" + Core.Utility.ASSET_NAME + "/Sequence Flow")]
		static void Init() {
			_window = GetWindow<SequenceFlowWindow>(typeof(SceneView));
			_window.titleContent = new GUIContent("Sequence Flow");
		}
#pragma warning restore 51

		[UnityEditor.Callbacks.OnOpenAsset]
		public static bool OnOpenAsset(int instanceID, int line) {
			var @object = EditorUtility.InstanceIDToObject(instanceID);
			if (@object is SequenceFlowObject sequenceFlowObject) {
				Init();
				_window.Focus();
				var field = _window.rootVisualElement.Q<ObjectField>("SequenceFlowObject");
				if (field != null) {
					field.value = sequenceFlowObject;
				}
			}
			return false;
		}

		public SequenceFlowObject sequenceFlowObject;

		DateTime lastInspectorUpdate = DateTime.MinValue;
		DateTime currentLayoutLastWriteTime = DateTime.MinValue;

		void Load(SequenceFlowObject newSequenceFlowObject) {
			sequenceFlowObject = newSequenceFlowObject;
			if (sequenceFlowObject) {
				sequenceFlowObject.ReadFromData();
			}
		}

		public void OnEnable() {
			// rootVisualElement.RegisterCallback<MouseDownEvent>(e => {
			// 	HideTransitionSettingsView();
			// });
			RefreshLayout();
		}

		void AddToolbar() {
			var toolbar = new Toolbar();
			toolbar.Add(new ObjectField {
				name = "SequenceFlowObject",
				allowSceneObjects = false
			});
			toolbar.Add(new Button(() => {
				RefreshLayout();
				RefreshView();
			}) { text = "Refresh" });
			rootVisualElement.Add(toolbar);
		}

		void OnInspectorUpdate() {
			if (_ReadFromData) {
				_ReadFromData = false;
				if (sequenceFlowObject) {
					sequenceFlowObject.ReadFromData();
				}
			}
			if ((DateTime.Now - lastInspectorUpdate).TotalSeconds >= 1) {
				lastInspectorUpdate = DateTime.Now;
				if (sequenceFlowObject && sequenceFlowObject.WriteToData()) {
					rootVisualElement.Q<InfoMessage>().Show("Sequence Flow has been saved");
				}
				var lastWriteTime = new System.IO.FileInfo(LAYOUT_PATH).LastWriteTime;
				if (currentLayoutLastWriteTime != lastWriteTime) {
					currentLayoutLastWriteTime = lastWriteTime;
					RefreshLayout();
					RefreshView();
				}
			}
		}

		void RefreshLayout() {
			rootVisualElement.Clear();

			var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLESHEET_PATH);
			rootVisualElement.styleSheets.Add(styleSheet);

			CreateAndAddView(styleSheet);
			AddToolbar();
			rootVisualElement.Add(new InfoMessage());

			var layoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LAYOUT_PATH);
			VisualElement layoutElement = layoutAsset.CloneTree();
			rootVisualElement.Add(layoutElement);

			rootVisualElement.Query<ObjectField>().ForEach(element => {
				if (_typeList.ContainsKey(element.name)) {
					element.objectType = _typeList[element.name];
				}
			});

			var field = rootVisualElement.Q<ObjectField>("SequenceFlowObject");
			if (field != null) {
				field.value = sequenceFlowObject;
				field.RegisterValueChangedCallback(e => {
					Load(e.newValue as SequenceFlowObject);
					CreateAndAddView(styleSheet);
					RefreshView();
				});
			}
		}

		void CreateAndAddView(StyleSheet styleSheet) {
			var oldView = _view;
			var oldIndex = -1;
			if (oldView != null) {
				oldView.RemoveTransitionSettings();
				oldIndex = rootVisualElement.IndexOf(oldView);
			}
			_view = new SequenceFlowGraphView() { name = "SequenceFlowView" };
			_view.styleSheets.Add(styleSheet);
			_view.StretchToParentSize();
			if (oldIndex > -1) {
				rootVisualElement.Remove(oldView);
				rootVisualElement.Insert(oldIndex, _view);
			} else {
				rootVisualElement.Add(_view);
			}
			_view.AddTransitionSettingsViewTo(rootVisualElement);
		}

		void RefreshView() {
			_view.Refresh(sequenceFlowObject ? sequenceFlowObject.sequenceFlow : null);
		}
	}
}
#endif