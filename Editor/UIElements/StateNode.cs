#if !MT_PACKAGES_PROJECT
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MT.Packages.SequenceFlow.Editor.UIElements
{
	public class StateNode : Node
	{
		public State state;
		public SequenceFlowPort input;
		public SequenceFlowPort output;
		public Core.Editor.UIElements.SequenceView sequenceView;

		public StateNode(State state) {
			this.state = state;

			sequenceView = new Core.Editor.UIElements.SequenceView {
				getSequence = () => state.GetSequence(),
				getSerializedCommands = () => {
					var serializedSequenceFlowObject = new SerializedObject(state.sequenceFlow.sequenceFlowObject);
					var serializedStateSequences = serializedSequenceFlowObject.FindProperty("stateSequences");
					var serializedStateSequence = serializedStateSequences.GetArrayElementAtIndex(state.GetSequenceIndex());
					return serializedStateSequence.FindPropertyRelative("commands");
				}
			};
			sequenceView.Refresh();

			inputContainer.parent.parent.Add(sequenceView);
			sequenceView.parent.style.backgroundColor = new Color(.216f, .216f, .216f, .78f);

			input = new SequenceFlowPort(Direction.Input, Port.Capacity.Multi);
			output = new SequenceFlowPort(Direction.Output, Port.Capacity.Multi);
			inputContainer.Add(input);
			outputContainer.Add(output);

			if (SequenceFlowEditorSettings.asset.portPosition == SequenceFlowEditorSettings.PortPosition.Side) {
				var parent1 = inputContainer.parent;
				var parent2 = parent1.parent;
				parent2.Remove(parent1);
				parent2.style.flexDirection = FlexDirection.Row;

				var inputElement = parent1.Q("input");
				inputElement.Q<Label>().style.display = DisplayStyle.None;

				var outputElement = parent1.Q("output");
				outputElement.Q<Label>().style.display = DisplayStyle.None;

				parent2.Insert(0, inputElement);
				parent2.Add(outputElement);

				parent2.AddToClassList("smaller");

			} else if (SequenceFlowEditorSettings.asset.portPosition == SequenceFlowEditorSettings.PortPosition.Bottom) {
				var parent1 = inputContainer.parent;
				var parent2 = parent1.parent;
				parent2.Remove(parent1);
				parent2.Add(parent1);
			}

			SetPosition(new Rect(state.GetPosition(), Vector2.zero));
			RefreshTitle();

			var detailViewButton = new Button { text = "◇" };
			detailViewButton.AddToClassList("DetailViewButton");
			detailViewButton.clicked += () => {
				if (sequenceView.detailViewEnabled) {
					sequenceView.detailViewEnabled = false;
					detailViewButton.text = "◇";
				} else {
					sequenceView.detailViewEnabled = true;
					detailViewButton.text = "◈";
				}
				sequenceView.Refresh();
			};
			titleButtonContainer.Add(detailViewButton);
		}

		public void RefreshTitle() {
			title = (state.sequenceFlow.startState == state ? "START " : "") + state.name + " ";
		}
	}
}
#endif