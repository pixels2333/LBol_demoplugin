using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x0200010A RID: 266
	public class SanaeSuck : EffectBullet
	{
		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000E91 RID: 3729 RVA: 0x000457E5 File Offset: 0x000439E5
		public float startSpeed
		{
			get
			{
				return 9f;
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000E92 RID: 3730 RVA: 0x000457EC File Offset: 0x000439EC
		public float freeFlyTime
		{
			get
			{
				return 0.7f + (float)this.Count * 0.013f;
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x06000E93 RID: 3731 RVA: 0x00045801 File Offset: 0x00043A01
		// (set) Token: 0x06000E94 RID: 3732 RVA: 0x00045809 File Offset: 0x00043A09
		public Vector3 TargetPosition { get; set; }

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x06000E95 RID: 3733 RVA: 0x00045812 File Offset: 0x00043A12
		// (set) Token: 0x06000E96 RID: 3734 RVA: 0x0004581A File Offset: 0x00043A1A
		private float Speed { get; set; }

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000E97 RID: 3735 RVA: 0x00045823 File Offset: 0x00043A23
		// (set) Token: 0x06000E98 RID: 3736 RVA: 0x0004582B File Offset: 0x00043A2B
		private float Angle { get; set; }

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000E99 RID: 3737 RVA: 0x00045834 File Offset: 0x00043A34
		// (set) Token: 0x06000E9A RID: 3738 RVA: 0x0004583C File Offset: 0x00043A3C
		private float FinalAngle { get; set; }

		// Token: 0x17000275 RID: 629
		// (get) Token: 0x06000E9B RID: 3739 RVA: 0x00045845 File Offset: 0x00043A45
		// (set) Token: 0x06000E9C RID: 3740 RVA: 0x0004584D File Offset: 0x00043A4D
		private bool AbsorbFlag { get; set; }

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000E9D RID: 3741 RVA: 0x00045856 File Offset: 0x00043A56
		// (set) Token: 0x06000E9E RID: 3742 RVA: 0x0004585E File Offset: 0x00043A5E
		private float EndTime { get; set; }

		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06000E9F RID: 3743 RVA: 0x00045867 File Offset: 0x00043A67
		private Vector2 RUnitVector
		{
			get
			{
				return new Vector2(Mathf.Cos(this.Angle * 0.017453292f), Mathf.Sin(this.Angle * 0.017453292f));
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x06000EA0 RID: 3744 RVA: 0x00045890 File Offset: 0x00043A90
		private Vector3 Velocity
		{
			get
			{
				return this.RUnitVector * this.Speed * 0.016666668f;
			}
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x000458B2 File Offset: 0x00043AB2
		public SanaeSuck(int group)
		{
			base.EffectName = "SanaeLine";
			this.Count = group;
		}

		// Token: 0x06000EA2 RID: 3746 RVA: 0x000458D8 File Offset: 0x00043AD8
		public override void FirstTick()
		{
			EffectManager.ModifyEffect(EffectManager.CreateEffect(base.EffectName, base.EffectBulletView.widgetRoot, 0f, new float?(0f), true, true), 1f, this.Count % 11 + 1);
			base.FirstTickFlag = false;
			this.Speed = this.startSpeed;
			this.Angle = (float)(Random.Range(30, 150) % 360);
			this.AbsorbFlag = false;
		}

		// Token: 0x06000EA3 RID: 3747 RVA: 0x00045954 File Offset: 0x00043B54
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

		// Token: 0x06000EA4 RID: 3748 RVA: 0x00045ADC File Offset: 0x00043CDC
		private void Absorb()
		{
			Vector2 vector = this.TargetPosition - base.Position;
			this.Speed = this.absorbSpeed;
			this.Angle = this.FinalAngle;
			this.EndTime = vector.magnitude / this.absorbSpeed;
		}

		// Token: 0x06000EA5 RID: 3749 RVA: 0x00045B2C File Offset: 0x00043D2C
		protected override void Die()
		{
			base.Die();
		}

		// Token: 0x04000AFC RID: 2812
		public float absorbSpeed = 14f;

		// Token: 0x04000AFD RID: 2813
		public int Count;
	}
}
