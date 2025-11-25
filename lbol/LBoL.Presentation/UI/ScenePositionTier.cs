using System;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	[ExecuteAlways]
	public class ScenePositionTier : MonoBehaviour
	{
		public Transform TargetTransform { get; set; }
		private void OnEnable()
		{
			this._parent = (RectTransform)base.transform.parent;
		}
		private void LateUpdate()
		{
			if (this.TargetTransform)
			{
				base.transform.localPosition = CameraController.ScenePositionToLocalPositionInRectTransform(this.TargetTransform.position, this._parent);
				return;
			}
			Object.Destroy(this);
		}
		private RectTransform _parent;
	}
}
