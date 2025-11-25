using System;
using LBoL.Base.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.Widgets
{
	public class CurveLine : MonoBehaviour
	{
		public void SetLine(Vector3 root, Vector3 target)
		{
			Vector2 vector = new Vector2(0.3f.Lerp(root.x, target.x), target.y);
			Vector2 vector2 = new Vector2(0.6f.Lerp(root.x, target.x), target.y);
			this.line.Points = new Vector2[] { root, vector, vector2, target };
			float num = Vector2.SignedAngle(Vector2.right, target - vector2);
			this.arrow.transform.localPosition = target;
			this.arrow.transform.localRotation = Quaternion.Euler(0f, 0f, num);
		}
		[SerializeField]
		private Image arrow;
		[SerializeField]
		private UILineRenderer line;
	}
}
