using System;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class LoveGirlEffectView : MonoBehaviour
	{
		private void OnEnable()
		{
			this.Position = this.initialPosition;
			base.transform.localPosition = this.Position;
			this.InTargetPosition = false;
			this.EnterTargetSpeed = 0f;
			this.StartStopTimer = 0.5f;
		}
		public void Calculation(float time, float deltaTime)
		{
			if (this.StartStopTimer > 0f)
			{
				this.StartStopTimer -= deltaTime;
				return;
			}
			this._orbitAngle = time * this.orbitAnglePerSecond + this.orbitAngleOffset;
			this._angle = time / this.cycleTime * 2f * 3.1415927f + this.phase;
			if (this.clockwise)
			{
				this._angle = -this._angle;
			}
			Vector3 vector = new Vector3(this.orbitRadius * Mathf.Cos(this._angle), 0f, this.orbitRadius * Mathf.Sin(this._angle));
			vector = Quaternion.AngleAxis(this.tilt, Vector3.forward) * vector;
			vector = Quaternion.AngleAxis(this._orbitAngle, Vector3.up) * vector;
			vector += this.center;
			if (this.halfCycleAmpY != 0f)
			{
				float num = time / this.cycleTime;
				float num2 = (num - (float)Mathf.FloorToInt(num)) * 4f * 3.1415927f;
				Vector3 vector2 = new Vector3(0f, Mathf.Sin(num2) * this.halfCycleAmpY, 0f);
				vector += vector2;
			}
			if (this.InTargetPosition)
			{
				this.Position = vector;
				return;
			}
			this.TargetPosition = vector;
			if (this.EnterTargetSpeed < 2f)
			{
				this.EnterTargetSpeed = Mathf.Min(1f * (1f + deltaTime), 2f);
			}
			float num3 = Vector3.Distance(this.Position, this.TargetPosition);
			float num4 = deltaTime * this.EnterTargetSpeed;
			if (num4 > num3)
			{
				this.InTargetPosition = true;
				this.Position = this.TargetPosition;
				return;
			}
			this.Position = Vector3.Lerp(this.Position, this.TargetPosition, num4 / num3);
		}
		private void LateUpdate()
		{
			base.transform.localPosition = this.Position;
		}
		public Vector3 Position { get; set; }
		public Vector3 TargetPosition { get; set; }
		public bool InTargetPosition { get; set; }
		public float EnterTargetSpeed { get; set; }
		public float StartStopTimer { get; set; }
		private const float EnterTargetAccelerate = 1f;
		public const float EnterTargetMaxSpeed = 2f;
		private readonly Vector3 initialPosition = new Vector3(0.7f, 1.2f, 0f);
		public Vector3 center;
		public float orbitRadius = 1.6f;
		public float cycleTime = 8f;
		public float halfCycleAmpY;
		public float tilt = 45f;
		public float phase;
		public float orbitAngleOffset;
		public bool clockwise;
		public float orbitAnglePerSecond = 3f;
		private float _angle;
		private float _orbitAngle;
	}
}
