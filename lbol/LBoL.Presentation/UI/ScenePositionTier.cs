using System;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000028 RID: 40
	[ExecuteAlways]
	public class ScenePositionTier : MonoBehaviour
	{
		// Token: 0x17000083 RID: 131
		// (get) Token: 0x0600032C RID: 812 RVA: 0x0000DE81 File Offset: 0x0000C081
		// (set) Token: 0x0600032D RID: 813 RVA: 0x0000DE89 File Offset: 0x0000C089
		public Transform TargetTransform { get; set; }

		// Token: 0x0600032E RID: 814 RVA: 0x0000DE92 File Offset: 0x0000C092
		private void OnEnable()
		{
			this._parent = (RectTransform)base.transform.parent;
		}

		// Token: 0x0600032F RID: 815 RVA: 0x0000DEAA File Offset: 0x0000C0AA
		private void LateUpdate()
		{
			if (this.TargetTransform)
			{
				base.transform.localPosition = CameraController.ScenePositionToLocalPositionInRectTransform(this.TargetTransform.position, this._parent);
				return;
			}
			Object.Destroy(this);
		}

		// Token: 0x0400016C RID: 364
		private RectTransform _parent;
	}
}
