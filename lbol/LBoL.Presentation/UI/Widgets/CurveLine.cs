using System;
using LBoL.Base.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200004F RID: 79
	public class CurveLine : MonoBehaviour
	{
		// Token: 0x060004AD RID: 1197 RVA: 0x000134B4 File Offset: 0x000116B4
		public void SetLine(Vector3 root, Vector3 target)
		{
			Vector2 vector = new Vector2(0.3f.Lerp(root.x, target.x), target.y);
			Vector2 vector2 = new Vector2(0.6f.Lerp(root.x, target.x), target.y);
			this.line.Points = new Vector2[] { root, vector, vector2, target };
			float num = Vector2.SignedAngle(Vector2.right, target - vector2);
			this.arrow.transform.localPosition = target;
			this.arrow.transform.localRotation = Quaternion.Euler(0f, 0f, num);
		}

		// Token: 0x0400027D RID: 637
		[SerializeField]
		private Image arrow;

		// Token: 0x0400027E RID: 638
		[SerializeField]
		private UILineRenderer line;
	}
}
