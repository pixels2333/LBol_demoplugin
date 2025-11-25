using System;
using UnityEngine;
namespace LBoL.Presentation.Effect
{
	public class RinOrb : EffectBullet
	{
		public static RinOrb CreateRinOrb(string effectName, int orbitIndex)
		{
			RinOrb rinOrb;
			if (orbitIndex != 1)
			{
				if (orbitIndex != 2)
				{
					rinOrb = new RinOrb();
				}
				else
				{
					rinOrb = new RinOrb
					{
						center = new Vector3(0f, 1f, 0f),
						orbitRadius = 1f,
						tilt = 0f,
						halfCycleAmpY = 0.2f,
						phase = 2.0943952f
					};
				}
			}
			else
			{
				rinOrb = new RinOrb
				{
					orbitAngleOffset = 180f,
					phase = 1.0471976f
				};
			}
			RinOrb rinOrb2 = rinOrb;
			rinOrb2.EffectName = effectName;
			return rinOrb2;
		}
		public override void Calculation()
		{
			this._orbitAngle = base.Time * this.orbitAnglePerSecond + this.orbitAngleOffset;
			this._angle = base.Time / this.cycleTime * 2f * 3.1415927f + this.phase;
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
				float num = base.Time / this.cycleTime;
				float num2 = (num - (float)Mathf.FloorToInt(num)) * 4f * 3.1415927f;
				Vector3 vector2 = new Vector3(0f, Mathf.Sin(num2) * this.halfCycleAmpY, 0f);
				vector += vector2;
			}
			base.Position = vector;
		}
		public Vector3 center = new Vector3(0f, 0.5f, 0f);
		public float orbitRadius = 1.2f;
		public float cycleTime = 4f;
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
