using System;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Presentation.Effect;
using LBoL.Presentation.Units;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public sealed class Laser : Projectile
	{
		private bool Attack { get; set; }
		private Vector2 Offset { get; set; }
		private Vector2 Size { get; set; }
		private float Start { get; set; }
		private int AttackTimes { get; set; }
		private LaserConfig Config { get; set; }
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
			GunManager.AttackingLasers.Remove(this);
			Object.Destroy(base.gameObject);
		}
		public void TryToAttack()
		{
			if ((float)base.LifeTick >= (float)this.AttackTimes * base.Launcher.Piece.HitInterval + this.Start)
			{
				this.InternalAttack();
				int attackTimes = this.AttackTimes;
				this.AttackTimes = attackTimes + 1;
			}
		}
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
		private void Graze(Transform hitPoint)
		{
			AudioManager.PlaySfx(this.Config.GrazeSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.GrazeEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}
		private void HitBlock(Transform hitPoint)
		{
			AudioManager.PlaySfx(this.Config.HitBlockSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitBlockEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}
		private void HitShield(Transform hitPoint)
		{
			AudioManager.PlaySfx(this.Config.HitShieldSfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitShieldEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}
		private void HitBody(Transform hitPoint)
		{
			AudioManager.PlaySfx(base.Piece.LaunchSfx.IsNullOrEmpty() ? this.Config.HitBodySfx : base.Piece.HitBodySfx, -1f);
			EffectWidget effectWidget = EffectManager.CreateEffect(base.HitBodyEffect, hitPoint, 0f, default(float?), false, true);
			base.ModifyEffect(effectWidget);
		}
		public BoxCollider2D laserCollider;
		private bool _isVanishing;
		private float _vanishCountDown;
		private Vector3 _preVanishScale;
	}
}
