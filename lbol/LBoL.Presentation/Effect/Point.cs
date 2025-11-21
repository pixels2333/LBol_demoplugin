using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000106 RID: 262
	public class Point : EffectBullet
	{
		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000E6F RID: 3695 RVA: 0x00044FA4 File Offset: 0x000431A4
		// (set) Token: 0x06000E70 RID: 3696 RVA: 0x00044FAC File Offset: 0x000431AC
		public Vector3 TargetPosition { get; set; }

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000E71 RID: 3697 RVA: 0x00044FB5 File Offset: 0x000431B5
		// (set) Token: 0x06000E72 RID: 3698 RVA: 0x00044FBD File Offset: 0x000431BD
		private float Speed { get; set; }

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000E73 RID: 3699 RVA: 0x00044FC6 File Offset: 0x000431C6
		// (set) Token: 0x06000E74 RID: 3700 RVA: 0x00044FCE File Offset: 0x000431CE
		private float Angle { get; set; }

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000E75 RID: 3701 RVA: 0x00044FD7 File Offset: 0x000431D7
		// (set) Token: 0x06000E76 RID: 3702 RVA: 0x00044FDF File Offset: 0x000431DF
		private bool AbsorbFlag { get; set; }

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000E77 RID: 3703 RVA: 0x00044FE8 File Offset: 0x000431E8
		// (set) Token: 0x06000E78 RID: 3704 RVA: 0x00044FF0 File Offset: 0x000431F0
		private float EndTime { get; set; }

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000E79 RID: 3705 RVA: 0x00044FF9 File Offset: 0x000431F9
		private Vector2 RUnitVector
		{
			get
			{
				return new Vector2(Mathf.Cos(this.Angle * 0.017453292f), Mathf.Sin(this.Angle * 0.017453292f));
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000E7A RID: 3706 RVA: 0x00045022 File Offset: 0x00043222
		private Vector3 Velocity
		{
			get
			{
				return this.RUnitVector * this.Speed * 0.016666668f;
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000E7B RID: 3707 RVA: 0x00045044 File Offset: 0x00043244
		// (set) Token: 0x06000E7C RID: 3708 RVA: 0x0004504C File Offset: 0x0004324C
		public Point.PointType Type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
				string text;
				switch (value)
				{
				case Point.PointType.Power:
					text = "PowerPoint";
					break;
				case Point.PointType.BigPower:
					text = "BigPowerPoint";
					break;
				case Point.PointType.Blue:
					text = "Point";
					break;
				case Point.PointType.BigBlue:
					text = "BigPoint";
					break;
				case Point.PointType.Money:
					text = "Money";
					break;
				case Point.PointType.BigMoney:
					text = "BigMoney";
					break;
				case Point.PointType.SuperBigMoney:
					text = "SuperBigMoney";
					break;
				default:
					text = base.EffectName;
					break;
				}
				base.EffectName = text;
			}
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x000450CA File Offset: 0x000432CA
		public override void FirstTick()
		{
			base.FirstTick();
			this.Speed = Random.Range(this.startSpeedMin, this.startSpeedMax);
			this.Angle = (float)Random.Range(45, 135);
			this.AbsorbFlag = false;
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x00045104 File Offset: 0x00043304
		public override void Calculation()
		{
			if (!this.AbsorbFlag && base.Time > this.freeFlyTime)
			{
				this.Absorb();
				this.AbsorbFlag = true;
			}
			base.Position += this.Velocity;
			if (!this.AbsorbFlag)
			{
				base.Position += new Vector3(0f, -this.gravity * base.Time * 0.016666668f, 0f);
			}
			if ((base.Time > this.EndTime + this.freeFlyTime) & this.AbsorbFlag)
			{
				this.Die();
			}
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x000451AC File Offset: 0x000433AC
		private void Absorb()
		{
			Vector2 vector = this.TargetPosition - base.Position;
			this.Speed = this.absorbSpeed;
			this.Angle = Vector2.SignedAngle(Vector2.right, vector);
			this.EndTime = vector.magnitude / this.absorbSpeed;
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x00045204 File Offset: 0x00043404
		protected override void Die()
		{
			base.Die();
			Point.PointType type = this.Type;
			AudioManager.PlaySfx((type == Point.PointType.Money || type == Point.PointType.BigMoney) ? "MoneyGain" : "Point", -1f);
		}

		// Token: 0x04000AD0 RID: 2768
		public float startSpeedMin = 3f;

		// Token: 0x04000AD1 RID: 2769
		public float startSpeedMax = 5f;

		// Token: 0x04000AD2 RID: 2770
		public float gravity = 12f;

		// Token: 0x04000AD3 RID: 2771
		public float freeFlyTime = 0.8f;

		// Token: 0x04000AD4 RID: 2772
		public float absorbSpeed = 25f;

		// Token: 0x04000ADA RID: 2778
		private Point.PointType _type;

		// Token: 0x0200032B RID: 811
		public enum PointType
		{
			// Token: 0x04001395 RID: 5013
			Power,
			// Token: 0x04001396 RID: 5014
			BigPower,
			// Token: 0x04001397 RID: 5015
			Blue,
			// Token: 0x04001398 RID: 5016
			BigBlue,
			// Token: 0x04001399 RID: 5017
			Money,
			// Token: 0x0400139A RID: 5018
			BigMoney,
			// Token: 0x0400139B RID: 5019
			SuperBigMoney
		}
	}
}
