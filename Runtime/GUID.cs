using System;

namespace MT.Packages.SequenceFlow
{
	public abstract class GUID
	{
#if !MT_PACKAGES_PROJECT
		[NonSerialized] public SequenceFlow sequenceFlow;
		public string guid;

		public void Setup(SequenceFlow sequenceFlow) {
			this.sequenceFlow = sequenceFlow;
			if (string.IsNullOrEmpty(guid)) {
				guid = Guid.NewGuid().ToString();
			}
		}
#endif
	}
}