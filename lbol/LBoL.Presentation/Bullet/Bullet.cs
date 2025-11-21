using System;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Presentation.Effect;
using LBoL.Presentation.Units;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x0200010E RID: 270
	public sealed class Bullet : Projectile
	{
		// Token: 0x17000286 RID: 646
		// (get) Token: 0x06000ECB RID: 3787 RVA: 0x00046388 File Offset: 0x00044588
		// (set) Token: 0x06000ECC RID: 3788 RVA: 0x00046390 File Offset: 0x00044590
		private string LaunchEffect { get; set; }

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06000ECD RID: 3789 RVA: 0x00046399 File Offset: 0x00044599
		// (set) Token: 0x06000ECE RID: 3790 RVA: 0x000463A1 File Offset: 0x000445A1
		private BulletConfig Config { get; set; }

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06000ECF RID: 3791 RVA: 0x000463AA File Offset: 0x000445AA
		// (set) Token: 0x06000ED0 RID: 3792 RVA: 0x000463B2 File Offset: 0x000445B2
		private int HitTicking { get; set; }

		// Token: 0x06000ED1 RID: 3793 RVA: 0x000463BC File Offset: 0x000445BC
		public void Set(BulletConfig config)
		{
			this.Config = config;
			base.name = config.Name + "(Bullet)";
			base.WidgetEffect = config.Widget;
			this.LaunchEffect = config.Launch;
			base.HitBodyEffect = config.HitBody;
			base.HitShieldEffect = config.HitShield;
			base.HitBlockEffect = config.HitBlock;
			base.GrazeEffect = config.Graze;
		}

		// Token: 0x06000ED2 RID: 3794 RVA: 0x00046430 File Offset: 0x00044630
		public override void Tick()
		{
			base.Tick();
			int num = this.HitTicking - 1;
			this.HitTicking = num;
			if (base.Active && base.HitRemainAmount > 0 && this.HitTicking < 1)
			{
				foreach (UnitView unitView in base.Gun.Targets)
				{
					if (unitView != null && unitView.ActiveCollider != null && this.bulletCollider.IsTouching(unitView.ActiveCollider))
					{
						unitView.Hit(base.Piece.LastWave, base.Piece.HitAnimationSpeed, false);
						this.HitTarget(unitView.HitType);
					}
				}
			}
		}

		// Token: 0x06000ED3 RID: 3795 RVA: 0x00046510 File Offset: 0x00044710
		protected override void FirstTick()
		{
			base.FirstTick();
			this.Launch();
			this.bulletCollider.radius = 0.2f * base.Launcher.Scale;
		}

		// Token: 0x06000ED4 RID: 3796 RVA: 0x0004653C File Offset: 0x0004473C
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

		// Token: 0x06000ED5 RID: 3797 RVA: 0x00046668 File Offset: 0x00044868
		protected override void Die()
		{
			base.Die();
			this.bulletCollider.radius = 0.2f;
			Object.Destroy(base.gameObject);
		}

		// Token: 0x06000ED6 RID: 3798 RVA: 0x0004668C File Offset: 0x0004488C
		private void Launch()
		{
			AudioManager.PlaySfx(base.Piece.LaunchSfx.IsNullOrEmpty() ? this.Config.LaunchSfx : base.Piece.LaunchSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(this.LaunchEffect, base.transform, 0f, default(float?), false, true);
			if (effectWidget != null)
			{
				effectWidget.transform.rotation = Quaternion.Euler(0f, 0f, base.Launcher.Angle);
			}
			base.ModifyEffect(effectWidget);
		}

		// Token: 0x06000ED7 RID: 3799 RVA: 0x00046720 File Offset: 0x00044920
		private void HitTarget(HitType type)
		{
			if (base.Piece.LastWave)
			{
				base.Gun.LastWaveHit();
			}
			int hitRemainAmount = base.HitRemainAmount;
			base.HitRemainAmount = hitRemainAmount - 1;
			this.HitTicking = hitRemainAmount;
			switch (type)
			{
			case HitType.Graze:
				this.Graze();
				return;
			case HitType.Block:
				this.HitBlock();
				return;
			case HitType.Shield:
				this.HitShield();
				return;
			case HitType.Body:
				this.HitBody();
				return;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
		}

		// Token: 0x06000ED8 RID: 3800 RVA: 0x000467A4 File Offset: 0x000449A4
		private void Graze()
		{
			AudioManager.PlaySfx(this.Config.GrazeSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.GrazeEffect, base.transform, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
			this.LookAtTarget(effectWidget);
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x000467F8 File Offset: 0x000449F8
		private void HitBlock()
		{
			AudioManager.PlaySfx(this.Config.HitBlockSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitBlockEffect, base.transform, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
			this.LookAtTarget(effectWidget);
			if (!base.Piece.ZeroHitNotDie && base.HitRemainAmount < 1)
			{
				this.Die();
			}
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x00046868 File Offset: 0x00044A68
		private void HitShield()
		{
			AudioManager.PlaySfx(this.Config.HitShieldSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitShieldEffect, base.transform, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
			this.LookAtTarget(effectWidget);
			if (!base.Piece.ZeroHitNotDie && base.HitRemainAmount < 1)
			{
				this.Die();
			}
		}

		// Token: 0x06000EDB RID: 3803 RVA: 0x000468D8 File Offset: 0x00044AD8
		private void HitBody()
		{
			AudioManager.PlaySfx(base.Piece.LaunchSfx.IsNullOrEmpty() ? this.Config.HitBodySfx : base.Piece.HitBodySfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitBodyEffect, base.transform, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
			this.LookAtTarget(effectWidget);
			if (!base.Piece.ZeroHitNotDie && base.HitRemainAmount < 1)
			{
				this.Die();
			}
		}

		// Token: 0x06000EDC RID: 3804 RVA: 0x00046968 File Offset: 0x00044B68
		private void LookAtTarget(EffectWidget effect)
		{
			if (effect && base.Gun.Target && base.Gun.Target.transform)
			{
				EffectManager.LookForwardWorldPosition(effect, base.Gun.Target.transform);
			}
		}

		// Token: 0x04000B25 RID: 2853
		public CircleCollider2D bulletCollider;

		// Token: 0x04000B27 RID: 2855
		private const float ColliderRadius = 0.2f;

		// Token: 0x04000B2A RID: 2858
		private bool _isVanishing;

		// Token: 0x04000B2B RID: 2859
		private float _vanishCountDown;

		// Token: 0x04000B2C RID: 2860
		private Vector3 _preVanishScale;
	}
}
