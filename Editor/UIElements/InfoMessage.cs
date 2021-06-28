using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MT.Packages.SequenceFlow.Editor.UIElements
{
	public class InfoMessage : VisualElement
	{
		readonly Label messageLabel;

		Core.Editor.EditorJob currentJob;

		public InfoMessage() {
			style.position = Position.Absolute;
			style.width = 400;
			style.height = 30;
			style.bottom = 10;
			style.right = 10;
			messageLabel = new Label { style = { unityTextAlign = TextAnchor.MiddleRight } };
			Add(messageLabel);
		}

		public void Show(string message) {
			if (currentJob != null && currentJob.IsRunning) {
				currentJob.Cancel();
				currentJob = null;
			}
			currentJob = Core.Editor.EditorJob.Run(GetShowProcess(message));
		}

		IEnumerator GetShowProcess(string message) {
			messageLabel.text = "";
			var waitCommand = Core.Editor.EditorJob.WaitCommand(.2f);
			while (Core.Editor.EditorJob.WaitFor(waitCommand)) {
				yield return null;
			}
			messageLabel.text = message;
			waitCommand = Core.Editor.EditorJob.WaitCommand(1.3f);
			while (Core.Editor.EditorJob.WaitFor(waitCommand)) {
				yield return null;
			}
			messageLabel.text = "";
		}
	}
}