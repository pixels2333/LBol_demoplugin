using System;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000027 RID: 39
	[ExecuteAlways]
	public class RectPositionTier : MonoBehaviour
	{
		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000325 RID: 805 RVA: 0x0000DD16 File Offset: 0x0000BF16
		// (set) Token: 0x06000326 RID: 806 RVA: 0x0000DD1E File Offset: 0x0000BF1E
		public RectTransform TargetTransform { get; set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000327 RID: 807 RVA: 0x0000DD27 File Offset: 0x0000BF27
		// (set) Token: 0x06000328 RID: 808 RVA: 0x0000DD2F File Offset: 0x0000BF2F
		public Vector2 TiedAnchor { get; set; }

		// Token: 0x06000329 RID: 809 RVA: 0x0000DD38 File Offset: 0x0000BF38
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

		// Token: 0x0600032A RID: 810 RVA: 0x0000DDBC File Offset: 0x0000BFBC
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
