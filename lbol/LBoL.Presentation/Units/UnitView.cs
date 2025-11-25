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
	public sealed class UnitView : MonoBehaviour, IRinView, IEnemyUnitView, IUnitView, IKokoroView, IDoremyView
	{
		public UnitView Target { get; set; }
		public List<UnitView> Targets { get; set; } = new List<UnitView>();
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
		public BoxCollider2D BoxCollider { get; private set; }
		public HitType HitType { get; private set; }
		public bool WillCrash { get; private set; }
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
		private EffectWidget BlockEffect { get; set; }
		private EffectWidget ShieldEffect { get; set; }
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
		private bool Guarding { get; set; }
		private float GuardTime { get; set; }
		private float GuardEffectTime { get; set; }
		private Coroutine GuardCoroutine { get; set; }
		private bool Crashing { get; set; }
		private global::Spine.AnimationState State { get; set; }
		private List<global::Spine.AnimationState> AllStates { get; } = new List<global::Spine.AnimationState>();
		private bool SpineLoaded { get; set; }
		private List<global::Spine.Animation> AllSpineAnimations { get; } = new List<global::Spine.Animation>();
		private List<string> AllAnimationsNames { get; } = new List<string>();
		private float LifeTime { get; set; }
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
		public IEnumerator DieViewer()
		{
			this._statusWidget.SetVisible(false, false);
			yield return this.DieRunner();
			yield break;
		}
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
		public void PerformShoot(string gunName)
		{
			Gun gun = GunManager.CreateGun(gunName);
			this.SetGunByAnimation(gun);
			gun.Aim(this.Unit is PlayerUnit, this.Target, this.Targets);
			base.StartCoroutine(this.PerformShootRunner(gun));
		}
		private IEnumerator PerformShootRunner(Gun gun)
		{
			yield return this.SpellDeclare(gun.Spell);
			GunManager.GunShoot(gun, false);
			yield break;
		}
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
		public IEnumerator ForceEndShoot()
		{
			if (this._status == UnitView.ShootStatus.Complex)
			{
				yield return this.ShootComplexForceEnd();
			}
			yield break;
		}
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
		private EffectWidget SpawnShooter(string shooterName, float delay)
		{
			this._shooterName = shooterName;
			Transform transform = this.shooterPoint;
			EffectManager.CreateEffect("Shooter" + shooterName + "Start", transform, true);
			return EffectManager.CreateEffect("Shooter" + shooterName + "Loop", transform, delay, new float?(0f), true, true);
		}
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
		private void ShootEndActs(Gun gun, bool instant)
		{
			base.StartCoroutine(this.ShootEndActsRunner(gun, instant));
		}
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
		private void ShowEndActs()
		{
			this._gunInShooting = null;
			this._status = UnitView.ShootStatus.Idle;
		}
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
		public void CastShieldToOther(bool cast = true)
		{
			if (cast)
			{
				this.DefendAnimation();
			}
		}
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
		private EffectWidget CreateEternalShieldEffect(string effectName, bool isShield = true)
		{
			EffectWidget effectWidget = EffectManager.CreateEffect(effectName, this.effectRoot, 0f, default(float?), true, false);
			EffectManager.ModifyEffect(effectWidget, isShield ? this._shieldRadius : this._blockRadius, 0);
			return effectWidget;
		}
		private void CreateLocalShieldEffect(string effectName, bool isShield = true)
		{
			EffectManager.ModifyEffect(EffectManager.CreateEffect(effectName, this.effectRoot, true), isShield ? this._shieldRadius : this._blockRadius, 0);
		}
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
		public void PlayAnimation(string animationName)
		{
			this.SetAnimation(animationName, 1f, false);
			this.spineSkeleton.Update(0f);
		}
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
		private void StartBlinking()
		{
			this._blinkTimer = Random.Range(2f, 5f);
			this._blinking = true;
		}
		private void StopBlinking()
		{
			this._blinking = false;
		}
		private void Blink()
		{
			this._blinkTimer = Random.Range(2f, 5f);
			foreach (global::Spine.AnimationState animationState in this.AllStates)
			{
				animationState.SetAnimation(1, "blink", false);
			}
		}
		private void StartPosing()
		{
			this._poseTimer = Random.Range(32f, 40f);
			this._posing = true;
		}
		private void StopPosing()
		{
			this._posing = false;
		}
		private void Pose()
		{
			this.SetAnimation("pose1", 1f, false);
		}
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
		private float SequenceTimer { get; set; }
		private TrackEntry CurrentTrack { get; set; }
		private List<Vector2> SequenceKeys { get; } = new List<Vector2>();
		private int SequenceIndicator { get; set; }
		private bool Sequencing { get; set; }
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
		private void DefendAnimation()
		{
			this.SetAnimation("defend", 1f, false);
		}
		private void GrazeAnimation(float speed)
		{
			if (!this._invincible && !this.DoremySleeping)
			{
				this.SetAnimation("graze", speed, false);
			}
		}
		private float LastGuardTime { get; set; }
		private void GuardAnimation(float speed = 1f)
		{
			if (!this._invincible && !this.DoremySleeping && this.LifeTime - this.LastGuardTime > this._guardRep)
			{
				this.SetAnimation("guard", speed, false);
				this.LastGuardTime = this.LifeTime;
			}
		}
		private void CrashAnimation(float speed)
		{
			if (!this._invincible)
			{
				this.SetAnimation("crash", speed, false);
			}
		}
		private float LastHitTime { get; set; }
		private void HitAnimation(float speed, bool ignoreCoolDown)
		{
			if (!this._invincible && (ignoreCoolDown || this.LifeTime - this.LastHitTime > this._hitRep))
			{
				this.SetAnimation("hit", speed, false);
				this.LastHitTime = this.LifeTime;
			}
		}
		public void DeathAnimation()
		{
			this._invincible = true;
			this.SetAnimation("hit", 0.1f, true);
		}
		private void SpellAnimation()
		{
			this.SetAnimation("spell", 1f, false);
		}
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
		private void CancelFadeIn()
		{
			CancellationTokenSource fadeInCts = this._fadeInCts;
			if (fadeInCts == null)
			{
				return;
			}
			fadeInCts.Cancel();
		}
		private async UniTask FadeInStatusTask(float delay)
		{
			await UniTask.WaitForSeconds(delay, false, PlayerLoopTiming.Update, this._fadeInCts.Token, false);
			this.SetStatusVisible(true, false);
		}
		private void SetTrigger(string triggerName)
		{
			this._animator.SetTrigger(triggerName);
		}
		public Action ClickHandler { get; set; }
		private bool IsPlayer { get; set; }
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
		public Transform ChatPoint
		{
			get
			{
				return this.chatPoint;
			}
		}
		public Transform HitPoint
		{
			get
			{
				return this.hitPoint;
			}
		}
		public Transform EffectRoot
		{
			get
			{
				return this.effectRoot;
			}
		}
		public Collider SelectorCollider
		{
			get
			{
				return this.selectorCollider;
			}
		}
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
		public Unit Unit { get; set; }
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
		private void Hide()
		{
			this.rendererParent.gameObject.SetActive(false);
			this.effectRoot.gameObject.SetActive(false);
			this.SetStatusVisible(false, true);
		}
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
		private void Die()
		{
			this.rendererParent.gameObject.SetActive(false);
			this.effectRoot.gameObject.SetActive(false);
			this.effectRootIgnoreHiding.gameObject.SetActive(false);
			this.selectorCollider.gameObject.SetActive(false);
			this._statusWidget.SetVisible(false, false);
			this._infoWidget.SetVisible(false, false);
		}
		private void Escape()
		{
			this.rendererParent.gameObject.SetActive(false);
			this.effectRoot.gameObject.SetActive(false);
			this.effectRootIgnoreHiding.gameObject.SetActive(false);
			this.selectorCollider.gameObject.SetActive(false);
			this._statusWidget.SetVisible(false, false);
			this._infoWidget.SetVisible(false, false);
		}
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
		public void Event_OnPointerEnter()
		{
			this.ShowName();
			this.StatusToFront();
		}
		public void Event_OnPointerExit()
		{
			this.HideName();
		}
		public void Event_OnPointerClick()
		{
			this.Clicked();
		}
		private void ShowName()
		{
			if (this._infoWidget)
			{
				this._infoWidget.IncreaseHoveringLevel();
			}
		}
		private void HideName()
		{
			if (this._infoWidget)
			{
				this._infoWidget.DecreaseHoveringLevel();
			}
		}
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
		private void Clicked()
		{
			Action clickHandler = this.ClickHandler;
			if (clickHandler == null)
			{
				return;
			}
			clickHandler.Invoke();
		}
		public void Chat(string content, float time, ChatWidget.CloudType type = ChatWidget.CloudType.LeftThink, float delay = 0f)
		{
			if (!this._chatWidget)
			{
				this._chatWidget = UiManager.GetPanel<UnitStatusHud>().CreateChatWidget(this);
				this._chatWidget.name = "Chat: " + base.name;
			}
			this._chatWidget.Chat(content, time, delay, type);
		}
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
		public void SetPlayerHpBarLength(int maxHp)
		{
			this._statusWidget.SetPlayerHpBarLength(maxHp);
		}
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
		public int RootIndex { get; set; }
		public int AngleIndex { get; set; }
		public void PlayEffectOneShot(string effectName, float delay)
		{
			EffectManager.CreateEffect(effectName, this.effectRoot, delay, default(float?), true, true);
		}
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
		public void OnMaxHpChanged()
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnMaxHpChanged();
			}
		}
		public void OnDamageReceived(DamageInfo damage)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnDamageReceived(damage);
			}
		}
		public void OnHealingReceived(int amount)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnHealingReceived(amount);
			}
		}
		public void OnForceKill()
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnForceKill();
			}
		}
		public void OnAddStatusEffect(StatusEffect se, StatusEffectAddResult addResult)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnAddStatusEffect(se, addResult);
			}
		}
		public void OnRemoveStatusEffect(StatusEffect se)
		{
			if (this._statusWidget)
			{
				this._statusWidget.OnRemoveStatusEffect(se);
			}
		}
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
		public void ShowSePopup(string content, StatusEffectType seType, int number = 0, UnitInfoWidget.SePopType popType = UnitInfoWidget.SePopType.Add)
		{
			if (this._infoWidget)
			{
				this._infoWidget.ShowSePopup(content, seType, number, popType);
			}
		}
		public void OnGlobalStatusChanged()
		{
			foreach (DollView dollView in this.Dolls)
			{
				dollView.OnChanged();
			}
		}
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
		private DollSlotView CreateDollSlotView(Transform parent)
		{
			this.ShowDoll = true;
			DollSlotView dollSlotView = Object.Instantiate<DollSlotView>(this.dollSlotTemplate, parent);
			Transform transform = dollSlotView.transform;
			transform.localPosition = transform.localPosition.WithZ(0.1f);
			return dollSlotView;
		}
		private DollView CreateDollView(Doll doll, Transform parent)
		{
			this.ShowDoll = true;
			DollView dollView = Object.Instantiate<DollView>(this.dollTemplate, parent);
			dollView.name = doll.Id + "(Doll)";
			dollView.Doll = doll;
			return dollView;
		}
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
		public DollView GetDoll(Doll doll)
		{
			return Enumerable.FirstOrDefault<DollView>(this.Dolls, (DollView d) => d.Doll == doll);
		}
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
		private IEnumerator ViewConsumeMagic(ConsumeMagicAction action)
		{
			DollView doll = this.GetDoll(action.Args.Doll);
			if (doll != null)
			{
				doll.InfoWidget.ConsumeMagic();
			}
			yield break;
		}
		public void UpdateIntentions()
		{
			if (this._infoWidget)
			{
				this._infoWidget.UpdateIntentions();
			}
		}
		public void SetMoveOrder(int order)
		{
			if (this._infoWidget)
			{
				this._infoWidget.SetMoveOrder(order);
			}
		}
		public void SetMoveOrderVisible(bool visible)
		{
			if (this._infoWidget)
			{
				this._infoWidget.IsMoveOrderVisible = visible;
			}
		}
		public void ShowMovePopup(string moveName, bool closeMoveName)
		{
			if (this._infoWidget)
			{
				this._infoWidget.ShowMovePopup(moveName, closeMoveName);
			}
		}
		public void SetOrb(string effectName, int orbitIndex)
		{
			EffectBullet effectBullet = RinOrb.CreateRinOrb(effectName, orbitIndex);
			Transform transform = this.effectRoot;
			EffectBulletView effectBulletView = EffectManager.CreateEffectBullet(effectBullet, default(Vector3), transform);
			this._orbs.Add(effectBulletView);
		}
		public IEnumerator MoveOrbToEnemy(EnemyUnit enemy)
		{
			yield return new WaitForSeconds(0.1f);
			yield break;
		}
		public IEnumerator RecycleOrb()
		{
			yield return new WaitForSeconds(0.1f);
			yield break;
		}
		public void SetEffect(SkirtColor skirtColor)
		{
			this._kokoroUnitController.SwitchToColor(skirtColor);
		}
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
		public void SetSleep(bool sleep)
		{
			this.DoremySleeping = sleep;
		}
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
		private const float GuardEffectInterval = 0.2f;
		private bool _hasBlock;
		private bool _hasShield;
		private string _shooterName;
		private EffectWidget _shooterLoopEffect;
		private Gun _complexFirstGun;
		private Gun _gunInShooting;
		private bool _shootCounting;
		private float _shootTime;
		private bool _shooterCounting;
		private float _shooterTime;
		private UnitView.ShootStatus _status;
		private Animator _animator;
		private CircleCollider2D _circleCollider;
		private bool _bothShield;
		private bool _invincible;
		private DamageInfo _comingDamage;
		private bool _spineNeedReload;
		private bool _doremySleeping;
		private bool _hasFly;
		private bool _hasBlink;
		private bool _blinking;
		private float _blinkTimer;
		private const float BlinkMin = 2f;
		private const float BlinkMax = 5f;
		private const int NormalTrack = 0;
		private const int BlinkEyeTrack = 1;
		private const int FlyTrack = 2;
		private bool _hasPose;
		private bool _posing;
		private float _poseTimer;
		private const float PoseMin = 32f;
		private const float PoseMax = 40f;
		private CancellationTokenSource _fadeInCts;
		private ChatWidget _chatWidget;
		private UnitStatusWidget _statusWidget;
		private UnitInfoWidget _infoWidget;
		private bool _flip;
		private bool _isHidden;
		private bool _selectingVisible;
		[Header("Spine")]
		[SerializeField]
		private Transform rendererParent;
		[SerializeField]
		private SkeletonAnimation spineSkeleton;
		[SerializeField]
		private List<SkeletonAnimation> prefabList;
		[Header("Binding")]
		[SerializeField]
		private SpriteRenderer spriteRenderer;
		[SerializeField]
		private BoxCollider selectorCollider;
		[Header("Points")]
		[SerializeField]
		private Transform pointsParent;
		[SerializeField]
		private Transform effectRoot;
		[SerializeField]
		private Transform effectRootIgnoreHiding;
		[SerializeField]
		private Transform chatPoint;
		[SerializeField]
		private Transform hitPoint;
		[SerializeField]
		private Transform hpBarPoint;
		[SerializeField]
		private Transform shooterPoint;
		[SerializeField]
		private Transform infoPoint;
		[SerializeField]
		private Transform dollRoot;
		[Header("Resource Ref")]
		[SerializeField]
		private RuntimeAnimatorController simpleUnitAnimatorController;
		[SerializeField]
		private DollSlotView dollSlotTemplate;
		[SerializeField]
		private DollView dollTemplate;
		private string _modelName;
		private UnitView.ModelType _type;
		private Vector2 _offset;
		private bool _flipModel;
		private int _dieLevel;
		private Vector2 _boxSize;
		private float _shieldRadius;
		private float _blockRadius;
		private Vector2 _hitPointV2;
		private Vector2 _hpV2;
		private float _hitRep;
		private float _guardRep;
		private float _hpLength;
		private Vector2 _infoV2;
		private Vector3 _selectV3;
		private Vector2 _chatV2;
		private Vector2 _spellV2;
		private float _spellScale;
		private Color[] _spellColors;
		private Sprite _spellPortrait;
		private List<float> _shootStartTimeList;
		private List<Vector2> _shootPointV2List;
		private List<Vector2> _shooterPointV2List;
		private const string FallbackCharacter = "Koishi";
		private bool _unitModelLoaded;
		private EffectWidget _effectModel;
		private readonly Dictionary<string, EffectWidget> _effectDictionary = new Dictionary<string, EffectWidget>();
		private bool _showDoll;
		private readonly List<DollSlotView> _slots = new List<DollSlotView>();
		public readonly List<DollView> Dolls = new List<DollView>();
		private const float DollMoveTime = 0.2f;
		private const float DollAddAndRemoveTime = 0.05f;
		private readonly List<Vector2> _slotPositions;
		private readonly List<EffectBulletView> _orbs;
		private const float MoveTime = 0.1f;
		private KokoroUnitController _kokoroUnitController;
		public enum ShootStatus
		{
			Idle,
			Direct,
			Complex
		}
		private enum ModelType
		{
			SingleSprite,
			Spine,
			Effect
		}
	}
}
