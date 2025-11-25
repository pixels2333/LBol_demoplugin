using System;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class Point : EffectBullet
	{
		public Vector3 TargetPosition { get; set; }
		private float Speed { get; set; }
		private float Angle { get; set; }
		private bool AbsorbFlag { get; set; }
		private float EndTime { get; set; }
		private Vector2 RUnitVector
		{
			get
			{
				return new Vector2(Mathf.Cos(this.Angle * 0.017453292f), Mathf.Sin(this.Angle * 0.017453292f));
			}
		}
		private Vector3 Velocity
		{
			get
			{
				return this.RUnitVector * this.Speed * 0.016666668f;
			}
		}
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
		public override void FirstTick()
		{
			base.FirstTick();
			this.Speed = Random.Range(this.startSpeedMin, this.startSpeedMax);
			this.Angle = (float)Random.Range(45, 135);
			this.AbsorbFlag = false;
		}
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
		private void Absorb()
		{
			Vector2 vector = this.TargetPosition - base.Position;
			this.Speed = this.absorbSpeed;
			this.Angle = Vector2.SignedAngle(Vector2.right, vector);
			this.EndTime = vector.magnitude / this.absorbSpeed;
		}
		protected override void Die()
		{
			base.Die();
			Point.PointType type = this.Type;
			AudioManager.PlaySfx((type == Point.PointType.Money || type == Point.PointType.BigMoney) ? "MoneyGain" : "Point", -1f);
		}
		public float startSpeedMin = 3f;
		public float startSpeedMax = 5f;
		public float gravity = 12f;
		public float freeFlyTime = 0.8f;
		public float absorbSpeed = 25f;
		private Point.PointType _type;
		public enum PointType
		{
			Power,
			BigPower,
			Blue,
			BigBlue,
			Money,
			BigMoney,
			SuperBigMoney
		}
	}
}
