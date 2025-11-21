using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x020000FE RID: 254
	public class EffectUIBulletView : EffectBulletView
	{
		// Token: 0x06000E3D RID: 3645 RVA: 0x00043BFB File Offset: 0x00041DFB
		protected override void Refresh()
		{
			RectTransform component = base.GetComponent<RectTransform>();
			component.position = base.EffectBullet.Position;
			component.localRotation = base.EffectBullet.Rotation;
		}
	}
}
