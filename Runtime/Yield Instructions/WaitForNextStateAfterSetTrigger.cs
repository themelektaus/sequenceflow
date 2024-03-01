using UnityEngine;

namespace Prototype.SequenceFlow
{
	public class WaitForNextStateAfterSetTrigger : CustomYieldInstruction
	{
		public override bool keepWaiting {
			get {
				if (!targetTime.HasValue) {
					var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
					if (currentStateInfo.fullPathHash == stateInfo.fullPathHash) {
						return true;
					}
					targetTime = Time.time + (stateInfo.length + stateInfo.normalizedTime) * multiplier;
				}
				return targetTime.Value > Time.time;
			}
		}

		readonly Animator animator;
		readonly AnimatorStateInfo currentStateInfo;

		float multiplier;
		float? targetTime;

		public WaitForNextStateAfterSetTrigger(Animator animator, string triggerName) : this(animator, triggerName, 1) {

		}

		public WaitForNextStateAfterSetTrigger(Animator animator, string triggerName, float multiplier) {
			this.animator = animator;
			animator.SetTrigger(triggerName);
			currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			WithMultiplier(multiplier);
		}

		public WaitForNextStateAfterSetTrigger WithMultiplier(float multiplier) {
			this.multiplier = multiplier;
			return this;
		}
	}
}