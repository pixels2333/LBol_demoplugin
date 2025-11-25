using System;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class SanaeSuck : EffectBullet
	{
		public float startSpeed
		{
			get
			{
				return 9f;
			}
		}
		public float freeFlyTime
		{
			get
			{
				return 0.7f + (float)this.Count * 0.013f;
			}
		}
		public Vector3 TargetPosition { get; set; }
		private float Speed { get; set; }
		private float Angle { get; set; }
		private float FinalAngle { get; set; }
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
		public SanaeSuck(int group)
		{
			base.EffectName = "SanaeLine";
			this.Count = group;
		}
		public override void FirstTick()
		{
			EffectManager.ModifyEffect(EffectManager.CreateEffect(base.EffectName, base.EffectBulletView.widgetRoot, 0f, new float?(0f), true, true), 1f, this.Count % 11 + 1);
			base.FirstTickFlag = false;
			this.Speed = this.startSpeed;
			this.Angle = (float)(Random.Range(30, 150) % 360);
			this.AbsorbFlag = false;
		}
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
		private void Absorb()
		{
			Vector2 vector = this.TargetPosition - base.Position;
			this.Speed = this.absorbSpeed;
			this.Angle = this.FinalAngle;
			this.EndTime = vector.magnitude / this.absorbSpeed;
		}
		protected override void Die()
		{
			base.Die();
		}
		public float absorbSpeed = 14f;
		public int Count;
	}
}
