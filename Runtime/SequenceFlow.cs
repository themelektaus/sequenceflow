using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [System.Serializable]
    public class SequenceFlow
    {
        [System.NonSerialized]
        public SequenceFlowObject sequenceFlowObject;

        [System.NonSerialized]
        public State startState;

        public string startStateGUID;
        public List<State> states = new();
        public List<Transition> transitions = new();

        [System.NonSerialized]
        public State currentState;

        [System.NonSerialized]
        public Transform activator;

        [System.NonSerialized]
        public MonoBehaviour owner;

        public bool Running => currentState is not null;

        public void Add(State state)
        {
            state.Setup(this);

            sequenceFlowObject.stateSequences.Add(new());

            states.Add(state);

            if (!states.Contains(startState))
                startState = states.FirstOrDefault();
        }

        public void Remove(State state)
        {
            for (var i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].source == state || transitions[i].destination == state)
                {
                    sequenceFlowObject.transitionStatements.Remove(transitions[i].GetStatement());
                    transitions.Remove(transitions[i]);
                    i--;
                }
            }
            sequenceFlowObject.stateSequences.RemoveAt(states.IndexOf(state));

            states.Remove(state);

            if (!states.Contains(startState))
                startState = states.FirstOrDefault();
        }

        public void Add(Transition transition)
        {
            transition.Setup(this);

            sequenceFlowObject.transitionStatements.Add(new());

            transitions.Add(transition);
        }

        public void Remove(Transition transition)
        {
            sequenceFlowObject.transitionStatements.RemoveAt(transitions.IndexOf(transition));
            transitions.Remove(transition);
        }

        public IEnumerator Start(Transform activator, MonoBehaviour owner, EventArgs e, SimpleData parameters)
        {
            if (Running)
                yield break;

            currentState = startState;
            this.activator = activator;
            this.owner = owner;
            
            do
            {
                var sequence = currentState.GetSequence();
                sequence.Start(activator, owner, e, parameters);

                yield return new WaitWhile(() => sequence.isRunning);

                sequence.Exit();

                if (currentState is not null)
                    currentState = currentState.GetNext(e, parameters);

            } while (currentState is not null);
        }

        public void Abort()
        {
            if (currentState is null)
                return;

            currentState.GetSequence()?.Abort();
            currentState = null;
        }
    }
}
