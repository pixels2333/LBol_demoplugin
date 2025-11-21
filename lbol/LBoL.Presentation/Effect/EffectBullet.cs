using System;
using LBoL.Base.Extensions;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x020000FB RID: 251
	public abstract class EffectBullet
	{
		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000E15 RID: 3605 RVA: 0x000435CC File Offset: 0x000417CC
		// (set) Token: 0x06000E16 RID: 3606 RVA: 0x000435D4 File Offset: 0x000417D4
		public EffectBulletView EffectBulletView { get; set; }

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000E17 RID: 3607 RVA: 0x000435DD File Offset: 0x000417DD
		// (set) Token: 0x06000E18 RID: 3608 RVA: 0x000435E5 File Offset: 0x000417E5
		public Vector3 Position { get; set; }

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06000E19 RID: 3609 RVA: 0x000435EE File Offset: 0x000417EE
		// (set) Token: 0x06000E1A RID: 3610 RVA: 0x000435F6 File Offset: 0x000417F6
		public Quaternion Rotation { get; set; }

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x06000E1B RID: 3611 RVA: 0x000435FF File Offset: 0x000417FF
		// (set) Token: 0x06000E1C RID: 3612 RVA: 0x00043607 File Offset: 0x00041807
		public bool Active { get; set; }

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x06000E1D RID: 3613 RVA: 0x00043610 File Offset: 0x00041810
		// (set) Token: 0x06000E1E RID: 3614 RVA: 0x00043618 File Offset: 0x00041818
		public float Time { get; set; }

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x06000E1F RID: 3615 RVA: 0x00043621 File Offset: 0x00041821
		// (set) Token: 0x06000E20 RID: 3616 RVA: 0x00043629 File Offset: 0x00041829
		public bool FirstTickFlag { get; set; } = true;

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x06000E21 RID: 3617 RVA: 0x00043632 File Offset: 0x00041832
		// (set) Token: 0x06000E22 RID: 3618 RVA: 0x0004363A File Offset: 0x0004183A
		public string EffectName { get; set; }

		// Token: 0x06000E23 RID: 3619 RVA: 0x00043643 File Offset: 0x00041843
		public void Tick()
		{
			if (this.FirstTickFlag)
			{
				this.FirstTick();
			}
			this.Time += 0.016666668f;
			this.Calculation();
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x0004366B File Offset: 0x0004186B
		public virtual void FirstTick()
		{
			if (!this.EffectName.IsNullOrEmpty())
			{
				EffectManager.CreateEffect(this.EffectName, this.EffectBulletView.widgetRoot, 0f, new float?(0f), true, true);
			}
			this.FirstTickFlag = false;
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x000436A9 File Offset: 0x000418A9
		public virtual void Calculation()
		{
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x000436AB File Offset: 0x000418AB
		protected virtual void Die()
		{
			this.Active = false;
			Singleton<EffectManager>.Instance.Remove(this);
			this.EffectBulletView.Die();
		}
	}
}
