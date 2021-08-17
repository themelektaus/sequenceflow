#if !MT_PACKAGES_PROJECT
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MT.Packages.SequenceFlow
{
    [System.Serializable]
    public class SequenceFlow
    {
        [System.NonSerialized] public SequenceFlowObject sequenceFlowObject;
        [System.NonSerialized] public State startState;

        public string startStateGUID;
        public List<State> states = new List<State>();
        public List<Transition> transitions = new List<Transition>();

        [System.NonSerialized] public State currentState;
        [System.NonSerialized] public Transform activator;
        [System.NonSerialized] public MonoBehaviour _self;

        [HideInInspector, System.NonSerialized] public Core.SimpleData simpleData = new Core.SimpleData();

        public bool Running { get { return currentState != null; } }

        public void Add(State state) {
            state.Setup(this);
            sequenceFlowObject.stateSequences.Add(new Core.Sequence());
            states.Add(state);
            if (!states.Contains(startState)) {
                startState = states.FirstOrDefault();
            }
        }

        public void Remove(State state) {
            for (var i = 0; i < transitions.Count; i++) {
                if (transitions[i].source == state || transitions[i].destination == state) {
                    sequenceFlowObject.transitionStatements.Remove(transitions[i].GetStatement());
                    transitions.Remove(transitions[i]);
                    i--;
                }
            }
            sequenceFlowObject.stateSequences.RemoveAt(states.IndexOf(state));
            states.Remove(state);
            if (!states.Contains(startState)) {
                startState = states.FirstOrDefault();
            }
        }

        public void Add(Transition transition) {
            transition.Setup(this);
            sequenceFlowObject.transitionStatements.Add(new Core.Statement());
            transitions.Add(transition);
        }

        public void Remove(Transition transition) {
            sequenceFlowObject.transitionStatements.RemoveAt(transitions.IndexOf(transition));
            transitions.Remove(transition);
        }

        public IEnumerator Start(Transform activator, MonoBehaviour _self, Core.SimpleData parameters, EventSystem.EventArgs e) {
            if (Running) {
                yield break;
            }
            currentState = startState;
            this.activator = activator;
            this._self = _self;
            do {
                var sequence = currentState.GetSequence();
                sequence.Start(activator, _self, parameters, e);
                while (sequence.Running) {
                    yield return null;
                }
                if (currentState != null) {
                    currentState = currentState.GetNext(e);
				}
            } while (currentState != null);
        }

        public void Abort() {
            if (currentState != null) {
                var sequence = currentState.GetSequence();
                if (sequence != null) {
                    sequence.Abort();
			    }
                currentState = null;
			}
        }
    }
}
#endif