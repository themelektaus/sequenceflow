using UnityEngine;

namespace MT.Packages.SequenceFlow
{
	public class SequenceFlowEditorSettings : ScriptableObject
	{
		static SequenceFlowEditorSettings _asset;
		public static SequenceFlowEditorSettings asset => Core.Utility.Get(ref _asset, "Sequence Flow", "Sequence Flow Editor Settings");

		public enum PortPosition { Top, Bottom, Side }

		public PortPosition portPosition = PortPosition.Top;
	}
}