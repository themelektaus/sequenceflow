using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class SequenceFlowGraphView : GraphView
    {
        protected override bool canCutSelection => false;
        protected override bool canCopySelection => false;
        protected override bool canPaste => false;
        protected override bool canDuplicateSelection => false;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent e)
        {
            if (e.target is GraphView)
            {
                e.StopPropagation();
                e.menu.AppendAction("Create state", a => CreateState(a.eventInfo.localMousePosition));
                return;
            }

            if (e.target is StateNode node)
            {
                e.menu.AppendAction("Set as Start", a => SetAsStart(node));
                e.menu.AppendAction("Reset Position", a => ResetPosition(node));
                return;
            }

            base.BuildContextualMenu(e);
        }

        SequenceFlow sequenceFlow;
        VisualElement transitionSettingsView;

        public SequenceFlowGraphView()
        {
            SetupZoom(0.2f, ContentZoomer.DefaultMaxScale * 2);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            graphViewChanged += OnGraphChanged;
            RegisterCallback<GeometryChangedEvent>(e => FrameAll());
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            OnSelectionChanged();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            OnSelectionChanged();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            OnSelectionChanged();
        }

        void OnSelectionChanged()
        {
            var edges = selection.Where(x => x is TransitionEdge).Cast<TransitionEdge>().ToList();

            if (edges.Count == 1)
            {
                ShowTransitionSettingsView(edges[0].transition);
                return;
            }

            HideTransitionSettingsView();
        }

        public override EventPropagation DeleteSelection()
        {
            HideTransitionSettingsView();
            return base.DeleteSelection();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var result = new List<Port>();

            ports.ForEach(endPort =>
            {
                if (startPort != endPort && startPort.node != endPort.node)
                    if (startPort.direction != endPort.direction)
                        result.Add(endPort);
            });

            return result;
        }

        public void Refresh(SequenceFlow sequenceFlow)
        {
            graphElements.ForEach(RemoveElement);

            this.sequenceFlow = sequenceFlow;

            if (sequenceFlow is null)
                return;

            foreach (var state in sequenceFlow.states)
                AddElement(new StateNode(state));

            nodes.ForEach(node =>
            {
                var sourceNode = node as StateNode;

                foreach (var outputTransition in sourceNode.state.GetOutputTransitions())
                {
                    var destinationNode = GetNode(outputTransition.destination);
                    var edge = new TransitionEdge(outputTransition)
                    {
                        output = sourceNode.output,
                        input = destinationNode.input
                    };
                    AddElement(edge);
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);

                    var conditions = outputTransition.GetStatement().conditions;
                    if (conditions is not null && conditions.Length > 0)
                    {
                        edge.input.AddToClassList("has-conditions");
                        continue;
                    }

                    edge.input.AddToClassList("has-no-conditions");
                }
            });
        }

        public void CreateState(Vector2 position)
        {
            var wb = contentViewContainer.worldBound;

            var state = new State(new Vector2(
                position.x + wb.x * -1,
                position.y + wb.y * -1
            ) * (worldBound.width / wb.width));

            sequenceFlow.Add(state);

            AddElement(new StateNode(state));
        }

        public void SetAsStart(StateNode node)
        {
            sequenceFlow.startState = node.state;
            nodes.ForEach(n => (n as StateNode)?.RefreshTitle());
        }

        public void ResetPosition(StateNode node)
        {
            node.state.SetPosition(0, 0);
            node.SetPosition(Rect.zero);
        }

        StateNode GetNode(State state)
        {
            foreach (StateNode node in nodes.ToList())
                if (node.state == state)
                    return node;

            return null;
        }

        GraphViewChange OnGraphChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove is not null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is StateNode node)
                        node.state.sequenceFlow.Remove(node.state);

                    if (element is TransitionEdge edge)
                        edge.transition.sequenceFlow.Remove(edge.transition);
                }
            }

            if (graphViewChange.movedElements is not null)
            {
                foreach (GraphElement element in graphViewChange.movedElements)
                {
                    if (element is StateNode node)
                    {
                        var rect = node.GetPosition();
                        node.state.SetPosition(rect.position);
                        node.SetPosition(new Rect(node.state.GetPosition(), Vector2.zero));
                    }
                }
            }

            if (graphViewChange.edgesToCreate is not null)
            {
                for (var i = 0; i < graphViewChange.edgesToCreate.Count; i++)
                {
                    var edge = graphViewChange.edgesToCreate[i];
                    if (edge is TransitionEdge)
                        continue;

                    var transition = new Transition
                    {
                        source = (edge.output.node as StateNode).state,
                        destination = (edge.input.node as StateNode).state
                    };

                    sequenceFlow.Add(transition);

                    var sequenceFlowEdge = new TransitionEdge(transition)
                    {
                        output = edge.output,
                        input = edge.input
                    };

                    sequenceFlowEdge.input.Connect(sequenceFlowEdge);
                    sequenceFlowEdge.output.Connect(sequenceFlowEdge);
                    graphViewChange.edgesToCreate[i] = sequenceFlowEdge;
                }
            }

            return graphViewChange;
        }

        public void AddTransitionSettingsViewTo(VisualElement parent)
        {
            transitionSettingsView = new VisualElement { name = "TransitionSettings" };
            transitionSettingsView.Add(new VisualElement { name = "Content" });
            parent.Add(transitionSettingsView);

            HideTransitionSettingsView();
        }

        public void RemoveTransitionSettings()
        {
            if (transitionSettingsView.parent is not null)
                transitionSettingsView.parent.Remove(transitionSettingsView);

            transitionSettingsView = null;
        }

        public void ShowTransitionSettingsView(Transition transition)
        {
            HideTransitionSettingsView();

            transitionSettingsView.Q("Content").Clear();

            var statementView = new StatementView()
            {
                getStatement = () => transition.GetStatement(),
                getSerializedStatement = () =>
                {
                    var serializedSequenceFlowObject = new SerializedObject(transition.sequenceFlow.sequenceFlowObject);
                    var serializedTransitionStatements = serializedSequenceFlowObject.FindProperty("transitionStatements");
                    return serializedTransitionStatements.GetArrayElementAtIndex(transition.GetStatementIndex());
                },
                parameters = SequenceFlowWindow.parameters
            };

            statementView.Refresh();

            transitionSettingsView.Q("Content").Add(statementView);
            transitionSettingsView.style.display = DisplayStyle.Flex;
        }

        public void HideTransitionSettingsView()
        {
            transitionSettingsView.style.display = DisplayStyle.None;
            transitionSettingsView.Q("Content").Clear();
        }
    }
}
