using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LBoL.Presentation.Effect
{
	// Token: 0x0200010C RID: 268
	public class SeijaExhibitView : MonoBehaviour
	{
		// Token: 0x06000EAA RID: 3754 RVA: 0x00045D60 File Offset: 0x00043F60
		private void OnEnable()
		{
			this.exhibitSprite.DOLocalRotate(new Vector3(0f, 0f, 20f), 5f, RotateMode.Fast).From(new Vector3(0f, 0f, -20f), true, false).SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo)
				.SetUpdate(true);
			this.Position = this.initialPosition;
			base.transform.localPosition = this.Position;
			this.InTargetPosition = false;
			this.EnterTargetSpeed = 0f;
			this.StartStopTimer = 0.5f;
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x00045DFC File Offset: 0x00043FFC
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

		// Token: 0x06000EAC RID: 3756 RVA: 0x00045FBC File Offset: 0x000441BC
		private void LateUpdate()
		{
			base.transform.localPosition = this.Position;
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06000EAD RID: 3757 RVA: 0x00045FCF File Offset: 0x000441CF
		// (set) Token: 0x06000EAE RID: 3758 RVA: 0x00045FD7 File Offset: 0x000441D7
		public Vector3 Position { get; set; }

		// Token: 0x1700027A RID: 634
		// (get) Token: 0x06000EAF RID: 3759 RVA: 0x00045FE0 File Offset: 0x000441E0
		// (set) Token: 0x06000EB0 RID: 3760 RVA: 0x00045FE8 File Offset: 0x000441E8
		public Vector3 TargetPosition { get; set; }

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x06000EB1 RID: 3761 RVA: 0x00045FF1 File Offset: 0x000441F1
		// (set) Token: 0x06000EB2 RID: 3762 RVA: 0x00045FF9 File Offset: 0x000441F9
		public bool InTargetPosition { get; set; }

		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000EB3 RID: 3763 RVA: 0x00046002 File Offset: 0x00044202
		// (set) Token: 0x06000EB4 RID: 3764 RVA: 0x0004600A File Offset: 0x0004420A
		public float EnterTargetSpeed { get; set; }

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06000EB5 RID: 3765 RVA: 0x00046013 File Offset: 0x00044213
		// (set) Token: 0x06000EB6 RID: 3766 RVA: 0x0004601B File Offset: 0x0004421B
		public float StartStopTimer { get; set; }

		// Token: 0x04000B07 RID: 2823
		[SerializeField]
		private Transform exhibitSprite;

		// Token: 0x04000B0D RID: 2829
		private const float EnterTargetAccelerate = 1f;

		// Token: 0x04000B0E RID: 2830
		public const float EnterTargetMaxSpeed = 2f;

		// Token: 0x04000B0F RID: 2831
		private readonly Vector3 initialPosition = new Vector3(0.7f, 1.2f, 0f);

		// Token: 0x04000B10 RID: 2832
		public Vector3 center;

		// Token: 0x04000B11 RID: 2833
		public float orbitRadius = 1.6f;

		// Token: 0x04000B12 RID: 2834
		public float cycleTime = 8f;

		// Token: 0x04000B13 RID: 2835
		public float halfCycleAmpY;

		// Token: 0x04000B14 RID: 2836
		public float tilt = 45f;

		// Token: 0x04000B15 RID: 2837
		public float phase;

		// Token: 0x04000B16 RID: 2838
		public float orbitAngleOffset;

		// Token: 0x04000B17 RID: 2839
		public bool clockwise;

		// Token: 0x04000B18 RID: 2840
		public float orbitAnglePerSecond = 3f;

		// Token: 0x04000B19 RID: 2841
		private float _angle;

		// Token: 0x04000B1A RID: 2842
		private float _orbitAngle;
	}
}
