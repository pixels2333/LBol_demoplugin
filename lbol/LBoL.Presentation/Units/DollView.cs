using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
namespace LBoL.Presentation.Units
{
	public sealed class DollView : MonoBehaviour
	{
		public DollInfoWidget InfoWidget { get; private set; }
		public Doll Doll
		{
			get
			{
				return this._doll;
			}
			set
			{
				if (this._doll == value)
				{
					return;
				}
				if (base.isActiveAndEnabled)
				{
					if (this._doll != null)
					{
						this.UnregisterEventHandlers(this._doll);
					}
					if (value != null)
					{
						this.RegisterEventHandlers(value);
						this.InfoWidget = UiManager.GetPanel<UnitStatusHud>().CreateDollWidget(value);
						this.InfoWidget.TargetTransform = base.transform;
						this.InfoWidget.name = "DollInfo: " + base.name;
					}
				}
				this._doll = value;
				this._changed = true;
				if (this._doll != null)
				{
					this.LoadSpriteAsync(this._doll.Id);
				}
			}
		}
		private async UniTask LoadSpriteAsync(string dollName)
		{
			SpriteRenderer spriteRenderer = this.spriteRenderer;
			Sprite sprite = await ResourcesHelper.LoadSimpleDollSpriteAsync(dollName);
			spriteRenderer.sprite = sprite;
			spriteRenderer = null;
			this.spriteRenderer.color = Color.white;
			if (this.Doll.Config.Flip)
			{
				this.spriteRenderer.flipX = true;
			}
		}
		private void OnEnable()
		{
			if (this._doll != null)
			{
				this.RegisterEventHandlers(this._doll);
				this.InfoWidget = UiManager.GetPanel<UnitStatusHud>().CreateDollWidget(this._doll);
				this.InfoWidget.TargetTransform = base.transform;
				this.InfoWidget.name = "DollInfo: " + base.name;
			}
			else if (this.InfoWidget)
			{
				Object.Destroy(this.InfoWidget.gameObject);
			}
			this._changed = true;
		}
		private void OnDisable()
		{
			if (this._doll != null)
			{
				this.UnregisterEventHandlers(this._doll);
			}
			if (this.InfoWidget)
			{
				Object.Destroy(this.InfoWidget.gameObject);
			}
		}
		private void RegisterEventHandlers(Doll doll)
		{
			doll.PropertyChanged += new Action(this.OnChanged);
			doll.PassiveActivating += new Action(this.OnPassiveActivating);
			doll.ActiveActivating += new Action(this.OnActiveActivating);
		}
		private void UnregisterEventHandlers(Doll doll)
		{
			doll.PropertyChanged -= new Action(this.OnChanged);
			doll.PassiveActivating -= new Action(this.OnPassiveActivating);
			doll.ActiveActivating -= new Action(this.OnActiveActivating);
		}
		private void LateUpdate()
		{
			if (this._changed)
			{
				this._changed = false;
				this.InfoWidget.Refresh();
			}
		}
		public void OnChanged()
		{
			this._changed = true;
		}
		private void OnPassiveActivating()
		{
			this.ShowTriggerEffect();
		}
		private void OnActiveActivating()
		{
			this.ShowTriggerEffect();
		}
		private void ShowTriggerEffect()
		{
			this.spriteRenderer.DOFade(0f, 0.1f).SetLoops(2, LoopType.Yoyo).SetLink(base.gameObject);
		}
		public void Tick()
		{
			if (this._shootCounting)
			{
				this._shootTime += 0.016666668f;
				if (!this._gunInShooting.LastWaveHitFlag && this._gunInShooting.ForceHitTime != null)
				{
					float shootTime = this._shootTime;
					float? forceHitTime = this._gunInShooting.ForceHitTime;
					if ((shootTime >= forceHitTime.GetValueOrDefault()) & (forceHitTime != null))
					{
						if (this._gunInShooting.ForceHitAnimation)
						{
							foreach (UnitView unitView in this._gunInShooting.Targets)
							{
								unitView.Hit(true, this._gunInShooting.ForceHitAnimationSpeed, true);
							}
						}
						this._gunInShooting.LastWaveHit();
					}
				}
			}
		}
		public IEnumerator Shoot(string gunName, GunType type, UnitView mainTarget, List<UnitView> targets)
		{
			Gun gun = GunManager.CreateGun(gunName);
			gun.ShootV2 = new Vector2(0.5f, 0.1f);
			gun.transform.position = base.transform.position + gun.ShootV2;
			gun.Aim(true, mainTarget, targets);
			if (type == GunType.Single)
			{
				this._gunInShooting = GunManager.GunShoot(gun, false);
				yield return base.StartCoroutine(this.WaitForShowEnd(this._gunInShooting.ShootEnd));
				this.ShowEndActs();
				yield break;
			}
			throw new InvalidOperationException("Doll can only shoot a direct gun now.");
		}
		private void ShowEndActs()
		{
			this._gunInShooting = null;
		}
		private IEnumerator WaitForShowEnd(float shootEnd)
		{
			float shootMinTime = Mathf.Max(shootEnd, 0.1f);
			this.ShootTimeSwitch(true);
			yield return new WaitUntil(() => this._gunInShooting.LastWaveHitFlag || this._shootTime > 5f);
			GameDirector.OnGunHit();
			yield return new WaitUntil(() => this._shootTime > shootMinTime || this._shootTime > 5f);
			this.ShootTimeSwitch(false);
			yield break;
		}
		private void ShootTimeSwitch(bool on)
		{
			if (on)
			{
				this._shootCounting = true;
				this._shootTime = 0f;
				return;
			}
			this._shootCounting = false;
		}
		[SerializeField]
		private SpriteRenderer spriteRenderer;
		private bool _changed;
		private Doll _doll;
		private Gun _gunInShooting;
		private bool _shootCounting;
		private float _shootTime;
	}
}
