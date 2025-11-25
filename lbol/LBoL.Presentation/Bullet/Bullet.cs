using System;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Presentation.Effect;
using LBoL.Presentation.Units;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public sealed class Bullet : Projectile
	{
		private string LaunchEffect { get; set; }
		private BulletConfig Config { get; set; }
		private int HitTicking { get; set; }
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
		protected override void FirstTick()
		{
			base.FirstTick();
			this.Launch();
			this.bulletCollider.radius = 0.2f * base.Launcher.Scale;
		}
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
		protected override void Die()
		{
			base.Die();
			this.bulletCollider.radius = 0.2f;
			Object.Destroy(base.gameObject);
		}
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
		private void Graze()
		{
			AudioManager.PlaySfx(this.Config.GrazeSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.GrazeEffect, base.transform, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
			this.LookAtTarget(effectWidget);
		}
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
		private void LookAtTarget(EffectWidget effect)
		{
			if (effect && base.Gun.Target && base.Gun.Target.transform)
			{
				EffectManager.LookForwardWorldPosition(effect, base.Gun.Target.transform);
			}
		}
		public CircleCollider2D bulletCollider;
		private const float ColliderRadius = 0.2f;
		private bool _isVanishing;
		private float _vanishCountDown;
		private Vector3 _preVanishScale;
	}
}
