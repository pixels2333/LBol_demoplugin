using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000109 RID: 265
	public class RinOrb : EffectBullet
	{
		// Token: 0x06000E8E RID: 3726 RVA: 0x000455CC File Offset: 0x000437CC
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

		// Token: 0x06000E8F RID: 3727 RVA: 0x00045660 File Offset: 0x00043860
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

		// Token: 0x04000AF1 RID: 2801
		public Vector3 center = new Vector3(0f, 0.5f, 0f);

		// Token: 0x04000AF2 RID: 2802
		public float orbitRadius = 1.2f;

		// Token: 0x04000AF3 RID: 2803
		public float cycleTime = 4f;

		// Token: 0x04000AF4 RID: 2804
		public float halfCycleAmpY;

		// Token: 0x04000AF5 RID: 2805
		public float tilt = 45f;

		// Token: 0x04000AF6 RID: 2806
		public float phase;

		// Token: 0x04000AF7 RID: 2807
		public float orbitAngleOffset;

		// Token: 0x04000AF8 RID: 2808
		public bool clockwise;

		// Token: 0x04000AF9 RID: 2809
		public float orbitAnglePerSecond = 3f;

		// Token: 0x04000AFA RID: 2810
		private float _angle;

		// Token: 0x04000AFB RID: 2811
		private float _orbitAngle;
	}
}
