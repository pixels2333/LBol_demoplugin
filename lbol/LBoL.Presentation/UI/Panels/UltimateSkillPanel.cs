using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.Presentation.Effect;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000C1 RID: 193
	public class UltimateSkillPanel : UiPanel, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06000B6B RID: 2923 RVA: 0x0003B80B File Offset: 0x00039A0B
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000B6C RID: 2924 RVA: 0x0003B80E File Offset: 0x00039A0E
		// (set) Token: 0x06000B6D RID: 2925 RVA: 0x0003B818 File Offset: 0x00039A18
		private int CurrentPower
		{
			get
			{
				return this._currentPower;
			}
			set
			{
				int powerPerLevel = base.GameRun.Player.PowerPerLevel;
				int num = this._currentPower / powerPerLevel;
				this._currentPower = value;
				int num2 = this._currentPower / powerPerLevel;
				int num3 = this._currentPower % powerPerLevel;
				Image image = this.gauge3;
				float num4;
				switch (num2)
				{
				case 0:
					num4 = 0f;
					break;
				case 1:
					num4 = 0f;
					break;
				case 2:
					num4 = (float)num3 / (float)powerPerLevel;
					break;
				case 3:
					num4 = 1f;
					break;
				default:
					num4 = 1f;
					break;
				}
				image.fillAmount = num4;
				image = this.gauge2;
				switch (num2)
				{
				case 0:
					num4 = 0f;
					break;
				case 1:
					num4 = (float)num3 / (float)powerPerLevel;
					break;
				case 2:
					num4 = 1f;
					break;
				case 3:
					num4 = 1f;
					break;
				default:
					num4 = 1f;
					break;
				}
				image.fillAmount = num4;
				Image image2 = this.gauge1;
				if (num2 == 0)
				{
					num4 = (float)num3 / (float)powerPerLevel;
				}
				else
				{
					num4 = 1f;
				}
				image2.fillAmount = num4;
				Color white;
				if (num != num2)
				{
					if (num2 > num)
					{
						switch (num2)
						{
						case 1:
							white = this.gauge1FontColor;
							break;
						case 2:
							white = this.gauge2FontColor;
							break;
						case 3:
							white = this.gauge3FontColor;
							break;
						default:
							white = this.gauge2FontColor;
							break;
						}
						Color color = white;
						ParticleSystem[] componentsInChildren = this.lightParticle.GetComponentsInChildren<ParticleSystem>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].main.startColor = color;
						}
						this.lightParticle.Play(true);
						if (num2 == 1)
						{
							this.fireParticle1.Play(true);
						}
						if (num2 == 2)
						{
							this.fireParticle1.Stop(true);
							this.fireParticle2.Play(true);
						}
						if (num2 == 3)
						{
							this.fireParticle2.Stop(true);
							this.fireParticle3.Play(true);
						}
						AudioManager.PlayUi("UltCanUse" + num2.ToString(), false);
					}
					else
					{
						if (num == 1)
						{
							this.fireParticle1.Stop(true);
						}
						if (num == 2)
						{
							this.fireParticle1.Play(true);
							this.fireParticle2.Stop(true);
						}
						if (num == 3)
						{
							this.fireParticle2.Play(true);
							this.fireParticle3.Stop(true);
						}
					}
				}
				switch (num2)
				{
				case 1:
					white = this.gauge1FontColor;
					break;
				case 2:
					white = this.gauge2FontColor;
					break;
				case 3:
					white = this.gauge3FontColor;
					break;
				default:
					white = Color.white;
					break;
				}
				Color color2 = white;
				this.powerText.text = string.Concat(new string[]
				{
					"<color=#",
					ColorUtility.ToHtmlStringRGB(color2),
					">",
					this._currentPower.ToString(),
					" </color>/ ",
					powerPerLevel.ToString()
				});
			}
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000B6E RID: 2926 RVA: 0x0003BAEC File Offset: 0x00039CEC
		// (set) Token: 0x06000B6F RID: 2927 RVA: 0x0003BAF4 File Offset: 0x00039CF4
		public bool IsShown
		{
			get
			{
				return this._shown;
			}
			set
			{
				this._descriptionTs.enabled = value;
				this._shown = value;
			}
		}

		// Token: 0x06000B70 RID: 2928 RVA: 0x0003BB0C File Offset: 0x00039D0C
		public void Awake()
		{
			this._currentPower = 0;
			this.canvasGroup.alpha = 0f;
			this._descriptionTs = this.skillImage.gameObject.AddComponent<UltimateSkillTooltipSource>();
			this.IsShown = false;
			this.gauge1.fillAmount = 0f;
			this.gauge2.fillAmount = 0f;
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x0003BB70 File Offset: 0x00039D70
		protected override void OnEnterGameRun()
		{
			UltimateSkill us = base.GameRun.Player.Us;
			this._descriptionTs.Skill = us;
			Sprite sprite = ResourcesHelper.TryGetSprite<UltimateSkill>(us.Id);
			if (sprite != null)
			{
				this.skillImage.sprite = sprite;
			}
			us.PropertyChanged += new Action(this.OnPropertyChanged);
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x0003BBD0 File Offset: 0x00039DD0
		protected override void OnEnterBattle()
		{
			base.Battle.ActionViewer.Register<GainPowerAction>(new BattleActionViewer<GainPowerAction>(this.ViewGainPower), null);
			base.Battle.ActionViewer.Register<ConsumePowerAction>(new BattleActionViewer<ConsumePowerAction>(this.ViewConsumePower), null);
			base.Battle.ActionViewer.Register<LosePowerAction>(new BattleActionViewer<LosePowerAction>(this.ViewLosePower), null);
			this._interactable = true;
			this.OnPowerChanged(true);
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x0003BC42 File Offset: 0x00039E42
		protected override void OnLeaveGameRun()
		{
			base.GameRun.Player.Us.PropertyChanged -= new Action(this.OnPropertyChanged);
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x0003BC65 File Offset: 0x00039E65
		private void OnPropertyChanged()
		{
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x0003BC68 File Offset: 0x00039E68
		protected override void OnLeaveBattle()
		{
			base.Battle.ActionViewer.Unregister<GainPowerAction>(new BattleActionViewer<GainPowerAction>(this.ViewGainPower));
			base.Battle.ActionViewer.Unregister<ConsumePowerAction>(new BattleActionViewer<ConsumePowerAction>(this.ViewConsumePower));
			base.Battle.ActionViewer.Unregister<LosePowerAction>(new BattleActionViewer<LosePowerAction>(this.ViewLosePower));
			this._interactable = false;
			this._pendingUse = false;
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x0003BCD7 File Offset: 0x00039ED7
		private IEnumerator ViewGainPower(GainPowerAction action)
		{
			Vector3 vector = UiManager.GetPanel<PlayBoard>().FindActionSourceWorldPosition(action.Source) ?? Vector3.zero;
			Vector3 vector2 = CameraController.ScenePositionToWorldPositionInUI(GameDirector.Player.transform.position);
			int i = action.Args.Power;
			int num = 0;
			while (i > 8)
			{
				i -= 5;
				num++;
			}
			for (int j = 0; j < i; j++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.Power,
					TargetPosition = vector2
				}, vector, UiManager.GetPanel<GameRunVisualPanel>().transform);
			}
			for (int k = 0; k < num; k++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.BigPower,
					TargetPosition = vector2
				}, vector, UiManager.GetPanel<GameRunVisualPanel>().transform);
			}
			yield return new WaitForSeconds(1.5f);
			this.GainPower(action.Args.Power);
			yield break;
		}

		// Token: 0x06000B77 RID: 2935 RVA: 0x0003BCED File Offset: 0x00039EED
		private IEnumerator ViewLosePower(LosePowerAction action)
		{
			this.LosePower(action.Args.Power);
			yield break;
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0003BD03 File Offset: 0x00039F03
		private IEnumerator ViewConsumePower(ConsumePowerAction action)
		{
			this.ConsumePower(action.Args.Power);
			this._pendingUse = false;
			yield break;
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x0003BD19 File Offset: 0x00039F19
		public void GainPower(int value)
		{
			AudioManager.PlayUi((value > 15) ? "UltGain" : "UltGainLittle", false);
			this.OnPowerChanged(false);
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x0003BD39 File Offset: 0x00039F39
		public void LosePower(int value)
		{
			AudioManager.PlayUi("UltLose", false);
			this.OnPowerChanged(false);
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x0003BD4D File Offset: 0x00039F4D
		public void ConsumePower(int value)
		{
			this.OnPowerChanged(false);
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x0003BD56 File Offset: 0x00039F56
		public void UseUsFromKey()
		{
			if (this._interactable)
			{
				this.StartUsingUltimateSkill(false);
			}
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x0003BD68 File Offset: 0x00039F68
		private void StartUsingUltimateSkill(bool fromClick)
		{
			if (this._pendingUse || !base.Battle.Player.IsInTurn)
			{
				return;
			}
			UltimateSkill us = base.Battle.Player.Us;
			if (!us.Available)
			{
				if (us.BattleAvailable)
				{
					UiManager.GetPanel<PlayBoard>().ShowUsUsedThisTurn();
				}
				else
				{
					UiManager.GetPanel<PlayBoard>().ShowUsUsedThisBattle();
				}
				AudioManager.PlayUi("UltCD", false);
				return;
			}
			if (base.Battle.Player.Power < us.PowerCost)
			{
				UiManager.GetPanel<PlayBoard>().ShowLowPower();
				AudioManager.PlayUi("UltCantUse", false);
				return;
			}
			AudioManager.PlayUi("UltClick", false);
			this._pendingUse = true;
			if (us.TargetType == TargetType.SingleEnemy)
			{
				UiManager.GetPanel<PlayBoard>().EnableSelector(us, this.skillImage.transform.position, fromClick);
				return;
			}
			UnitSelector unitSelector;
			switch (us.TargetType)
			{
			case TargetType.Nobody:
				unitSelector = UnitSelector.Nobody;
				goto IL_0119;
			case TargetType.AllEnemies:
				unitSelector = UnitSelector.AllEnemies;
				goto IL_0119;
			case TargetType.RandomEnemy:
				unitSelector = UnitSelector.RandomEnemy;
				goto IL_0119;
			case TargetType.Self:
				unitSelector = UnitSelector.Self;
				goto IL_0119;
			case TargetType.All:
				unitSelector = UnitSelector.All;
				goto IL_0119;
			}
			throw new ArgumentOutOfRangeException();
			IL_0119:
			UnitSelector unitSelector2 = unitSelector;
			UiManager.GetPanel<PlayBoard>().RequestUseUs(unitSelector2);
			foreach (UnitView unitView in GameDirector.EnumeratePotentialTargets(us.TargetType))
			{
				unitView.SelectingVisible = false;
			}
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x0003BEE4 File Offset: 0x0003A0E4
		public void CancelUse()
		{
			this._pendingUse = false;
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x0003BEED File Offset: 0x0003A0ED
		public void ShowInDialog()
		{
			if (this.IsShown)
			{
				return;
			}
			this.canvasGroup.DOKill(false);
			this.canvasGroup.DOFade(1f, 0.2f);
			this.IsShown = true;
			this.OnPowerChanged(true);
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x0003BF29 File Offset: 0x0003A129
		public void HideInDialog()
		{
			this.canvasGroup.DOKill(false);
			this.canvasGroup.DOFade(0f, 0.2f);
			this.IsShown = false;
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x0003BF58 File Offset: 0x0003A158
		private void OnPowerChanged(bool instant = false)
		{
			if (!this.IsShown)
			{
				this.canvasGroup.DOKill(false);
				this._descriptionTs.enabled = true;
				this.IsShown = true;
				this.canvasGroup.DOFade(1f, 0.2f);
			}
			DOTween.Kill(base.gameObject, false);
			int power = base.GameRun.Player.Power;
			if (instant)
			{
				this.CurrentPower = power;
				return;
			}
			DOTween.To(() => this.CurrentPower, delegate(int value)
			{
				this.CurrentPower = value;
			}, power, 1f).SetTarget(base.gameObject).SetUpdate(UpdateType.Fixed);
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x0003C004 File Offset: 0x0003A204
		public void OnPointerEnter(PointerEventData eventData)
		{
			AudioManager.Button(2);
			if (base.Battle != null)
			{
				UltimateSkill us = base.Battle.Player.Us;
				if (!this._pendingUse && us.Available && base.Battle.Player.Power >= us.PowerCost)
				{
					TargetType targetType = base.Battle.Player.Us.TargetType;
					if (targetType != TargetType.SingleEnemy)
					{
						foreach (UnitView unitView in GameDirector.EnumeratePotentialTargets(targetType))
						{
							unitView.SelectingVisible = true;
						}
					}
				}
			}
		}

		// Token: 0x06000B83 RID: 2947 RVA: 0x0003C0B4 File Offset: 0x0003A2B4
		public void OnPointerExit(PointerEventData eventData)
		{
			if (base.Battle != null && base.Battle.Player.Us.TargetType != TargetType.SingleEnemy)
			{
				foreach (UnitView unitView in GameDirector.EnumeratePotentialTargets(base.Battle.Player.Us.TargetType))
				{
					unitView.SelectingVisible = false;
				}
			}
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x0003C134 File Offset: 0x0003A334
		public void OnPointerDown(PointerEventData eventData)
		{
			if (this._interactable && eventData.button == PointerEventData.InputButton.Left && !UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize)
			{
				this.StartUsingUltimateSkill(true);
			}
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x0003C159 File Offset: 0x0003A359
		public void OnPointerUp(PointerEventData eventData)
		{
			if (this._interactable && eventData.button == PointerEventData.InputButton.Left)
			{
				UiManager.GetPanel<PlayBoard>().OnPointerUp(eventData);
			}
		}

		// Token: 0x040008F5 RID: 2293
		[SerializeField]
		private Transform root;

		// Token: 0x040008F6 RID: 2294
		[SerializeField]
		private CanvasGroup canvasGroup;

		// Token: 0x040008F7 RID: 2295
		[SerializeField]
		private Image skillImage;

		// Token: 0x040008F8 RID: 2296
		[SerializeField]
		private TextMeshProUGUI powerText;

		// Token: 0x040008F9 RID: 2297
		[SerializeField]
		private Image gauge1;

		// Token: 0x040008FA RID: 2298
		[SerializeField]
		private Image gauge2;

		// Token: 0x040008FB RID: 2299
		[SerializeField]
		private Image gauge3;

		// Token: 0x040008FC RID: 2300
		[SerializeField]
		private Color gauge1FontColor;

		// Token: 0x040008FD RID: 2301
		[SerializeField]
		private Color gauge2FontColor;

		// Token: 0x040008FE RID: 2302
		[SerializeField]
		private Color gauge3FontColor;

		// Token: 0x040008FF RID: 2303
		[SerializeField]
		private ParticleSystem fireParticle1;

		// Token: 0x04000900 RID: 2304
		[SerializeField]
		private ParticleSystem fireParticle2;

		// Token: 0x04000901 RID: 2305
		[SerializeField]
		private ParticleSystem fireParticle3;

		// Token: 0x04000902 RID: 2306
		[SerializeField]
		private ParticleSystem lightParticle;

		// Token: 0x04000903 RID: 2307
		private UltimateSkillTooltipSource _descriptionTs;

		// Token: 0x04000904 RID: 2308
		private int _currentPower;

		// Token: 0x04000905 RID: 2309
		private bool _pendingUse;

		// Token: 0x04000906 RID: 2310
		private bool _interactable;

		// Token: 0x04000907 RID: 2311
		private bool _shown;
	}
}
