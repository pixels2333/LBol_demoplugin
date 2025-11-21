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
	// Token: 0x02000016 RID: 22
	public sealed class DollView : MonoBehaviour
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000196 RID: 406 RVA: 0x000083F7 File Offset: 0x000065F7
		// (set) Token: 0x06000197 RID: 407 RVA: 0x000083FF File Offset: 0x000065FF
		public DollInfoWidget InfoWidget { get; private set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000198 RID: 408 RVA: 0x00008408 File Offset: 0x00006608
		// (set) Token: 0x06000199 RID: 409 RVA: 0x00008410 File Offset: 0x00006610
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

		// Token: 0x0600019A RID: 410 RVA: 0x000084B4 File Offset: 0x000066B4
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

		// Token: 0x0600019B RID: 411 RVA: 0x00008500 File Offset: 0x00006700
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

		// Token: 0x0600019C RID: 412 RVA: 0x00008589 File Offset: 0x00006789
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

		// Token: 0x0600019D RID: 413 RVA: 0x000085BC File Offset: 0x000067BC
		private void RegisterEventHandlers(Doll doll)
		{
			doll.PropertyChanged += new Action(this.OnChanged);
			doll.PassiveActivating += new Action(this.OnPassiveActivating);
			doll.ActiveActivating += new Action(this.OnActiveActivating);
		}

		// Token: 0x0600019E RID: 414 RVA: 0x000085F4 File Offset: 0x000067F4
		private void UnregisterEventHandlers(Doll doll)
		{
			doll.PropertyChanged -= new Action(this.OnChanged);
			doll.PassiveActivating -= new Action(this.OnPassiveActivating);
			doll.ActiveActivating -= new Action(this.OnActiveActivating);
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000862C File Offset: 0x0000682C
		private void LateUpdate()
		{
			if (this._changed)
			{
				this._changed = false;
				this.InfoWidget.Refresh();
			}
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00008648 File Offset: 0x00006848
		public void OnChanged()
		{
			this._changed = true;
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x00008651 File Offset: 0x00006851
		private void OnPassiveActivating()
		{
			this.ShowTriggerEffect();
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00008659 File Offset: 0x00006859
		private void OnActiveActivating()
		{
			this.ShowTriggerEffect();
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00008661 File Offset: 0x00006861
		private void ShowTriggerEffect()
		{
			this.spriteRenderer.DOFade(0f, 0.1f).SetLoops(2, LoopType.Yoyo).SetLink(base.gameObject);
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x0000868C File Offset: 0x0000688C
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

		// Token: 0x060001A5 RID: 421 RVA: 0x00008774 File Offset: 0x00006974
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

		// Token: 0x060001A6 RID: 422 RVA: 0x000087A0 File Offset: 0x000069A0
		private void ShowEndActs()
		{
			this._gunInShooting = null;
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000087A9 File Offset: 0x000069A9
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

		// Token: 0x060001A8 RID: 424 RVA: 0x000087BF File Offset: 0x000069BF
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

		// Token: 0x04000090 RID: 144
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		// Token: 0x04000092 RID: 146
		private bool _changed;

		// Token: 0x04000093 RID: 147
		private Doll _doll;

		// Token: 0x04000094 RID: 148
		private Gun _gunInShooting;

		// Token: 0x04000095 RID: 149
		private bool _shootCounting;

		// Token: 0x04000096 RID: 150
		private float _shootTime;
	}
}
