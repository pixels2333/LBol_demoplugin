using System;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI
{
	// Token: 0x02000026 RID: 38
	[RequireComponent(typeof(RawImage))]
	public sealed class RawImageUvTweener : MonoBehaviour
	{
		// Token: 0x06000322 RID: 802 RVA: 0x0000DCAA File Offset: 0x0000BEAA
		private void Awake()
		{
			this._rawImage = base.GetComponent<RawImage>();
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000DCB8 File Offset: 0x0000BEB8
		private void Update()
		{
			Rect uvRect = this._rawImage.uvRect;
			uvRect.position += (this.timeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime) * this.speed;
			this._rawImage.uvRect = uvRect;
		}

		// Token: 0x04000166 RID: 358
		[SerializeField]
		private Vector2 speed;

		// Token: 0x04000167 RID: 359
		[SerializeField]
		private bool timeScaleIndependent;

		// Token: 0x04000168 RID: 360
		private RawImage _rawImage;
	}
}
