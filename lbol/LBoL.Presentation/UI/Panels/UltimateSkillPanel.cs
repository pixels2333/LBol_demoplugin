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
	public class UltimateSkillPanel : UiPanel, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}
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
		public void Awake()
		{
			this._currentPower = 0;
			this.canvasGroup.alpha = 0f;
			this._descriptionTs = this.skillImage.gameObject.AddComponent<UltimateSkillTooltipSource>();
			this.IsShown = false;
			this.gauge1.fillAmount = 0f;
			this.gauge2.fillAmount = 0f;
		}
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
		protected override void OnEnterBattle()
		{
			base.Battle.ActionViewer.Register<GainPowerAction>(new BattleActionViewer<GainPowerAction>(this.ViewGainPower), null);
			base.Battle.ActionViewer.Register<ConsumePowerAction>(new BattleActionViewer<ConsumePowerAction>(this.ViewConsumePower), null);
			base.Battle.ActionViewer.Register<LosePowerAction>(new BattleActionViewer<LosePowerAction>(this.ViewLosePower), null);
			this._interactable = true;
			this.OnPowerChanged(true);
		}
		protected override void OnLeaveGameRun()
		{
			base.GameRun.Player.Us.PropertyChanged -= new Action(this.OnPropertyChanged);
		}
		private void OnPropertyChanged()
		{
		}
		protected override void OnLeaveBattle()
		{
			base.Battle.ActionViewer.Unregister<GainPowerAction>(new BattleActionViewer<GainPowerAction>(this.ViewGainPower));
			base.Battle.ActionViewer.Unregister<ConsumePowerAction>(new BattleActionViewer<ConsumePowerAction>(this.ViewConsumePower));
			base.Battle.ActionViewer.Unregister<LosePowerAction>(new BattleActionViewer<LosePowerAction>(this.ViewLosePower));
			this._interactable = false;
			this._pendingUse = false;
		}
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
		private IEnumerator ViewLosePower(LosePowerAction action)
		{
			this.LosePower(action.Args.Power);
			yield break;
		}
		private IEnumerator ViewConsumePower(ConsumePowerAction action)
		{
			this.ConsumePower(action.Args.Power);
			this._pendingUse = false;
			yield break;
		}
		public void GainPower(int value)
		{
			AudioManager.PlayUi((value > 15) ? "UltGain" : "UltGainLittle", false);
			this.OnPowerChanged(false);
		}
		public void LosePower(int value)
		{
			AudioManager.PlayUi("UltLose", false);
			this.OnPowerChanged(false);
		}
		public void ConsumePower(int value)
		{
			this.OnPowerChanged(false);
		}
		public void UseUsFromKey()
		{
			if (this._interactable)
			{
				this.StartUsingUltimateSkill(false);
			}
		}
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
		public void CancelUse()
		{
			this._pendingUse = false;
		}
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
		public void HideInDialog()
		{
			this.canvasGroup.DOKill(false);
			this.canvasGroup.DOFade(0f, 0.2f);
			this.IsShown = false;
		}
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
		public void OnPointerDown(PointerEventData eventData)
		{
			if (this._interactable && eventData.button == PointerEventData.InputButton.Left && !UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize)
			{
				this.StartUsingUltimateSkill(true);
			}
		}
		public void OnPointerUp(PointerEventData eventData)
		{
			if (this._interactable && eventData.button == PointerEventData.InputButton.Left)
			{
				UiManager.GetPanel<PlayBoard>().OnPointerUp(eventData);
			}
		}
		[SerializeField]
		private Transform root;
		[SerializeField]
		private CanvasGroup canvasGroup;
		[SerializeField]
		private Image skillImage;
		[SerializeField]
		private TextMeshProUGUI powerText;
		[SerializeField]
		private Image gauge1;
		[SerializeField]
		private Image gauge2;
		[SerializeField]
		private Image gauge3;
		[SerializeField]
		private Color gauge1FontColor;
		[SerializeField]
		private Color gauge2FontColor;
		[SerializeField]
		private Color gauge3FontColor;
		[SerializeField]
		private ParticleSystem fireParticle1;
		[SerializeField]
		private ParticleSystem fireParticle2;
		[SerializeField]
		private ParticleSystem fireParticle3;
		[SerializeField]
		private ParticleSystem lightParticle;
		private UltimateSkillTooltipSource _descriptionTs;
		private int _currentPower;
		private bool _pendingUse;
		private bool _interactable;
		private bool _shown;
	}
}
