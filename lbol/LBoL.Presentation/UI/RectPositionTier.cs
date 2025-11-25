using System;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	[ExecuteAlways]
	public class RectPositionTier : MonoBehaviour
	{
		public RectTransform TargetTransform { get; set; }
		public Vector2 TiedAnchor { get; set; }
		private void LateUpdate()
		{
			RectTransform targetTransform = this.TargetTransform;
			if (targetTransform)
			{
				Transform transform = base.transform;
				Rect rect = targetTransform.rect;
				Vector2 vector = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, this.TiedAnchor.x), Mathf.Lerp(rect.yMin, rect.yMax, this.TiedAnchor.y));
				transform.position = targetTransform.TransformPoint(vector);
				return;
			}
			Object.Destroy(this);
		}
		private void OnDrawGizmos()
		{
			RectTransform targetTransform = this.TargetTransform;
			if (targetTransform)
			{
				Transform transform = base.transform;
				Rect rect = targetTransform.rect;
				Vector2 vector = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, this.TiedAnchor.x), Mathf.Lerp(rect.yMin, rect.yMax, this.TiedAnchor.y));
				Vector3 vector2 = targetTransform.TransformPoint(vector);
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(vector2 + Vector3.left, vector2 + Vector3.right);
				Gizmos.DrawLine(vector2 + Vector3.down, vector2 + Vector3.up);
			}
		}
	}
}
