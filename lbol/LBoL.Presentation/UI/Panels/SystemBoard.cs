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
	public class SystemBoard : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}
		public List<BaseManaWidget> GetNotLockedBaseManaWidgets()
		{
			return Enumerable.ToList<BaseManaWidget>(Enumerable.Where<BaseManaWidget>(this._baseManaWidgets, (BaseManaWidget widget) => !widget.IsLocked));
		}
		public List<BaseManaWidget> GetExtraTurnManaWidgets()
		{
			return Enumerable.ToList<BaseManaWidget>(this._extraTurnManaWidgets);
		}
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
		private void OnEnable()
		{
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.SetGameMode);
		}
		private void OnDisable()
		{
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.SetGameMode);
		}
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
		protected override void OnEnterBattle()
		{
			this.SetBaseManaLockStatus(base.Battle.LockedTurnMana);
			base.Battle.ActionViewer.Register<LockTurnManaAction>(new BattleActionViewer<LockTurnManaAction>(this.ViewLockMana), null);
			base.Battle.ActionViewer.Register<UnlockTurnManaAction>(new BattleActionViewer<UnlockTurnManaAction>(this.ViewUnlockMana), null);
			base.Battle.ActionViewer.Register<GainTurnManaAction>(new BattleActionViewer<GainTurnManaAction>(this.ViewGainTurnMana), null);
			base.Battle.ActionViewer.Register<LoseTurnManaAction>(new BattleActionViewer<LoseTurnManaAction>(this.ViewLoseTurnMana), null);
		}
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
		private void OnStageEntered()
		{
			this.SetStageLevel(base.GameRun.CurrentStage.Level);
			this.SetLevel(0);
		}
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
		public void EnterStation(Station station)
		{
			this.SetLevel(station.Level);
		}
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
		private IEnumerator ViewLockMana(LockTurnManaAction action)
		{
			this.SetBaseManaLockStatus(base.Battle.LockedTurnMana);
			yield break;
		}
		private IEnumerator ViewUnlockMana(UnlockTurnManaAction action)
		{
			this.SetBaseManaLockStatus(base.Battle.LockedTurnMana);
			yield break;
		}
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
		private void UnlockAllBaseManaWidgets()
		{
			foreach (BaseManaWidget baseManaWidget in this._baseManaWidgets)
			{
				baseManaWidget.IsLocked = false;
			}
		}
		private IEnumerator ViewGainTurnMana(GainTurnManaAction action)
		{
			this.SetExtraTurnMana(base.Battle.ExtraTurnMana);
			yield break;
		}
		private IEnumerator ViewLoseTurnMana(LoseTurnManaAction action)
		{
			this.SetExtraTurnMana(base.Battle.ExtraTurnMana);
			yield break;
		}
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
		private void SetLevel(int level)
		{
			this._levelTs.SetWithTooltipKeyAndArgs("Level", new object[] { level });
			this.levelValue.text = this._levelTs.Title;
		}
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
		private void SetHp(int hp)
		{
			this._hp = hp;
			this.hpValue.text = string.Format("{0}/{1}", this._hp, this._maxHp);
		}
		private void SetMaxHp(int maxHp)
		{
			this._maxHp = maxHp;
			this.hpValue.text = string.Format("{0}/{1}", this._hp, this._maxHp);
		}
		private void SetMoney(int money)
		{
			this._money = money;
			this.moneyValue.text = money.ToString();
		}
		public void OnHpChanged()
		{
			DOTween.Kill(this.hpValue, true);
			DOTween.To(() => this._hp, new DOSetter<int>(this.SetHp), base.GameRun.Player.Hp, 1f).SetTarget(this.hpValue).OnComplete(new TweenCallback(this.SetHeart));
			this._hpTs.SetWithTooltipKeyAndArgs("Hp", new object[] { base.GameRun.Player.Hp });
		}
		public void OnMaxHpChanged()
		{
			DOTween.Kill(this.hpValue, true);
			DOTween.To(() => this._hp, new DOSetter<int>(this.SetHp), base.GameRun.Player.Hp, 1f).SetTarget(this.hpValue).OnComplete(new TweenCallback(this.SetHeart));
			DOTween.To(() => this._maxHp, new DOSetter<int>(this.SetMaxHp), base.GameRun.Player.MaxHp, 1f).SetTarget(this.hpValue);
			this._hpTs.SetWithTooltipKeyAndArgs("Hp", new object[] { base.GameRun.Player.Hp });
		}
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
		public void OnDeckChanged()
		{
			this.deckSize.text = base.GameRun.BaseDeck.Count.ToString();
		}
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
		private void OnMenuClicked()
		{
			SettingPanel panel = UiManager.GetPanel<SettingPanel>();
			if (!panel.IsVisible)
			{
				panel.Show(SettingsPanelType.InGame);
			}
		}
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
		private void SetCharacter(PlayerUnit playerUnit)
		{
			this.characterImage.sprite = ResourcesHelper.LoadCharacterAvatarSprite(playerUnit.ModelName);
			this._characterTs.SetWithTooltipKeyAndArgs("Character", new object[] { playerUnit.Name });
		}
		private void SetDifficulty()
		{
			this.difficultyImage.sprite = CollectionExtensions.GetValueOrDefault<GameDifficulty, Sprite>(this.difficultyTable, base.GameRun.Difficulty);
			int puzzleLevel = PuzzleFlags.GetPuzzleLevel(base.GameRun.Puzzles);
			this.puzzleRankText.gameObject.SetActive(puzzleLevel > 0);
			this.puzzleRankText.text = puzzleLevel.ToString();
		}
		private void InitialVersionAndSeed()
		{
			this.gameVersion.text = VersionInfo.Current.Version;
			this.gameSeedButton.onClick.AddListener(new UnityAction(this.CopySeed));
			SimpleTooltipSource.CreateWithTooltipKey(this.gameSeedButton.gameObject, "GameSeed").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.HideHint();
		}
		private void SetSeed()
		{
			this.gameSeed.text = RandomGen.SeedToString(base.GameRun.RootSeed);
		}
		private void CopySeed()
		{
			GUIUtility.systemCopyBuffer = this.gameSeed.text;
			this.ShowHint("UI.CopyHint", 0.8f);
		}
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
		private void HideHint()
		{
			this.hintParent.SetActive(false);
		}
		public void ShowGameSaveHint()
		{
			this.ShowHint("UI.SaveHint", 3f);
		}
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
		public void SetBattleStatus(BattleStatus status)
		{
			this.BattleStatus = status;
			this.RefreshBattleStatus();
		}
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
		public BattleStatus BattleStatus { get; set; }
		public Vector2 ExhibitPanelShortSize { get; private set; }
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
		public ExhibitWidget FindExhibit(Exhibit exhibit)
		{
			return Enumerable.FirstOrDefault<ExhibitWidget>(this._exhibitWidgets, (ExhibitWidget widget) => widget.Exhibit == exhibit);
		}
		private void PageLeftClicked()
		{
			this.scrollRect.content.anchoredPosition += new Vector2(this.pageRollLength, 0f);
		}
		private void PageRightClicked()
		{
			this.scrollRect.content.anchoredPosition -= new Vector2(this.pageRollLength, 0f);
		}
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
		public RectTransform GetRectForHint()
		{
			return this.hintRect;
		}
		private RectTransform JadeBoxPanel
		{
			get
			{
				return UiManager.GetPanel<TopMessagePanel>().jadeBoxPanel;
			}
		}
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
		public void UI_OpenJadeBoxPanel()
		{
			this.ShowJadeBoxPanel = true;
		}
		private bool MouseInArea(RectTransform area)
		{
			Vector2 anchoredPosition = area.anchoredPosition;
			Vector2 sizeDelta = area.sizeDelta;
			return anchoredPosition.x < this._pointerX && anchoredPosition.y < this._pointerY && this._pointerX < anchoredPosition.x + sizeDelta.x && this._pointerY < anchoredPosition.y + sizeDelta.y;
		}
		private int _hp;
		private int _maxHp;
		private int _money;
		private bool _isEnterNextStage;
		private SimpleTooltipSource _characterTs;
		private SimpleTooltipSource _hpTs;
		private SimpleTooltipSource _moneyTs;
		private SimpleTooltipSource _difficultyTs;
		private SimpleTooltipSource _puzzleTs;
		private SimpleTooltipSource _stageTs;
		private SimpleTooltipSource _levelTs;
		private IList<string> _numbers;
		private readonly List<BaseManaWidget> _baseManaWidgets = new List<BaseManaWidget>();
		private readonly List<BaseManaWidget> _extraTurnManaWidgets = new List<BaseManaWidget>();
		private bool _hasMoneyGainVisual;
		private float _moneyGainDelay = 0.8f;
		private const float ExplodeTime = 0.3f;
		private const float PauseTime = 0.2f;
		private const float AbsorbTime = 0.3f;
		private const float HeartRatio1 = 0.7f;
		private const float HeartRatio2 = 0.3f;
		private int _heartIndex = -1;
		[Header("DynamicValues")]
		[SerializeField]
		private GameObject hpObj;
		[SerializeField]
		private GameObject moneyObj;
		[SerializeField]
		private GameObject levelObj;
		[SerializeField]
		private TextMeshProUGUI hpValue;
		[SerializeField]
		private Image heartImage;
		[SerializeField]
		private List<Sprite> heartSprites;
		[SerializeField]
		private TextMeshProUGUI moneyValue;
		[SerializeField]
		private TextMeshProUGUI levelValue;
		[SerializeField]
		private GameObject stageObj;
		[SerializeField]
		private TextMeshProUGUI stageValue;
		[Header("Buttons")]
		[SerializeField]
		private Button mapButton;
		public Button deckButton;
		[SerializeField]
		private Button menuButton;
		[SerializeField]
		private TextMeshProUGUI deckSize;
		[Header("Infos")]
		[SerializeField]
		private GameObject characterRaycast;
		[SerializeField]
		private Image characterImage;
		[SerializeField]
		private Image difficultyImage;
		[SerializeField]
		private TextMeshProUGUI puzzleRankText;
		[SerializeField]
		private AssociationList<GameDifficulty, Sprite> difficultyTable;
		[SerializeField]
		private TextMeshProUGUI gameMode;
		[SerializeField]
		private TextMeshProUGUI gameVersion;
		[SerializeField]
		private Button gameSeedButton;
		[SerializeField]
		private TextMeshProUGUI gameSeed;
		[SerializeField]
		private GameObject hintParent;
		[SerializeField]
		private TextMeshProUGUI hintTmp;
		[SerializeField]
		private Image hintCircle;
		[SerializeField]
		private BaseManaWidget baseManaTemplate;
		[SerializeField]
		private Transform baseManaContent;
		[SerializeField]
		private Transform baseManaParent;
		[SerializeField]
		private Transform extraManaParent;
		[SerializeField]
		private TextMeshProUGUI doremyLevel;
		[SerializeField]
		private TextMeshProUGUI battleStatusText;
		[Header("Exhibit")]
		public RectTransform exhibitPanel;
		[SerializeField]
		private Button pageLeft;
		[SerializeField]
		private Button pageRight;
		[SerializeField]
		private ExhibitWidget exhibitTemplate;
		[SerializeField]
		private ScrollRect scrollRect;
		[SerializeField]
		private float pageRollLength;
		[SerializeField]
		private Vector2 cellSize;
		[SerializeField]
		private Vector2 spacing;
		[SerializeField]
		private Vector2 padding;
		[SerializeField]
		private RectTransform hintRect;
		private readonly List<ExhibitWidget> _exhibitWidgets = new List<ExhibitWidget>();
		public List<ExhibitWidget> sortedExhibitWidgets = new List<ExhibitWidget>();
		private Dictionary<ExhibitWidget, float> _losingExhibitWidgets = new Dictionary<ExhibitWidget, float>();
		private GameEntity _losingSource;
		private const float LoseExhibitInterval = 0.1f;
		[Header("JadeBox")]
		public RectTransform jadeBoxHint;
		private bool _showJadeBoxPanel;
		private float _pointerX;
		private float _pointerY;
		private float _screenScale = 1f;
	}
}
