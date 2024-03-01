using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class StateNode : Node
    {
        public readonly State state;
        public readonly SequenceFlowPort input;
        public readonly SequenceFlowPort output;
        public readonly SequenceView sequenceView;

        public StateNode(State state)
        {
            this.state = state;

            sequenceView = new()
            {
                getSequence = () => state.GetSequence(),
                getSerializedCommands = () =>
                {
                    var serializedSequenceFlowObject = new SerializedObject(state.sequenceFlow.sequenceFlowObject);
                    var serializedStateSequences = serializedSequenceFlowObject.FindProperty("stateSequences");
                    var serializedStateSequence = serializedStateSequences.GetArrayElementAtIndex(state.GetSequenceIndex());
                    return serializedStateSequence.FindPropertyRelative("commands");
                },
                parameters = SequenceFlowWindow.parameters
            };
            sequenceView.Refresh();

            inputContainer.parent.parent.Add(sequenceView);
            sequenceView.parent.style.backgroundColor = new Color(.216f, .216f, .216f, .78f);

            input = new(Direction.Input, Port.Capacity.Multi);
            output = new(Direction.Output, Port.Capacity.Multi);
            inputContainer.Add(input);
            outputContainer.Add(output);

            if (!SequenceFlowEditorSettings.instance)
                SequenceFlowEditorSettings.Load();

            VisualElement parent1, parent2;

            switch (SequenceFlowEditorSettings.instance.portPosition)
            {
                case SequenceFlowEditorSettings.PortPosition.Bottom:
                    parent1 = inputContainer.parent;
                    parent2 = parent1.parent;
                    parent2.Remove(parent1);
                    parent2.Add(parent1);
                    break;
            }

            SetPosition(new Rect(state.GetPosition(), Vector2.zero));
            RefreshTitle();
        }

        public void RefreshTitle()
        {
            title = (state.sequenceFlow.startState == state ? "START " : "") + state.name + " ";
        }
    }
}
