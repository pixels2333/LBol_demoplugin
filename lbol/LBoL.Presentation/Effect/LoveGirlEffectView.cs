using System;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x02000103 RID: 259
	public class LoveGirlEffectView : MonoBehaviour
	{
		// Token: 0x06000E5B RID: 3675 RVA: 0x00044B4B File Offset: 0x00042D4B
		private void OnEnable()
		{
			this.Position = this.initialPosition;
			base.transform.localPosition = this.Position;
			this.InTargetPosition = false;
			this.EnterTargetSpeed = 0f;
			this.StartStopTimer = 0.5f;
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x00044B88 File Offset: 0x00042D88
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

		// Token: 0x06000E5D RID: 3677 RVA: 0x00044D48 File Offset: 0x00042F48
		private void LateUpdate()
		{
			base.transform.localPosition = this.Position;
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x06000E5E RID: 3678 RVA: 0x00044D5B File Offset: 0x00042F5B
		// (set) Token: 0x06000E5F RID: 3679 RVA: 0x00044D63 File Offset: 0x00042F63
		public Vector3 Position { get; set; }

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x06000E60 RID: 3680 RVA: 0x00044D6C File Offset: 0x00042F6C
		// (set) Token: 0x06000E61 RID: 3681 RVA: 0x00044D74 File Offset: 0x00042F74
		public Vector3 TargetPosition { get; set; }

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x06000E62 RID: 3682 RVA: 0x00044D7D File Offset: 0x00042F7D
		// (set) Token: 0x06000E63 RID: 3683 RVA: 0x00044D85 File Offset: 0x00042F85
		public bool InTargetPosition { get; set; }

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000E64 RID: 3684 RVA: 0x00044D8E File Offset: 0x00042F8E
		// (set) Token: 0x06000E65 RID: 3685 RVA: 0x00044D96 File Offset: 0x00042F96
		public float EnterTargetSpeed { get; set; }

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000E66 RID: 3686 RVA: 0x00044D9F File Offset: 0x00042F9F
		// (set) Token: 0x06000E67 RID: 3687 RVA: 0x00044DA7 File Offset: 0x00042FA7
		public float StartStopTimer { get; set; }

		// Token: 0x04000ABE RID: 2750
		private const float EnterTargetAccelerate = 1f;

		// Token: 0x04000ABF RID: 2751
		public const float EnterTargetMaxSpeed = 2f;

		// Token: 0x04000AC0 RID: 2752
		private readonly Vector3 initialPosition = new Vector3(0.7f, 1.2f, 0f);

		// Token: 0x04000AC1 RID: 2753
		public Vector3 center;

		// Token: 0x04000AC2 RID: 2754
		public float orbitRadius = 1.6f;

		// Token: 0x04000AC3 RID: 2755
		public float cycleTime = 8f;

		// Token: 0x04000AC4 RID: 2756
		public float halfCycleAmpY;

		// Token: 0x04000AC5 RID: 2757
		public float tilt = 45f;

		// Token: 0x04000AC6 RID: 2758
		public float phase;

		// Token: 0x04000AC7 RID: 2759
		public float orbitAngleOffset;

		// Token: 0x04000AC8 RID: 2760
		public bool clockwise;

		// Token: 0x04000AC9 RID: 2761
		public float orbitAnglePerSecond = 3f;

		// Token: 0x04000ACA RID: 2762
		private float _angle;

		// Token: 0x04000ACB RID: 2763
		private float _orbitAngle;
	}
}
