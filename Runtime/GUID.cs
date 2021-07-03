using System;

namespace MT.Packages.SequenceFlow
{
	public abstract class GUID
	{
		[NonSerialized] public SequenceFlow sequenceFlow;
		public string guid;

		public void Setup(SequenceFlow sequenceFlow) {
			this.sequenceFlow = sequenceFlow;
			if (string.IsNullOrEmpty(guid)) {
				guid = Guid.NewGuid().ToString();
			}
		}
	}
}