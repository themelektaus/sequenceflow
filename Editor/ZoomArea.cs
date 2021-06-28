using UnityEngine;

namespace MT.Packages.SequenceFlow.Editor
{
	public class ZoomArea
	{
		static Matrix4x4 _matrix;

		public static Rect Begin(float zoom, Rect rect) {
			GUI.EndGroup();
			var pivotPoint = new Vector2(rect.xMin, rect.yMin);
			var clippedRect = rect;
			clippedRect.x -= pivotPoint.x;
			clippedRect.y -= pivotPoint.y;
			clippedRect.xMin *= 1.0f / zoom;
			clippedRect.xMax *= 1.0f / zoom;
			clippedRect.yMin *= 1.0f / zoom;
			clippedRect.yMax *= 1.0f / zoom;
			clippedRect.x += pivotPoint.x;
			clippedRect.y += pivotPoint.y;
			clippedRect.y += 21;
			GUI.BeginGroup(clippedRect);
			_matrix = GUI.matrix;
			var translation = Matrix4x4.TRS(new Vector2(clippedRect.xMin, clippedRect.yMin), Quaternion.identity, Vector3.one);
			var scale = Matrix4x4.Scale(new Vector3(zoom, zoom, 1.0f));
			GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
			return clippedRect;
		}

		public static void ScrollPositionCorrection(ref Vector2 scrollPosition, Vector2 mousePosition, float oldZoom, float newZoom) {
			scrollPosition.x -= (mousePosition.x - scrollPosition.x) / newZoom * (oldZoom - newZoom);
			scrollPosition.y -= (mousePosition.y - scrollPosition.y) / newZoom * (oldZoom - newZoom);
		}

		public static void End() {
			GUI.matrix = _matrix;
			GUI.EndGroup();
			GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));
		}
	}
}
