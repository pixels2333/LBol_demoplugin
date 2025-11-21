using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x0200010D RID: 269
	public class VampireBlood : EffectBullet
	{
		// Token: 0x1700027E RID: 638
		// (get) Token: 0x06000EB8 RID: 3768 RVA: 0x0004607D File Offset: 0x0004427D
		// (set) Token: 0x06000EB9 RID: 3769 RVA: 0x00046085 File Offset: 0x00044285
		public Vector3 TargetPosition { get; set; }

		// Token: 0x1700027F RID: 639
		// (get) Token: 0x06000EBA RID: 3770 RVA: 0x0004608E File Offset: 0x0004428E
		// (set) Token: 0x06000EBB RID: 3771 RVA: 0x00046096 File Offset: 0x00044296
		private float Speed { get; set; }

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x06000EBC RID: 3772 RVA: 0x0004609F File Offset: 0x0004429F
		// (set) Token: 0x06000EBD RID: 3773 RVA: 0x000460A7 File Offset: 0x000442A7
		private float Angle { get; set; }

		// Token: 0x17000281 RID: 641
		// (get) Token: 0x06000EBE RID: 3774 RVA: 0x000460B0 File Offset: 0x000442B0
		// (set) Token: 0x06000EBF RID: 3775 RVA: 0x000460B8 File Offset: 0x000442B8
		private float FinalAngle { get; set; }

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x06000EC0 RID: 3776 RVA: 0x000460C1 File Offset: 0x000442C1
		// (set) Token: 0x06000EC1 RID: 3777 RVA: 0x000460C9 File Offset: 0x000442C9
		private bool AbsorbFlag { get; set; }

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06000EC2 RID: 3778 RVA: 0x000460D2 File Offset: 0x000442D2
		// (set) Token: 0x06000EC3 RID: 3779 RVA: 0x000460DA File Offset: 0x000442DA
		private float EndTime { get; set; }

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06000EC4 RID: 3780 RVA: 0x000460E3 File Offset: 0x000442E3
		private Vector2 RUnitVector
		{
			get
			{
				return new Vector2(Mathf.Cos(this.Angle * 0.017453292f), Mathf.Sin(this.Angle * 0.017453292f));
			}
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06000EC5 RID: 3781 RVA: 0x0004610C File Offset: 0x0004430C
		private Vector3 Velocity
		{
			get
			{
				return this.RUnitVector * this.Speed * 0.016666668f;
			}
		}

		// Token: 0x06000EC6 RID: 3782 RVA: 0x0004612E File Offset: 0x0004432E
		public VampireBlood()
		{
			base.EffectName = "VampireHeal";
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x0004616D File Offset: 0x0004436D
		public override void FirstTick()
		{
			base.FirstTick();
			this.Speed = Random.Range(this.startSpeedMin, this.startSpeedMax);
			this.Angle = (float)Random.Range(45, 135);
			this.AbsorbFlag = false;
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x000461A8 File Offset: 0x000443A8
		public override void Calculation()
		{
			if (!this.AbsorbFlag)
			{
				Vector2 vector = this.TargetPosition - base.Position;
				this.FinalAngle = Vector2.SignedAngle(Vector2.right, vector);
				if (base.Time > this.freeFlyTime)
				{
					this.Absorb();
					this.AbsorbFlag = true;
				}
				else if (vector.x > 0f)
				{
					if (this.FinalAngle - this.Angle < 0f)
					{
						this.Angle += (this.FinalAngle - this.Angle) * Mathf.Pow(base.Time / this.freeFlyTime, 2f);
					}
				}
				else
				{
					if (this.FinalAngle < 0f)
					{
						this.FinalAngle += 360f;
					}
					if (this.FinalAngle - this.Angle > 0f)
					{
						this.Angle += (this.FinalAngle - this.Angle) * Mathf.Pow(base.Time / this.freeFlyTime, 2f);
					}
				}
			}
			base.Position += this.Velocity;
			if ((base.Time > this.EndTime + this.freeFlyTime) & this.AbsorbFlag)
			{
				this.Speed = 0f;
			}
			if ((base.Time > this.EndTime + this.freeFlyTime + 1f) & this.AbsorbFlag)
			{
				this.Die();
			}
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x00046330 File Offset: 0x00044530
		private void Absorb()
		{
			Vector2 vector = this.TargetPosition - base.Position;
			this.Speed = this.absorbSpeed;
			this.Angle = this.FinalAngle;
			this.EndTime = vector.magnitude / this.absorbSpeed;
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x00046380 File Offset: 0x00044580
		protected override void Die()
		{
			base.Die();
		}

		// Token: 0x04000B1B RID: 2843
		public float startSpeedMin = 12f;

		// Token: 0x04000B1C RID: 2844
		public float startSpeedMax = 14f;

		// Token: 0x04000B1D RID: 2845
		public float freeFlyTime = 0.7f;

		// Token: 0x04000B1E RID: 2846
		public float absorbSpeed = 14f;
	}
}
