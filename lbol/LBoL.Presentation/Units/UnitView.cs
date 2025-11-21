using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.Effect;
using LBoL.Presentation.Environments;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units.SpecialUnits;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace LBoL.Presentation.Units
{
	// Token: 0x0200001A RID: 26
	public sealed class UnitView : MonoBehaviour, IRinView, IEnemyUnitView, IUnitView, IKokoroView, IDoremyView
	{
		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000212 RID: 530 RVA: 0x0000A289 File Offset: 0x00008489
		// (set) Token: 0x06000213 RID: 531 RVA: 0x0000A291 File Offset: 0x00008491
		public UnitView Target { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000214 RID: 532 RVA: 0x0000A29A File Offset: 0x0000849A
		// (set) Token: 0x06000215 RID: 533 RVA: 0x0000A2A2 File Offset: 0x000084A2
		public List<UnitView> Targets { get; set; } = new List<UnitView>();

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000216 RID: 534 RVA: 0x0000A2AB File Offset: 0x000084AB
		public Collider2D ActiveCollider
		{
			get
			{
				if (base.gameObject == null || !base.gameObject.activeInHierarchy)
				{
					return null;
				}
				if (!this._circleCollider.enabled)
				{
					return this.BoxCollider;
				}
				return this._circleCollider;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000217 RID: 535 RVA: 0x0000A2E4 File Offset: 0x000084E4
		// (set) Token: 0x06000218 RID: 536 RVA: 0x0000A2EC File Offset: 0x000084EC
		public BoxCollider2D BoxCollider { get; private set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000219 RID: 537 RVA: 0x0000A2F5 File Offset: 0x000084F5
		// (set) Token: 0x0600021A RID: 538 RVA: 0x0000A2FD File Offset: 0x000084FD
		public HitType HitType { get; private set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600021B RID: 539 RVA: 0x0000A306 File Offset: 0x00008506
		// (set) Token: 0x0600021C RID: 540 RVA: 0x0000A30E File Offset: 0x0000850E
		public bool WillCrash { get; private set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600021D RID: 541 RVA: 0x0000A317 File Offset: 0x00008517
		// (set) Token: 0x0600021E RID: 542 RVA: 0x0000A320 File Offset: 0x00008520
		public DamageInfo ComingDamage
		{
			get
			{
				return this._comingDamage;
			}
			set
			{
				this._comingDamage = value;
				this.HitType = (this._comingDamage.IsGrazed ? HitType.Graze : ((this._comingDamage.DamageBlocked > 0f) ? HitType.Block : ((this._comingDamage.DamageShielded > 0f) ? HitType.Shield : ((this._comingDamage.Damage > 0f) ? HitType.Body : (this.HasBlock ? HitType.Block : (this.HasShield ? HitType.Shield : HitType.Body))))));
				if (this.HitType == HitType.Block)
				{
					this._bothShield = this._comingDamage.DamageShielded > 0f;
				}
				HitType hitType = this.HitType;
				if (hitType == HitType.Block || hitType == HitType.Shield)
				{
					this.WillCrash = this._comingDamage.Damage > 0f;
				}
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600021F RID: 543 RVA: 0x0000A3E8 File Offset: 0x000085E8
		// (set) Token: 0x06000220 RID: 544 RVA: 0x0000A3F0 File Offset: 0x000085F0
		private EffectWidget BlockEffect { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000221 RID: 545 RVA: 0x0000A3F9 File Offset: 0x000085F9
		// (set) Token: 0x06000222 RID: 546 RVA: 0x0000A401 File Offset: 0x00008601
		private EffectWidget ShieldEffect { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000223 RID: 547 RVA: 0x0000A40A File Offset: 0x0000860A
		// (set) Token: 0x06000224 RID: 548 RVA: 0x0000A412 File Offset: 0x00008612
		private bool HasShield
		{
			get
			{
				return this._hasShield;
			}
			set
			{
				this._hasShield = value;
				this.ShieldEffect.gameObject.SetActive(value);
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000225 RID: 549 RVA: 0x0000A42C File Offset: 0x0000862C
		// (set) Token: 0x06000226 RID: 550 RVA: 0x0000A434 File Offset: 0x00008634
		private bool HasBlock
		{
			get
			{
				return this._hasBlock;
			}
			set
			{
				this._hasBlock = value;
				this.BlockEffect.gameObject.SetActive(value);
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000227 RID: 551 RVA: 0x0000A44E File Offset: 0x0000864E
		// (set) Token: 0x06000228 RID: 552 RVA: 0x0000A456 File Offset: 0x00008656
		private bool Guarding { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000229 RID: 553 RVA: 0x0000A45F File Offset: 0x0000865F
		// (set) Token: 0x0600022A RID: 554 RVA: 0x0000A467 File Offset: 0x00008667
		private float GuardTime { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600022B RID: 555 RVA: 0x0000A470 File Offset: 0x00008670
		// (set) Token: 0x0600022C RID: 556 RVA: 0x0000A478 File Offset: 0x00008678
		private float GuardEffectTime { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600022D RID: 557 RVA: 0x0000A481 File Offset: 0x00008681
		// (set) Token: 0x0600022E RID: 558 RVA: 0x0000A489 File Offset: 0x00008689
		private Coroutine GuardCoroutine { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600022F RID: 559 RVA: 0x0000A492 File Offset: 0x00008692
		// (set) Token: 0x06000230 RID: 560 RVA: 0x0000A49A File Offset: 0x0000869A
		private bool Crashing { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000231 RID: 561 RVA: 0x0000A4A3 File Offset: 0x000086A3
		// (set) Token: 0x06000232 RID: 562 RVA: 0x0000A4AB File Offset: 0x000086AB
		private global::Spine.AnimationState State { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000233 RID: 563 RVA: 0x0000A4B4 File Offset: 0x000086B4
		private List<global::Spine.AnimationState> AllStates { get; } = new List<global::Spine.AnimationState>();

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000234 RID: 564 RVA: 0x0000A4BC File Offset: 0x000086BC
		// (set) Token: 0x06000235 RID: 565 RVA: 0x0000A4C4 File Offset: 0x000086C4
		private bool SpineLoaded { get; set; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000236 RID: 566 RVA: 0x0000A4CD File Offset: 0x000086CD
		private List<global::Spine.Animation> AllSpineAnimations { get; } = new List<global::Spine.Animation>();

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000237 RID: 567 RVA: 0x0000A4D5 File Offset: 0x000086D5
		private List<string> AllAnimationsNames { get; } = new List<string>();

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000238 RID: 568 RVA: 0x0000A4DD File Offset: 0x000086DD
		// (set) Token: 0x06000239 RID: 569 RVA: 0x0000A4E5 File Offset: 0x000086E5
		private float LifeTime { get; set; }

		// Token: 0x0600023A RID: 570 RVA: 0x0000A4F0 File Offset: 0x000086F0
		public void Tick()
		{
			this.LifeTime += 0.016666668f;
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
			if (this._shooterCounting)
			{
				this._shooterTime += 0.016666668f;
			}
			if (!this.IsHidden)
			{
				if (this._blinking)
				{
					this._blinkTimer -= 0.016666668f;
					if (this._blinkTimer < 0f)
					{
						this.Blink();
					}
				}
				if (this._posing && GameMaster.ShowPoseAnimation)
				{
					this._poseTimer -= 0.016666668f / Time.timeScale;
					if (this._poseTimer < 0f)
					{
						this.Pose();
					}
				}
			}
			if (this.Sequencing)
			{
				if (this.CurrentTrack == null)
				{
					this.Sequencing = false;
					this.SpineIdle(true);
					return;
				}
				if (this.SequenceIndicator >= this.SequenceKeys.Count || this.CurrentTrack.IsComplete)
				{
					this.CurrentTrack.TimeScale = 1f;
					this.Sequencing = false;
					this.CurrentTrack = null;
					this.SpineIdle(true);
					return;
				}
				this.SequenceTimer += 0.016666668f;
				Vector2 vector = this.SequenceKeys[this.SequenceIndicator];
				if (this.SequenceTimer >= vector.x)
				{
					this.CurrentTrack.TimeScale = vector.y;
					int num = this.SequenceIndicator + 1;
					this.SequenceIndicator = num;
				}
			}
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0000A730 File Offset: 0x00008930
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

		// Token: 0x0600023C RID: 572 RVA: 0x0000A74F File Offset: 0x0000894F
		private void ShooterTimeSwitch(bool on)
		{
			if (on)
			{
				this._shooterCounting = true;
				this._shooterTime = 0f;
				return;
			}
			this._shootCounting = false;
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0000A76E File Offset: 0x0000896E
		public IEnumerator DieViewer()
		{
			this._statusWidget.SetVisible(false, false);
			yield return this.DieRunner();
			yield break;
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000A77D File Offset: 0x0000897D
		private IEnumerator DieRunner()
		{
			this.DeathAnimation();
			string text = "UnitDeath";
			float num = 1f;
			switch (this._dieLevel)
			{
			case 0:
				text = "UnitDeathSmall";
				num = 0.5f;
				break;
			case 1:
				text = "UnitDeath";
				num = 1f;
				break;
			case 2:
				text = "UnitDeathLarge";
				num = 1.8f;
				break;
			default:
				Debug.Log(((this != null) ? this.ToString() : null) + "配置的DieLargeExplode未定义");
				break;
			}
			EffectManager.CreateEffect(text, base.transform, 0f, default(float?), false, true);
			yield return new WaitForSeconds(num);
			switch (this._dieLevel)
			{
			case 0:
				AudioManager.PlaySfx("UnitDeathExplodeSmall", -1f);
				break;
			case 1:
				AudioManager.PlaySfx("UnitDeathExplode", -1f);
				break;
			case 2:
				AudioManager.PlaySfx("UnitDeathExplodeLarge", -1f);
				break;
			}
			this.Die();
			yield break;
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000A78C File Offset: 0x0000898C
		public IEnumerator EscapeViewer(int performType = 0)
		{
			this._statusWidget.SetVisible(false, false);
			if (performType != 1)
			{
				if (performType != 2)
				{
					yield return DOTween.Sequence().Append(base.transform.DOScaleX(-1f, 0.2f)).Append(base.transform.DOLocalMoveX(0.5f, 0.3f, false))
						.SetEase(Ease.InSine)
						.SetUpdate(true)
						.SetAutoKill(true)
						.WaitForCompletion();
				}
				else
				{
					this.SetAnimation("shoot2", 1f, true);
					yield return new WaitForSeconds(1f);
				}
			}
			else
			{
				yield return DOTween.Sequence().Join(base.transform.DOScale(new Vector3(0.7f, 1.3f, 1f), 0.2f).SetEase(Ease.OutQuad)).Join(base.transform.DOLocalMoveY(0.5f, 0.2f, false).SetEase(Ease.OutQuad))
					.SetUpdate(true)
					.SetAutoKill(true)
					.WaitForCompletion();
			}
			this.Escape();
			yield break;
		}

		// Token: 0x06000240 RID: 576 RVA: 0x0000A7A2 File Offset: 0x000089A2
		public IEnumerator SpellDeclare(string spellName)
		{
			if (spellName.IsNullOrEmpty())
			{
				yield break;
			}
			this.SpellDeclareHandler(spellName);
			yield return new WaitForSeconds(2f);
			yield break;
		}

		// Token: 0x06000241 RID: 577 RVA: 0x0000A7B8 File Offset: 0x000089B8
		private void SpellDeclareHandler(string spellName)
		{
			if (this.IsPlayer)
			{
				this.SpellAnimation();
			}
			AudioManager.PlaySfx("SpellDeclare", -1f);
			SpellConfig spellConfig = SpellConfig.FromId(spellName);
			if (spellConfig == null)
			{
				spellName = "未填写符卡名";
				spellConfig = SpellConfig.FromId(spellName);
				if (spellConfig == null)
				{
					throw new InvalidOperationException("丢失了默认SpellConfig字段");
				}
			}
			SpellPanel panel = UiManager.GetPanel<SpellPanel>();
			panel.SetSpellNameByKey(spellName, spellConfig.Resource);
			panel.CallSpellDeclare(this._spellPortrait, this._spellColors ?? this.GetDefaultSpellColors(), this._spellV2.x, this._spellV2.y, this._spellScale);
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000A851 File Offset: 0x00008A51
		public IEnumerator Shoot(string gunName, GunType type)
		{
			Gun gun = GunManager.CreateGun(gunName);
			this.SetGunByAnimation(gun);
			gun.Aim(this.Unit is PlayerUnit, this.Target, this.Targets);
			switch (type)
			{
			case GunType.Single:
				yield return this.ShootDirect(gun);
				break;
			case GunType.First:
				yield return this.ShootComplexStart(gun);
				yield return this.ShootComplexFire(gun, true);
				break;
			case GunType.Middle:
				yield return this.ShootComplexFire(gun, false);
				break;
			case GunType.Last:
				yield return this.ShootComplexFire(gun, false);
				yield return this.ShootComplexEnd();
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
			yield break;
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000A870 File Offset: 0x00008A70
		public void PerformShoot(string gunName)
		{
			Gun gun = GunManager.CreateGun(gunName);
			this.SetGunByAnimation(gun);
			gun.Aim(this.Unit is PlayerUnit, this.Target, this.Targets);
			base.StartCoroutine(this.PerformShootRunner(gun));
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000A8B9 File Offset: 0x00008AB9
		private IEnumerator PerformShootRunner(Gun gun)
		{
			yield return this.SpellDeclare(gun.Spell);
			GunManager.GunShoot(gun, false);
			yield break;
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0000A8CF File Offset: 0x00008ACF
		private IEnumerator ShootDirect(Gun gun)
		{
			yield return this.SpellDeclare(gun.Spell);
			if (this._status == UnitView.ShootStatus.Idle)
			{
				this.ShootStartActs(gun, UnitView.ShootStatus.Direct);
				this._gunInShooting = GunManager.GunShoot(gun, false);
				this.ShootEndActs(this._gunInShooting, false);
				yield return base.StartCoroutine(this.WaitForShowEnd(this._gunInShooting.ShootEnd));
				this.ShowEndActs();
			}
			else
			{
				Debug.LogWarning("在多段射击过程中触发了另一次射击。");
				yield return null;
			}
			yield break;
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0000A8E5 File Offset: 0x00008AE5
		private IEnumerator ShootComplexStart(Gun gun)
		{
			yield return this.SpellDeclare(gun.Spell);
			if (this._status == UnitView.ShootStatus.Idle)
			{
				this.ShootStartActs(gun, UnitView.ShootStatus.Complex);
				yield return new WaitForSeconds(gun.StartTime);
				this.ShooterTimeSwitch(true);
			}
			else
			{
				Debug.LogWarning("在多段射击过程中触发了另一次射击。");
				yield return null;
			}
			yield break;
		}

		// Token: 0x06000247 RID: 583 RVA: 0x0000A8FB File Offset: 0x00008AFB
		private IEnumerator ShootComplexFire(Gun gun, bool first = false)
		{
			if (this._status == UnitView.ShootStatus.Complex)
			{
				this._gunInShooting = GunManager.GunShoot(gun, true);
				if (first)
				{
					this._complexFirstGun = this._gunInShooting;
				}
				yield return base.StartCoroutine(this.WaitForShowEnd(this._gunInShooting.ShootEnd));
			}
			else
			{
				Debug.LogWarning("在多段射击过程中触发了另一次射击。");
				yield return null;
			}
			yield break;
		}

		// Token: 0x06000248 RID: 584 RVA: 0x0000A918 File Offset: 0x00008B18
		private IEnumerator ShootComplexEnd()
		{
			if (this._status == UnitView.ShootStatus.Complex)
			{
				yield return new WaitUntil(() => this._shooterTime > 0.3f);
				this.ShooterTimeSwitch(false);
				this.ShootEndActs(this._complexFirstGun, true);
				yield return new WaitForSeconds(0.3f);
				this.ShowEndActs();
			}
			else
			{
				Debug.LogWarning("在多段射击过程中触发了另一次射击。");
				yield return null;
			}
			yield break;
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000A927 File Offset: 0x00008B27
		public IEnumerator ForceEndShoot()
		{
			if (this._status == UnitView.ShootStatus.Complex)
			{
				yield return this.ShootComplexForceEnd();
			}
			yield break;
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000A936 File Offset: 0x00008B36
		private IEnumerator ShootComplexForceEnd()
		{
			if (this._status == UnitView.ShootStatus.Complex)
			{
				yield return new WaitForSeconds(0.1f);
				this.ShooterTimeSwitch(false);
				this.ShootEndActs(this._gunInShooting, true);
				yield return new WaitForSeconds(0.3f);
				this.ShowEndActs();
				yield break;
			}
			throw new InvalidOperationException("ComplexForceEnd called in wrong ShootType.");
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000A945 File Offset: 0x00008B45
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

		// Token: 0x0600024C RID: 588 RVA: 0x0000A95C File Offset: 0x00008B5C
		private EffectWidget SpawnShooter(string shooterName, float delay)
		{
			this._shooterName = shooterName;
			Transform transform = this.shooterPoint;
			EffectManager.CreateEffect("Shooter" + shooterName + "Start", transform, true);
			return EffectManager.CreateEffect("Shooter" + shooterName + "Loop", transform, delay, new float?(0f), true, true);
		}

		// Token: 0x0600024D RID: 589 RVA: 0x0000A9B4 File Offset: 0x00008BB4
		private void DestroyShooter()
		{
			if (this._shooterName != null)
			{
				Transform transform = this.shooterPoint;
				Object.Destroy(this._shooterLoopEffect.gameObject);
				EffectManager.CreateEffect("Shooter" + this._shooterName + "End", transform, true);
				this._shooterName = null;
			}
		}

		// Token: 0x0600024E RID: 590 RVA: 0x0000AA04 File Offset: 0x00008C04
		private void ShootStartActs(Gun gun, UnitView.ShootStatus status)
		{
			this._status = status;
			if (gun.Shooter != null && gun.Shooter != "Direct")
			{
				this._shooterLoopEffect = this.SpawnShooter(gun.Shooter, gun.StartTime);
			}
			if (gun.HasAnimation)
			{
				if (gun.Sequence.IsNullOrEmpty())
				{
					this.ShootStartAnimation(gun.Animation, 1f);
					return;
				}
				SequenceConfig sequenceConfig = SequenceConfig.FromId(gun.Sequence);
				if (sequenceConfig == null)
				{
					this.ShootStartAnimation(gun.Animation, 1f);
					return;
				}
				List<float> list = Enumerable.ToList<float>(sequenceConfig.KeyTime);
				List<float> list2 = Enumerable.ToList<float>(sequenceConfig.Speed);
				float num = list[0];
				List<float> list3 = new List<float>();
				list3.Add(num);
				List<float> list4 = list3;
				for (int i = 1; i < list.Count; i++)
				{
					float num2 = Mathf.Max(list[i] - list[i - 1], 0f);
					num += num2 / list2[i - 1];
					list4.Add(num);
				}
				this.ShootStartAnimation(sequenceConfig.Animation, list4, list2);
			}
		}

		// Token: 0x0600024F RID: 591 RVA: 0x0000AB1E File Offset: 0x00008D1E
		private void ShootEndActs(Gun gun, bool instant)
		{
			base.StartCoroutine(this.ShootEndActsRunner(gun, instant));
		}

		// Token: 0x06000250 RID: 592 RVA: 0x0000AB2F File Offset: 0x00008D2F
		private IEnumerator ShootEndActsRunner(Gun gun, bool instant)
		{
			if (!instant)
			{
				yield return new WaitForSeconds(Mathf.Max(gun.ShootEnd, 0.5f));
			}
			this.DestroyShooter();
			if (gun.HasAnimation)
			{
				this.ShootEndAnimation(gun.Animation, 1f);
			}
			yield break;
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000AB4C File Offset: 0x00008D4C
		private void ShowEndActs()
		{
			this._gunInShooting = null;
			this._status = UnitView.ShootStatus.Idle;
		}

		// Token: 0x06000252 RID: 594 RVA: 0x0000AB5C File Offset: 0x00008D5C
		public IEnumerator GainBlockShieldToSelfViewer(CastBlockShieldAction action)
		{
			bool flag = action.Args.Shield > 0f;
			bool flag2 = action.Args.Block > 0f;
			ActionCause cause = action.Cause;
			if (cause == ActionCause.Card || cause == ActionCause.Us || cause == ActionCause.EnemyAction)
			{
				if (action.Cast)
				{
					yield return this.CastShieldToSelf(flag, flag2);
				}
				else
				{
					yield return this.GainShieldToSelf(flag, flag2);
				}
			}
			else
			{
				yield return this.GainShieldToSelf(flag, flag2);
			}
			yield break;
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000AB72 File Offset: 0x00008D72
		private IEnumerator CastShieldToSelf(bool shield, bool block)
		{
			this.DefendAnimation();
			yield return new WaitForSeconds(0.2f);
			if (shield)
			{
				this.CreateLocalShieldEffect("CastShield", true);
			}
			else if (block)
			{
				this.CreateLocalShieldEffect("CastBlock", false);
			}
			AudioManager.PlaySfx("ShieldCast", -1f);
			yield return new WaitForSeconds(0.2f);
			this.UpdateShieldColliders();
			if (this._statusWidget)
			{
				this._statusWidget.OnBlockShieldChanged();
			}
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000AB8F File Offset: 0x00008D8F
		private IEnumerator GainShieldToSelf(bool shield, bool block)
		{
			if (shield)
			{
				this.CreateLocalShieldEffect("GainShield", true);
			}
			if (block)
			{
				this.CreateLocalShieldEffect("GainBlock", false);
			}
			AudioManager.PlaySfx("ShieldCast", -1f);
			yield return new WaitForSeconds(0.2f);
			this.UpdateShieldColliders();
			if (this._statusWidget)
			{
				this._statusWidget.OnBlockShieldChanged();
			}
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000ABAC File Offset: 0x00008DAC
		public void CastShieldToOther(bool cast = true)
		{
			if (cast)
			{
				this.DefendAnimation();
			}
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000ABB7 File Offset: 0x00008DB7
		public IEnumerator ReceiveShieldViewer(CastBlockShieldAction action)
		{
			bool shield = action.Args.Shield > 0f;
			bool block = action.Args.Block > 0f;
			if (action.Cast)
			{
				yield return new WaitForSeconds(0.2f);
			}
			AudioManager.PlaySfx("ShieldCast", -1f);
			if (shield)
			{
				this.CreateLocalShieldEffect("CastShield", true);
			}
			else if (block)
			{
				this.CreateLocalShieldEffect("CastBlock", false);
			}
			yield return new WaitForSeconds(0.2f);
			this.UpdateShieldColliders();
			if (this._statusWidget)
			{
				this._statusWidget.OnBlockShieldChanged();
			}
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000ABCD File Offset: 0x00008DCD
		public IEnumerator LoseBlockShieldViewer(LoseBlockShieldAction action)
		{
			this.UpdateShieldColliders();
			if (this._statusWidget)
			{
				this._statusWidget.OnBlockShieldChanged();
			}
			bool flag = false;
			bool flag2 = false;
			if (action.Args.Shield != 0f && this.Unit.Shield == 0)
			{
				flag = true;
			}
			if (action.Args.Block != 0f && this.Unit.Block == 0)
			{
				flag2 = true;
			}
			if (flag || flag2)
			{
				yield return this.LoseShieldEffect(flag, flag2);
			}
			yield break;
		}

		// Token: 0x06000258 RID: 600 RVA: 0x0000ABE3 File Offset: 0x00008DE3
		private IEnumerator LoseShieldEffect(bool shield, bool block)
		{
			if (shield)
			{
				this.CreateLocalShieldEffect("LoseShield", true);
			}
			if (block)
			{
				this.CreateLocalShieldEffect("LoseBlock", false);
			}
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x06000259 RID: 601 RVA: 0x0000AC00 File Offset: 0x00008E00
		private void UpdateShieldColliders()
		{
			this.HasShield = this.Unit.Shield != 0;
			this.HasBlock = this.Unit.Block != 0;
			if (this.HasBlock || this.HasShield)
			{
				this.BoxCollider.enabled = false;
				this._circleCollider.enabled = true;
			}
			else
			{
				this.BoxCollider.enabled = true;
				this._circleCollider.enabled = false;
			}
			if (this.HasBlock)
			{
				this._circleCollider.radius = this._blockRadius;
				return;
			}
			if (this.HasShield)
			{
				this._circleCollider.radius = this._shieldRadius;
			}
		}

		// Token: 0x0600025A RID: 602 RVA: 0x0000ACAC File Offset: 0x00008EAC
		private EffectWidget CreateEternalShieldEffect(string effectName, bool isShield = true)
		{
			EffectWidget effectWidget = EffectManager.CreateEffect(effectName, this.effectRoot, 0f, default(float?), true, false);
			EffectManager.ModifyEffect(effectWidget, isShield ? this._shieldRadius : this._blockRadius, 0);
			return effectWidget;
		}

		// Token: 0x0600025B RID: 603 RVA: 0x0000ACED File Offset: 0x00008EED
		private void CreateLocalShieldEffect(string effectName, bool isShield = true)
		{
			EffectManager.ModifyEffect(EffectManager.CreateEffect(effectName, this.effectRoot, true), isShield ? this._shieldRadius : this._blockRadius, 0);
		}

		// Token: 0x0600025C RID: 604 RVA: 0x0000AD14 File Offset: 0x00008F14
		public void Hit(bool lastWave = false, float animationSpeed = 1f, bool ignoreCoolDown = false)
		{
			switch (this.HitType)
			{
			case HitType.Graze:
				this.GrazeAnimation(animationSpeed);
				return;
			case HitType.Block:
				if (!this.ComingDamage.ZeroDamage)
				{
					this.HitBlock(lastWave, animationSpeed);
					return;
				}
				break;
			case HitType.Shield:
				if (!this.ComingDamage.ZeroDamage)
				{
					this.HitShield(lastWave, animationSpeed);
					return;
				}
				break;
			case HitType.Body:
				if (!this.ComingDamage.ZeroDamage)
				{
					this.HitAnimation(animationSpeed, ignoreCoolDown);
					return;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x0600025D RID: 605 RVA: 0x0000AD99 File Offset: 0x00008F99
		private void HitBlock(bool lastWave, float speed)
		{
			if (!this.WillCrash)
			{
				this.Guard(this._bothShield, true, speed);
				return;
			}
			if (this.Crashing || lastWave)
			{
				this.Crash(this._bothShield, true, speed);
				return;
			}
			this.Guard(this._bothShield, true, speed);
		}

		// Token: 0x0600025E RID: 606 RVA: 0x0000ADD9 File Offset: 0x00008FD9
		private void HitShield(bool lastWave, float speed)
		{
			if (!this.WillCrash)
			{
				this.Guard(true, false, speed);
				return;
			}
			if (this.Crashing || lastWave)
			{
				this.Crash(true, false, speed);
				return;
			}
			this.Guard(true, false, speed);
		}

		// Token: 0x0600025F RID: 607 RVA: 0x0000AE0C File Offset: 0x0000900C
		private void Guard(bool shield, bool block, float speed)
		{
			this.GuardAnimation(speed);
			this.GuardTime = 0f;
			if (this.Guarding)
			{
				if (this.GuardEffectTime > 0.2f)
				{
					this.PlayGuardEffect(shield, block);
					return;
				}
			}
			else
			{
				this.GuardCoroutine = base.StartCoroutine(this.GuardRunner(shield, block));
			}
		}

		// Token: 0x06000260 RID: 608 RVA: 0x0000AE5D File Offset: 0x0000905D
		private IEnumerator GuardRunner(bool shield, bool block)
		{
			this.Guarding = true;
			this.PlayGuardEffect(shield, block);
			while (this.GuardTime < 0.3f)
			{
				this.GuardTime += Time.deltaTime;
				this.GuardEffectTime += Time.deltaTime;
				yield return null;
			}
			this.Guarding = false;
			yield break;
		}

		// Token: 0x06000261 RID: 609 RVA: 0x0000AE7A File Offset: 0x0000907A
		private void PlayGuardEffect(bool shield, bool block)
		{
			this.GuardEffectTime = 0f;
			if (shield)
			{
				this.CreateLocalShieldEffect("ShieldActive", true);
			}
			if (block)
			{
				this.CreateLocalShieldEffect("BlockActive", false);
			}
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0000AEA5 File Offset: 0x000090A5
		private void Crash(bool shield, bool block, float speed)
		{
			if (this.Guarding)
			{
				base.StopCoroutine(this.GuardCoroutine);
				this.Guarding = false;
			}
			if (!this.Crashing)
			{
				base.StartCoroutine(this.CrashRunner(shield, block, speed));
			}
		}

		// Token: 0x06000263 RID: 611 RVA: 0x0000AEDA File Offset: 0x000090DA
		private IEnumerator CrashRunner(bool shield, bool block, float speed)
		{
			this.Crashing = true;
			this.CrashAnimation(speed);
			this.UpdateShieldColliders();
			AudioManager.PlaySfx("ShieldCrash", -1f);
			if (shield)
			{
				this.CreateLocalShieldEffect("CrashShield", true);
			}
			if (block)
			{
				this.CreateLocalShieldEffect("CrashBlock", false);
			}
			yield return new WaitForSeconds(0.35f);
			this.HitType = HitType.Body;
			this.Crashing = false;
			yield break;
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0000AF00 File Offset: 0x00009100
		public void HitEnd()
		{
			if (this.WillCrash)
			{
				this.UpdateShieldColliders();
			}
			else
			{
				bool hasShield = this.HasShield;
				bool hasBlock = this.HasBlock;
				this.UpdateShieldColliders();
				bool flag = hasShield && !this.HasShield;
				bool flag2 = hasBlock && !this.HasBlock;
				if (flag)
				{
					this.CreateLocalShieldEffect("LoseShield", true);
				}
				if (flag2)
				{
					this.CreateLocalShieldEffect("LoseBlock", false);
				}
			}
			if (this._statusWidget)
			{
				this._statusWidget.OnBlockShieldChanged();
			}
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000AF84 File Offset: 0x00009184
		private void SpineInitialize()
		{
			if (this.SpineLoaded)
			{
				return;
			}
			this.rendererParent.gameObject.SetActive(true);
			this.spineSkeleton.gameObject.SetActive(true);
			this.spriteRenderer.gameObject.SetActive(false);
			this.State = this.spineSkeleton.state;
			this.AllStates.Add(this.State);
			if (this._kokoroUnitController)
			{
				this.AllStates.Add(this._kokoroUnitController.TopMuskState);
			}
			foreach (global::Spine.AnimationState animationState in this.AllStates)
			{
				animationState.Event += this.OnEvent;
			}
			foreach (global::Spine.Animation animation in this.State.Data.SkeletonData.Animations)
			{
				this.AllSpineAnimations.Add(animation);
				this.AllAnimationsNames.Add(animation.Name);
			}
			if (!this.AllAnimationsNames.Contains("idle"))
			{
				throw new InvalidOperationException(this._modelName + " has no idle animation!");
			}
			if (this.AllAnimationsNames.Contains("blink"))
			{
				this._hasBlink = true;
			}
			if (this.AllAnimationsNames.Contains("pose1"))
			{
				this._hasPose = true;
			}
			if (this.AllAnimationsNames.Contains("fly"))
			{
				this._hasFly = true;
			}
			if (this.Unit is Doremy)
			{
				this.DoremySleeping = true;
			}
			else
			{
				this.SpineIdle(false);
			}
			this.SpineLoaded = true;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0000B160 File Offset: 0x00009360
		private void SpineClear()
		{
			if (!this.SpineLoaded)
			{
				return;
			}
			this.spineSkeleton.gameObject.SetActive(false);
			this.spineSkeleton.transform.localScale = Vector3.one;
			this.State = null;
			this.AllStates.Clear();
			this.AllSpineAnimations.Clear();
			this.AllAnimationsNames.Clear();
			this._hasBlink = false;
			this._hasPose = false;
			this._hasFly = false;
			this.DoremySleeping = false;
			this._spineNeedReload = true;
			this.SpineLoaded = false;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0000B1F0 File Offset: 0x000093F0
		private void OnEvent(TrackEntry trackEntry, global::Spine.Event e)
		{
			SpineEventConfig spineEventConfig = SpineEventConfig.FromName(e.Data.Name);
			if (spineEventConfig != null)
			{
				if (!spineEventConfig.Effect.IsNullOrEmpty())
				{
					EffectWidget effectWidget = EffectManager.CreateEffect(spineEventConfig.Effect, this.effectRoot, false);
					if (this.Flip)
					{
						effectWidget.transform.Rotate(Vector3.up, 180f);
					}
				}
				if (!spineEventConfig.Sfx.IsNullOrEmpty())
				{
					AudioManager.PlaySfx(spineEventConfig.Sfx, -1f);
					return;
				}
			}
			else
			{
				AudioManager.PlaySfx(e.Data.Name, -1f);
			}
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000B281 File Offset: 0x00009481
		public void PlayAnimation(string animationName)
		{
			this.SetAnimation(animationName, 1f, false);
			this.spineSkeleton.Update(0f);
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0000B2A0 File Offset: 0x000094A0
		private void SetAnimation(string order, float speed = 1f, bool stop = false)
		{
			if (this.SpineLoaded)
			{
				this.CurrentTrack = null;
				if (this._hasBlink)
				{
					this.StopBlinking();
				}
				if (this._hasPose)
				{
					this.StopPosing();
				}
				order = order.ToLower();
				if (this.AllAnimationsNames.Contains(order))
				{
					foreach (global::Spine.AnimationState animationState in this.AllStates)
					{
						animationState.SetAnimation(0, order, false).TimeScale = speed;
					}
					if (!stop)
					{
						this.SpineIdle(true);
						return;
					}
				}
				else
				{
					string text = Enumerable.Aggregate<char, string>(Enumerable.Where<char>(order, (char c) => !char.IsNumber(c)), null, (string current, char c) => current + c.ToString());
					if (!this.AllAnimationsNames.Contains(text))
					{
						Debug.LogWarning(base.name + " has no animation: " + order);
						return;
					}
					foreach (global::Spine.AnimationState animationState2 in this.AllStates)
					{
						animationState2.SetAnimation(0, text, false).TimeScale = speed;
					}
					if (!stop)
					{
						this.SpineIdle(true);
						return;
					}
				}
			}
			else
			{
				this.SetTrigger(order);
				this._animator.speed = speed;
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x0600026A RID: 618 RVA: 0x0000B424 File Offset: 0x00009624
		// (set) Token: 0x0600026B RID: 619 RVA: 0x0000B42C File Offset: 0x0000962C
		public bool DoremySleeping
		{
			get
			{
				return this._doremySleeping;
			}
			set
			{
				this._doremySleeping = value;
				if (this._doremySleeping)
				{
					this._hasBlink = false;
					this.SpineIdle(false);
					return;
				}
				this._hasBlink = true;
				this.SpineIdle(false);
			}
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0000B45C File Offset: 0x0000965C
		private void SpineIdle(bool add = false)
		{
			if (this.DoremySleeping)
			{
				if (add)
				{
					using (List<global::Spine.AnimationState>.Enumerator enumerator = this.AllStates.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							global::Spine.AnimationState animationState = enumerator.Current;
							animationState.AddAnimation(0, "sleep", true, 0f);
						}
						goto IL_010D;
					}
				}
				using (List<global::Spine.AnimationState>.Enumerator enumerator = this.AllStates.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						global::Spine.AnimationState animationState2 = enumerator.Current;
						animationState2.SetAnimation(0, "sleep", true);
					}
					goto IL_010D;
				}
			}
			if (add)
			{
				using (List<global::Spine.AnimationState>.Enumerator enumerator = this.AllStates.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						global::Spine.AnimationState animationState3 = enumerator.Current;
						animationState3.AddAnimation(0, "idle", true, 0f);
					}
					goto IL_010D;
				}
			}
			foreach (global::Spine.AnimationState animationState4 in this.AllStates)
			{
				animationState4.SetAnimation(0, "idle", true);
			}
			IL_010D:
			if (this._hasBlink)
			{
				this.StartBlinking();
			}
			if (this._hasPose)
			{
				this.StartPosing();
			}
			if (this._hasFly)
			{
				foreach (global::Spine.AnimationState animationState5 in this.AllStates)
				{
					animationState5.SetAnimation(2, "fly", true);
				}
			}
		}

		// Token: 0x0600026D RID: 621 RVA: 0x0000B618 File Offset: 0x00009818
		private void StartBlinking()
		{
			this._blinkTimer = Random.Range(2f, 5f);
			this._blinking = true;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0000B636 File Offset: 0x00009836
		private void StopBlinking()
		{
			this._blinking = false;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0000B640 File Offset: 0x00009840
		private void Blink()
		{
			this._blinkTimer = Random.Range(2f, 5f);
			foreach (global::Spine.AnimationState animationState in this.AllStates)
			{
				animationState.SetAnimation(1, "blink", false);
			}
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0000B6B0 File Offset: 0x000098B0
		private void StartPosing()
		{
			this._poseTimer = Random.Range(32f, 40f);
			this._posing = true;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000B6CE File Offset: 0x000098CE
		private void StopPosing()
		{
			this._posing = false;
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0000B6D7 File Offset: 0x000098D7
		private void Pose()
		{
			this.SetAnimation("pose1", 1f, false);
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0000B6EC File Offset: 0x000098EC
		private void ShootStartAnimation(string order, float speed = 1f)
		{
			List<float> list = new List<float>();
			list.Add(0f);
			List<float> list2 = list;
			List<float> list3 = new List<float>();
			list3.Add(1f);
			List<float> list4 = list3;
			this.ShootStartAnimation(order, list2, list4);
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0000B724 File Offset: 0x00009924
		private void ShootStartAnimation(string order, List<float> keyTimes, List<float> speeds)
		{
			this._invincible = true;
			if (this.SpineLoaded)
			{
				this.CurrentTrack = null;
				order = order.ToLower();
				if (this.AllAnimationsNames.Contains(order + "start"))
				{
					float num = 1f;
					if (speeds.Count > 0)
					{
						num = speeds[0];
					}
					using (List<global::Spine.AnimationState>.Enumerator enumerator = this.AllStates.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							global::Spine.AnimationState animationState = enumerator.Current;
							animationState.SetAnimation(0, order + "start", false).TimeScale = num;
							animationState.AddAnimation(0, order + "loop", true, 0f);
						}
						return;
					}
				}
				if (this.AllAnimationsNames.Contains(order))
				{
					using (List<global::Spine.AnimationState>.Enumerator enumerator = this.AllStates.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							global::Spine.AnimationState animationState2 = enumerator.Current;
							TrackEntry trackEntry = animationState2.SetAnimation(0, order, false);
							trackEntry.TimeScale = 1f;
							this.StartSequence(trackEntry, keyTimes, speeds);
						}
						return;
					}
				}
				Debug.LogError(base.name + " has no animation: " + order);
				return;
			}
			this.SetTrigger(order);
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000275 RID: 629 RVA: 0x0000B878 File Offset: 0x00009A78
		// (set) Token: 0x06000276 RID: 630 RVA: 0x0000B880 File Offset: 0x00009A80
		private float SequenceTimer { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000277 RID: 631 RVA: 0x0000B889 File Offset: 0x00009A89
		// (set) Token: 0x06000278 RID: 632 RVA: 0x0000B891 File Offset: 0x00009A91
		private TrackEntry CurrentTrack { get; set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000279 RID: 633 RVA: 0x0000B89A File Offset: 0x00009A9A
		private List<Vector2> SequenceKeys { get; } = new List<Vector2>();

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x0600027A RID: 634 RVA: 0x0000B8A2 File Offset: 0x00009AA2
		// (set) Token: 0x0600027B RID: 635 RVA: 0x0000B8AA File Offset: 0x00009AAA
		private int SequenceIndicator { get; set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600027C RID: 636 RVA: 0x0000B8B3 File Offset: 0x00009AB3
		// (set) Token: 0x0600027D RID: 637 RVA: 0x0000B8BB File Offset: 0x00009ABB
		private bool Sequencing { get; set; }

		// Token: 0x0600027E RID: 638 RVA: 0x0000B8C4 File Offset: 0x00009AC4
		private void StartSequence(TrackEntry track, List<float> keyTimes, List<float> speeds)
		{
			this.CurrentTrack = track;
			this.SequenceKeys.Clear();
			if (keyTimes.Count > 0)
			{
				for (int i = 0; i < keyTimes.Count; i++)
				{
					float num = speeds.TryGetValue(i);
					if (num <= 0f)
					{
						num = 1f;
					}
					this.SequenceKeys.Add(new Vector2(keyTimes[i], num));
				}
				this.SequenceTimer = 0f;
				this.SequenceIndicator = 0;
				this.Sequencing = true;
				return;
			}
			throw new InvalidOperationException("Sequence has no keys.");
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000B954 File Offset: 0x00009B54
		private void ShootEndAnimation(string order, float speed = 1f)
		{
			this._invincible = false;
			if (this.SpineLoaded)
			{
				this.CurrentTrack = null;
				order = order.ToLower();
				if (this.AllAnimationsNames.Contains(order + "end"))
				{
					foreach (global::Spine.AnimationState animationState in this.AllStates)
					{
						animationState.SetAnimation(0, order + "end", false).TimeScale = speed;
					}
				}
				this.SpineIdle(true);
				return;
			}
			if (order == "shoot1")
			{
				this.SetTrigger("shoot1end");
			}
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0000BA10 File Offset: 0x00009C10
		private void DefendAnimation()
		{
			this.SetAnimation("defend", 1f, false);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000BA23 File Offset: 0x00009C23
		private void GrazeAnimation(float speed)
		{
			if (!this._invincible && !this.DoremySleeping)
			{
				this.SetAnimation("graze", speed, false);
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000282 RID: 642 RVA: 0x0000BA42 File Offset: 0x00009C42
		// (set) Token: 0x06000283 RID: 643 RVA: 0x0000BA4A File Offset: 0x00009C4A
		private float LastGuardTime { get; set; }

		// Token: 0x06000284 RID: 644 RVA: 0x0000BA53 File Offset: 0x00009C53
		private void GuardAnimation(float speed = 1f)
		{
			if (!this._invincible && !this.DoremySleeping && this.LifeTime - this.LastGuardTime > this._guardRep)
			{
				this.SetAnimation("guard", speed, false);
				this.LastGuardTime = this.LifeTime;
			}
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0000BA93 File Offset: 0x00009C93
		private void CrashAnimation(float speed)
		{
			if (!this._invincible)
			{
				this.SetAnimation("crash", speed, false);
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000286 RID: 646 RVA: 0x0000BAAA File Offset: 0x00009CAA
		// (set) Token: 0x06000287 RID: 647 RVA: 0x0000BAB2 File Offset: 0x00009CB2
		private float LastHitTime { get; set; }

		// Token: 0x06000288 RID: 648 RVA: 0x0000BABB File Offset: 0x00009CBB
		private void HitAnimation(float speed, bool ignoreCoolDown)
		{
			if (!this._invincible && (ignoreCoolDown || this.LifeTime - this.LastHitTime > this._hitRep))
			{
				this.SetAnimation("hit", speed, false);
				this.LastHitTime = this.LifeTime;
			}
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0000BAF6 File Offset: 0x00009CF6
		public void DeathAnimation()
		{
			this._invincible = true;
			this.SetAnimation("hit", 0.1f, true);
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0000BB10 File Offset: 0x00009D10
		private void SpellAnimation()
		{
			this.SetAnimation("spell", 1f, false);
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000BB24 File Offset: 0x00009D24
		public void DebutAnimation()
		{
			if (this.AllAnimationsNames.Contains("debut"))
			{
				this.PlayAnimation("debut");
				global::Spine.Animation animation = Enumerable.FirstOrDefault<global::Spine.Animation>(this.AllSpineAnimations, (global::Spine.Animation anime) => anime.Name == "debut");
				if (animation != null)
				{
					this.SetStatusVisible(false, true);
					this._fadeInCts = new CancellationTokenSource();
					this.FadeInStatusTask(animation.Duration).Forget();
					return;
				}
			}
			this.SetStatusVisible(true, false);
		}

		// Token: 0x0600028C RID: 652 RVA: 0x0000BBA9 File Offset: 0x00009DA9
		private void CancelFadeIn()
		{
			CancellationTokenSource fadeInCts = this._fadeInCts;
			if (fadeInCts == null)
			{
				return;
			}
			fadeInCts.Cancel();
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0000BBBC File Offset: 0x00009DBC
		private async UniTask FadeInStatusTask(float delay)
		{
			await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, this._fadeInCts.Token, false);
			this.SetStatusVisible(true, false);
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0000BC07 File Offset: 0x00009E07
		private void SetTrigger(string triggerName)
		{
			this._animator.SetTrigger(triggerName);
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x0600028F RID: 655 RVA: 0x0000BC15 File Offset: 0x00009E15
		// (set) Token: 0x06000290 RID: 656 RVA: 0x0000BC1D File Offset: 0x00009E1D
		public Action ClickHandler { get; set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000291 RID: 657 RVA: 0x0000BC26 File Offset: 0x00009E26
		// (set) Token: 0x06000292 RID: 658 RVA: 0x0000BC2E File Offset: 0x00009E2E
		private bool IsPlayer { get; set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000293 RID: 659 RVA: 0x0000BC37 File Offset: 0x00009E37
		// (set) Token: 0x06000294 RID: 660 RVA: 0x0000BC40 File Offset: 0x00009E40
		public bool Flip
		{
			get
			{
				return this._flip;
			}
			set
			{
				if (this._flip == value)
				{
					return;
				}
				this._flip = value;
				Transform transform = this.rendererParent.transform;
				transform.localPosition = transform.localPosition.FlipX();
				transform.localScale = transform.localScale.FlipX();
				Transform transform2 = this.pointsParent.transform;
				transform2.localScale = transform2.localScale.FlipX();
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000295 RID: 661 RVA: 0x0000BCA5 File Offset: 0x00009EA5
		// (set) Token: 0x06000296 RID: 662 RVA: 0x0000BCAD File Offset: 0x00009EAD
		public bool IsHidden
		{
			get
			{
				return this._isHidden;
			}
			set
			{
				this._isHidden = value;
				if (value)
				{
					this.Hide();
					return;
				}
				this.Show(false);
			}
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000BCC8 File Offset: 0x00009EC8
		public void SetStatusWidget(UnitStatusWidget widget, float initAlpha)
		{
			if (this._statusWidget)
			{
				Object.Destroy(this._statusWidget.gameObject);
			}
			this._statusWidget = widget;
			if (widget)
			{
				widget.TargetTransform = this.hpBarPoint;
			}
			widget.Alpha = initAlpha;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000BD14 File Offset: 0x00009F14
		public void SetInfoWidget(UnitInfoWidget widget, float initAlpha)
		{
			if (this._infoWidget)
			{
				Object.Destroy(this._infoWidget);
			}
			this._infoWidget = widget;
			if (widget)
			{
				widget.TargetTransform = this.infoPoint;
			}
			widget.Alpha = initAlpha;
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0000BD50 File Offset: 0x00009F50
		public void SetStatusVisible(bool visible, bool instant = false)
		{
			this.CancelFadeIn();
			if (this._statusWidget)
			{
				this._statusWidget.SetVisible(visible, instant);
			}
			if (this._infoWidget)
			{
				this._statusWidget.SetVisible(visible, instant);
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600029A RID: 666 RVA: 0x0000BD8C File Offset: 0x00009F8C
		public Transform ChatPoint
		{
			get
			{
				return this.chatPoint;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600029B RID: 667 RVA: 0x0000BD94 File Offset: 0x00009F94
		public Transform HitPoint
		{
			get
			{
				return this.hitPoint;
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600029C RID: 668 RVA: 0x0000BD9C File Offset: 0x00009F9C
		public Transform EffectRoot
		{
			get
			{
				return this.effectRoot;
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x0600029D RID: 669 RVA: 0x0000BDA4 File Offset: 0x00009FA4
		public Collider SelectorCollider
		{
			get
			{
				return this.selectorCollider;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600029E RID: 670 RVA: 0x0000BDAC File Offset: 0x00009FAC
		// (set) Token: 0x0600029F RID: 671 RVA: 0x0000BDB4 File Offset: 0x00009FB4
		public bool SelectingVisible
		{
			get
			{
				return this._selectingVisible;
			}
			set
			{
				if (this._selectingVisible == value)
				{
					return;
				}
				this._selectingVisible = value;
				int num = 0;
				switch (this._type)
				{
				case UnitView.ModelType.SingleSprite:
					num = 0;
					break;
				case UnitView.ModelType.Spine:
					num = 0;
					break;
				case UnitView.ModelType.Effect:
					num = 1;
					break;
				}
				HighlightManager.SetOutlineEnabled(this.spineSkeleton.transform.parent.gameObject, num, value);
				if (value)
				{
					this.ShowName();
					this.StatusToFront();
					return;
				}
				this.HideName();
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x060002A0 RID: 672 RVA: 0x0000BE2B File Offset: 0x0000A02B
		// (set) Token: 0x060002A1 RID: 673 RVA: 0x0000BE33 File Offset: 0x0000A033
		public Unit Unit { get; set; }

		// Token: 0x060002A2 RID: 674 RVA: 0x0000BE3C File Offset: 0x0000A03C
		public void Awake()
		{
			this.SelectingVisible = false;
			this._isHidden = false;
			this.IsHidden = true;
			this._status = UnitView.ShootStatus.Idle;
			this._animator = base.GetComponent<Animator>();
			this.BoxCollider = base.GetComponent<BoxCollider2D>();
			this._circleCollider = base.GetComponent<CircleCollider2D>();
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0000BE8C File Offset: 0x0000A08C
		public void OnDestroy()
		{
			if (this._statusWidget)
			{
				Object.Destroy(this._statusWidget.gameObject);
			}
			if (this._chatWidget)
			{
				Object.Destroy(this._chatWidget.gameObject);
			}
			if (this._infoWidget)
			{
				Object.Destroy(this._infoWidget.gameObject);
			}
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0000BEF0 File Offset: 0x0000A0F0
		private void Hide()
		{
			this.rendererParent.gameObject.SetActive(false);
			this.effectRoot.gameObject.SetActive(false);
			this.SetStatusVisible(false, true);
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000BF1C File Offset: 0x0000A11C
		public void Show(bool withStatus)
		{
			this._isHidden = false;
			this.rendererParent.gameObject.SetActive(true);
			this.effectRoot.gameObject.SetActive(true);
			if (withStatus)
			{
				this.SetStatusVisible(true, false);
			}
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0000BF54 File Offset: 0x0000A154
		private void Die()
		{
			this.rendererParent.gameObject.SetActive(false);
			this.effectRoot.gameObject.SetActive(false);
			this.effectRootIgnoreHiding.gameObject.SetActive(false);
			this.selectorCollider.gameObject.SetActive(false);
			this._statusWidget.SetVisible(false, false);
			this._infoWidget.SetVisible(false, false);
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000BFC0 File Offset: 0x0000A1C0
		private void Escape()
		{
			this.rendererParent.gameObject.SetActive(false);
			this.effectRoot.gameObject.SetActive(false);
			this.effectRootIgnoreHiding.gameObject.SetActive(false);
			this.selectorCollider.gameObject.SetActive(false);
			this._statusWidget.SetVisible(false, false);
			this._infoWidget.SetVisible(false, false);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000C02C File Offset: 0x0000A22C
		public void EnterBattle(BattleController battle)
		{
			if (this.Unit is PlayerUnit)
			{
				battle.ActionViewer.Register<AddDollSlotAction>(new BattleActionViewer<AddDollSlotAction>(this.ViewAddDollSlot), null);
				battle.ActionViewer.Register<RemoveDollSlotAction>(new BattleActionViewer<RemoveDollSlotAction>(this.ViewRemoveDollSlot), null);
				battle.ActionViewer.Register<AddDollAction>(new BattleActionViewer<AddDollAction>(this.ViewAddDoll), null);
				battle.ActionViewer.Register<RemoveDollAction>(new BattleActionViewer<RemoveDollAction>(this.ViewRemoveDoll), null);
				battle.ActionViewer.Register<ConsumeMagicAction>(new BattleActionViewer<ConsumeMagicAction>(this.ViewConsumeMagic), null);
				this.InitDollSlots();
			}
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000C0C4 File Offset: 0x0000A2C4
		public void LeaveBattle(BattleController battle)
		{
			if (this.Unit is PlayerUnit)
			{
				battle.ActionViewer.Unregister<AddDollSlotAction>(new BattleActionViewer<AddDollSlotAction>(this.ViewAddDollSlot));
				battle.ActionViewer.Unregister<RemoveDollSlotAction>(new BattleActionViewer<RemoveDollSlotAction>(this.ViewRemoveDollSlot));
				battle.ActionViewer.Unregister<AddDollAction>(new BattleActionViewer<AddDollAction>(this.ViewAddDoll));
				battle.ActionViewer.Unregister<RemoveDollAction>(new BattleActionViewer<RemoveDollAction>(this.ViewRemoveDoll));
				battle.ActionViewer.Unregister<ConsumeMagicAction>(new BattleActionViewer<ConsumeMagicAction>(this.ViewConsumeMagic));
				this.ClearDolls();
			}
			this.UpdateShieldColliders();
			if (this._statusWidget)
			{
				this._statusWidget.OnBlockShieldChanged();
				this._statusWidget.ClearStatusEffects();
			}
			foreach (KeyValuePair<string, EffectWidget> keyValuePair in this._effectDictionary)
			{
				Object.Destroy(keyValuePair.Value.gameObject);
			}
			this._effectDictionary.Clear();
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000C1DC File Offset: 0x0000A3DC
		public void Event_OnPointerEnter()
		{
			this.ShowName();
			this.StatusToFront();
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000C1EA File Offset: 0x0000A3EA
		public void Event_OnPointerExit()
		{
			this.HideName();
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000C1F2 File Offset: 0x0000A3F2
		public void Event_OnPointerClick()
		{
			this.Clicked();
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000C1FA File Offset: 0x0000A3FA
		private void ShowName()
		{
			if (this._infoWidget)
			{
				this._infoWidget.IncreaseHoveringLevel();
			}
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000C214 File Offset: 0x0000A414
		private void HideName()
		{
			if (this._infoWidget)
			{
				this._infoWidget.DecreaseHoveringLevel();
			}
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000C230 File Offset: 0x0000A430
		private void StatusToFront()
		{
			UnitStatusHud panel = UiManager.GetPanel<UnitStatusHud>();
			if (this._statusWidget)
			{
				panel.StatusToFront(this._statusWidget.transform);
			}
			if (this._infoWidget)
			{
				panel.StatusToFront(this._infoWidget.transform);
			}
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000C27F File Offset: 0x0000A47F
		private void Clicked()
		{
			Action clickHandler = this.ClickHandler;
			if (clickHandler == null)
			{
				return;
			}
			clickHandler.Invoke();
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000C294 File Offset: 0x0000A494
		public void Chat(string content, float time, ChatWidget.CloudType type = ChatWidget.CloudType.LeftThink, float delay = 0f)
		{
			if (!this._chatWidget)
			{
				this._chatWidget = UiManager.GetPanel<UnitStatusHud>().CreateChatWidget(this);
				this._chatWidget.name = "Chat: " + base.name;
			}
			this._chatWidget.Chat(content, time, delay, type);
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000C2EC File Offset: 0x0000A4EC
		private Color[] GetDefaultSpellColors()
		{
			Debug.LogError(this._modelName + " has no spell color in config");
			return new Color[]
			{
				new Color32(104, 15, 5, byte.MaxValue),
				new Color32(byte.MaxValue, 46, 72, byte.MaxValue),
				new Color32(74, 0, 1, 150),
				new Color32(byte.MaxValue, 46, 72, byte.MaxValue)
			};
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0000C388 File Offset: 0x0000A588
		public async UniTask LoadUnitModelAsync(string modelName, bool player = false, float? forceHpLength = null)
		{
			if (this._unitModelLoaded)
			{
				this.ClearUnitModel();
			}
			UnitModelConfig config = UnitModelConfig.FromName(modelName);
			if (config != null)
			{
				this._modelName = config.Name;
				this._type = (UnitView.ModelType)config.Type;
				this._offset = config.Offset;
				this._flipModel = config.Flip;
				this._dieLevel = config.Dielevel;
				this._boxSize = config.Box;
				this._shieldRadius = config.Shield;
				this._blockRadius = config.Block;
				this._hitPointV2 = config.Hit;
				this._hpV2 = config.Hp;
				this._hitRep = config.HitRep;
				this._guardRep = config.GuardRep;
				this._hpLength = config.HpLength;
				this._infoV2 = config.Info;
				this._selectV3 = new Vector3(config.Select.x, config.Select.y, 1f);
				this._chatV2 = config.Chat;
				this._spellV2 = config.SpellPosition;
				this._spellScale = config.SpellScale;
				IReadOnlyList<Color32> spellColor = config.SpellColor;
				Color[] array;
				if (spellColor.Count != 0)
				{
					array = Enumerable.ToArray<Color>(Enumerable.Select<Color32, Color>(spellColor, (Color32 c) => c));
				}
				else
				{
					array = null;
				}
				this._spellColors = array;
				this.BoxCollider.size = this._boxSize;
				this._circleCollider.radius = this._shieldRadius;
				this.selectorCollider.size = this._selectV3;
				this.BoxCollider.enabled = true;
				this._circleCollider.enabled = false;
				this.ShieldEffect = this.CreateEternalShieldEffect("Shield", true);
				this.BlockEffect = this.CreateEternalShieldEffect("Block", false);
				this.UpdateShieldColliders();
				this.chatPoint.localPosition = new Vector3(this._chatV2.x, this._chatV2.y);
				this.hitPoint.localPosition = new Vector3(this._hitPointV2.x, this._hitPointV2.y);
				this.hpBarPoint.localPosition = new Vector3(this._hpV2.x, this._hpV2.y);
				this._shootStartTimeList = Enumerable.ToList<float>(config.ShootStartTime);
				this._shootPointV2List = Enumerable.ToList<Vector2>(config.ShootPoint);
				this._shooterPointV2List = Enumerable.ToList<Vector2>(config.ShooterPoint);
				try
				{
					Sprite sprite;
					if (config.HasSpellPortrait)
					{
						sprite = await ResourcesHelper.LoadSpellPortraitAsync(this._modelName);
					}
					else
					{
						sprite = null;
					}
					this._spellPortrait = sprite;
				}
				catch
				{
					Debug.LogError("[UnitView] Cannot load spell portrait for " + this._modelName);
				}
				switch (this._type)
				{
				case UnitView.ModelType.SingleSprite:
				{
					RuntimeAnimatorController runtimeAnimatorController = Object.Instantiate<RuntimeAnimatorController>(this.simpleUnitAnimatorController);
					this._animator.runtimeAnimatorController = runtimeAnimatorController;
					this.rendererParent.localPosition += new Vector3(this._offset.x, this._offset.y);
					if (this._flipModel)
					{
						Transform transform = this.spriteRenderer.transform;
						transform.localScale = transform.localScale.FlipX();
					}
					int num = 0;
					try
					{
						SpriteRenderer spriteRenderer = this.spriteRenderer;
						Sprite sprite = await ResourcesHelper.LoadSimpleUnitSpriteAsync(modelName);
						spriteRenderer.sprite = sprite;
						spriteRenderer = null;
						this.spriteRenderer.gameObject.SetActive(true);
						this.spriteRenderer.enabled = true;
					}
					catch (Exception obj)
					{
						num = 1;
					}
					object obj;
					if (num == 1)
					{
						Debug.LogError("Cannot load sprite unit " + modelName + ", using Koishi\n  Error: " + ((Exception)obj).Message);
						this._offset = UnitModelConfig.FromName("Koishi").Offset;
						await this.LoadSpineAsync("Koishi");
					}
					obj = null;
					break;
				}
				case UnitView.ModelType.Spine:
					await this.LoadSpineAsync(modelName);
					break;
				case UnitView.ModelType.Effect:
				{
					RuntimeAnimatorController runtimeAnimatorController2 = Object.Instantiate<RuntimeAnimatorController>(this.simpleUnitAnimatorController);
					this._animator.runtimeAnimatorController = runtimeAnimatorController2;
					this.rendererParent.localPosition += new Vector3(this._offset.x, this._offset.y);
					if (this._flipModel)
					{
						Transform transform2 = this.spriteRenderer.transform;
						transform2.localScale = transform2.localScale.FlipX();
					}
					this._effectModel = EffectManager.CreateEffect(config.EffectName, this.spriteRenderer.transform, true);
					this.spriteRenderer.gameObject.SetActive(true);
					this.spriteRenderer.enabled = false;
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
				if (player)
				{
					base.name = ((this.Unit.Id == this._modelName) ? (this.Unit.Id + "(Player)") : (this.Unit.Id + "." + this._modelName + "(Player)"));
					this.IsPlayer = true;
				}
				else
				{
					base.name = ((this.Unit.Id == this._modelName) ? (this.Unit.Id + "(Enemy)") : (this.Unit.Id + "." + this._modelName + "(Enemy)"));
					this._statusWidget.BarLength = forceHpLength ?? this._hpLength;
					this.Flip = true;
				}
				this._statusWidget.name = "Status: " + base.name;
				this._infoWidget.name = "Info: " + base.name;
			}
			else
			{
				Debug.LogError("No such unit model name: " + modelName);
			}
			this.infoPoint.localPosition = new Vector3(this._infoV2.x, this._infoV2.y);
			this._unitModelLoaded = true;
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000C3E4 File Offset: 0x0000A5E4
		private void ClearUnitModel()
		{
			if (this.ShieldEffect)
			{
				Object.Destroy(this.ShieldEffect.gameObject);
			}
			if (this.BlockEffect)
			{
				Object.Destroy(this.BlockEffect.gameObject);
			}
			this.rendererParent.localPosition = Vector3.zero;
			this.spriteRenderer.transform.localScale = Vector3.one;
			switch (this._type)
			{
			case UnitView.ModelType.SingleSprite:
				if (this._animator.runtimeAnimatorController)
				{
					Object.Destroy(this._animator.runtimeAnimatorController);
					this._animator.runtimeAnimatorController = null;
					return;
				}
				break;
			case UnitView.ModelType.Spine:
				this.SpineClear();
				return;
			case UnitView.ModelType.Effect:
				if (this._animator.runtimeAnimatorController)
				{
					Object.Destroy(this._animator.runtimeAnimatorController);
					this._animator.runtimeAnimatorController = null;
				}
				if (this._effectModel)
				{
					Object.Destroy(this._effectModel.gameObject);
					return;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000C4F6 File Offset: 0x0000A6F6
		public void SetPlayerHpBarLength(int maxHp)
		{
			this._statusWidget.SetPlayerHpBarLength(maxHp);
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0000C504 File Offset: 0x0000A704
		private async UniTask LoadSpineAsync(string modelName)
		{
			bool flag = false;
			foreach (SkeletonAnimation skeletonAnimation in this.prefabList)
			{
				if (skeletonAnimation.name == modelName)
				{
					Object.Destroy(this.spineSkeleton.gameObject);
					this.spineSkeleton = Object.Instantiate<SkeletonAnimation>(skeletonAnimation, this.rendererParent);
					if (modelName == "Kokoro")
					{
						this._kokoroUnitController = this.spineSkeleton.GetComponent<KokoroUnitController>();
						if (!this._kokoroUnitController)
						{
							throw new InvalidOperationException("Kokoro UnitView has no effect controller.");
						}
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				SkeletonDataAsset skeletonDataAsset = await ResourcesHelper.LoadSpineUnitAsync(modelName);
				this.spineSkeleton.skeletonDataAsset = skeletonDataAsset;
				if (this._spineNeedReload)
				{
					this.spineSkeleton.Initialize(true, false);
				}
			}
			this.SpineInitialize();
			this.rendererParent.localPosition += new Vector3(this._offset.x, this._offset.y);
			if (this._flipModel)
			{
				Transform transform = this.spineSkeleton.transform;
				transform.localScale = transform.localScale.FlipX();
			}
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000C550 File Offset: 0x0000A750
		private void SetGunByAnimation(Gun gun)
		{
			bool flag = false;
			string text;
			if (gun.Sequence.IsNullOrEmpty())
			{
				text = gun.Animation;
			}
			else
			{
				SequenceConfig sequenceConfig = SequenceConfig.FromId(gun.Sequence);
				if (sequenceConfig == null)
				{
					throw new InvalidOperationException("Lacking of Sequence: " + gun.Sequence);
				}
				text = sequenceConfig.Animation;
				gun.StartTime = sequenceConfig.StartTime;
				flag = true;
			}
			int num;
			if (!(text == "shoot2"))
			{
				if (!(text == "shoot3"))
				{
					if (!(text == "shoot4"))
					{
						num = 0;
					}
					else
					{
						num = 3;
					}
				}
				else
				{
					num = 2;
				}
			}
			else
			{
				num = 1;
			}
			int num2 = num;
			if (!flag)
			{
				gun.StartTime = this._shootStartTimeList.TryGetValue(num2);
			}
			gun.ShootV2 = (this.Flip ? this._shootPointV2List.TryGetValue(num2).FlipX() : this._shootPointV2List.TryGetValue(num2));
			this.shooterPoint.localPosition = this._shooterPointV2List.TryGetValue(num2);
			gun.transform.position = base.transform.position + gun.ShootV2;
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x0000C673 File Offset: 0x0000A873
		// (set) Token: 0x060002B9 RID: 697 RVA: 0x0000C67B File Offset: 0x0000A87B
		public int RootIndex { get; set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x060002BA RID: 698 RVA: 0x0000C684 File Offset: 0x0000A884
		// (set) Token: 0x060002BB RID: 699 RVA: 0x0000C68C File Offset: 0x0000A88C
		public int AngleIndex { get; set; }

		// Token: 0x060002BC RID: 700 RVA: 0x0000C698 File Offset: 0x0000A898
		public void PlayEffectOneShot(string effectName, float delay)
		{
			EffectManager.CreateEffect(effectName, this.effectRoot, delay, default(float?), true, true);
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000C6C0 File Offset: 0x0000A8C0
		public void PlayEffectLoop(string effectName)
		{
			if (this._effectDictionary.ContainsKey(effectName))
			{
				Debug.LogError("There is an other Effect in Dictionary with same effectName, don't support play dupes.");
				return;
			}
			EffectWidget effectWidget = EffectManager.CreateEffect(effectName, this.effectRoot, true);
			this._effectDictionary.Add(effectName, effectWidget);
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000C704 File Offset: 0x0000A904
		public bool TryPlayEffectLoop(string effectName)
		{
			if (this._effectDictionary.ContainsKey(effectName))
			{
				return false;
			}
			EffectWidget effectWidget = EffectManager.CreateEffect(effectName, this.effectRoot, true);
			this._effectDictionary.Add(effectName, effectWidget);
			return true;
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000C740 File Offset: 0x0000A940
		public void SendEffectMessage(string effectName, string message, object args)
		{
			EffectWidget effectWidget;
			if (!this._effectDictionary.TryGetValue(effectName, ref effectWidget))
			{
				Debug.LogWarning("[UnitView]: Cannot find effect " + effectName + " in effect table of " + base.name);
				return;
			}
			effectWidget.SendMessage(message, args, SendMessageOptions.DontRequireReceiver);
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000C784 File Offset: 0x0000A984
		public void EndEffectLoop(string effectName, bool instant)
		{
			if (this._effectDictionary.ContainsKey(effectName))
			{
				if (instant)
				{
					Object.Destroy(this._effectDictionary[effectName].gameObject);
				}
				else
				{
					this._effectDictionary[effectName].DieOut();
				}
				this._effectDictionary.Remove(effectName);
				return;
			}
			Debug.LogWarning("There is no such Effect in Dictionary: " + effectName);
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000C7E9 File Offset: 0x0000A9E9
		public void OnMaxHpChanged()
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnMaxHpChanged();
			}
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000C803 File Offset: 0x0000AA03
		public void OnDamageReceived(DamageInfo damage)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnDamageReceived(damage);
			}
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000C81E File Offset: 0x0000AA1E
		public void OnHealingReceived(int amount)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnHealingReceived(amount);
			}
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000C839 File Offset: 0x0000AA39
		public void OnForceKill()
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnForceKill();
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000C853 File Offset: 0x0000AA53
		public void OnAddStatusEffect(StatusEffect se, StatusEffectAddResult addResult)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnAddStatusEffect(se, addResult);
			}
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0000C86F File Offset: 0x0000AA6F
		public void OnRemoveStatusEffect(StatusEffect se)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnRemoveStatusEffect(se);
			}
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000C88C File Offset: 0x0000AA8C
		public Vector3? FindStatusEffectWorldPosition(StatusEffect se)
		{
			if (this._statusWidget)
			{
				StatusEffectWidget statusEffectWidget = this._statusWidget.FindStatusEffect(se);
				if (statusEffectWidget)
				{
					return new Vector3?(statusEffectWidget.CenterWorldPosition);
				}
			}
			return default(Vector3?);
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000C8D0 File Offset: 0x0000AAD0
		public void ShowSePopup(string content, StatusEffectType seType, int number = 0, UnitInfoWidget.SePopType popType = UnitInfoWidget.SePopType.Add)
		{
			if (this._infoWidget)
			{
				this._infoWidget.ShowSePopup(content, seType, number, popType);
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000C8F0 File Offset: 0x0000AAF0
		public void OnGlobalStatusChanged()
		{
			foreach (DollView dollView in this.Dolls)
			{
				dollView.OnChanged();
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x060002CA RID: 714 RVA: 0x0000C940 File Offset: 0x0000AB40
		// (set) Token: 0x060002CB RID: 715 RVA: 0x0000C948 File Offset: 0x0000AB48
		public bool ShowDoll
		{
			get
			{
				return this._showDoll;
			}
			set
			{
				this._showDoll = value;
				this.dollRoot.gameObject.SetActive(value);
			}
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000C964 File Offset: 0x0000AB64
		private void InitDollSlots()
		{
			PlayerUnit playerUnit = this.Unit as PlayerUnit;
			if (playerUnit == null)
			{
				return;
			}
			if (this._slots.NotEmpty<DollSlotView>())
			{
				this.ClearDolls();
			}
			using (BasicTypeExtensions.RangeEnumerator enumerator = Range.EndAt(playerUnit.DollSlotCount).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int num = enumerator.Current;
					this._slots.Add(this.CreateDollSlotView(this.dollRoot));
				}
			}
			this.AdjustDollSlotsPosition(false);
			this.AdjustDollsPosition(false);
			if (!playerUnit.ShowDollSlotByDefault)
			{
				this.ShowDoll = false;
			}
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000CA14 File Offset: 0x0000AC14
		private void ClearDolls()
		{
			PlayerUnit playerUnit = this.Unit as PlayerUnit;
			if (playerUnit == null)
			{
				return;
			}
			foreach (DollSlotView dollSlotView in this._slots)
			{
				dollSlotView.DOKill(false);
				Object.Destroy(dollSlotView.gameObject);
			}
			this._slots.Clear();
			foreach (DollView dollView in this.Dolls)
			{
				dollView.DOKill(false);
				Object.Destroy(dollView.gameObject);
			}
			this.Dolls.Clear();
			if (!playerUnit.ShowDollSlotByDefault)
			{
				this.ShowDoll = false;
			}
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000CAF4 File Offset: 0x0000ACF4
		private DollSlotView CreateDollSlotView(Transform parent)
		{
			this.ShowDoll = true;
			DollSlotView dollSlotView = Object.Instantiate<DollSlotView>(this.dollSlotTemplate, parent);
			Transform transform = dollSlotView.transform;
			transform.localPosition = transform.localPosition.WithZ(0.1f);
			return dollSlotView;
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0000CB24 File Offset: 0x0000AD24
		private DollView CreateDollView(Doll doll, Transform parent)
		{
			this.ShowDoll = true;
			DollView dollView = Object.Instantiate<DollView>(this.dollTemplate, parent);
			dollView.name = doll.Id + "(Doll)";
			dollView.Doll = doll;
			return dollView;
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000CB58 File Offset: 0x0000AD58
		private void AdjustDollSlotsPosition(bool tween)
		{
			foreach (ValueTuple<int, DollSlotView> valueTuple in this._slots.WithIndices<DollSlotView>())
			{
				int item = valueTuple.Item1;
				Transform transform = valueTuple.Item2.transform;
				transform.DOKill(false);
				if (tween)
				{
					transform.DOLocalMove(new Vector3(this._slotPositions[item].x, this._slotPositions[item].y, 0.1f), 0.2f, false);
				}
				else
				{
					transform.localPosition = new Vector3(this._slotPositions[item].x, this._slotPositions[item].y, 0.1f);
				}
			}
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000CC34 File Offset: 0x0000AE34
		private void AdjustDollsPosition(bool tween)
		{
			foreach (ValueTuple<int, DollView> valueTuple in this.Dolls.WithIndices<DollView>())
			{
				int item = valueTuple.Item1;
				Transform transform = valueTuple.Item2.transform;
				transform.DOKill(false);
				if (tween)
				{
					transform.DOLocalMove(this._slotPositions[item], 0.2f, false);
				}
				else
				{
					transform.localPosition = this._slotPositions[item];
				}
			}
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x0000CCD4 File Offset: 0x0000AED4
		public DollView GetDoll(Doll doll)
		{
			return Enumerable.FirstOrDefault<DollView>(this.Dolls, (DollView d) => d.Doll == doll);
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000CD05 File Offset: 0x0000AF05
		private IEnumerator ViewAddDoll(AddDollAction action)
		{
			Doll doll = action.Args.Doll;
			DollView dollView = this.CreateDollView(doll, this.dollRoot);
			Transform transform = dollView.transform;
			Vector3 localPosition = this._slots[this.Dolls.Count].transform.localPosition;
			transform.localPosition = transform.localPosition.With(new float?(localPosition.x), new float?(localPosition.y), default(float?));
			this.Dolls.Add(dollView);
			yield return new WaitForSeconds(0.05f);
			yield break;
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000CD1B File Offset: 0x0000AF1B
		private IEnumerator ViewRemoveDoll(RemoveDollAction action)
		{
			Doll doll = action.Args.Doll;
			DollView doll2 = this.GetDoll(doll);
			if (doll2 != null)
			{
				Object.Destroy(doll2.gameObject);
				this.Dolls.Remove(doll2);
				this.AdjustDollsPosition(true);
			}
			else
			{
				Debug.LogError("[UnitView] Cannot find doll view for " + doll.DebugName);
			}
			yield return new WaitForSeconds(0.05f);
			yield break;
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000CD31 File Offset: 0x0000AF31
		private IEnumerator ViewAddDollSlot(AddDollSlotAction action)
		{
			int dollSlotCount = ((PlayerUnit)this.Unit).DollSlotCount;
			if (dollSlotCount < this._slots.Count)
			{
				Debug.LogError(string.Format("[UnitView] Target doll slot count {0} is lesser than current ({1})", dollSlotCount, this._slots.Count));
				yield break;
			}
			for (int i = this._slots.Count; i < dollSlotCount; i++)
			{
				this._slots.Add(this.CreateDollSlotView(this.dollRoot));
			}
			this.AdjustDollSlotsPosition(true);
			this.AdjustDollsPosition(true);
			yield return new WaitForSeconds(0.2f);
			yield break;
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000CD40 File Offset: 0x0000AF40
		private IEnumerator ViewRemoveDollSlot(RemoveDollSlotAction action)
		{
			int dollSlotCount = ((PlayerUnit)this.Unit).DollSlotCount;
			if (dollSlotCount > this._slots.Count)
			{
				Debug.LogError(string.Format("[UnitView] Target doll slot count {0} is lesser than current ({1})", dollSlotCount, this._slots.Count));
				yield break;
			}
			for (int i = dollSlotCount; i < this._slots.Count; i++)
			{
				Object.Destroy(this._slots[i].gameObject);
			}
			this._slots.RemoveRange(dollSlotCount, this._slots.Count - dollSlotCount);
			for (int j = dollSlotCount; j < this.Dolls.Count; j++)
			{
				Object.Destroy(this.Dolls[j].gameObject);
			}
			this.Dolls.RemoveRange(dollSlotCount, this.Dolls.Count - dollSlotCount);
			this.AdjustDollsPosition(true);
			this.AdjustDollSlotsPosition(true);
			yield break;
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0000CD4F File Offset: 0x0000AF4F
		private IEnumerator ViewConsumeMagic(ConsumeMagicAction action)
		{
			DollView doll = this.GetDoll(action.Args.Doll);
			if (doll != null)
			{
				doll.InfoWidget.ConsumeMagic();
			}
			yield break;
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0000CD65 File Offset: 0x0000AF65
		public void UpdateIntentions()
		{
			if (this._infoWidget)
			{
				this._infoWidget.UpdateIntentions();
			}
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000CD7F File Offset: 0x0000AF7F
		public void SetMoveOrder(int order)
		{
			if (this._infoWidget)
			{
				this._infoWidget.SetMoveOrder(order);
			}
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000CD9A File Offset: 0x0000AF9A
		public void SetMoveOrderVisible(bool visible)
		{
			if (this._infoWidget)
			{
				this._infoWidget.IsMoveOrderVisible = visible;
			}
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000CDB5 File Offset: 0x0000AFB5
		public void ShowMovePopup(string moveName, bool closeMoveName)
		{
			if (this._infoWidget)
			{
				this._infoWidget.ShowMovePopup(moveName, closeMoveName);
			}
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0000CDD4 File Offset: 0x0000AFD4
		public void SetOrb(string effectName, int orbitIndex)
		{
			EffectBullet effectBullet = RinOrb.CreateRinOrb(effectName, orbitIndex);
			Transform transform = this.effectRoot;
			EffectBulletView effectBulletView = EffectManager.CreateEffectBullet(effectBullet, default(Vector3), transform);
			this._orbs.Add(effectBulletView);
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000CE0B File Offset: 0x0000B00B
		public IEnumerator MoveOrbToEnemy(EnemyUnit enemy)
		{
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0000CE13 File Offset: 0x0000B013
		public IEnumerator RecycleOrb()
		{
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x060002DF RID: 735 RVA: 0x0000CE1B File Offset: 0x0000B01B
		public void SetEffect(SkirtColor skirtColor)
		{
			this._kokoroUnitController.SwitchToColor(skirtColor);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0000CE29 File Offset: 0x0000B029
		public IEnumerator SwitchToFace(SkirtColor skirtColor)
		{
			this.SetEffect(skirtColor);
			foreach (global::Spine.AnimationState animationState in Enumerable.Concat<global::Spine.AnimationState>(this.AllStates, this._kokoroUnitController.AllMuskStates))
			{
				animationState.SetAnimation(3, "switch", false);
				animationState.SetAnimation(4, "face" + skirtColor.ToString().ToLower(), false);
			}
			yield return new WaitForSeconds(0.1f);
			yield break;
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0000CE3F File Offset: 0x0000B03F
		public void SetSleep(bool sleep)
		{
			this.DoremySleeping = sleep;
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000CE48 File Offset: 0x0000B048
		public void SetEffect(bool show, int level)
		{
			UiManager.GetPanel<SystemBoard>().SetDoremyLevel(show, level);
			if (show)
			{
				GameDirector.AddDeremyEnvironment(EffectManager.CreateEffect("DreamL" + level.ToString(), Environment.Instance.transform, 0f, default(float?), false, true));
				return;
			}
			GameDirector.ClearDoremyEnvironment();
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000CEA0 File Offset: 0x0000B0A0
		public UnitView()
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(1.5f, 1.2f));
			list.Add(new Vector2(2f, 0.5f));
			list.Add(new Vector2(2f, -0.5f));
			list.Add(new Vector2(1.5f, -1.2f));
			this._slotPositions = list;
			this._orbs = new List<EffectBulletView>(3);
			base..ctor();
		}

		// Token: 0x040000B5 RID: 181
		private const float GuardEffectInterval = 0.2f;

		// Token: 0x040000B6 RID: 182
		private bool _hasBlock;

		// Token: 0x040000B7 RID: 183
		private bool _hasShield;

		// Token: 0x040000BA RID: 186
		private string _shooterName;

		// Token: 0x040000BB RID: 187
		private EffectWidget _shooterLoopEffect;

		// Token: 0x040000BC RID: 188
		private Gun _complexFirstGun;

		// Token: 0x040000BD RID: 189
		private Gun _gunInShooting;

		// Token: 0x040000BE RID: 190
		private bool _shootCounting;

		// Token: 0x040000BF RID: 191
		private float _shootTime;

		// Token: 0x040000C0 RID: 192
		private bool _shooterCounting;

		// Token: 0x040000C1 RID: 193
		private float _shooterTime;

		// Token: 0x040000C2 RID: 194
		private UnitView.ShootStatus _status;

		// Token: 0x040000C3 RID: 195
		private Animator _animator;

		// Token: 0x040000C5 RID: 197
		private CircleCollider2D _circleCollider;

		// Token: 0x040000C8 RID: 200
		private bool _bothShield;

		// Token: 0x040000C9 RID: 201
		private bool _invincible;

		// Token: 0x040000CA RID: 202
		private DamageInfo _comingDamage;

		// Token: 0x040000D8 RID: 216
		private bool _spineNeedReload;

		// Token: 0x040000D9 RID: 217
		private bool _doremySleeping;

		// Token: 0x040000DA RID: 218
		private bool _hasFly;

		// Token: 0x040000DB RID: 219
		private bool _hasBlink;

		// Token: 0x040000DC RID: 220
		private bool _blinking;

		// Token: 0x040000DD RID: 221
		private float _blinkTimer;

		// Token: 0x040000DE RID: 222
		private const float BlinkMin = 2f;

		// Token: 0x040000DF RID: 223
		private const float BlinkMax = 5f;

		// Token: 0x040000E0 RID: 224
		private const int NormalTrack = 0;

		// Token: 0x040000E1 RID: 225
		private const int BlinkEyeTrack = 1;

		// Token: 0x040000E2 RID: 226
		private const int FlyTrack = 2;

		// Token: 0x040000E3 RID: 227
		private bool _hasPose;

		// Token: 0x040000E4 RID: 228
		private bool _posing;

		// Token: 0x040000E5 RID: 229
		private float _poseTimer;

		// Token: 0x040000E6 RID: 230
		private const float PoseMin = 32f;

		// Token: 0x040000E7 RID: 231
		private const float PoseMax = 40f;

		// Token: 0x040000EF RID: 239
		private CancellationTokenSource _fadeInCts;

		// Token: 0x040000F0 RID: 240
		private ChatWidget _chatWidget;

		// Token: 0x040000F1 RID: 241
		private UnitStatusWidget _statusWidget;

		// Token: 0x040000F2 RID: 242
		private UnitInfoWidget _infoWidget;

		// Token: 0x040000F5 RID: 245
		private bool _flip;

		// Token: 0x040000F6 RID: 246
		private bool _isHidden;

		// Token: 0x040000F7 RID: 247
		private bool _selectingVisible;

		// Token: 0x040000F9 RID: 249
		[Header("Spine")]
		[SerializeField]
		private Transform rendererParent;

		// Token: 0x040000FA RID: 250
		[SerializeField]
		private SkeletonAnimation spineSkeleton;

		// Token: 0x040000FB RID: 251
		[SerializeField]
		private List<SkeletonAnimation> prefabList;

		// Token: 0x040000FC RID: 252
		[Header("Binding")]
		[SerializeField]
		private SpriteRenderer spriteRenderer;

		// Token: 0x040000FD RID: 253
		[SerializeField]
		private BoxCollider selectorCollider;

		// Token: 0x040000FE RID: 254
		[Header("Points")]
		[SerializeField]
		private Transform pointsParent;

		// Token: 0x040000FF RID: 255
		[SerializeField]
		private Transform effectRoot;

		// Token: 0x04000100 RID: 256
		[SerializeField]
		private Transform effectRootIgnoreHiding;

		// Token: 0x04000101 RID: 257
		[SerializeField]
		private Transform chatPoint;

		// Token: 0x04000102 RID: 258
		[SerializeField]
		private Transform hitPoint;

		// Token: 0x04000103 RID: 259
		[SerializeField]
		private Transform hpBarPoint;

		// Token: 0x04000104 RID: 260
		[SerializeField]
		private Transform shooterPoint;

		// Token: 0x04000105 RID: 261
		[SerializeField]
		private Transform infoPoint;

		// Token: 0x04000106 RID: 262
		[SerializeField]
		private Transform dollRoot;

		// Token: 0x04000107 RID: 263
		[Header("Resource Ref")]
		[SerializeField]
		private RuntimeAnimatorController simpleUnitAnimatorController;

		// Token: 0x04000108 RID: 264
		[SerializeField]
		private DollSlotView dollSlotTemplate;

		// Token: 0x04000109 RID: 265
		[SerializeField]
		private DollView dollTemplate;

		// Token: 0x0400010A RID: 266
		private string _modelName;

		// Token: 0x0400010B RID: 267
		private UnitView.ModelType _type;

		// Token: 0x0400010C RID: 268
		private Vector2 _offset;

		// Token: 0x0400010D RID: 269
		private bool _flipModel;

		// Token: 0x0400010E RID: 270
		private int _dieLevel;

		// Token: 0x0400010F RID: 271
		private Vector2 _boxSize;

		// Token: 0x04000110 RID: 272
		private float _shieldRadius;

		// Token: 0x04000111 RID: 273
		private float _blockRadius;

		// Token: 0x04000112 RID: 274
		private Vector2 _hitPointV2;

		// Token: 0x04000113 RID: 275
		private Vector2 _hpV2;

		// Token: 0x04000114 RID: 276
		private float _hitRep;

		// Token: 0x04000115 RID: 277
		private float _guardRep;

		// Token: 0x04000116 RID: 278
		private float _hpLength;

		// Token: 0x04000117 RID: 279
		private Vector2 _infoV2;

		// Token: 0x04000118 RID: 280
		private Vector3 _selectV3;

		// Token: 0x04000119 RID: 281
		private Vector2 _chatV2;

		// Token: 0x0400011A RID: 282
		private Vector2 _spellV2;

		// Token: 0x0400011B RID: 283
		private float _spellScale;

		// Token: 0x0400011C RID: 284
		private Color[] _spellColors;

		// Token: 0x0400011D RID: 285
		private Sprite _spellPortrait;

		// Token: 0x0400011E RID: 286
		private List<float> _shootStartTimeList;

		// Token: 0x0400011F RID: 287
		private List<Vector2> _shootPointV2List;

		// Token: 0x04000120 RID: 288
		private List<Vector2> _shooterPointV2List;

		// Token: 0x04000121 RID: 289
		private const string FallbackCharacter = "Koishi";

		// Token: 0x04000122 RID: 290
		private bool _unitModelLoaded;

		// Token: 0x04000123 RID: 291
		private EffectWidget _effectModel;

		// Token: 0x04000126 RID: 294
		private readonly Dictionary<string, EffectWidget> _effectDictionary = new Dictionary<string, EffectWidget>();

		// Token: 0x04000127 RID: 295
		private bool _showDoll;

		// Token: 0x04000128 RID: 296
		private readonly List<DollSlotView> _slots = new List<DollSlotView>();

		// Token: 0x04000129 RID: 297
		public readonly List<DollView> Dolls = new List<DollView>();

		// Token: 0x0400012A RID: 298
		private const float DollMoveTime = 0.2f;

		// Token: 0x0400012B RID: 299
		private const float DollAddAndRemoveTime = 0.05f;

		// Token: 0x0400012C RID: 300
		private readonly List<Vector2> _slotPositions;

		// Token: 0x0400012D RID: 301
		private readonly List<EffectBulletView> _orbs;

		// Token: 0x0400012E RID: 302
		private const float MoveTime = 0.1f;

		// Token: 0x0400012F RID: 303
		private KokoroUnitController _kokoroUnitController;

		// Token: 0x02000196 RID: 406
		public enum ShootStatus
		{
			// Token: 0x04000E08 RID: 3592
			Idle,
			// Token: 0x04000E09 RID: 3593
			Direct,
			// Token: 0x04000E0A RID: 3594
			Complex
		}

		// Token: 0x02000197 RID: 407
		private enum ModelType
		{
			// Token: 0x04000E0C RID: 3596
			SingleSprite,
			// Token: 0x04000E0D RID: 3597
			Spine,
			// Token: 0x04000E0E RID: 3598
			Effect
		}
	}
}
