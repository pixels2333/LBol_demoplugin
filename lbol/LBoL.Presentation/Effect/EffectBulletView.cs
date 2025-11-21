using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x020000FC RID: 252
	public class EffectBulletView : MonoBehaviour
	{
		// Token: 0x1700025E RID: 606
		// (get) Token: 0x06000E28 RID: 3624 RVA: 0x000436D9 File Offset: 0x000418D9
		// (set) Token: 0x06000E29 RID: 3625 RVA: 0x000436E1 File Offset: 0x000418E1
		public EffectBullet EffectBullet { get; set; }

		// Token: 0x06000E2A RID: 3626 RVA: 0x000436EA File Offset: 0x000418EA
		public void SetEffectBullet(EffectBullet eb)
		{
			this.EffectBullet = eb;
			eb.EffectBulletView = this;
			eb.Calculation();
			this.Refresh();
			base.name = eb.EffectName + "(EffectBullet)";
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x0004371C File Offset: 0x0004191C
		private void LateUpdate()
		{
			this.Refresh();
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x00043724 File Offset: 0x00041924
		protected virtual void Refresh()
		{
			Transform transform = base.transform;
			transform.localPosition = this.EffectBullet.Position;
			transform.localRotation = this.EffectBullet.Rotation;
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x0004374D File Offset: 0x0004194D
		public void Die()
		{
			Object.Destroy(base.gameObject);
		}

		// Token: 0x04000A94 RID: 2708
		public Transform widgetRoot;
	}
}
