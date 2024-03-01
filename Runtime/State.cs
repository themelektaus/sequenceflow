using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [System.Serializable]
    public class State : Guid
    {
        const float PIXEL_SIZE = .1f;

        public string name;

        [SerializeField] int x;
        [SerializeField] int y;

        public State() : base()
        {

        }

        public State(float x, float y) : base()
        {
            SetPosition(x, y);
        }

        public State(Vector2 position) : base()
        {
            SetPosition(position);
        }

        public int GetSequenceIndex()
        {
            return sequenceFlow.states.IndexOf(this);
        }

        public Sequence GetSequence()
        {
            return sequenceFlow.sequenceFlowObject.stateSequences[GetSequenceIndex()];
        }

        public IEnumerable<Transition> GetInputTransitions()
        {
            return sequenceFlow.transitions.Where(x => x.destination == this).OrderByDescending(x => x.source.GetY());
        }

        public IEnumerable<Transition> GetOutputTransitions()
        {
            return sequenceFlow.transitions.Where(x => x.source == this);
        }

        public State GetNext(EventArgs e)
        {
            var transitions = sequenceFlow.transitions.Where(t =>
            {
                if (t.source == this)
                {
                    var statement = t.GetStatement();

                    if (statement is null)
                        return true;

                    if (statement.Check(sequenceFlow.activator, sequenceFlow.owner, e))
                        return true;
                }

                return false;

            }).ToArray();

            if (transitions.Length == 0)
                return null;

            if (transitions.Length == 1)
                return transitions[0].destination;

            var maxPriority = transitions.Max(t => t.priority);
            
            transitions = transitions.Where(t => t.priority == maxPriority).ToArray();

            if (transitions.Length == 1)
                return transitions[0].destination;

            return transitions
                .OrderBy(t => t.destination.GetY())
                .FirstOrDefault()
                .destination;
        }

        public int GetX() => x;

        public int GetY() => y;

        public Vector2 GetPosition()
        {
            return new Vector2(x * PIXEL_SIZE, y * PIXEL_SIZE);
        }

        public void SetPosition(Vector2 position)
        {
            SetPosition(position.x, position.y);
        }

        public void SetPosition(float x, float y)
        {
            this.x = Mathf.RoundToInt(x / PIXEL_SIZE);
            this.y = Mathf.RoundToInt(y / PIXEL_SIZE);
        }
    }
}
