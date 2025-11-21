using System;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Presentation.Effect;
using LBoL.Presentation.Units;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000113 RID: 275
	public sealed class Laser : Projectile
	{
		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06000F2E RID: 3886 RVA: 0x0004805B File Offset: 0x0004625B
		// (set) Token: 0x06000F2F RID: 3887 RVA: 0x00048063 File Offset: 0x00046263
		private bool Attack { get; set; }

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06000F30 RID: 3888 RVA: 0x0004806C File Offset: 0x0004626C
		// (set) Token: 0x06000F31 RID: 3889 RVA: 0x00048074 File Offset: 0x00046274
		private Vector2 Offset { get; set; }

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x06000F32 RID: 3890 RVA: 0x0004807D File Offset: 0x0004627D
		// (set) Token: 0x06000F33 RID: 3891 RVA: 0x00048085 File Offset: 0x00046285
		private Vector2 Size { get; set; }

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000F34 RID: 3892 RVA: 0x0004808E File Offset: 0x0004628E
		// (set) Token: 0x06000F35 RID: 3893 RVA: 0x00048096 File Offset: 0x00046296
		private float Start { get; set; }

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000F36 RID: 3894 RVA: 0x0004809F File Offset: 0x0004629F
		// (set) Token: 0x06000F37 RID: 3895 RVA: 0x000480A7 File Offset: 0x000462A7
		private int AttackTimes { get; set; }

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06000F38 RID: 3896 RVA: 0x000480B0 File Offset: 0x000462B0
		// (set) Token: 0x06000F39 RID: 3897 RVA: 0x000480B8 File Offset: 0x000462B8
		private LaserConfig Config { get; set; }

		// Token: 0x06000F3A RID: 3898 RVA: 0x000480C4 File Offset: 0x000462C4
		public void Set(LaserConfig config)
		{
			this.Config = config;
			base.name = config.Name + "(Laser)";
			this.Size = config.Size;
			this.Offset = config.Offset;
			this.Start = (float)config.Start;
			base.WidgetEffect = config.Widget;
			base.HitBodyEffect = config.HitBody;
			base.HitShieldEffect = config.HitShield;
			base.HitBlockEffect = config.HitBlock;
			base.GrazeEffect = config.Graze;
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x00048150 File Offset: 0x00046350
		public override void Spawn()
		{
			base.Spawn();
			if (base.HitRemainAmount > 0)
			{
				this.laserCollider.offset = this.Offset;
				this.laserCollider.size = this.Size;
				this.laserCollider.enabled = true;
				this.AttackTimes = 0;
				GunManager.AttackingLasers.Add(this);
			}
			else
			{
				this.laserCollider.enabled = false;
			}
			AudioManager.PlaySfx(base.Piece.LaunchSfx.IsNullOrEmpty() ? this.Config.LaunchSfx : base.Piece.LaunchSfx, -1f);
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x000481F0 File Offset: 0x000463F0
		protected override void Vanish()
		{
			base.Vanish();
			if (!this._isVanishing)
			{
				this._isVanishing = true;
				this._preVanishScale = base.transform.localScale;
				float num = Mathf.Max(new float[]
				{
					base.Piece.VanishV3.x,
					base.Piece.VanishV3.y,
					base.Piece.VanishV3.z
				});
				this._vanishCountDown = 1f / num;
				return;
			}
			this._vanishCountDown -= 1f;
			if (this._vanishCountDown <= 0f)
			{
				base.transform.localScale = this._preVanishScale;
				this.Die();
				return;
			}
			base.transform.localScale -= new Vector3(this._preVanishScale.x * base.Piece.VanishV3.x, this._preVanishScale.y * base.Piece.VanishV3.y, this._preVanishScale.z * base.Piece.VanishV3.z);
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x0004831C File Offset: 0x0004651C
		protected override void Die()
		{
			base.Die();
			GunManager.AttackingLasers.Remove(this);
			Object.Destroy(base.gameObject);
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x0004833C File Offset: 0x0004653C
		public void TryToAttack()
		{
			if ((float)base.LifeTick >= (float)this.AttackTimes * base.Launcher.Piece.HitInterval + this.Start)
			{
				this.InternalAttack();
				int attackTimes = this.AttackTimes;
				this.AttackTimes = attackTimes + 1;
			}
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x00048388 File Offset: 0x00046588
		private void InternalAttack()
		{
			foreach (UnitView unitView in base.Gun.Targets)
			{
				if (unitView != null && unitView.ActiveCollider != null && this.laserCollider.IsTouching(unitView.ActiveCollider))
				{
					this.HitTarget(unitView.HitType, unitView.HitPoint);
					unitView.Hit(base.Piece.LastWave, 1f, false);
				}
			}
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x0004842C File Offset: 0x0004662C
		private void HitTarget(HitType type, Transform hitPoint)
		{
			switch (type)
			{
			case HitType.Graze:
				this.Graze(hitPoint);
				break;
			case HitType.Block:
				this.HitBlock(hitPoint);
				break;
			case HitType.Shield:
				this.HitShield(hitPoint);
				break;
			case HitType.Body:
				this.HitBody(hitPoint);
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
			if (base.Piece.LastWave && base.Timer > base.Piece.LaserLastWave)
			{
				base.Gun.LastWaveHit();
			}
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x000484B4 File Offset: 0x000466B4
		private void Graze(Transform hitPoint)
		{
			AudioManager.PlaySfx(this.Config.GrazeSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.GrazeEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x000484FC File Offset: 0x000466FC
		private void HitBlock(Transform hitPoint)
		{
			AudioManager.PlaySfx(this.Config.HitBlockSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitBlockEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x00048544 File Offset: 0x00046744
		private void HitShield(Transform hitPoint)
		{
			AudioManager.PlaySfx(this.Config.HitShieldSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitShieldEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x0004858C File Offset: 0x0004678C
		private void HitBody(Transform hitPoint)
		{
			AudioManager.PlaySfx(base.Piece.LaunchSfx.IsNullOrEmpty() ? this.Config.HitBodySfx : base.Piece.HitBodySfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitBodyEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}

		// Token: 0x04000B59 RID: 2905
		public BoxCollider2D laserCollider;

		// Token: 0x04000B60 RID: 2912
		private bool _isVanishing;

		// Token: 0x04000B61 RID: 2913
		private float _vanishCountDown;

		// Token: 0x04000B62 RID: 2914
		private Vector3 _preVanishScale;
	}
}
