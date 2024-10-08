﻿using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

namespace Prototype.SequenceFlow.Editor
{
    public class SequenceFlowPort : Port
    {
        public SequenceFlowPort(
            Direction portDirection,
            Capacity portCapacity
        ) : base(
            Orientation.Horizontal,
            portDirection,
            portCapacity,
            null
        )
        {
            m_ConnectorText.text = portDirection == Direction.Input ? "Entry" : "Exit";
            m_EdgeConnector = new EdgeConnector<Edge>(new DefaultEdgeConnectorListener());

            this.AddManipulator(m_EdgeConnector);
        }

        class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            GraphViewChange m_GraphViewChange;
            List<Edge> m_EdgesToCreate;
            List<GraphElement> m_EdgesToDelete;

            public DefaultEdgeConnectorListener()
            {
                m_EdgesToCreate = new List<Edge>();
                m_EdgesToDelete = new List<GraphElement>();
                m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {

            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_EdgesToCreate.Clear();
                m_EdgesToCreate.Add(edge);
                m_EdgesToDelete.Clear();

                if (edge.input.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.input.connections)
                        if (edgeToDelete != edge)
                            m_EdgesToDelete.Add(edgeToDelete);

                if (edge.output.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.output.connections)
                        if (edgeToDelete != edge)
                            m_EdgesToDelete.Add(edgeToDelete);

                if (m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements(m_EdgesToDelete);

                var edgesToCreate = m_EdgesToCreate;

                if (graphView.graphViewChanged is not null)
                    edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;

                foreach (var newEdge in edgesToCreate)
                {
                    graphView.AddElement(newEdge);
                    edge.input.Connect(newEdge);
                    edge.output.Connect(newEdge);
                }
            }
        }
    }
}
