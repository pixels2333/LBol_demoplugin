using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.Effect;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000BE RID: 190
	public class SystemBoard : UiPanel
	{
		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000AEF RID: 2799 RVA: 0x00038321 File Offset: 0x00036521
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x00038324 File Offset: 0x00036524
		public List<BaseManaWidget> GetNotLockedBaseManaWidgets()
		{
			return Enumerable.ToList<BaseManaWidget>(Enumerable.Where<BaseManaWidget>(this._baseManaWidgets, (BaseManaWidget widget) => !widget.IsLocked));
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x00038355 File Offset: 0x00036555
		public List<BaseManaWidget> GetExtraTurnManaWidgets()
		{
			return Enumerable.ToList<BaseManaWidget>(this._extraTurnManaWidgets);
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x00038364 File Offset: 0x00036564
		private void Awake()
		{
			this.baseManaParent.DestroyChildren();
			this.extraManaParent.DestroyChildren();
			this.mapButton.onClick.AddListener(new UnityAction(this.ToggleMapPanel));
			this.deckButton.onClick.AddListener(new UnityAction(this.ShowBaseDeck));
			this.menuButton.onClick.AddListener(new UnityAction(this.OnMenuClicked));
			this._characterTs = SimpleTooltipSource.CreateDirect(this.characterRaycast, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._hpTs = SimpleTooltipSource.CreateDirect(this.hpObj, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._moneyTs = SimpleTooltipSource.CreateDirect(this.moneyObj, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._difficultyTs = SimpleTooltipSource.CreateDirect(this.difficultyImage.gameObject, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._puzzleTs = SimpleTooltipSource.CreateDirect(this.puzzleRankText.gameObject, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this._stageTs = SimpleTooltipSource.CreateDirect(this.stageObj, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			this._levelTs = SimpleTooltipSource.CreateDirect(this.levelObj, "", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.mapButton.gameObject, "Map.Map", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.deckButton.gameObject, "Game.Deck", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.menuButton.gameObject, "System.SystemMenu", null).WithPosition(TooltipDirection.Bottom, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithTooltipKey(this.baseManaContent.gameObject, "BaseMana").WithPosition(TooltipDirection.Bottom, TooltipAlignment.Min);
			this.InitialVersionAndSeed();
			this.doremyLevel.gameObject.SetActive(false);
			this.battleStatusText.gameObject.SetActive(false);
			this.ExhibitPanelShortSize = this.exhibitPanel.sizeDelta;
			this.pageLeft.onClick.AddListener(new UnityAction(this.PageLeftClicked));
			this.pageRight.onClick.AddListener(new UnityAction(this.PageRightClicked));
			this.jadeBoxHint.gameObject.SetActive(true);
			this.ShowJadeBoxPanel = false;
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x000385B7 File Offset: 0x000367B7
		private void OnEnable()
		{
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.SetGameMode);
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x000385CA File Offset: 0x000367CA
		private void OnDisable()
		{
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.SetGameMode);
		}

		// Token: 0x06000AF5 RID: 2805 RVA: 0x000385E0 File Offset: 0x000367E0
		public void Update()
		{
			if (this._losingExhibitWidgets.Count > 0)
			{
				StatusEffect statusEffect = this._losingSource as StatusEffect;
				if (statusEffect != null && statusEffect.Owner != null)
				{
					this.scrollRect.normalizedPosition = Vector2.zero;
					Transform exhibitTransform = GameDirector.GetUnit(statusEffect.Owner).EffectRoot.GetComponentInChildren<SeijaExhibitManager>().GetExhibitTransform("HolyGrailSe");
					foreach (ExhibitWidget exhibitWidget in Enumerable.ToList<ExhibitWidget>(Enumerable.Select<KeyValuePair<ExhibitWidget, float>, ExhibitWidget>(Enumerable.Where<KeyValuePair<ExhibitWidget, float>>(this._losingExhibitWidgets, (KeyValuePair<ExhibitWidget, float> pair) => pair.Value < 0f), (KeyValuePair<ExhibitWidget, float> pair) => pair.Key)))
					{
						exhibitWidget.transform.SetParent(base.GetComponent<RectTransform>(), true);
						exhibitWidget.ShowObtained(false);
						this._exhibitWidgets.Remove(exhibitWidget);
						this.SortExhibits();
						this.GridExhibits();
						Vector3 vector = exhibitWidget.transform.position - new Vector3((float)Random.Range(-3, 3), (float)Random.Range(2, 4), 0f);
						exhibitWidget.gameObject.AddComponent<CubicBezierMoving>().Init(0.6f, vector, exhibitTransform.transform, true);
						this._losingExhibitWidgets.Remove(exhibitWidget);
					}
					Dictionary<ExhibitWidget, float> dictionary = new Dictionary<ExhibitWidget, float>();
					foreach (KeyValuePair<ExhibitWidget, float> keyValuePair in this._losingExhibitWidgets)
					{
						ExhibitWidget exhibitWidget2;
						float num;
						keyValuePair.Deconstruct(ref exhibitWidget2, ref num);
						ExhibitWidget exhibitWidget3 = exhibitWidget2;
						dictionary.Add(exhibitWidget3, this._losingExhibitWidgets[exhibitWidget3] - Time.deltaTime);
					}
					this._losingExhibitWidgets = dictionary;
				}
			}
			if (this.ShowJadeBoxPanel)
			{
				if (Mouse.current == null)
				{
					return;
				}
				float num;
				float num2;
				Mouse.current.position.ReadValue().Deconstruct(out num, out num2);
				float num3 = num;
				float num4 = num2;
				int width = Screen.width;
				if (Math.Abs(width - 3840) > 1)
				{
					this._screenScale = 3840f / (float)width;
				}
				else
				{
					this._screenScale = 1f;
				}
				this._pointerX = num3 * this._screenScale;
				this._pointerY = num4 * this._screenScale;
				if (!this.MouseInArea(this.jadeBoxHint) && !this.MouseInArea(this.JadeBoxPanel))
				{
					this.ShowJadeBoxPanel = false;
				}
			}
		}

		// Token: 0x06000AF6 RID: 2806 RVA: 0x0003888C File Offset: 0x00036A8C
		public override void OnLocaleChanged()
		{
			this._numbers = "UI.Numbers".LocalizeStrings(true);
			GameRunController gameRun = base.GameRun;
			if (gameRun != null)
			{
				this.SetCharacter(base.GameRun.Player);
				Stage currentStage = gameRun.CurrentStage;
				this.SetStageLevel((currentStage != null) ? currentStage.Level : 0);
				Station currentStation = gameRun.CurrentStation;
				this.SetLevel((currentStation != null) ? currentStation.Level : 0);
				this.SetGameMode(null);
			}
			if (base.Battle != null)
			{
				this.RefreshBattleStatus();
			}
		}

		// Token: 0x06000AF7 RID: 2807 RVA: 0x0003890C File Offset: 0x00036B0C
		protected override void OnEnterGameRun()
		{
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.StageEntered, delegate(GameEventArgs _)
			{
				this.OnStageEntered();
			});
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.BaseManaChanged, delegate(GameEventArgs _)
			{
				this.SetBaseMana(base.GameRun.BaseMana);
			});
			this.SetTooltips();
			this.SetMaxHp(base.GameRun.Player.MaxHp);
			this.SetHp(base.GameRun.Player.Hp);
			this.SetHeart();
			this.SetMoney(base.GameRun.Money);
			this.SetBaseMana(base.GameRun.BaseMana);
			this.OnDeckChanged();
			this.SetCharacter(base.GameRun.Player);
			this.SetDifficulty();
			this.SetSeed();
			this.SetGameMode(null);
			Stage currentStage = base.GameRun.CurrentStage;
			this.SetStageLevel((currentStage != null) ? currentStage.Level : 0);
			this.ResetExhibits(base.GameRun.Player.Exhibits);
			UiManager.GetPanel<TopMessagePanel>().SetJadeBoxes(Enumerable.ToList<JadeBox>(base.GameRun.JadeBoxes));
		}

		// Token: 0x06000AF8 RID: 2808 RVA: 0x00038A24 File Offset: 0x00036C24
		protected override void OnEnterBattle()
		{
			this.SetBaseManaLockStatus(base.Battle.LockedTurnMana);
			base.Battle.ActionViewer.Register<LockTurnManaAction>(new BattleActionViewer<LockTurnManaAction>(this.ViewLockMana), null);
			base.Battle.ActionViewer.Register<UnlockTurnManaAction>(new BattleActionViewer<UnlockTurnManaAction>(this.ViewUnlockMana), null);
			base.Battle.ActionViewer.Register<GainTurnManaAction>(new BattleActionViewer<GainTurnManaAction>(this.ViewGainTurnMana), null);
			base.Battle.ActionViewer.Register<LoseTurnManaAction>(new BattleActionViewer<LoseTurnManaAction>(this.ViewLoseTurnMana), null);
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x00038AB8 File Offset: 0x00036CB8
		protected override void OnLeaveBattle()
		{
			this.UnlockAllBaseManaWidgets();
			this.extraManaParent.DestroyChildren();
			this._extraTurnManaWidgets.Clear();
			base.Battle.ActionViewer.Unregister<LockTurnManaAction>(new BattleActionViewer<LockTurnManaAction>(this.ViewLockMana));
			base.Battle.ActionViewer.Unregister<UnlockTurnManaAction>(new BattleActionViewer<UnlockTurnManaAction>(this.ViewUnlockMana));
			base.Battle.ActionViewer.Unregister<GainTurnManaAction>(new BattleActionViewer<GainTurnManaAction>(this.ViewGainTurnMana));
			base.Battle.ActionViewer.Unregister<LoseTurnManaAction>(new BattleActionViewer<LoseTurnManaAction>(this.ViewLoseTurnMana));
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x00038B51 File Offset: 0x00036D51
		private void OnStageEntered()
		{
			this.SetStageLevel(base.GameRun.CurrentStage.Level);
			this.SetLevel(0);
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x00038B70 File Offset: 0x00036D70
		private void SetStageLevel(int level)
		{
			string text;
			string text2;
			if (level == 4)
			{
				text = "Tooltip.FinalStage";
				text2 = (text + ".Name").Localize(true);
			}
			else
			{
				text = "Tooltip.Stage";
				text2 = string.Format((text + ".Name").Localize(true), this._numbers[level]);
			}
			string text3 = StringDecorator.Decorate((text + ".Description").Localize(true));
			this._stageTs.SetDirect(text2, text3);
			this.stageValue.text = text2;
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x00038BF4 File Offset: 0x00036DF4
		public void EnterStation(Station station)
		{
			this.SetLevel(station.Level);
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x00038C04 File Offset: 0x00036E04
		private void SetBaseMana(ManaGroup baseMana)
		{
			this.baseManaParent.DestroyChildren();
			this._baseManaWidgets.Clear();
			foreach (ManaColor manaColor in baseMana.EnumerateComponents())
			{
				BaseManaWidget baseManaWidget = Object.Instantiate<BaseManaWidget>(this.baseManaTemplate, this.baseManaParent);
				baseManaWidget.SetBaseMana(manaColor);
				this._baseManaWidgets.Add(baseManaWidget);
			}
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x00038C88 File Offset: 0x00036E88
		private IEnumerator ViewLockMana(LockTurnManaAction action)
		{
			this.SetBaseManaLockStatus(base.Battle.LockedTurnMana);
			yield break;
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x00038C97 File Offset: 0x00036E97
		private IEnumerator ViewUnlockMana(UnlockTurnManaAction action)
		{
			this.SetBaseManaLockStatus(base.Battle.LockedTurnMana);
			yield break;
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x00038CA8 File Offset: 0x00036EA8
		private void SetBaseManaLockStatus(ManaGroup lockMana)
		{
			using (IEnumerator<ManaColor> enumerator = ManaColors.Colors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ManaColor color = enumerator.Current;
					foreach (ValueTuple<int, BaseManaWidget> valueTuple in Enumerable.Where<BaseManaWidget>(this._baseManaWidgets, (BaseManaWidget widget) => widget.ManaColor == color).WithIndices<BaseManaWidget>())
					{
						int item = valueTuple.Item1;
						valueTuple.Item2.IsLocked = item < lockMana.GetValue(color);
					}
				}
			}
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x00038D64 File Offset: 0x00036F64
		private void UnlockAllBaseManaWidgets()
		{
			foreach (BaseManaWidget baseManaWidget in this._baseManaWidgets)
			{
				baseManaWidget.IsLocked = false;
			}
		}

		// Token: 0x06000B02 RID: 2818 RVA: 0x00038DB8 File Offset: 0x00036FB8
		private IEnumerator ViewGainTurnMana(GainTurnManaAction action)
		{
			this.SetExtraTurnMana(base.Battle.ExtraTurnMana);
			yield break;
		}

		// Token: 0x06000B03 RID: 2819 RVA: 0x00038DC7 File Offset: 0x00036FC7
		private IEnumerator ViewLoseTurnMana(LoseTurnManaAction action)
		{
			this.SetExtraTurnMana(base.Battle.ExtraTurnMana);
			yield break;
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x00038DD8 File Offset: 0x00036FD8
		private void SetExtraTurnMana(ManaGroup extraTurnMana)
		{
			using (IEnumerator<ManaColor> enumerator = ManaColors.Colors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ManaColor color = enumerator.Current;
					BaseManaWidget baseManaWidget = Enumerable.FirstOrDefault<BaseManaWidget>(this._extraTurnManaWidgets, (BaseManaWidget widget) => widget.ManaColor == color);
					if (extraTurnMana.HasColor(color))
					{
						if (baseManaWidget != null)
						{
							baseManaWidget.SetBattleMana(color, extraTurnMana.GetValue(color));
						}
						else
						{
							baseManaWidget = Object.Instantiate<BaseManaWidget>(this.baseManaTemplate, this.extraManaParent);
							baseManaWidget.SetBattleMana(color, extraTurnMana.GetValue(color));
							this._extraTurnManaWidgets.Add(baseManaWidget);
						}
					}
					else if (baseManaWidget != null)
					{
						Object.Destroy(baseManaWidget.gameObject);
						this._extraTurnManaWidgets.Remove(baseManaWidget);
					}
				}
			}
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x00038ED4 File Offset: 0x000370D4
		private void SetLevel(int level)
		{
			this._levelTs.SetWithTooltipKeyAndArgs("Level", new object[] { level });
			this.levelValue.text = this._levelTs.Title;
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x00038F0C File Offset: 0x0003710C
		private void SetTooltips()
		{
			this._characterTs.SetWithTooltipKeyAndArgs("Character", new object[] { base.GameRun.Player.Name });
			this._hpTs.SetWithTooltipKeyAndArgs("Hp", new object[] { base.GameRun.Player.Hp });
			this._moneyTs.SetWithTooltipKeyAndArgs("Money", new object[] { base.GameRun.Money });
			this._difficultyTs.SetWithTooltipKey("Difficulty" + base.GameRun.Difficulty.ToString());
			SimpleTooltipSource stageTs = this._stageTs;
			string text = "Stage";
			object[] array = new object[1];
			int num = 0;
			IList<string> numbers = this._numbers;
			Stage currentStage = base.GameRun.CurrentStage;
			array[num] = numbers[(currentStage != null) ? currentStage.Index : 1];
			stageTs.SetWithTooltipKeyAndArgs(text, array);
			SimpleTooltipSource levelTs = this._levelTs;
			string text2 = "Level";
			object[] array2 = new object[1];
			int num2 = 0;
			Stage currentStage2 = base.GameRun.CurrentStage;
			array2[num2] = ((currentStage2 != null) ? currentStage2.Level : 0);
			levelTs.SetWithTooltipKeyAndArgs(text2, array2);
			string text3 = "";
			int puzzleLevel = PuzzleFlags.GetPuzzleLevel(base.GameRun.Puzzles);
			foreach (ValueTuple<int, PuzzleFlag> valueTuple in PuzzleFlags.EnumerateComponents(base.GameRun.Puzzles).WithIndices<PuzzleFlag>())
			{
				int item = valueTuple.Item1;
				PuzzleFlagDisplayWord displayWord = PuzzleFlags.GetDisplayWord(valueTuple.Item2);
				text3 = string.Concat(new string[]
				{
					text3,
					this._numbers[item + 1],
					"UI.Divide".Localize(true),
					displayWord.Description,
					"\n"
				});
			}
			string text4 = string.Format("StartGame.Puzzles".Localize(true), this._numbers[puzzleLevel]);
			this._puzzleTs.SetDirect(text4, text3);
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x0003911C File Offset: 0x0003731C
		public void CreateMoneyGainVisual(Vector3 from, int money, Transform parent, float duration = 1f)
		{
			AudioManager.PlayUi("MoneyAcquire", false);
			this._moneyGainDelay = duration;
			this._hasMoneyGainVisual = true;
			int i = money;
			int num = 0;
			int num2 = 0;
			while (i > 120)
			{
				i -= 100;
				num2++;
			}
			while (i > 8)
			{
				i -= 5;
				num++;
			}
			RectTransform rectTransform = (RectTransform)this.moneyObj.transform;
			rectTransform.TransformPoint(rectTransform.rect.center);
			for (int j = 0; j < i; j++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.Money,
					TargetPosition = rectTransform.TransformPoint(rectTransform.rect.center)
				}, from, parent);
			}
			for (int k = 0; k < num; k++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.BigMoney,
					TargetPosition = rectTransform.TransformPoint(rectTransform.rect.center)
				}, from, parent);
			}
			for (int l = 0; l < num2; l++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.SuperBigMoney,
					TargetPosition = rectTransform.TransformPoint(rectTransform.rect.center)
				}, from, parent);
			}
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x0003925F File Offset: 0x0003745F
		private void SetHp(int hp)
		{
			this._hp = hp;
			this.hpValue.text = string.Format("{0}/{1}", this._hp, this._maxHp);
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x00039293 File Offset: 0x00037493
		private void SetMaxHp(int maxHp)
		{
			this._maxHp = maxHp;
			this.hpValue.text = string.Format("{0}/{1}", this._hp, this._maxHp);
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x000392C7 File Offset: 0x000374C7
		private void SetMoney(int money)
		{
			this._money = money;
			this.moneyValue.text = money.ToString();
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x000392E4 File Offset: 0x000374E4
		public void OnHpChanged()
		{
			DOTween.Kill(this.hpValue, true);
			DOTween.To(() => this._hp, new DOSetter<int>(this.SetHp), base.GameRun.Player.Hp, 1f).SetTarget(this.hpValue).OnComplete(new TweenCallback(this.SetHeart));
			this._hpTs.SetWithTooltipKeyAndArgs("Hp", new object[] { base.GameRun.Player.Hp });
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x0003937C File Offset: 0x0003757C
		public void OnMaxHpChanged()
		{
			DOTween.Kill(this.hpValue, true);
			DOTween.To(() => this._hp, new DOSetter<int>(this.SetHp), base.GameRun.Player.Hp, 1f).SetTarget(this.hpValue).OnComplete(new TweenCallback(this.SetHeart));
			DOTween.To(() => this._maxHp, new DOSetter<int>(this.SetMaxHp), base.GameRun.Player.MaxHp, 1f).SetTarget(this.hpValue);
			this._hpTs.SetWithTooltipKeyAndArgs("Hp", new object[] { base.GameRun.Player.Hp });
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000B0D RID: 2829 RVA: 0x00039451 File Offset: 0x00037651
		// (set) Token: 0x06000B0E RID: 2830 RVA: 0x00039459 File Offset: 0x00037659
		private int HeartIndex
		{
			get
			{
				return this._heartIndex;
			}
			set
			{
				if (this._heartIndex == value)
				{
					return;
				}
				this._heartIndex = value;
				this.heartImage.sprite = this.heartSprites[this._heartIndex];
			}
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x00039488 File Offset: 0x00037688
		private void SetHeart()
		{
			float num = (float)this._hp / (float)this._maxHp;
			int num2;
			if (num >= 0.3f)
			{
				if (num >= 0.7f)
				{
					num2 = 0;
				}
				else
				{
					num2 = 1;
				}
			}
			else
			{
				num2 = 2;
			}
			int num3 = num2;
			this.HeartIndex = num3;
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x000394CC File Offset: 0x000376CC
		public void OnMoneyChanged()
		{
			DOTween.Kill(this.moneyValue, true);
			TweenerCore<int, int, NoOptions> tweenerCore = DOTween.To(() => this._money, new DOSetter<int>(this.SetMoney), base.GameRun.Money, 1f).SetTarget(this.moneyValue);
			if (this._hasMoneyGainVisual)
			{
				tweenerCore.SetDelay(this._moneyGainDelay);
			}
			this._moneyTs.SetWithTooltipKeyAndArgs("Money", new object[] { base.GameRun.Money });
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x00039560 File Offset: 0x00037760
		public void OnDeckChanged()
		{
			this.deckSize.text = base.GameRun.BaseDeck.Count.ToString();
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x00039590 File Offset: 0x00037790
		public void ShowBaseDeck()
		{
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.Deck".Localize(true),
				Description = "Cards.Show".Localize(true),
				Cards = base.GameRun.BaseDeck,
				InteractionType = InteractionType.None,
				CardZone = ShowCardZone.Library
			};
			UiManager.GetPanel<ShowCardsPanel>().Show(showCardsPayload);
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x000395F0 File Offset: 0x000377F0
		private void OnMenuClicked()
		{
			SettingPanel panel = UiManager.GetPanel<SettingPanel>();
			if (!panel.IsVisible)
			{
				panel.Show(SettingsPanelType.InGame);
			}
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x00039614 File Offset: 0x00037814
		public void ToggleMapPanel()
		{
			MapPanel panel = UiManager.GetPanel<MapPanel>();
			if (panel.IsVisible)
			{
				panel.Hide();
				return;
			}
			if (base.GameRun.CurrentStation != null)
			{
				panel.Show();
			}
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x00039649 File Offset: 0x00037849
		private void SetCharacter(PlayerUnit playerUnit)
		{
			this.characterImage.sprite = ResourcesHelper.LoadCharacterAvatarSprite(playerUnit.ModelName);
			this._characterTs.SetWithTooltipKeyAndArgs("Character", new object[] { playerUnit.Name });
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x00039680 File Offset: 0x00037880
		private void SetDifficulty()
		{
			this.difficultyImage.sprite = CollectionExtensions.GetValueOrDefault<GameDifficulty, Sprite>(this.difficultyTable, base.GameRun.Difficulty);
			int puzzleLevel = PuzzleFlags.GetPuzzleLevel(base.GameRun.Puzzles);
			this.puzzleRankText.gameObject.SetActive(puzzleLevel > 0);
			this.puzzleRankText.text = puzzleLevel.ToString();
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x000396E8 File Offset: 0x000378E8
		private void InitialVersionAndSeed()
		{
			this.gameVersion.text = VersionInfo.Current.Version;
			this.gameSeedButton.onClick.AddListener(new UnityAction(this.CopySeed));
			SimpleTooltipSource.CreateWithTooltipKey(this.gameSeedButton.gameObject, "GameSeed").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.HideHint();
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x00039749 File Offset: 0x00037949
		private void SetSeed()
		{
			this.gameSeed.text = RandomGen.SeedToString(base.GameRun.RootSeed);
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x00039766 File Offset: 0x00037966
		private void CopySeed()
		{
			GUIUtility.systemCopyBuffer = this.gameSeed.text;
			this.ShowHint("UI.CopyHint", 0.8f);
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x00039788 File Offset: 0x00037988
		private void ShowHint(string hintKey, float time)
		{
			DOTween.KillAll(this.hintParent);
			this.hintParent.SetActive(true);
			this.hintTmp.text = hintKey.Localize(true);
			this.hintCircle.fillAmount = 0f;
			DOTween.Sequence().Join(this.hintTmp.DOFade(1f, 0.2f).From(0f, true, false)).Join(this.hintCircle.DOFade(1f, 0.2f).From(0f, true, false))
				.Append(this.hintCircle.DOFillAmount(1f, time).SetEase(Ease.Linear))
				.Insert(time + 0.2f, this.hintTmp.DOFade(0f, 0.2f))
				.Insert(time + 0.2f, this.hintCircle.DOFade(0f, 0.2f))
				.SetUpdate(true)
				.SetTarget(this.hintParent)
				.OnComplete(new TweenCallback(this.HideHint));
		}

		// Token: 0x06000B1B RID: 2843 RVA: 0x000398A6 File Offset: 0x00037AA6
		private void HideHint()
		{
			this.hintParent.SetActive(false);
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x000398B4 File Offset: 0x00037AB4
		public void ShowGameSaveHint()
		{
			this.ShowHint("UI.SaveHint", 3f);
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x000398C8 File Offset: 0x00037AC8
		public void SetGameMode(GameSettingsSaveData saveData = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if ((saveData != null && saveData.IsTurboMode) || (saveData == null && GameMaster.IsTurboMode))
			{
				stringBuilder.Append("Setting.TurboPlaying".LocalizeFormat(new object[] { GlobalConfig.TurboModeTimeScaleString }));
				stringBuilder.Append("  ");
			}
			GameMode mode = base.GameRun.Mode;
			if (mode != GameMode.StoryMode)
			{
				if (mode != GameMode.FreeMode)
				{
					throw new ArgumentOutOfRangeException();
				}
				stringBuilder.Append("StartGame.FreeMode".Localize(true));
			}
			else
			{
				stringBuilder.Append("StartGame.StoryMode".Localize(true));
			}
			this.gameMode.text = stringBuilder.ToString();
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00039970 File Offset: 0x00037B70
		public void SetDoremyLevel(bool show, int level)
		{
			if (level < 0)
			{
				level = 0;
				Debug.LogWarning("Doremy Level out of range.");
			}
			if (level > 10)
			{
				level = 10;
				Debug.LogWarning("Doremy Level out of range.");
			}
			this.doremyLevel.text = string.Format("System.DoremyLevel".Localize(true), "UI.Numbers".LocalizeStrings(true)[level]);
			this.doremyLevel.gameObject.SetActive(show);
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x000399DE File Offset: 0x00037BDE
		public void SetBattleStatus(BattleStatus status)
		{
			this.BattleStatus = status;
			this.RefreshBattleStatus();
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x000399F0 File Offset: 0x00037BF0
		private void RefreshBattleStatus()
		{
			switch (this.BattleStatus)
			{
			case BattleStatus.OutOfBattle:
				this.battleStatusText.gameObject.SetActive(false);
				return;
			case BattleStatus.BattleStart:
				this.battleStatusText.text = "Game.BattleStart".Localize(true);
				this.battleStatusText.gameObject.SetActive(true);
				return;
			case BattleStatus.PlayerTurn:
			{
				string text = string.Format("Game.RoundCounter".Localize(true), base.Battle.RoundCounter);
				text += "  ";
				if (base.Battle.RoundCounter == base.Battle.Player.TurnCounter)
				{
					text += string.Format("Game.PlayerNameTurn".Localize(true), base.Battle.Player.ShortName);
				}
				else
				{
					text += string.Format("Game.PlayerNameTurnCounter".Localize(true), base.Battle.Player.ShortName, base.Battle.Player.TurnCounter);
				}
				this.battleStatusText.text = text;
				this.battleStatusText.gameObject.SetActive(true);
				return;
			}
			case BattleStatus.EnemyTurn:
			{
				string text = string.Format("Game.RoundCounter".Localize(true), base.Battle.RoundCounter);
				text += "  ";
				text += "Game.EnemyTurn".Localize(true);
				this.battleStatusText.text = text;
				this.battleStatusText.gameObject.SetActive(true);
				return;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000B21 RID: 2849 RVA: 0x00039B88 File Offset: 0x00037D88
		// (set) Token: 0x06000B22 RID: 2850 RVA: 0x00039B90 File Offset: 0x00037D90
		public BattleStatus BattleStatus { get; set; }

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06000B23 RID: 2851 RVA: 0x00039B99 File Offset: 0x00037D99
		// (set) Token: 0x06000B24 RID: 2852 RVA: 0x00039BA1 File Offset: 0x00037DA1
		public Vector2 ExhibitPanelShortSize { get; private set; }

		// Token: 0x06000B25 RID: 2853 RVA: 0x00039BAC File Offset: 0x00037DAC
		public void OnExhibitAdded(Exhibit exhibit, float delay)
		{
			ExhibitWidget newWidget = this.CreateExhibitWidget(exhibit);
			if (delay == 0f)
			{
				this.SortExhibits();
				this.GridExhibits();
				AudioManager.PlayUi("ExhibitGet", false);
				return;
			}
			newWidget.GetComponent<CanvasGroup>().alpha = 0f;
			this.SortExhibits();
			int num = this.sortedExhibitWidgets.IndexOf(newWidget);
			if (num != this.sortedExhibitWidgets.Count - 1)
			{
				for (int i = num + 1; i < this.sortedExhibitWidgets.Count; i++)
				{
					this.sortedExhibitWidgets[i].transform.DOLocalMoveX(this.cellSize.x + this.spacing.x, delay, false).SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>();
				}
			}
			newWidget.GetComponent<CanvasGroup>().DOFade(1f, 0f).SetDelay(delay)
				.OnComplete(delegate
				{
					AudioManager.PlayUi("ExhibitGet", false);
					newWidget.ShowObtained(true);
					this.GridExhibits();
				});
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x00039CB0 File Offset: 0x00037EB0
		public void OnExhibitRemoved(Exhibit exhibit)
		{
			ExhibitWidget exhibitWidget = this.FindExhibit(exhibit);
			if (exhibitWidget != null)
			{
				this._exhibitWidgets.Remove(exhibitWidget);
				Object.Destroy(exhibitWidget.gameObject);
			}
			this.SortExhibits();
			this.GridExhibits();
		}

		// Token: 0x06000B27 RID: 2855 RVA: 0x00039CF4 File Offset: 0x00037EF4
		private void ResetExhibits(IEnumerable<Exhibit> exhibits)
		{
			this._exhibitWidgets.Clear();
			this.scrollRect.content.DestroyChildren();
			foreach (Exhibit exhibit in exhibits)
			{
				this.CreateExhibitWidget(exhibit);
			}
			this.SortExhibits();
			this.GridExhibits();
		}

		// Token: 0x06000B28 RID: 2856 RVA: 0x00039D64 File Offset: 0x00037F64
		private void SortExhibits()
		{
			this.sortedExhibitWidgets.Clear();
			this._exhibitWidgets.ForEach(delegate(ExhibitWidget i)
			{
				this.sortedExhibitWidgets.Add(i);
			});
			List<ExhibitWidget> list = new List<ExhibitWidget>();
			int num = 0;
			int num2 = 0;
			foreach (ExhibitWidget exhibitWidget in this.sortedExhibitWidgets)
			{
				if (exhibitWidget.Exhibit.Config.Rarity == Rarity.Mythic)
				{
					list.Insert(num2++, exhibitWidget);
					num++;
				}
				else if (exhibitWidget.Exhibit.Config.Rarity == Rarity.Shining)
				{
					list.Insert(num++, exhibitWidget);
				}
				else
				{
					list.Add(exhibitWidget);
				}
			}
			this.sortedExhibitWidgets = list;
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x00039E38 File Offset: 0x00038038
		private void GridExhibits()
		{
			for (int i = 0; i < this.sortedExhibitWidgets.Count; i++)
			{
				RectTransform component = this.sortedExhibitWidgets[i].GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0f, 0.5f);
				component.anchorMax = new Vector2(0f, 0.5f);
				component.pivot = new Vector2(0f, 0.5f);
				component.sizeDelta = this.cellSize;
				component.anchoredPosition = new Vector2(this.padding.x + (float)i * (this.cellSize.x + this.spacing.x), 0f);
			}
			this.scrollRect.content.sizeDelta = new Vector2(this.padding.x + (float)this.sortedExhibitWidgets.Count * (this.cellSize.x + this.spacing.x), 0f);
		}

		// Token: 0x06000B2A RID: 2858 RVA: 0x00039F3C File Offset: 0x0003813C
		private ExhibitWidget CreateExhibitWidget(Exhibit exhibit)
		{
			ExhibitWidget widget = Object.Instantiate<ExhibitWidget>(this.exhibitTemplate, this.scrollRect.content);
			RectTransform component = widget.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 0.5f);
			component.anchorMax = new Vector2(0f, 0.5f);
			component.pivot = new Vector2(0f, 0.5f);
			component.sizeDelta = this.cellSize;
			widget.name = "Exhibit: " + exhibit.Id;
			widget.gameObject.SetActive(true);
			widget.Exhibit = exhibit;
			widget.ExhibitClicked += delegate
			{
				UiManager.GetPanel<ExhibitInfoPanel>().Show(widget.Exhibit);
			};
			widget.transform.position = this.NewExhibitWorldPosition(exhibit);
			widget.ShowBattleStatus = true;
			this._exhibitWidgets.Add(widget);
			return widget;
		}

		// Token: 0x06000B2B RID: 2859 RVA: 0x0003A04C File Offset: 0x0003824C
		public ExhibitWidget FindExhibit(Exhibit exhibit)
		{
			return Enumerable.FirstOrDefault<ExhibitWidget>(this._exhibitWidgets, (ExhibitWidget widget) => widget.Exhibit == exhibit);
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x0003A07D File Offset: 0x0003827D
		private void PageLeftClicked()
		{
			this.scrollRect.content.anchoredPosition += new Vector2(this.pageRollLength, 0f);
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x0003A0AA File Offset: 0x000382AA
		private void PageRightClicked()
		{
			this.scrollRect.content.anchoredPosition -= new Vector2(this.pageRollLength, 0f);
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x0003A0D8 File Offset: 0x000382D8
		public Vector3 NewExhibitWorldPosition(Exhibit newExhibit)
		{
			List<ExhibitWidget> list = this.sortedExhibitWidgets;
			if (this._exhibitWidgets.Empty<ExhibitWidget>() || this.sortedExhibitWidgets.Empty<ExhibitWidget>())
			{
				return this.scrollRect.content.TransformPoint(new Vector2(this.padding.x, 0f));
			}
			Rarity rarity = newExhibit.Config.Rarity;
			if (rarity == Rarity.Shining || rarity == Rarity.Mythic)
			{
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (list[i].Exhibit.Config.Rarity >= newExhibit.Config.Rarity)
					{
						RectTransform component = list[i].GetComponent<RectTransform>();
						return this.scrollRect.content.TransformPoint(component.localPosition + new Vector3(this.cellSize.x + this.spacing.x, 0f, 0f));
					}
					if (i == 0)
					{
						return this.scrollRect.content.TransformPoint(new Vector2(this.padding.x, 0f));
					}
				}
			}
			RectTransform component2 = Enumerable.Last<ExhibitWidget>(list).GetComponent<RectTransform>();
			return this.scrollRect.content.TransformPoint(component2.localPosition + new Vector3(this.cellSize.x + this.spacing.x, 0f, 0f));
		}

		// Token: 0x06000B2F RID: 2863 RVA: 0x0003A250 File Offset: 0x00038450
		public void CreateExhibitGainVisual(Exhibit e, Vector3 from, float duration)
		{
			AudioManager.PlayUi("ExhibitChoose", false);
			ExhibitWidget widget = Object.Instantiate<ExhibitWidget>(this.exhibitTemplate, UiManager.GetPanel<TooltipsLayer>().transform);
			widget.Exhibit = e;
			RectTransform component = widget.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 0.5f);
			component.anchorMax = new Vector2(0f, 0.5f);
			component.pivot = new Vector2(0f, 0.5f);
			component.sizeDelta = this.cellSize;
			widget.transform.position = from;
			widget.ShowObtained(false);
			this.scrollRect.content.sizeDelta += new Vector2(this.cellSize.x + this.spacing.x, 0f);
			Rarity rarity = e.Config.Rarity;
			if (rarity == Rarity.Shining || rarity == Rarity.Mythic)
			{
				this.scrollRect.normalizedPosition = Vector2.zero;
			}
			else
			{
				this.scrollRect.normalizedPosition = Vector2.one;
			}
			if (duration != 0f)
			{
				Vector3 vector = this.NewExhibitWorldPosition(e);
				widget.transform.DOMove(vector, duration, false).SetEase(Ease.OutQuad).OnComplete(delegate
				{
					widget.gameObject.SetActive(false);
				});
			}
			widget.GetComponent<ExhibitTooltipSource>().enabled = false;
			Object.Destroy(widget.gameObject, duration + 0.1f);
		}

		// Token: 0x06000B30 RID: 2864 RVA: 0x0003A3DC File Offset: 0x000385DC
		public IEnumerator LoseAllExhibits(IEnumerable<Exhibit> exhibits, GameEntity source)
		{
			List<Exhibit> list = Enumerable.ToList<Exhibit>(exhibits);
			if (list.Count > 0)
			{
				foreach (ValueTuple<int, Exhibit> valueTuple in list.WithIndices<Exhibit>())
				{
					int item = valueTuple.Item1;
					Exhibit item2 = valueTuple.Item2;
					if (item == 0)
					{
						AudioManager.PlayUi("ExhibitChoose", false);
					}
					ExhibitWidget exhibitWidget = this.FindExhibit(item2);
					StatusEffect statusEffect = source as StatusEffect;
					if (statusEffect != null && statusEffect.Owner != null)
					{
						this._losingSource = source;
						this._losingExhibitWidgets.Add(exhibitWidget, (float)item * 0.1f);
					}
					else
					{
						Object.Destroy(exhibitWidget.gameObject);
					}
				}
				yield return new WaitForSeconds(1f + 0.1f * (float)list.Count);
			}
			yield break;
		}

		// Token: 0x06000B31 RID: 2865 RVA: 0x0003A3F9 File Offset: 0x000385F9
		public RectTransform GetRectForHint()
		{
			return this.hintRect;
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000B32 RID: 2866 RVA: 0x0003A401 File Offset: 0x00038601
		private RectTransform JadeBoxPanel
		{
			get
			{
				return UiManager.GetPanel<TopMessagePanel>().jadeBoxPanel;
			}
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06000B33 RID: 2867 RVA: 0x0003A40D File Offset: 0x0003860D
		// (set) Token: 0x06000B34 RID: 2868 RVA: 0x0003A415 File Offset: 0x00038615
		private bool ShowJadeBoxPanel
		{
			get
			{
				return this._showJadeBoxPanel;
			}
			set
			{
				this._showJadeBoxPanel = value;
				UiManager.GetPanel<TopMessagePanel>().ShowJadeBoxPanel = value;
			}
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x0003A429 File Offset: 0x00038629
		public void UI_OpenJadeBoxPanel()
		{
			this.ShowJadeBoxPanel = true;
		}

		// Token: 0x06000B36 RID: 2870 RVA: 0x0003A434 File Offset: 0x00038634
		private bool MouseInArea(RectTransform area)
		{
			Vector2 anchoredPosition = area.anchoredPosition;
			Vector2 sizeDelta = area.sizeDelta;
			return anchoredPosition.x < this._pointerX && anchoredPosition.y < this._pointerY && this._pointerX < anchoredPosition.x + sizeDelta.x && this._pointerY < anchoredPosition.y + sizeDelta.y;
		}

		// Token: 0x0400088D RID: 2189
		private int _hp;

		// Token: 0x0400088E RID: 2190
		private int _maxHp;

		// Token: 0x0400088F RID: 2191
		private int _money;

		// Token: 0x04000890 RID: 2192
		private bool _isEnterNextStage;

		// Token: 0x04000891 RID: 2193
		private SimpleTooltipSource _characterTs;

		// Token: 0x04000892 RID: 2194
		private SimpleTooltipSource _hpTs;

		// Token: 0x04000893 RID: 2195
		private SimpleTooltipSource _moneyTs;

		// Token: 0x04000894 RID: 2196
		private SimpleTooltipSource _difficultyTs;

		// Token: 0x04000895 RID: 2197
		private SimpleTooltipSource _puzzleTs;

		// Token: 0x04000896 RID: 2198
		private SimpleTooltipSource _stageTs;

		// Token: 0x04000897 RID: 2199
		private SimpleTooltipSource _levelTs;

		// Token: 0x04000898 RID: 2200
		private IList<string> _numbers;

		// Token: 0x04000899 RID: 2201
		private readonly List<BaseManaWidget> _baseManaWidgets = new List<BaseManaWidget>();

		// Token: 0x0400089A RID: 2202
		private readonly List<BaseManaWidget> _extraTurnManaWidgets = new List<BaseManaWidget>();

		// Token: 0x0400089B RID: 2203
		private bool _hasMoneyGainVisual;

		// Token: 0x0400089C RID: 2204
		private float _moneyGainDelay = 0.8f;

		// Token: 0x0400089D RID: 2205
		private const float ExplodeTime = 0.3f;

		// Token: 0x0400089E RID: 2206
		private const float PauseTime = 0.2f;

		// Token: 0x0400089F RID: 2207
		private const float AbsorbTime = 0.3f;

		// Token: 0x040008A0 RID: 2208
		private const float HeartRatio1 = 0.7f;

		// Token: 0x040008A1 RID: 2209
		private const float HeartRatio2 = 0.3f;

		// Token: 0x040008A2 RID: 2210
		private int _heartIndex = -1;

		// Token: 0x040008A4 RID: 2212
		[Header("DynamicValues")]
		[SerializeField]
		private GameObject hpObj;

		// Token: 0x040008A5 RID: 2213
		[SerializeField]
		private GameObject moneyObj;

		// Token: 0x040008A6 RID: 2214
		[SerializeField]
		private GameObject levelObj;

		// Token: 0x040008A7 RID: 2215
		[SerializeField]
		private TextMeshProUGUI hpValue;

		// Token: 0x040008A8 RID: 2216
		[SerializeField]
		private Image heartImage;

		// Token: 0x040008A9 RID: 2217
		[SerializeField]
		private List<Sprite> heartSprites;

		// Token: 0x040008AA RID: 2218
		[SerializeField]
		private TextMeshProUGUI moneyValue;

		// Token: 0x040008AB RID: 2219
		[SerializeField]
		private TextMeshProUGUI levelValue;

		// Token: 0x040008AC RID: 2220
		[SerializeField]
		private GameObject stageObj;

		// Token: 0x040008AD RID: 2221
		[SerializeField]
		private TextMeshProUGUI stageValue;

		// Token: 0x040008AE RID: 2222
		[Header("Buttons")]
		[SerializeField]
		private Button mapButton;

		// Token: 0x040008AF RID: 2223
		public Button deckButton;

		// Token: 0x040008B0 RID: 2224
		[SerializeField]
		private Button menuButton;

		// Token: 0x040008B1 RID: 2225
		[SerializeField]
		private TextMeshProUGUI deckSize;

		// Token: 0x040008B2 RID: 2226
		[Header("Infos")]
		[SerializeField]
		private GameObject characterRaycast;

		// Token: 0x040008B3 RID: 2227
		[SerializeField]
		private Image characterImage;

		// Token: 0x040008B4 RID: 2228
		[SerializeField]
		private Image difficultyImage;

		// Token: 0x040008B5 RID: 2229
		[SerializeField]
		private TextMeshProUGUI puzzleRankText;

		// Token: 0x040008B6 RID: 2230
		[SerializeField]
		private AssociationList<GameDifficulty, Sprite> difficultyTable;

		// Token: 0x040008B7 RID: 2231
		[SerializeField]
		private TextMeshProUGUI gameMode;

		// Token: 0x040008B8 RID: 2232
		[SerializeField]
		private TextMeshProUGUI gameVersion;

		// Token: 0x040008B9 RID: 2233
		[SerializeField]
		private Button gameSeedButton;

		// Token: 0x040008BA RID: 2234
		[SerializeField]
		private TextMeshProUGUI gameSeed;

		// Token: 0x040008BB RID: 2235
		[SerializeField]
		private GameObject hintParent;

		// Token: 0x040008BC RID: 2236
		[SerializeField]
		private TextMeshProUGUI hintTmp;

		// Token: 0x040008BD RID: 2237
		[SerializeField]
		private Image hintCircle;

		// Token: 0x040008BE RID: 2238
		[SerializeField]
		private BaseManaWidget baseManaTemplate;

		// Token: 0x040008BF RID: 2239
		[SerializeField]
		private Transform baseManaContent;

		// Token: 0x040008C0 RID: 2240
		[SerializeField]
		private Transform baseManaParent;

		// Token: 0x040008C1 RID: 2241
		[SerializeField]
		private Transform extraManaParent;

		// Token: 0x040008C2 RID: 2242
		[SerializeField]
		private TextMeshProUGUI doremyLevel;

		// Token: 0x040008C3 RID: 2243
		[SerializeField]
		private TextMeshProUGUI battleStatusText;

		// Token: 0x040008C4 RID: 2244
		[Header("Exhibit")]
		public RectTransform exhibitPanel;

		// Token: 0x040008C6 RID: 2246
		[SerializeField]
		private Button pageLeft;

		// Token: 0x040008C7 RID: 2247
		[SerializeField]
		private Button pageRight;

		// Token: 0x040008C8 RID: 2248
		[SerializeField]
		private ExhibitWidget exhibitTemplate;

		// Token: 0x040008C9 RID: 2249
		[SerializeField]
		private ScrollRect scrollRect;

		// Token: 0x040008CA RID: 2250
		[SerializeField]
		private float pageRollLength;

		// Token: 0x040008CB RID: 2251
		[SerializeField]
		private Vector2 cellSize;

		// Token: 0x040008CC RID: 2252
		[SerializeField]
		private Vector2 spacing;

		// Token: 0x040008CD RID: 2253
		[SerializeField]
		private Vector2 padding;

		// Token: 0x040008CE RID: 2254
		[SerializeField]
		private RectTransform hintRect;

		// Token: 0x040008CF RID: 2255
		private readonly List<ExhibitWidget> _exhibitWidgets = new List<ExhibitWidget>();

		// Token: 0x040008D0 RID: 2256
		public List<ExhibitWidget> sortedExhibitWidgets = new List<ExhibitWidget>();

		// Token: 0x040008D1 RID: 2257
		private Dictionary<ExhibitWidget, float> _losingExhibitWidgets = new Dictionary<ExhibitWidget, float>();

		// Token: 0x040008D2 RID: 2258
		private GameEntity _losingSource;

		// Token: 0x040008D3 RID: 2259
		private const float LoseExhibitInterval = 0.1f;

		// Token: 0x040008D4 RID: 2260
		[Header("JadeBox")]
		public RectTransform jadeBoxHint;

		// Token: 0x040008D5 RID: 2261
		private bool _showJadeBoxPanel;

		// Token: 0x040008D6 RID: 2262
		private float _pointerX;

		// Token: 0x040008D7 RID: 2263
		private float _pointerY;

		// Token: 0x040008D8 RID: 2264
		private float _screenScale = 1f;
	}
}
