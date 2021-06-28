using MT.Packages.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MT.Packages.SequenceFlow
{
	[System.Serializable]
	public class State : GUID
	{
		const float PIXEL_SIZE = .1f;

		public string name;

		[SerializeField] int x;
		[SerializeField] int y;

		public State() : base() {

		}

		public State(float x, float y) : base() {
			SetPosition(x, y);
		}

		public State(Vector2 position) : base() {
			SetPosition(position);
		}

		public override string ToString() {
			if (!string.IsNullOrWhiteSpace(name)) {
				return name;
			}
			return GetSequence().ToString();
		}

		public int GetSequenceIndex() {
			return sequenceFlow.states.IndexOf(this);
		}

		public Sequence GetSequence() {
			return sequenceFlow.sequenceFlowObject.stateSequences[GetSequenceIndex()];
		}

		public IEnumerable<Transition> GetInputTransitions() {
			return sequenceFlow.transitions.Where(x => x.destination == this).OrderByDescending(x => x.source.GetY());
		}

		public IEnumerable<Transition> GetOutputTransitions() {
			return sequenceFlow.transitions.Where(x => x.source == this);
		}

		public State GetNext(EventSystem.EventArgs e) {
			var transitions = sequenceFlow.transitions.Where(t => {
				if (t.source == this) {
					var statement = t.GetStatement();
					if (statement == null || statement.Check(sequenceFlow.activator, sequenceFlow._self, e)) {
						return true;
					}
				}
				return false;
			}).ToArray();
			if (transitions.Length == 0) {
				return null;
			}
			if (transitions.Length == 1) {
				return transitions[0].destination;
			}
			var maxPriority = transitions.Max(t => t.priority);
			transitions = transitions.Where(t => t.priority == maxPriority).ToArray();
			if (transitions.Length == 1) {
				return transitions[0].destination;
			}
			return transitions.OrderBy(t => t.destination.GetY()).FirstOrDefault().destination;
		}

#if UNITY_EDITOR
		public int GetX() => x;

		public int GetY() => y;

		public Vector2 GetPosition() {
			return new Vector2(x * PIXEL_SIZE, y * PIXEL_SIZE);
		}

		public void SetPosition(Vector2 position) {
			SetPosition(position.x, position.y);
		}

		public void SetPosition(float x, float y) {
			this.x = Mathf.RoundToInt(x / PIXEL_SIZE);
			this.y = Mathf.RoundToInt(y / PIXEL_SIZE);
		}

		[System.Obsolete]
		public Rect GetRect() {
			var empty = true;
			var height = 45;
			var sequence = GetSequence();
			if (sequence != null && sequence.commands != null) {
				foreach (var command in sequence.commands) {
					empty = false;
					height += 37;
					if (command.method != null) {
						foreach (var parameter in Utility.GetParameters(typeof(Sequence), typeof(IEnumerator), command.method.name)) {
							height += 20;
							if (parameter.HasAttribute<Core.Attributes.BigTextAttribute>()) {
								height += Core.Attributes.BigTextAttribute.BIG_TEXT_EXTRA_HEIGHT;
							}
						}
					}
				}
			}
			var width = empty ? 90 : 460;
			if (!empty && sequence.collapsed) {
				width = 300;
				height = 20 + sequence.commands.Length * 18;
			}
			return new Rect(GetPosition(), new Vector2(width, empty ? 60 : height));
		}
#endif
	}
}