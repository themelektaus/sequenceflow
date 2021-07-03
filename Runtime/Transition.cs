using System.Linq;
using UnityEngine;

namespace MT.Packages.SequenceFlow
{
	[System.Serializable]
	public class Transition : GUID
	{
		[System.NonSerialized] public State source;
		[System.NonSerialized] public State destination;

		public const int ARROW_SIZE = 12;

		public int priority;
		public string sourceGUID;
		public string destinationGUID;

		public int GetStatementIndex() {
#if MT_PACKAGES_PROJECT
			return -1;
#else
			return sequenceFlow.transitions.IndexOf(this);
#endif
		}

		public Core.Statement GetStatement() {
#if MT_PACKAGES_PROJECT
			return null;
#else
			return sequenceFlow.sequenceFlowObject.transitionStatements[GetStatementIndex()];
#endif
		}

#if UNITY_EDITOR

		[System.Obsolete]
		public bool IsLeftToRight() {
			if (source == null || destination == null) {
				return true;
			}
			return source.GetX() <= destination.GetX();
		}

		[System.Obsolete]
		public (Vector2 start, Vector2 end) GetPositions() {
			var sourceRect = source == null ? Rect.zero : source.GetRect();
			var destinationRect = destination == null ? Rect.zero : destination.GetRect();
			(Vector2 start, Vector2 end) result;
			if (IsLeftToRight()) {
				result = (
					new Vector2(sourceRect.x + sourceRect.width, sourceRect.y + sourceRect.height / 2),
					new Vector2(destinationRect.x, destinationRect.y + destinationRect.height / 2)
				);
			} else {
				result = (
					new Vector2(sourceRect.x, sourceRect.y + sourceRect.height / 2),
					new Vector2(destinationRect.x + destinationRect.width, destinationRect.y + destinationRect.height / 2)
				);
			}
			var index = 0;
			if (destination != null) {
				index = destination.GetInputTransitions().ToList().IndexOf(this);
			}
			result.end.y -= (ARROW_SIZE * 1.25f) * (index + 1) - ARROW_SIZE / 2;
			return result;
		}

		[System.Obsolete]
		public Rect GetArrowRect() {
			var size = new Vector2(ARROW_SIZE, ARROW_SIZE);
			Vector2 position = GetPositions().end;
			if (IsLeftToRight()) {
				position -= new Vector2(ARROW_SIZE, ARROW_SIZE / 2);
			} else {
				position += new Vector2(0, -ARROW_SIZE / 2);
			}
			return new Rect(position, size);
		}
#endif
	}
}