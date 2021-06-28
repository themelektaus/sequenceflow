using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MT.Packages.SequenceFlow.Editor
{
	[System.Obsolete("Use the new Editor based on UIElements")]
	public class SequenceFlowEditorWindow : EditorWindow
	{
#pragma warning disable IDE0051

		static bool _ReadFromData = false;

		[UnityEditor.Callbacks.DidReloadScripts]
		static void OnReloadScripts() {
			_ReadFromData = true;
		}

		[MenuItem("Tools/" + Core.Utility.ASSET_NAME + "/Sequence Flow (Legacy, Deprecated)")]
		static void Init() {
			_window = GetWindow<SequenceFlowEditorWindow>(typeof(SceneView));
			_window.titleContent = new GUIContent("Sequence Flow");
			_window.Show();
		}

#pragma warning restore IDE0051

		// [UnityEditor.Callbacks.OnOpenAsset]
		// public static bool OnOpenAsset(int instanceID, int line) {
		// 	var @object = EditorUtility.InstanceIDToObject(instanceID);
		// 	if (@object is SequenceFlowObject sequenceFlowObject) {
		// 		Init();
		// 		_window.Focus();
		// 		_window.Load(sequenceFlowObject);
		// 	}
		// 	return false;
		// }

		static SequenceFlowEditorWindow _window;

		Texture2D[] _ArrowTextures;
		Texture2D[] ArrowTextures {
			get {
				if (_ArrowTextures == null || _ArrowTextures.Length != 2 || !_ArrowTextures[0] || !_ArrowTextures[1]) {
					_ArrowTextures = new[] {
						AssetDatabase.LoadAssetAtPath<Texture2D>(Core.Utility.GetPackagesPath() + "/Sequence Flow/Editor/Arrow Right.png"),
						AssetDatabase.LoadAssetAtPath<Texture2D>(Core.Utility.GetPackagesPath() + "/Sequence Flow/Editor/Arrow Left.png")
					};
				}
				return _ArrowTextures;
			}
		}

		public SequenceFlowObject sequenceFlowObject;

		public int RightAreaWidth {
			get {
				if (_selection is Transition) {
					return 354;
				}
				return 0;
			}
		}
		public Rect MainAreaContainerRect { get { return new Rect(0, 40, position.width - RightAreaWidth, position.height - 40); } }
		public Rect MainAreaRect { get { return new Rect(0, 0, 10000, 10000); } }
		public Vector2 RealMousePosition { get; private set; }
		public Vector2 MousePosition { get; private set; }
		public bool MouseLeftDown { get; private set; }
		public bool MouseLeftUp { get; private set; }
		public bool MouseRightDown { get; private set; }
		public bool MouseRightUp { get; private set; }

		bool _updateMousePosition;
		Vector2 _lastMousePosition;
		Vector2 _scrollPosition;
		Vector2 _targetScrollPosition;
		Vector2 _lastScrollPosition;

		[System.NonSerialized] object _selection;
		[System.NonSerialized] Transition _newTransition;
		[System.NonSerialized] float _zoom = 1f;

		void Update() {
			if (focusedWindow == this && mouseOverWindow == this) {
				_updateMousePosition = true;
			} else if (MousePosition.x > 0 || MousePosition.y > 0) {
				MousePosition = new Vector2(-1, -1);
				_updateMousePosition = false;
			}
			if (_newTransition != null) {
				Repaint();
			}
		}

		void Load() {
			_selection = null;
			_newTransition = null;
			_zoom = 1;
			sequenceFlowObject.ReadFromData();
			var states = sequenceFlowObject.sequenceFlow.states;
			if (states.Count > 0) {
				Rect rect = new Rect {
					xMin = (from s in states orderby s.GetX() select s).FirstOrDefault().GetRect().xMin,
					yMin = (from s in states orderby s.GetY() select s).FirstOrDefault().GetRect().yMin,
					xMax = (from s in states orderby s.GetX() descending select s).FirstOrDefault().GetRect().xMax,
					yMax = (from s in states orderby s.GetY() descending select s).FirstOrDefault().GetRect().yMax
				};
				_scrollPosition = new Vector2(
					rect.x - (MainAreaContainerRect.width - rect.width) / 2,
					rect.y - (MainAreaContainerRect.height - rect.height) / 2
				);
			} else {
				_scrollPosition = new Vector2(MainAreaRect.width / 2, MainAreaRect.height / 2);
			}
			_targetScrollPosition = _scrollPosition;
		}

		void OnGUI() {
			OnGUI_Init();
			if (OnGUI_DrawSequenceFlowObject()) {
				return;
			}

			if (!sequenceFlowObject) {
				EditorGUILayout.LabelField("Select a Sequence Flow Object (Scriptable Object)");
				return;
			}

			var e = Event.current;

			OnGUI_Hotkeys(e);
			OnGUI_SetupMouse(e);
			OnGUI_SetupZoom(e);

			var serializedObject = new SerializedObject(sequenceFlowObject);
			var sequenceFlow = sequenceFlowObject.sequenceFlow;

			if (!(e.type == EventType.MouseDown && e.button == 2)) {
				OnGUI_DrawRightArea(sequenceFlow, serializedObject);

				if ((MouseLeftDown || MouseRightDown) && MainAreaContainerRect.Contains(RealMousePosition)) {
					OnGUI_UpdateSelection(sequenceFlow);
				}

				ZoomArea.Begin(_zoom, MainAreaContainerRect);
				OnGUI_DrawMainArea(sequenceFlow, serializedObject);
				ZoomArea.End();
			}
			OnGUI_MousePanning();

			if (MouseRightUp && (_newTransition == null || _newTransition.source == null) && MainAreaContainerRect.Contains(RealMousePosition)) {
				OnGUI_ShowRightClickMenu(sequenceFlow);
			}

			if (MouseLeftUp || MouseRightUp) {
				Repaint();
			}
		}

		void OnGUI_Init() {
			if (_ReadFromData) {
				_ReadFromData = false;
				if (sequenceFlowObject) {
					Load();
				}
			}
		}

		bool OnGUI_DrawSequenceFlowObject() {
			EditorGUILayout.BeginVertical(new GUIStyle {
				padding = new RectOffset(10, 10, 10, 10)
			});
			var newSequenceFlowObject = EditorGUILayout.ObjectField(sequenceFlowObject, typeof(SequenceFlowObject), false) as SequenceFlowObject;
			EditorGUILayout.EndVertical();

			if (sequenceFlowObject != newSequenceFlowObject) {
				Load(newSequenceFlowObject);
				return true;
			}
			return false;
		}

		void Load(SequenceFlowObject newSequenceFlowObject) {
			if (sequenceFlowObject != null) {
				sequenceFlowObject.WriteToData();
			}
			sequenceFlowObject = newSequenceFlowObject;
			if (sequenceFlowObject != null) {
				Load();
			}
		}

		void OnGUI_Hotkeys(Event e) {
			if (e.type == EventType.KeyDown) {
				if (e.control && e.keyCode == KeyCode.S) {
					sequenceFlowObject.WriteToData();
				}
			}
		}

		void OnGUI_SetupMouse(Event e) {
			if (_updateMousePosition) {
				RealMousePosition = e.mousePosition;
				MousePosition = new Vector2(
					RealMousePosition.x / _zoom + _scrollPosition.x - MainAreaContainerRect.x / _zoom,
					RealMousePosition.y / _zoom + _scrollPosition.y - MainAreaContainerRect.y / _zoom
				);
				MouseLeftDown = e.type == EventType.MouseDown && e.button == 0;
				MouseLeftUp = e.type == EventType.MouseUp && e.button == 0;
				MouseRightDown = e.type == EventType.MouseDown && e.button == 1;
				MouseRightUp = e.type == EventType.MouseUp && e.button == 1;
			}
		}

		void OnGUI_SetupZoom(Event e) {
			if (_updateMousePosition) {
				if (e.type == EventType.ScrollWheel && e.delta.y != 0) {
					var previousZoom = _zoom;
					_zoom += -e.delta.y / 50.0f;
					_zoom = Mathf.Clamp(_zoom, .4f, 3f);
					ZoomArea.ScrollPositionCorrection(ref _scrollPosition, MousePosition, previousZoom, _zoom);
					_targetScrollPosition = _scrollPosition;
				}
			}
		}

		void OnGUI_DrawRightArea(SequenceFlow sequenceFlow, SerializedObject serializedObject) {
			GUILayout.BeginArea(new Rect(
				MainAreaContainerRect.xMax,
				MainAreaContainerRect.yMin,
				RightAreaWidth,
				MainAreaContainerRect.height
			), new GUIStyle {
				padding = new RectOffset(2, 2, 5, 5)
			});
			if (_selection is Transition selectedTransition) {
				var labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 60;
				selectedTransition.priority = EditorGUILayout.IntField("Priority", selectedTransition.priority);
				EditorGUIUtility.labelWidth = labelWidth;
				EditorGUILayout.Space(10);
				var transitionStatementsProperty = serializedObject.FindProperty("transitionStatements");
				var index = sequenceFlow.transitions.IndexOf(selectedTransition);
				Core.Editor.StatementMethodPropertyDrawer.DrawStatement(
					sequenceFlowObject.transitionStatements[index],
					transitionStatementsProperty.GetArrayElementAtIndex(index)
				);
				serializedObject.ApplyModifiedProperties();
			}
			GUILayout.EndArea();
		}

		void OnGUI_UpdateSelection(SequenceFlow sequenceFlow) {
			var currentSelection = _selection;
			var state = (
				from x in sequenceFlow.states
				where x.GetRect().Contains(MousePosition)
				select x
			).FirstOrDefault();
			if (_newTransition == null) {
				if (state == null) {
					_selection = (
						from x in sequenceFlow.transitions
						where x.GetArrowRect().Contains(MousePosition)
						select x
					).FirstOrDefault();
				} else {
					_selection = state;
				}
			} else {
				if (_selection != state && state != null) {
					_selection = state;
				}
			}
			if (currentSelection != _selection) {
				Repaint();
			}
		}

		void OnGUI_DrawMainArea(SequenceFlow sequenceFlow, SerializedObject serializedObject) {
			GUILayout.BeginArea(new Rect(Vector2.zero, MainAreaContainerRect.size / _zoom));
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);

			var backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(.35f, .35f, .35f);
			GUILayout.Box("", GUI.skin.button, GUILayout.Width(MainAreaRect.width), GUILayout.Height(MainAreaRect.height));
			GUI.backgroundColor = backgroundColor;

			foreach (var transition in sequenceFlow.transitions) {
				OnGUI_DrawMainArea_Transition(transition);
			}

			if (_updateMousePosition) {
				if (_newTransition != null && _newTransition.source != null) {
					OnGUI_DrawMainArea_Transition(_newTransition);
					if (MouseLeftDown) {
						if (_selection is State selectedState && selectedState != _newTransition.source) {
							_newTransition.destination = selectedState;
							if (sequenceFlow.transitions.Where(x => {
								return x.source == _newTransition.source
								&& x.destination == _newTransition.destination;
							}).FirstOrDefault() == null) {
								sequenceFlow.Add(_newTransition);
							}
						}
						_newTransition = null;
					}
				}
			}

			var stateSequencesProperty = serializedObject.FindProperty("stateSequences");
			BeginWindows();
			for (int i = 0; i < sequenceFlow.states.Count; i++) {
				var state = sequenceFlow.states[i];
				var rect = state.GetRect();
				var color = GUI.color;
				GUI.color = state == _selection ? new Color(1, .7f, .5f) : new Color(.7f, .7f, .7f);
				var _i = i;
				var windowTitle = state.ToString();
				if (state == sequenceFlow.startState) {
					if (string.IsNullOrEmpty(windowTitle)) {
						windowTitle = "[START]";
					} else {
						windowTitle = "[START] " + windowTitle;
					}
				}
				rect = GUI.Window(i + 1, rect, id => {
					Core.Editor.SequenceMethodPropertyDrawer.DrawSequence(
						rect,
						sequenceFlowObject.stateSequences[_i],
						stateSequencesProperty.GetArrayElementAtIndex(_i)
					);
					serializedObject.ApplyModifiedProperties();
					if (Event.current.button != 2) {
						GUI.DragWindow();
					}
				}, windowTitle); // $"{state.name} (Window ID: {i + 1})");

				GUI.color = color;
				//state.x = Mathf.RoundToInt(rect.x / State.PIXEL_SIZE);
				//state.y = Mathf.RoundToInt(rect.y / State.PIXEL_SIZE);
				state.SetPosition(rect.x, rect.y);
			}
			EndWindows();

			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		void OnGUI_DrawMainArea_Transition(Transition transition) {
			var (start, end) = transition.GetPositions();
			if (transition.destination == null) {
				end = MousePosition;
			}
			var startTangent = start;
			var endTangent = end;
			var bezierEndPosition = end;
			if (transition.IsLeftToRight()) {
				startTangent += Vector2.right * 50;
				endTangent += Vector2.left * 50;
				bezierEndPosition.x -= Transition.ARROW_SIZE;
			} else {
				startTangent += Vector2.left * 50;
				endTangent += Vector2.right * 50;
				bezierEndPosition.x += Transition.ARROW_SIZE;
			}
			var color = transition == _selection ? new Color(1, .5f, 0) : Color.white;
			for (var i = 0; i < 3; i++) {
				Handles.DrawBezier(start, bezierEndPosition, startTangent, endTangent, color, null, 2);
			}
			OnGUI_DrawMainArea_Transition_Arrow(transition, color);
		}

		void OnGUI_DrawMainArea_Transition_Arrow(Transition transition, Color color) {
			Rect rect;
			if (transition.destination == null) {
				rect = new Rect(
					MousePosition.x - Transition.ARROW_SIZE,
					MousePosition.y - Transition.ARROW_SIZE / 2,
					Transition.ARROW_SIZE, Transition.ARROW_SIZE
				);
			} else {
				rect = transition.GetArrowRect();
			}
			var texture = transition.IsLeftToRight() ? ArrowTextures[0] : ArrowTextures[1];
			var _color = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, texture);
			GUI.color = _color;
			if (transition.destination != null && transition.priority != 0) {
				if (transition.IsLeftToRight()) {
					GUI.Label(new Rect(rect.x - 32, rect.y - 23, 40, 20), transition.priority.ToString(), new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleRight
					});
				} else {
					GUI.Label(new Rect(rect.x + 2, rect.y - 23, 40, 20), transition.priority.ToString(), new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleLeft
					});
				}
			}
		}

		void OnGUI_MousePanning() {
			if (Event.current.button == 2) {
				Vector2 mousePosition = Event.current.mousePosition / _zoom;
				switch (Event.current.type) {
					case EventType.MouseDown:
						_lastScrollPosition = _targetScrollPosition;
						_lastMousePosition = mousePosition;
						break;
					case EventType.MouseDrag:
						_targetScrollPosition = _lastScrollPosition + _lastMousePosition - mousePosition;
						Event.current.Use();
						break;
				}
			}
			_targetScrollPosition.x = Mathf.Min(Mathf.Max(MainAreaRect.x, _targetScrollPosition.x), MainAreaRect.width);
			_targetScrollPosition.y = Mathf.Min(Mathf.Max(MainAreaRect.y, _targetScrollPosition.y), MainAreaRect.width);
			_scrollPosition = _targetScrollPosition;
		}

		void OnGUI_ShowRightClickMenu(SequenceFlow sequenceFlow) {
			var menu = new GenericMenu();
			if (_selection == null) {
				menu.AddItem(new GUIContent("Create State"), false, () => {
					sequenceFlow.Add(new State(MousePosition));
				});
			} else if (_selection is State selectedState) {
				menu.AddItem(new GUIContent("Make Transition"), false, () => {
					_newTransition = new Transition { source = selectedState };
				});
				menu.AddItem(new GUIContent("Remove State"), false, () => {
					sequenceFlow.Remove(selectedState);
					_selection = null;
				});
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Set as Start State"), false, () => {
					sequenceFlow.startState = selectedState;
				});
			} else if (_selection is Transition) {
				menu.AddItem(new GUIContent("Remove Transition"), false, () => {
					sequenceFlow.Remove(_selection as Transition);
					_selection = null;
				});
			}
			menu.ShowAsContext();
		}
	}
}
