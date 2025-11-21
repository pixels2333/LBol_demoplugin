using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Lore;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.Presentation.Effect;
using LBoL.Presentation.Environments;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn;

namespace LBoL.Presentation
{
	// Token: 0x0200000A RID: 10
	public class GameMaster : Singleton<GameMaster>, IGameRunAchievementHandler, IGameRunVisualTrigger
	{
		// Token: 0x06000081 RID: 129 RVA: 0x000038DC File Offset: 0x00001ADC
		public static void IncreaseStats(ProfileStatsKey key)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile != null)
			{
				if (key != ProfileStatsKey.PayMoney)
				{
					throw new ArgumentOutOfRangeException("key", key, null);
				}
				int num = currentProfile.PayMoneyCount;
				if (num < 5)
				{
					num++;
					currentProfile.PayMoneyCount = num;
				}
				if (num >= 5)
				{
					GameMaster.UnlockAchievement(AchievementKey.MystiaAdventure);
					return;
				}
			}
			else
			{
				Debug.LogError("Cannot increase profile stats while profile is null");
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00003938 File Offset: 0x00001B38
		public static void UnlockAchievement(string key)
		{
			GameMaster.PlatformHandler.SetAchievement(key);
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile != null)
			{
				if (!currentProfile.Achievements.Contains(key))
				{
					currentProfile.Achievements.Add(key);
					currentProfile.Achievements.Sort();
					Singleton<GameMaster>.Instance.SaveProfile();
					UiManager.GetPanel<TopMessagePanel>().UnlockAchievement(key);
					return;
				}
			}
			else
			{
				Debug.LogError("Cannot unlock achievement while profile is null");
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x000039A3 File Offset: 0x00001BA3
		public static void UnlockAchievement(AchievementKey achievementKey)
		{
			GameMaster.UnlockAchievement(achievementKey.ToString());
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000039B8 File Offset: 0x00001BB8
		public static void ClearAchievement(string key)
		{
			GameMaster.PlatformHandler.ClearAchievement(key);
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile != null)
			{
				currentProfile.Achievements.Remove(key);
				Singleton<GameMaster>.Instance.SaveProfile();
				return;
			}
			Debug.LogError("Cannot clear achievement while profile is null");
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00003A00 File Offset: 0x00001C00
		public static void ClearAllAchievements()
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile != null)
			{
				foreach (string text in currentProfile.Achievements)
				{
					GameMaster.PlatformHandler.ClearAchievement(text);
				}
				currentProfile.Achievements.Clear();
				Singleton<GameMaster>.Instance.SaveProfile();
				return;
			}
			Debug.LogError("Cannot clear achievement while profile is null");
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00003A88 File Offset: 0x00001C88
		public static void UnlockAllAchievements()
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile != null)
			{
				foreach (AchievementConfig achievementConfig in AchievementConfig.AllConfig())
				{
					string id = achievementConfig.Id;
					if (!currentProfile.Achievements.Contains(id))
					{
						currentProfile.Achievements.Add(id);
						UiManager.GetPanel<TopMessagePanel>().UnlockAchievement(id);
					}
				}
				currentProfile.Achievements.Sort();
				Singleton<GameMaster>.Instance.SaveProfile();
				return;
			}
			Debug.LogError("Cannot unlock achievement while profile is null");
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00003B28 File Offset: 0x00001D28
		void IGameRunAchievementHandler.IncreaseStats(ProfileStatsKey statsKey)
		{
			GameMaster.IncreaseStats(statsKey);
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00003B30 File Offset: 0x00001D30
		void IGameRunAchievementHandler.UnlockAchievement(AchievementKey achievementKey)
		{
			GameMaster.UnlockAchievement(achievementKey);
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000089 RID: 137 RVA: 0x00003B38 File Offset: 0x00001D38
		// (set) Token: 0x0600008A RID: 138 RVA: 0x00003B40 File Offset: 0x00001D40
		public GameRunController CurrentGameRun { get; private set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00003B49 File Offset: 0x00001D49
		// (set) Token: 0x0600008C RID: 140 RVA: 0x00003B50 File Offset: 0x00001D50
		public static bool ShowPoseAnimation { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600008D RID: 141 RVA: 0x00003B58 File Offset: 0x00001D58
		public int CurrentGameRunPlayedSeconds
		{
			get
			{
				if (this.CurrentGameRun == null)
				{
					throw new InvalidOperationException("Cannot query played-seconds while CurrentGameRun is null");
				}
				return (DateTime.Now - this._currentGameRunStartTime).TotalSeconds.CeilingToInt() + this.CurrentGameRun.PlayedSeconds;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600008E RID: 142 RVA: 0x00003BA1 File Offset: 0x00001DA1
		public static bool ShowRandomResult
		{
			get
			{
				return Singleton<GameMaster>.Instance.CurrentGameRun != null && Singleton<GameMaster>.Instance.CurrentGameRun.ShowRandomResult;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00003BC0 File Offset: 0x00001DC0
		public static PlatformHandler PlatformHandler
		{
			get
			{
				return PlatformHandlerRunner.Instance.PlatformHandler;
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003BCC File Offset: 0x00001DCC
		private static void ShowErrorDialog(string error)
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				Text = error,
				Icon = MessageIcon.Error
			});
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00003BEB File Offset: 0x00001DEB
		private static void ShowErrorDialogWithKey(string errorKey)
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = errorKey,
				Icon = MessageIcon.Error
			});
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00003C0A File Offset: 0x00001E0A
		private static void ShowWarningDialogWithKey(string warningKey, [MaybeNull] string warningSubTextKey = null)
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = warningKey,
				SubTextKey = warningSubTextKey,
				Icon = MessageIcon.Warning
			});
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00003C30 File Offset: 0x00001E30
		public static void StartupEnterMainMenu(int? saveIndex)
		{
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoStartupEnterMainMenu(saveIndex));
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000094 RID: 148 RVA: 0x00003C48 File Offset: 0x00001E48
		// (set) Token: 0x06000095 RID: 149 RVA: 0x00003C4F File Offset: 0x00001E4F
		public static bool ShowCardId { get; set; }

		// Token: 0x06000096 RID: 150 RVA: 0x00003C57 File Offset: 0x00001E57
		private IEnumerator CoStartupEnterMainMenu(int? saveIndex)
		{
			AsyncOperation loadAsync = SceneManager.LoadSceneAsync("GameRun");
			loadAsync.allowSceneActivation = false;
			while (loadAsync.progress < 0.9f)
			{
				yield return null;
			}
			yield return UiManager.ShowLoading(0.5f).ToCoroutine(null);
			loadAsync.allowSceneActivation = true;
			while (!loadAsync.isDone)
			{
				yield return null;
			}
			yield return GameMaster.LoadMainMenuUiAsync().ToCoroutine(null);
			if (saveIndex != null)
			{
				GameMaster.SelectProfile(saveIndex);
			}
			if (UiManager.GetPanel<MuseumPanel>().Initialization())
			{
				UiManager.GetPanel<MuseumPanel>().gameObject.SetActive(true);
				yield return null;
				UiManager.GetPanel<MuseumPanel>().gameObject.SetActive(false);
			}
			this.ShowMainMenu();
			AudioManager.EnterMainMenu();
			GameMaster.Status = GameMaster.GameMasterStatus.MainMenu;
			yield return UiManager.HideLoading(0.5f).ToCoroutine(null);
			GameMaster.ShowCardId = false;
			yield break;
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003C70 File Offset: 0x00001E70
		public static void RequestAbandonGameRun(bool skipResult = false)
		{
			GameRunSaveData gameRunSaveData = Singleton<GameMaster>.Instance.GameRunSaveData;
			if (gameRunSaveData.Timing == SaveTiming.EnterMapNode && gameRunSaveData.EnteringNode == null)
			{
				GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
				return;
			}
			try
			{
				GameRunController gameRunController = GameRunController.Restore(gameRunSaveData, false);
				gameRunController.AbandonGameRun();
				GameRunRecordSaveData gameRunRecord = gameRunController.GameRunRecord;
				gameRunRecord.TotalSeconds = gameRunController.PlayedSeconds;
				GameMaster.GameStatisticData gameStatisticData = GameMaster.EndGameStatistics(gameRunController, gameRunRecord.ResultType);
				int exp = Singleton<GameMaster>.Instance._currentProfileSaveData.Exp;
				Singleton<GameMaster>.Instance.SaveProfileWithEndingGameRun(gameRunController, gameStatisticData.BluePoint, gameRunRecord);
				if (!skipResult)
				{
					Singleton<GameMaster>.Instance.StartCoroutine(GameMaster.CoAbandonGameRun(gameRunSaveData, exp, gameStatisticData));
				}
				else
				{
					UiManager.GetPanel<MainMenuPanel>().RefreshProfile();
				}
			}
			catch (Exception ex)
			{
				GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
				Debug.LogException(ex);
				GameMaster.TryDeleteSaveData(GameMaster.GetGameRunFileName(Singleton<GameMaster>.Instance.CurrentSaveIndex.Value), true);
				Singleton<GameMaster>.Instance.GameRunSaveData = null;
				UiManager.GetPanel<MainMenuPanel>().RefreshProfile();
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00003D70 File Offset: 0x00001F70
		private static IEnumerator CoAbandonGameRun(GameRunSaveData saveData, int previousTotalExp, GameMaster.GameStatisticData data)
		{
			GameResultPanel panel = UiManager.GetPanel<GameResultPanel>();
			panel.Show(new GameResultData
			{
				PlayerId = saveData.Player.Name,
				Type = (saveData.IsNormalEndFinished ? GameResultType.TrueEndFail : GameResultType.Failure),
				PreviousTotalExp = previousTotalExp,
				BluePoint = data.BluePoint,
				ScoreDatas = data.ScoreDatas,
				DifficultyMultipler = data.DifficultyMultipler,
				DebugExp = data.DebugExp
			});
			yield return new WaitWhile(() => panel.IsVisible);
			yield break;
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003D90 File Offset: 0x00001F90
		public static void StartGame(GameDifficulty difficulty, PuzzleFlag puzzles, PlayerUnit player, PlayerType playerType, Exhibit initExhibit, int? initMoneyOverride, IEnumerable<Card> deck, IEnumerable<Stage> stages, Type debutAdventureType, IEnumerable<JadeBox> jadeBoxes, GameMode gameMode = GameMode.FreeMode, bool showRandomResult = true)
		{
			GameMaster.StartGame(default(ulong?), difficulty, puzzles, player, playerType, initExhibit, initMoneyOverride, deck, stages, debutAdventureType, jadeBoxes, gameMode, showRandomResult);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003DC0 File Offset: 0x00001FC0
		public static void StartGame(ulong? seed, GameDifficulty difficulty, PuzzleFlag puzzles, PlayerUnit player, PlayerType playerType, Exhibit initExhibit, int? initMoneyOverride, IEnumerable<Card> deck, IEnumerable<Stage> stages, Type debutAdventureType, IEnumerable<JadeBox> jadeBoxes, GameMode gameMode = GameMode.FreeMode, bool showRandomResult = true)
		{
			if (Singleton<GameMaster>.Instance.CurrentGameRun != null)
			{
				GameMaster.ShowErrorDialogWithKey("AlreadyInGameRun");
				return;
			}
			ProfileSaveData currentProfileSaveData = Singleton<GameMaster>.Instance._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				GameMaster.ShowErrorDialogWithKey("CannotStartGameRunWithoutProfile");
				return;
			}
			GameRunController gameRunController = GameRunController.Create(new GameRunStartupParameters
			{
				Mode = gameMode,
				ShowRandomResult = showRandomResult,
				Seed = seed,
				Difficulty = difficulty,
				Puzzles = puzzles,
				Player = player,
				PlayerType = playerType,
				InitExhibit = initExhibit,
				InitMoneyOverride = initMoneyOverride,
				Deck = deck,
				Stages = stages,
				DebutAdventureType = debutAdventureType,
				UserProfile = currentProfileSaveData,
				UnlockLevel = ExpHelper.GetLevelForTotalExp(currentProfileSaveData.Exp),
				JadeBoxes = jadeBoxes
			});
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoNewGameRun(gameRunController));
			currentProfileSaveData.HasClearBonus = false;
			Singleton<GameMaster>.Instance.SaveProfile();
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00003EAC File Offset: 0x000020AC
		public static void RestoreGameRun(GameRunSaveData saveData)
		{
			if (Singleton<GameMaster>.Instance.CurrentGameRun != null)
			{
				GameMaster.ShowErrorDialogWithKey("AlreadyInGameRun");
				return;
			}
			if (saveData.Timing == SaveTiming.RunEnd)
			{
				GameMaster.ShowErrorDialogWithKey("CannotRestoreEndedGameRun");
				return;
			}
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoRestoreGameRun(saveData));
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00003EF9 File Offset: 0x000020F9
		private IEnumerator CoNewGameRun(GameRunController gameRun)
		{
			yield return this.CoSetupGameRun(gameRun);
			yield return this.CoEnterNextStage();
			GameMaster.Status = GameMaster.GameMasterStatus.InGame;
			GameMaster.ShowPoseAnimation = true;
			yield break;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00003F0F File Offset: 0x0000210F
		private IEnumerator CoRestoreGameRun(GameRunSaveData saveData)
		{
			if (saveData.Timing == SaveTiming.EnterMapNode && saveData.EnteringNode == null)
			{
				Debug.LogError("[GameMaster] corrupted game-run save data: timing = EnterMapNode, EnteringNode = null");
				GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
				yield break;
			}
			if (saveData.Timing == SaveTiming.BattleFinish && saveData.BattleStationEnemyGroup == null)
			{
				Debug.LogError("[GameMaster] corrupted game-run save data: timing = BattleFinish, BattleStationEnemyGroup = null");
				GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
				yield break;
			}
			if (saveData.Timing == SaveTiming.Adventure && saveData.AdventureState == null)
			{
				Debug.LogError("[GameMaster] corrupted game-run save data: timing = Adventure, AdventureState = null");
				GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
				yield break;
			}
			GameRunController gameRun;
			try
			{
				gameRun = GameRunController.Restore(saveData, true);
				this.SaveGameRun(saveData, false);
			}
			catch (Exception ex)
			{
				GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
				Debug.LogException(ex);
				yield break;
			}
			yield return this.CoSetupGameRun(gameRun);
			switch (saveData.Timing)
			{
			case SaveTiming.EnterMapNode:
			{
				MapNodeSaveData enteringNode = saveData.EnteringNode;
				MapNode mapNode = gameRun.CurrentMap.Nodes[enteringNode.X, enteringNode.Y];
				yield return this.CoEnterMapNode(mapNode, true, false);
				break;
			}
			case SaveTiming.BattleFinish:
			case SaveTiming.AfterBossReward:
			{
				BattleStation battleStation = (BattleStation)gameRun.CurrentStation;
				yield return this.CoEnterBattleStationFromFinishingSave(battleStation, saveData.Timing);
				break;
			}
			case SaveTiming.Adventure:
				yield return this.CoEnterAdventureFromSave(gameRun.CurrentStation, saveData.AdventureState);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			GameMaster.Status = GameMaster.GameMasterStatus.InGame;
			GameMaster.ShowPoseAnimation = true;
			GameMaster.PlatformHandler.SetGameRunInfo(gameRun);
			yield break;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00003F25 File Offset: 0x00002125
		private IEnumerator CoSetupGameRun(GameRunController gameRun)
		{
			this.CurrentGameRun = gameRun;
			this._currentGameRunStartTime = DateTime.Now;
			yield return UiManager.ShowLoading(0.5f).ToCoroutine(null);
			GameMaster.UnloadMainMenuUi();
			yield return GameMaster.CoLoadGameRunUi().ToCoroutine(null);
			yield return GameDirector.LoadPlayerAsync(gameRun.Player, false).ToCoroutine(null, null);
			gameRun.VisualTrigger = this;
			gameRun.AchievementHandler = this;
			gameRun.CardRevealed += delegate(Card card)
			{
				GameMaster.RevealCard(card.Id);
			};
			gameRun.ExhibitRevealed += delegate(Exhibit exhibit)
			{
				GameMaster.RevealExhibit(exhibit.Id);
			};
			gameRun.EnemyGroupRevealed += delegate(EnemyGroup enemyGroup)
			{
				GameMaster.RevealEnemyGroup(enemyGroup.Id);
			};
			UiManager.EnterGameRun(gameRun);
			yield break;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00003F3C File Offset: 0x0000213C
		public static void RequestEnterNextStage()
		{
			if (Singleton<GameMaster>.Instance.CurrentGameRun == null)
			{
				throw new InvalidOperationException("Not in game-run");
			}
			Button nextButton = UiManager.GetPanel<VnPanel>().nextButton;
			nextButton.interactable = false;
			nextButton.gameObject.SetActive(false);
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoEnterNextStage());
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00003F91 File Offset: 0x00002191
		private IEnumerator CoEnterNextStage()
		{
			GameRunController gameRun = this.CurrentGameRun;
			yield return this.CoLeaveStation(gameRun.CurrentStation);
			try
			{
				gameRun.EnterNextStage();
			}
			catch (Exception ex)
			{
				Debug.LogError("GameRun.EnterNextStage failed");
				Debug.LogException(ex);
				yield break;
			}
			if (gameRun.CurrentStage.EnterWithSpecialPresentation)
			{
				GameDirector.FadeOutPlayerStatus();
				yield return Environment.Instance.IntoFinalTask().ToCoroutine(null);
			}
			MapNode startNode = gameRun.CurrentMap.StartNode;
			yield return this.CoEnterMapNode(startNode, false, true);
			yield break;
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00003FA0 File Offset: 0x000021A0
		public static void RequestEnterMapNode(int x, int y)
		{
			GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
			if (currentGameRun == null)
			{
				throw new InvalidOperationException("Not in game-run");
			}
			MapNode mapNode = currentGameRun.CurrentMap.Nodes[x, y];
			if (mapNode.Status == MapNodeStatus.Active)
			{
				Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoEnterMapNode(mapNode, false, true));
				return;
			}
			if (mapNode.Status == MapNodeStatus.CrossActive)
			{
				Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoEnterMapNode(mapNode, false, true));
				return;
			}
			throw new InvalidOperationException(string.Format("Cannot enter map node ({0}, {1}) with status == {2}", x, y, mapNode.Status));
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00004044 File Offset: 0x00002244
		public static void TeleportToBossNode()
		{
			GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
			if (currentGameRun == null)
			{
				throw new InvalidOperationException("Not in game-run");
			}
			MapNode bossNode = currentGameRun.CurrentMap.BossNode;
			if (bossNode == null)
			{
				throw new InvalidOperationException("Current map has no boss-node");
			}
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoTeleportToNode(bossNode));
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00004098 File Offset: 0x00002298
		private IEnumerator CoTeleportToNode(MapNode mapNode)
		{
			yield return UiManager.ShowLoading(0.2f).ToCoroutine(null);
			MapPanel panel = UiManager.GetPanel<MapPanel>();
			MapNodeWidget mapNodeWidget = panel.GetMapNodeWidget(mapNode.X, mapNode.Y);
			panel.EnterNode(mapNodeWidget);
			yield return this.CoEnterMapNode(mapNode, false, true);
			yield break;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000040AE File Offset: 0x000022AE
		private IEnumerator CoEnterMapNode(MapNode mapNode, bool forced, bool save)
		{
			GameRunController gameRun = this.CurrentGameRun;
			yield return this.CoLeaveStation(gameRun.CurrentStation);
			GameRunSaveData gameRunSaveData = gameRun.EnterMapNode(mapNode, forced);
			if (save)
			{
				this.SaveGameRun(gameRunSaveData, true);
			}
			Station enteringStation = gameRun.CurrentStation;
			GameMaster.PlatformHandler.SetGameRunInfo(gameRun);
			yield return this.CoEnterStation(enteringStation);
			string text = null;
			BattleStation battleStation = gameRun.CurrentStation as BattleStation;
			if (battleStation != null)
			{
				text = battleStation.EnemyGroup.Id;
			}
			this.PlayMusic(mapNode.StationType, gameRun.CurrentStage.Level, text, gameRun.CurrentStage.EternalStageMusic);
			yield return this.RunInnerStation(enteringStation);
			yield break;
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x000040D4 File Offset: 0x000022D4
		private void PlayMusic(StationType stationType, int stageLevel, string enemyId, bool eternalStageMusic)
		{
			switch (stationType)
			{
			case StationType.None:
				AudioManager.EnterStage(stageLevel, true);
				return;
			case StationType.Enemy:
				AudioManager.EnterStage(stageLevel, true);
				return;
			case StationType.EliteEnemy:
				if (eternalStageMusic)
				{
					AudioManager.EnterStage(stageLevel, true);
					return;
				}
				AudioManager.EnterStage(stageLevel, false);
				AudioManager.PlayEliteBgm(enemyId);
				return;
			case StationType.Supply:
				AudioManager.EnterStage(stageLevel, false);
				return;
			case StationType.Gap:
				if (eternalStageMusic)
				{
					AudioManager.EnterStage(stageLevel, true);
					return;
				}
				AudioManager.EnterStage(stageLevel, false);
				AudioManager.PlayGapBgm();
				return;
			case StationType.Shop:
				if (eternalStageMusic)
				{
					AudioManager.EnterStage(stageLevel, true);
					return;
				}
				AudioManager.EnterStage(stageLevel, false);
				AudioManager.PlayShopBgm();
				return;
			case StationType.Adventure:
				AudioManager.EnterStage(stageLevel, false);
				return;
			case StationType.Entry:
				AudioManager.EnterStage(stageLevel, true);
				return;
			case StationType.Select:
				AudioManager.EnterStage(stageLevel, true);
				return;
			case StationType.Trade:
				AudioManager.EnterStage(stageLevel, false);
				return;
			case StationType.Boss:
				AudioManager.PlayBossBgm(enemyId);
				return;
			case StationType.BattleAdvTest:
				AudioManager.EnterStage(stageLevel, true);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x000041B4 File Offset: 0x000023B4
		private void PlayMusicFromSave(StationType stationType, int stageLevel)
		{
			if (stationType == StationType.Boss)
			{
				AudioManager.PlayAdventureBgm(2);
				return;
			}
			AudioManager.EnterStage(stageLevel, true);
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x000041C9 File Offset: 0x000023C9
		private IEnumerator CoLeaveStation(Station station)
		{
			if (station != null)
			{
				this.LeaveStation(station);
			}
			yield break;
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x000041DF File Offset: 0x000023DF
		private IEnumerator CoEnterStation(Station station)
		{
			yield return this.SwitchToStation(station);
			UiManager.GetPanel<SystemBoard>().EnterStation(station);
			if (UiManager.IsShowingLoading)
			{
				if (!this.CurrentProfile.OpeningPlayed)
				{
					VnPanel panel = UiManager.GetPanel<VnPanel>();
					panel.ResetComic();
					yield return panel.RunDialog("Opening", new DialogStorage(), new global::Yarn.Library(), null, null, null, new VnExtraSettings
					{
						HideLoadingBeforeStart = true
					});
					this.CurrentProfile.OpeningPlayed = true;
				}
				else
				{
					yield return UiManager.HideLoading(0.2f).ToCoroutine(null);
				}
			}
			MapPanel panel2 = UiManager.GetPanel<MapPanel>();
			if (panel2.IsVisible)
			{
				panel2.Hide();
				yield return new WaitForSeconds(0.2f);
			}
			if (station.Type == StationType.Boss)
			{
				ProfileSaveData currentProfileSaveData = Singleton<GameMaster>.Instance._currentProfileSaveData;
				if (currentProfileSaveData != null)
				{
					currentProfileSaveData.HasClearBonus = true;
				}
			}
			this.SaveProfile();
			yield break;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x000041F5 File Offset: 0x000023F5
		private IEnumerator CoEnterBattleStationFromFinishingSave(BattleStation battleStation, SaveTiming saveTiming)
		{
			yield return this.SwitchToStationFromSave(battleStation);
			UiManager.GetPanel<SystemBoard>().EnterStation(battleStation);
			if (UiManager.IsShowingLoading)
			{
				yield return UiManager.HideLoading(0.2f).ToCoroutine(null);
			}
			this.PlayMusicFromSave(battleStation.Type, battleStation.GameRun.CurrentStage.Level);
			yield return this.BattleStationFlowFromEndSave(battleStation, saveTiming);
			yield break;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00004212 File Offset: 0x00002412
		private IEnumerator CoEnterAdventureFromSave(Station station, AdventureSaveData saveData)
		{
			yield return this.SwitchToStationFromSave(station);
			UiManager.GetPanel<SystemBoard>().EnterStation(station);
			if (UiManager.IsShowingLoading)
			{
				yield return UiManager.HideLoading(0.2f).ToCoroutine(null);
			}
			this.PlayMusicFromSave(station.Type, station.GameRun.CurrentStage.Level);
			yield return GameMaster.AdventureRestoreFlow(station, saveData);
			if (station.GameRun.Player.IsDead)
			{
				GameMaster.EndGameProcedure(station.GameRun);
			}
			else if (station.IsNormalEnd && !station.GameRun.CanEnterTrueEnding())
			{
				GameMaster.EndGameProcedure(station.GameRun);
			}
			else if (station.IsTrueEnd)
			{
				GameMaster.EndGameProcedure(station.GameRun);
			}
			else
			{
				yield return this.EndStationFlow(station, false);
			}
			yield break;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00004230 File Offset: 0x00002430
		public static void GameRunFailCheck()
		{
			GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
			if (currentGameRun != null)
			{
				if (currentGameRun.Status == GameRunStatus.Failure)
				{
					GameMaster.EndGameProcedure(currentGameRun);
					return;
				}
			}
			else
			{
				Debug.LogError("GameRunFailCheck invoked without game-run");
			}
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00004268 File Offset: 0x00002468
		private static void EndGameProcedure(GameRunController gameRun)
		{
			Debug.Log(string.Format("[GameMaster] End game procedure, status = {0}", gameRun.Status));
			UiManager.Hide<SystemBoard>(true);
			UiManager.GetPanel<VnPanel>().Stop();
			GameRunRecordSaveData gameRunRecord = gameRun.GameRunRecord;
			gameRunRecord.TotalSeconds = Singleton<GameMaster>.Instance.CurrentGameRunPlayedSeconds;
			GameMaster.GameStatisticData gameStatisticData = GameMaster.EndGameStatistics(gameRun, gameRunRecord.ResultType);
			int exp = Singleton<GameMaster>.Instance._currentProfileSaveData.Exp;
			Singleton<GameMaster>.Instance.SaveProfileWithEndingGameRun(gameRun, gameStatisticData.BluePoint, gameRunRecord);
			UiManager.GetPanel<SettingPanel>().SetReenterStationInteractable(false);
			Singleton<GameMaster>.Instance.StartCoroutine(GameMaster.CoShowResultAndLeaveGameRun(new GameResultData
			{
				PlayerId = gameRun.Player.Id,
				Type = gameRunRecord.ResultType,
				ScoreDatas = gameStatisticData.ScoreDatas,
				PreviousTotalExp = exp,
				BluePoint = gameStatisticData.BluePoint,
				DifficultyMultipler = gameStatisticData.DifficultyMultipler,
				DebugExp = gameStatisticData.DebugExp
			}));
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004359 File Offset: 0x00002559
		private static IEnumerator CoShowResultAndLeaveGameRun(GameResultData data)
		{
			UiManager.GetPanel<UltimateSkillPanel>().Hide();
			GameResultPanel panel = UiManager.GetPanel<GameResultPanel>();
			panel.Show(data);
			yield return new WaitWhile(() => panel.IsVisible);
			GameMaster.LeaveGameRun();
			yield break;
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00004368 File Offset: 0x00002568
		// (set) Token: 0x060000AF RID: 175 RVA: 0x0000436F File Offset: 0x0000256F
		public static int? TempDebugExp { get; set; }

		// Token: 0x060000B0 RID: 176 RVA: 0x00004378 File Offset: 0x00002578
		private static GameMaster.GameStatisticData EndGameStatistics(GameRunController gameRun, GameResultType resultType)
		{
			GameMaster.<>c__DisplayClass60_0 CS$<>8__locals1;
			CS$<>8__locals1.bluePoint = 0;
			GameRunStats stats = gameRun.Stats;
			CS$<>8__locals1.scoreData = new List<ScoreData>();
			int num = Enumerable.Sum<StageStats>(stats.Stages, (StageStats s) => s.NormalEnemyDefeated);
			int num2 = Enumerable.Sum<StageStats>(stats.Stages, (StageStats s) => s.NormalEnemyBluePoint);
			if (num > 0 || num2 > 0)
			{
				CS$<>8__locals1.scoreData.Add(new ScoreData
				{
					Id = "NormalEnemy",
					Num = num,
					TotalBluePoint = num2
				});
				CS$<>8__locals1.bluePoint += num2;
			}
			foreach (ValueTuple<Stage, StageStats> valueTuple in gameRun.Stages.Zip(stats.Stages))
			{
				Stage item = valueTuple.Item1;
				StageStats item2 = valueTuple.Item2;
				if (item2.EliteEnemyDefeated > 0 || item2.EliteEnemyBluePoint > 0)
				{
					CS$<>8__locals1.scoreData.Add(new ScoreData
					{
						Id = "EliteEnemy",
						NameArguments = new object[] { item.Name },
						DescriptionArguments = new object[] { item.Name },
						Num = item2.EliteEnemyDefeated,
						TotalBluePoint = item2.EliteEnemyBluePoint
					});
					CS$<>8__locals1.bluePoint += item2.EliteEnemyBluePoint;
				}
			}
			int num3 = Enumerable.Sum<StageStats>(stats.Stages, (StageStats s) => s.BossDefeated);
			int num4 = Enumerable.Sum<StageStats>(stats.Stages, (StageStats s) => s.BossBluePoint);
			if (num3 > 0 || num4 > 0)
			{
				CS$<>8__locals1.scoreData.Add(new ScoreData
				{
					Id = "Boss",
					Num = num3,
					TotalBluePoint = num4
				});
				CS$<>8__locals1.bluePoint += num4;
			}
			int trivialColorCount = gameRun.BaseMana.TrivialColorCount;
			if (trivialColorCount >= 5)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Rainbow3", ref CS$<>8__locals1);
			}
			else if (trivialColorCount == 4)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Rainbow2", ref CS$<>8__locals1);
			}
			else if (trivialColorCount == 3)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Rainbow1", ref CS$<>8__locals1);
			}
			if (stats.PlayerSuicide)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Suicide", ref CS$<>8__locals1);
			}
			if (resultType == GameResultType.TrueEnd)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("TrueEnd", ref CS$<>8__locals1);
			}
			else if (resultType == GameResultType.NormalEnd)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("NormalEnd", ref CS$<>8__locals1);
			}
			if (stats.ContinuousTurnCount >= 5)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("MyTurn2", ref CS$<>8__locals1);
			}
			else if (stats.ContinuousTurnCount >= 3)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("MyTurn1", ref CS$<>8__locals1);
			}
			if (stats.PerfectElite > 0)
			{
				GameMaster.<EndGameStatistics>g__AddCountableStatData|60_1("PerfectElite", stats.PerfectElite, ref CS$<>8__locals1);
			}
			if (stats.Bosses.Count > 0)
			{
				int num5 = Enumerable.Count<Stage>(gameRun.Stages, (Stage s) => !s.IsTrueEndFinalStage);
				if (stats.PerfectBoss == num5)
				{
					GameMaster.<EndGameStatistics>g__AddStatData|60_0("PerfectBossAll", ref CS$<>8__locals1);
				}
				else if (stats.PerfectBoss > 0)
				{
					GameMaster.<EndGameStatistics>g__AddCountableStatData|60_1("PerfectBoss", stats.PerfectBoss, ref CS$<>8__locals1);
				}
			}
			int count = gameRun.ExhibitRecord.Count;
			if (count >= 30)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Robber2", ref CS$<>8__locals1);
			}
			else if (count >= 20)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Robber1", ref CS$<>8__locals1);
			}
			int count2 = gameRun.BaseDeck.Count;
			if (count2 >= 40)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Librarian2", ref CS$<>8__locals1);
			}
			else if (count2 >= 30)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Librarian1", ref CS$<>8__locals1);
			}
			if (Enumerable.All<Card>(gameRun.BaseDeck, (Card card) => card.Config.Rarity.CompareTo(Rarity.Rare) < 0))
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Pauper", ref CS$<>8__locals1);
			}
			if (Enumerable.Count<Card>(gameRun.BaseDeck, (Card card) => card.CardType == CardType.Misfortune) >= 3)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Misfortune", ref CS$<>8__locals1);
			}
			if (Enumerable.All<IGrouping<string, Card>>(Enumerable.GroupBy<Card, string>(gameRun.BaseDeck, (Card card) => card.Id), (IGrouping<string, Card> g) => Enumerable.Count<Card>(g) == 1))
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Universe", ref CS$<>8__locals1);
			}
			if (stats.MaxSingleAttackDamage >= 100)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("OneHit", ref CS$<>8__locals1);
			}
			if (stats.ShopConsumed >= 3000)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Rich3", ref CS$<>8__locals1);
			}
			else if (stats.ShopConsumed >= 2000)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Rich2", ref CS$<>8__locals1);
			}
			else if (stats.ShopConsumed >= 1000)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Rich1", ref CS$<>8__locals1);
			}
			if (stats.MaxHpGained >= 40)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Food3", ref CS$<>8__locals1);
			}
			else if (stats.MaxHpGained >= 30)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Food2", ref CS$<>8__locals1);
			}
			else if (stats.MaxHpGained >= 20)
			{
				GameMaster.<EndGameStatistics>g__AddStatData|60_0("Food1", ref CS$<>8__locals1);
			}
			if (resultType != GameResultType.Failure)
			{
				int num6 = Enumerable.Count<PuzzleFlag>(PuzzleFlags.EnumerateComponents(gameRun.Puzzles));
				if (num6 > 0)
				{
					GameMaster.<EndGameStatistics>g__AddCountableStatData|60_1("Puzzles", num6, ref CS$<>8__locals1);
				}
			}
			int? tempDebugExp = GameMaster.TempDebugExp;
			int num7;
			if (tempDebugExp != null)
			{
				int valueOrDefault = tempDebugExp.GetValueOrDefault();
				CS$<>8__locals1.bluePoint += valueOrDefault;
				num7 = valueOrDefault;
				GameMaster.TempDebugExp = default(int?);
			}
			else
			{
				num7 = 0;
			}
			float num8;
			switch (gameRun.Difficulty)
			{
			case GameDifficulty.Easy:
				num8 = 0.75f;
				break;
			case GameDifficulty.Normal:
				num8 = 1f;
				break;
			case GameDifficulty.Hard:
				num8 = 1.25f;
				break;
			case GameDifficulty.Lunatic:
				num8 = 1.5f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			float num9 = num8;
			CS$<>8__locals1.bluePoint = ((float)CS$<>8__locals1.bluePoint * num9).RoundToInt(1);
			return new GameMaster.GameStatisticData
			{
				BluePoint = CS$<>8__locals1.bluePoint,
				ScoreDatas = CS$<>8__locals1.scoreData,
				DifficultyMultipler = num9,
				DebugExp = num7
			};
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x000049E4 File Offset: 0x00002BE4
		public static void LeaveGameRun()
		{
			GameDirector.StopLoreChat();
			Singleton<GameMaster>.Instance.StopAllCoroutines();
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoLeaveGameRun());
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x00004A0A File Offset: 0x00002C0A
		private IEnumerator CoLeaveGameRun()
		{
			if (this.CurrentGameRun == null)
			{
				throw new InvalidOperationException("Not in game-run");
			}
			yield return UiManager.ShowLoading(0.5f).ToCoroutine(null);
			if (this.CurrentGameRun.Battle != null)
			{
				UiManager.LeaveBattle();
			}
			UiManager.LeaveGameRun();
			this.CurrentGameRun = null;
			Environment.Instance.ClearEnvironment();
			GameDirector.ClearAll();
			GameMaster.UnloadGameRunUi();
			GC.Collect();
			Resources.UnloadUnusedAssets();
			yield return GameMaster.LoadMainMenuUiAsync().ToCoroutine(null);
			this.ShowMainMenu();
			AudioManager.EnterMainMenu();
			GameMaster.Status = GameMaster.GameMasterStatus.MainMenu;
			yield return UiManager.HideLoading(0.5f).ToCoroutine(null);
			yield break;
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x00004A1C File Offset: 0x00002C1C
		public static void RequestReenterStation()
		{
			GameMaster instance = Singleton<GameMaster>.Instance;
			GameRunController currentGameRun = instance.CurrentGameRun;
			if (((currentGameRun != null) ? currentGameRun.CurrentStation : null) != null)
			{
				GameRunSaveData gameRunSaveData = instance.GameRunSaveData;
				if (gameRunSaveData != null)
				{
					SaveTiming timing = gameRunSaveData.Timing;
					if (timing != SaveTiming.EnterMapNode && timing != SaveTiming.BattleFinish && timing != SaveTiming.AfterBossReward && timing != SaveTiming.Adventure)
					{
						Debug.LogError(string.Format("Cannot restart with SaveData.Timing={0}", gameRunSaveData.Timing));
						return;
					}
					instance.StopAllCoroutines();
					instance.StartCoroutine(instance.CoReenterStation(gameRunSaveData));
					GameDirector.ClearDoremyEnvironment();
					return;
				}
			}
			Debug.LogError("Cannot reenter station while not in station");
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00004AA4 File Offset: 0x00002CA4
		public static void RequestSaveGameRunInAdventure(AdventureSaveData advSaveData)
		{
			GameMaster instance = Singleton<GameMaster>.Instance;
			GameRunController currentGameRun = instance.CurrentGameRun;
			if (currentGameRun == null)
			{
				Debug.LogError("Cannot save adventure while not in adventure");
				return;
			}
			IAdventureStation adventureStation = currentGameRun.CurrentStation as IAdventureStation;
			if (adventureStation == null || adventureStation.Adventure == null)
			{
				Debug.LogError("Cannot save adveture while not in IAdventureStation");
				return;
			}
			GameRunSaveData gameRunSaveData = currentGameRun.Save();
			gameRunSaveData.Timing = SaveTiming.Adventure;
			gameRunSaveData.EnteredStationType = new StationType?(currentGameRun.CurrentStation.Type);
			gameRunSaveData.AdventureState = advSaveData;
			instance.SaveGameRun(gameRunSaveData, true);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004B21 File Offset: 0x00002D21
		private IEnumerator CoReenterStation(GameRunSaveData saveData)
		{
			yield return UiManager.ShowLoading(0.5f).ToCoroutine(null);
			if (this.CurrentGameRun.Battle != null)
			{
				UiManager.LeaveBattle();
			}
			UiManager.LeaveGameRun();
			this.CurrentGameRun = null;
			GameDirector.ClearAll();
			GameMaster.UnloadGameRunUi();
			GC.Collect();
			Resources.UnloadUnusedAssets();
			Environment.Instance.ClearEnvironment();
			GameRunController gameRun = GameRunController.Restore(saveData, true);
			this.SaveGameRun(saveData, false);
			yield return this.CoSetupGameRun(gameRun);
			if (saveData.Timing == SaveTiming.EnterMapNode)
			{
				MapNodeSaveData enteringNode = saveData.EnteringNode;
				MapNode mapNode = gameRun.CurrentMap.Nodes[enteringNode.X, enteringNode.Y];
				yield return this.CoEnterMapNode(mapNode, true, false);
			}
			else
			{
				SaveTiming timing = saveData.Timing;
				if (timing == SaveTiming.BattleFinish || timing == SaveTiming.AfterBossReward)
				{
					BattleStation battleStation = (BattleStation)gameRun.CurrentStation;
					yield return this.CoEnterBattleStationFromFinishingSave(battleStation, saveData.Timing);
				}
				else if (saveData.Timing == SaveTiming.Adventure)
				{
					AdventureStation adventureStation = gameRun.CurrentStation as AdventureStation;
					if (adventureStation != null)
					{
						yield return this.CoEnterAdventureFromSave(adventureStation, saveData.AdventureState);
					}
					else
					{
						SupplyStation supplyStation = gameRun.CurrentStation as SupplyStation;
						if (supplyStation != null)
						{
							yield return this.CoEnterAdventureFromSave(supplyStation, saveData.AdventureState);
						}
						else
						{
							TradeStation tradeStation = gameRun.CurrentStation as TradeStation;
							if (tradeStation != null)
							{
								yield return this.CoEnterAdventureFromSave(tradeStation, saveData.AdventureState);
							}
						}
					}
				}
			}
			GameMaster.PlatformHandler.SetGameRunInfo(gameRun);
			yield return UiManager.HideLoading(0.5f).ToCoroutine(null);
			yield break;
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00004B38 File Offset: 0x00002D38
		private void ShowMainMenu()
		{
			if (this._currentSaveIndex == null)
			{
				UiManager.Show<EntryPanel>();
				UiManager.Show<MainMenuPanel>();
				UiManager.GetPanel<MainMenuPanel>().ChangeToLogo();
			}
			else
			{
				UiManager.Show<MainMenuPanel>();
				UiManager.GetPanel<MainMenuPanel>().ChangeToMain(3.5f);
			}
			GameMaster.PlatformHandler.SetMainMenuInfo(MainMenuStatus.Idle);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004B87 File Offset: 0x00002D87
		public static IEnumerator BattleFlow(EnemyGroup enemyGroup)
		{
			GameRunController gameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
			if (gameRun == null)
			{
				throw new InvalidOperationException("Not in game-run");
			}
			gameRun.EnterBattle(enemyGroup);
			BattleController battle = gameRun.Battle;
			UiManager.EnterBattle(battle);
			BattleHintPanel hintPanel = UiManager.GetPanel<BattleHintPanel>();
			if (!Singleton<GameMaster>.Instance.CurrentProfile.HintStatus.BattleHintShown)
			{
				hintPanel.Show();
				Singleton<GameMaster>.Instance.CurrentProfile.HintStatus.BattleHintShown = true;
				Singleton<GameMaster>.Instance.SaveProfile();
			}
			Singleton<GameDirector>.Instance.EnterBattle(battle);
			yield return gameRun.Battle.Flow();
			BattleStats battleStats = gameRun.LeaveBattle(enemyGroup);
			UiManager.LeaveBattle();
			Singleton<GameDirector>.Instance.LeaveBattle(battle);
			if (hintPanel.IsVisible)
			{
				hintPanel.Hide();
			}
			if (gameRun.Status != GameRunStatus.Failure)
			{
				if (enemyGroup.EnemyType == EnemyType.Boss)
				{
					Stage currentStage = gameRun.CurrentStage;
					if (!(currentStage is FinalStage) && (!(currentStage is WindGodLake) || gameRun.CanEnterTrueEnding()))
					{
						AudioManager.WinBoss();
					}
					if (gameRun.IsAutoSeed && gameRun.JadeBoxes.Empty<JadeBox>())
					{
						GameMaster.UnlockAchievement(enemyGroup.Id);
						if (battleStats.TotalRounds <= 1)
						{
							GameMaster.UnlockAchievement(AchievementKey.FirstTurn);
						}
						if (!battleStats.PlayerDamaged)
						{
							GameMaster.UnlockAchievement(AchievementKey.PerfectBoss);
						}
					}
				}
				else
				{
					AudioManager.WinBattle();
				}
				if (gameRun.IsAutoSeed && gameRun.JadeBoxes.Empty<JadeBox>())
				{
					if (gameRun.Player.Hp == 1)
					{
						GameMaster.UnlockAchievement(AchievementKey.OneHp);
					}
					if (battleStats.RemainCardCount <= 3)
					{
						GameMaster.UnlockAchievement(AchievementKey.LowCardsBattle);
					}
				}
			}
			yield break;
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004B96 File Offset: 0x00002D96
		private static IEnumerator AdventureFlow(Station station)
		{
			IAdventureStation adventureStation = station as IAdventureStation;
			Adventure adventure2;
			if (adventureStation == null)
			{
				EntryStation entryStation = station as EntryStation;
				if (entryStation == null)
				{
					BattleAdvTestStation battleAdvTestStation = station as BattleAdvTestStation;
					if (battleAdvTestStation == null)
					{
						throw new InvalidOperationException("[GameMaster] Cannot run adventure from " + station.GetType().Name);
					}
					adventure2 = battleAdvTestStation.Adventure;
				}
				else
				{
					adventure2 = entryStation.DebutAdventure;
				}
			}
			else
			{
				adventure2 = adventureStation.Adventure;
			}
			Adventure adventure = adventure2;
			DialogStorage dialogStorage = new DialogStorage();
			adventure.SetStorage(dialogStorage);
			RuntimeCommandHandler runtimeCommandHandler = RuntimeCommandHandler.Create(adventure);
			List<IAdventureHandler> handlers;
			if (GameMaster.ExtraAdventureHandlers.TryGetValue(adventure.GetType(), ref handlers))
			{
				foreach (IAdventureHandler adventureHandler in handlers)
				{
					runtimeCommandHandler = RuntimeCommandHandler.Merge(runtimeCommandHandler, RuntimeCommandHandler.Create(adventureHandler));
					adventureHandler.EnterAdventure(adventure);
				}
			}
			yield return UiManager.GetPanel<VnPanel>().RunDialog("Adventure/" + adventure.DialogName, dialogStorage, adventure.GameRun.DialogLibrary, runtimeCommandHandler, null, adventure, null);
			if (handlers != null)
			{
				foreach (IAdventureHandler adventureHandler2 in handlers)
				{
					adventureHandler2.LeaveAdventure(adventure);
				}
			}
			if (station.Type == StationType.Adventure && !(adventure is FakeAdventure))
			{
				Type type = adventure.GetType();
				station.Stage.AdventureHistory.Add(type);
				station.GameRun.AdventureHistory.Add(type);
			}
			station.Finish();
			yield break;
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00004BA5 File Offset: 0x00002DA5
		private static IEnumerator AdventureRestoreFlow(Station station, AdventureSaveData saveData)
		{
			IAdventureStation adventureStation = station as IAdventureStation;
			Adventure adventure2;
			if (adventureStation == null)
			{
				EntryStation entryStation = station as EntryStation;
				if (entryStation == null)
				{
					throw new InvalidOperationException("[GameMaster] Cannot restore adventure from " + station.GetType().Name);
				}
				adventure2 = entryStation.DebutAdventure;
			}
			else
			{
				adventure2 = adventureStation.Adventure;
			}
			Adventure adventure = adventure2;
			if (adventure == null)
			{
				station.Finish();
				yield break;
			}
			DialogStorage dialogStorage = DialogStorage.Restore(saveData.StorageYaml);
			adventure.RestoreStorage(dialogStorage);
			RuntimeCommandHandler runtimeCommandHandler = RuntimeCommandHandler.Create(adventure);
			List<IAdventureHandler> handlers;
			if (GameMaster.ExtraAdventureHandlers.TryGetValue(adventure.GetType(), ref handlers))
			{
				foreach (IAdventureHandler adventureHandler in handlers)
				{
					runtimeCommandHandler = RuntimeCommandHandler.Merge(runtimeCommandHandler, RuntimeCommandHandler.Create(adventureHandler));
					adventureHandler.EnterAdventure(adventure);
				}
			}
			yield return UiManager.GetPanel<VnPanel>().RestoreAdventure("Adventure/" + adventure.DialogName, adventure, saveData, dialogStorage, adventure.GameRun.DialogLibrary, runtimeCommandHandler, null);
			if (handlers != null)
			{
				foreach (IAdventureHandler adventureHandler2 in handlers)
				{
					adventureHandler2.LeaveAdventure(adventure);
				}
			}
			if (station.Type == StationType.Adventure && !(adventure is FakeAdventure))
			{
				Type type = adventure.GetType();
				station.Stage.AdventureHistory.Add(type);
				station.GameRun.AdventureHistory.Add(type);
			}
			station.Finish();
			yield break;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00004BBC File Offset: 0x00002DBC
		private static UniTask LoadSharedUiTo(IList<Type> ui)
		{
			GameMaster.<LoadSharedUiTo>d__70 <LoadSharedUiTo>d__;
			<LoadSharedUiTo>d__.<>t__builder = AsyncUniTaskMethodBuilder.Create();
			<LoadSharedUiTo>d__.ui = ui;
			<LoadSharedUiTo>d__.<>1__state = -1;
			<LoadSharedUiTo>d__.<>t__builder.Start<GameMaster.<LoadSharedUiTo>d__70>(ref <LoadSharedUiTo>d__);
			return <LoadSharedUiTo>d__.<>t__builder.Task;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00004C00 File Offset: 0x00002E00
		private static async UniTask LoadMainMenuUiAsync()
		{
			UiManager.ClearActionHandler();
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<MainMenuPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<EntryPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<StartGamePanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<MuseumPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<LicensesPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<ProfilePanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<ChangeLogPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<MusicRoomPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<ComplexRulesPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<HistoryPanel>(false);
			await GameMaster.<LoadMainMenuUiAsync>g__Load|72_0<CreditsPanel>(false);
			await GameMaster.LoadSharedUiTo(GameMaster.MainMenuUiList);
			UiManager.PushActionHandler(new GameMaster.MainMenuInputActionHandler());
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004C3C File Offset: 0x00002E3C
		private static void UnloadMainMenuUi()
		{
			foreach (Type type in GameMaster.MainMenuUiList)
			{
				UiManager.UnloadPanel(type);
			}
			GameMaster.MainMenuUiList.Clear();
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004C98 File Offset: 0x00002E98
		private static async UniTask CoLoadGameRunUi()
		{
			UiManager.ClearActionHandler();
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<UnitStatusHud>(true);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<UltimateSkillPanel>(true);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<RewardPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<SystemBoard>(true);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<BattleManaPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<PlayBoard>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<ExhibitInfoPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<ShopPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<GapOptionsPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<BackgroundPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<PopupHud>(true);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<NazrinDetectPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<BossExhibitPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<BattleNotifier>(true);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<ShowCardsPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<SelectCardPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<SelectBaseManaPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<GameRunVisualPanel>(true);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<MapPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<SelectDebugPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<BattleHintPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<HintPanel>(false);
			await GameMaster.<CoLoadGameRunUi>g__Load|75_0<DebugBattleLogPanel>(false);
			await GameMaster.LoadSharedUiTo(GameMaster.GameRunUiList);
			UiManager.PushActionHandler(new GameMaster.GameRunInputActionHandler());
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004CD4 File Offset: 0x00002ED4
		private static void UnloadGameRunUi()
		{
			foreach (Type type in GameMaster.GameRunUiList)
			{
				UiManager.UnloadPanel(type);
			}
			GameMaster.GameRunUiList.Clear();
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004D30 File Offset: 0x00002F30
		public static void RegisterExtraAdventureHandlers(Type adventureType, IAdventureHandler handlerInstance)
		{
			List<IAdventureHandler> list;
			if (!GameMaster.ExtraAdventureHandlers.TryGetValue(adventureType, ref list))
			{
				list = new List<IAdventureHandler>();
				GameMaster.ExtraAdventureHandlers.Add(adventureType, list);
			}
			list.Add(handlerInstance);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00004D65 File Offset: 0x00002F65
		public static void RegisterExtraAdventureHandlers<TAdventure>(IAdventureHandler handlerInstance) where TAdventure : Adventure
		{
			GameMaster.RegisterExtraAdventureHandlers(typeof(TAdventure), handlerInstance);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00004D78 File Offset: 0x00002F78
		public static void UnregisterExtraAdventureHandlers(Type adventureType, IAdventureHandler handlerInstance)
		{
			List<IAdventureHandler> list;
			if (GameMaster.ExtraAdventureHandlers.TryGetValue(adventureType, ref list))
			{
				list.Remove(handlerInstance);
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004D9C File Offset: 0x00002F9C
		public static void UnregisterExtraAdventureHandlers<TAdventure>(IAdventureHandler handlerInstance) where TAdventure : Adventure
		{
			GameMaster.UnregisterExtraAdventureHandlers(typeof(TAdventure), handlerInstance);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00004DB0 File Offset: 0x00002FB0
		public static bool OnWantsToQuit()
		{
			if (GameMaster._quitConfirmed)
			{
				return true;
			}
			UiDialog<MessageContent> dialog = UiManager.GetDialog<MessageDialog>();
			MessageContent messageContent = new MessageContent();
			messageContent.TextKey = "QuitGame";
			messageContent.Buttons = DialogButtons.ConfirmCancel;
			messageContent.OnConfirm = delegate
			{
				GameMaster._quitConfirmed = true;
				Application.Quit();
			};
			dialog.Show(messageContent);
			return false;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00004E0D File Offset: 0x0000300D
		public static void QuitGame()
		{
			GameMaster._quitConfirmed = true;
			Application.Quit();
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004E1A File Offset: 0x0000301A
		private static void SetTurboMode(bool turboMode)
		{
			Time.timeScale = (turboMode ? 2f : 1f);
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00004E30 File Offset: 0x00003030
		public static void DebugGainExhibit(Exhibit exhibit)
		{
			GameRunController currentGameRun = Singleton<GameMaster>.Instance.CurrentGameRun;
			if (currentGameRun != null)
			{
				Type type = exhibit.GetType();
				currentGameRun.ExhibitPool.Remove(type);
				currentGameRun.ShiningExhibitPool.Remove(type);
				Singleton<GameMaster>.Instance.StartCoroutine(currentGameRun.GainExhibitRunner(exhibit, true, new VisualSourceData
				{
					SourceType = VisualSourceType.Debug
				}));
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x00004E8B File Offset: 0x0000308B
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x00004E92 File Offset: 0x00003092
		public static bool ShowAllCardsInMuseum { get; set; }

		// Token: 0x060000C9 RID: 201 RVA: 0x00004E9C File Offset: 0x0000309C
		public static void InitializePlayerPrefs(Texture2D cursorTexture)
		{
			GameMaster.CursorTexture = cursorTexture;
			if (PlayerPrefs.HasKey("UseLbolCursor"))
			{
				GameMaster.SetCursor(PlayerPrefs.GetInt("UseLbolCursor") != 0);
			}
			else
			{
				GameMaster.SetCursor(true);
			}
			if (!PlayerPrefs.HasKey("IsAnimatingEnvironmentEnabled"))
			{
				PlayerPrefs.SetInt("IsAnimatingEnvironmentEnabled", 1);
			}
			GameMaster._isAnimatingEnvironmentEnabled = PlayerPrefs.GetInt("IsAnimatingEnvironmentEnabled") != 0;
			if (PlayerPrefs.HasKey("VsyncCount"))
			{
				QualitySettings.vSyncCount = PlayerPrefs.GetInt("VsyncCount");
			}
			if (PlayerPrefs.HasKey("TargetFrameRate"))
			{
				Application.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate");
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000CA RID: 202 RVA: 0x00004F34 File Offset: 0x00003134
		// (set) Token: 0x060000CB RID: 203 RVA: 0x00004F3B File Offset: 0x0000313B
		private static Texture2D CursorTexture { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000CC RID: 204 RVA: 0x00004F43 File Offset: 0x00003143
		// (set) Token: 0x060000CD RID: 205 RVA: 0x00004F4A File Offset: 0x0000314A
		public static bool UseLbolCursor { get; private set; }

		// Token: 0x060000CE RID: 206 RVA: 0x00004F52 File Offset: 0x00003152
		public static void SetCursor(bool isOn)
		{
			if (isOn)
			{
				Cursor.SetCursor(GameMaster.CursorTexture, GameMaster.CursorHotpot, CursorMode.Auto);
			}
			else
			{
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
			GameMaster.UseLbolCursor = isOn;
			PlayerPrefs.SetInt("UseLbolCursor", isOn ? 1 : 0);
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000CF RID: 207 RVA: 0x00004F8C File Offset: 0x0000318C
		// (set) Token: 0x060000D0 RID: 208 RVA: 0x00004F93 File Offset: 0x00003193
		public static bool IsAnimatingEnvironmentEnabled
		{
			get
			{
				return GameMaster._isAnimatingEnvironmentEnabled;
			}
			set
			{
				if (GameMaster._isAnimatingEnvironmentEnabled == value)
				{
					return;
				}
				GameMaster._isAnimatingEnvironmentEnabled = value;
				PlayerPrefs.SetInt("IsAnimatingEnvironmentEnabled", value ? 1 : 0);
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x00004FB5 File Offset: 0x000031B5
		// (set) Token: 0x060000D2 RID: 210 RVA: 0x00004FBC File Offset: 0x000031BC
		public static GameMaster.GameMasterStatus Status { get; set; }

		// Token: 0x060000D3 RID: 211 RVA: 0x00004FC4 File Offset: 0x000031C4
		public void OnGainMaxHp(int deltaMaxHp, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
			GameDirector.Player.OnMaxHpChanged();
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00004FDA File Offset: 0x000031DA
		public void OnLoseMaxHp(int deltaMaxHp, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
			GameDirector.Player.OnMaxHpChanged();
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00004FF0 File Offset: 0x000031F0
		public void OnSetHpAndMaxHp(int hp, int maxHp, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
			GameDirector.Player.OnMaxHpChanged();
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00005006 File Offset: 0x00003206
		public void OnEnemySetHpAndMaxHp(int index, int hp, int maxHp, bool triggerVisual)
		{
			GameDirector.GetEnemyByRootIndex(index).OnMaxHpChanged();
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00005013 File Offset: 0x00003213
		public void OnDamage(DamageInfo damage, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnHpChanged();
			GameDirector.Player.OnDamageReceived(damage);
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x0000502A File Offset: 0x0000322A
		private static UnitView PlayerView
		{
			get
			{
				return Singleton<GameDirector>.Instance.PlayerUnitView;
			}
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00005038 File Offset: 0x00003238
		public void OnHeal(int amount, bool triggerVisual, string audioName)
		{
			if (triggerVisual)
			{
				bool flag = amount > 12;
				EffectManager.CreateEffect(flag ? "UnitHealLarge" : "UnitHeal", GameMaster.PlayerView.EffectRoot, true);
				if (audioName.IsNullOrEmpty())
				{
					AudioManager.PlayUi(flag ? "HealLarge" : "Heal", false);
				}
				else
				{
					AudioManager.PlayUi(audioName, false);
				}
				PopupHud.Instance.HealPopupFromScene(amount, GameMaster.PlayerView.transform.position);
			}
			UiManager.GetPanel<SystemBoard>().OnHpChanged();
			GameDirector.Player.OnHealingReceived(amount);
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000050C4 File Offset: 0x000032C4
		public void OnGainMoney(int value, bool triggerVisual, [MaybeNull] VisualSourceData sourceData)
		{
			VisualSourceType? visualSourceType = ((sourceData != null) ? new VisualSourceType?(sourceData.SourceType) : default(VisualSourceType?));
			bool flag;
			if (visualSourceType != null)
			{
				VisualSourceType valueOrDefault = visualSourceType.GetValueOrDefault();
				if (valueOrDefault - VisualSourceType.Vn <= 1)
				{
					flag = true;
					goto IL_0037;
				}
			}
			flag = false;
			IL_0037:
			if (flag)
			{
				UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(CameraController.ScenePositionToWorldPositionInUI(Vector3.zero), value, UiManager.GetPanel<GameRunVisualPanel>().transform, 1f);
			}
			if (!(((sourceData != null) ? new VisualSourceType?(sourceData.SourceType) : default(VisualSourceType?)) != VisualSourceType.Entity))
			{
				Vector3? vector = UiManager.GetPanel<PlayBoard>().FindActionSourceWorldPosition(sourceData.Source);
				if (vector != null)
				{
					Vector3 valueOrDefault2 = vector.GetValueOrDefault();
					UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(valueOrDefault2, value, UiManager.GetPanel<GameRunVisualPanel>().transform, 1f);
				}
				else
				{
					UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(CameraController.ScenePositionToWorldPositionInUI(Vector3.zero), value, UiManager.GetPanel<GameRunVisualPanel>().transform, 1f);
				}
			}
			if (!(((sourceData != null) ? new VisualSourceType?(sourceData.SourceType) : default(VisualSourceType?)) != VisualSourceType.AbandonReward))
			{
				UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(UiManager.GetPanel<RewardPanel>().AbandonPos, value, UiManager.GetPanel<GameRunVisualPanel>().transform, 1f);
			}
			UiManager.GetPanel<SystemBoard>().OnMoneyChanged();
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000521D File Offset: 0x0000341D
		public void OnConsumeMoney(int value)
		{
			UiManager.GetPanel<SystemBoard>().OnMoneyChanged();
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00005229 File Offset: 0x00003429
		public void OnLoseMoney(int value)
		{
			UiManager.GetPanel<SystemBoard>().OnMoneyChanged();
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00005235 File Offset: 0x00003435
		public void OnGainPower(int value, bool triggerVisual)
		{
			UiManager.GetPanel<UltimateSkillPanel>().GainPower(value);
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00005242 File Offset: 0x00003442
		public void OnConsumePower(int value, bool triggerVisual)
		{
			UiManager.GetPanel<UltimateSkillPanel>().ConsumePower(value);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000524F File Offset: 0x0000344F
		public void OnLosePower(int value, bool triggerVisual)
		{
			UiManager.GetPanel<UltimateSkillPanel>().LosePower(value);
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000525C File Offset: 0x0000345C
		public void OnAddDeckCards(Card[] cards, bool triggerVisual, [MaybeNull] VisualSourceData sourceData)
		{
			UiManager.GetPanel<SystemBoard>().OnDeckChanged();
			if (sourceData == null)
			{
				UiManager.GetPanel<GameRunVisualPanel>().PlayAddToDeckEffect(cards, null, 0.5f + 0.2f * (float)cards.Length);
				return;
			}
			if (sourceData.SourceType == VisualSourceType.Reward)
			{
				UiManager.GetPanel<GameRunVisualPanel>().PlayAddToDeckEffect(cards, new CardWidget[] { UiManager.GetPanel<RewardPanel>().ExtractBufferedCard() }, 0f);
				return;
			}
			if (sourceData.SourceType == VisualSourceType.Shop)
			{
				UiManager.GetPanel<GameRunVisualPanel>().PlayAddToDeckEffect(cards, new CardWidget[] { UiManager.GetPanel<ShopPanel>().ExtractBufferedCard() }, 0f);
				return;
			}
			if (sourceData.SourceType == VisualSourceType.CardSelect)
			{
				UiManager.GetPanel<GameRunVisualPanel>().PlayAddToDeckEffect(cards, new CardWidget[] { UiManager.GetPanel<SelectCardPanel>().ExtractBufferedCard() }, 0f);
				return;
			}
			UiManager.GetPanel<GameRunVisualPanel>().PlayAddToDeckEffect(cards, null, 0.5f + 0.2f * (float)cards.Length);
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00005338 File Offset: 0x00003538
		public void OnRemoveDeckCards(Card[] cards, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnDeckChanged();
			ShowCardsPanel panel = UiManager.GetPanel<ShowCardsPanel>();
			if (panel.IsVisible)
			{
				panel.RemoveCardsIfContains(cards);
			}
			if (triggerVisual)
			{
				UiManager.GetPanel<GameRunVisualPanel>().ViewRemoveDeckCards(cards);
			}
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00005372 File Offset: 0x00003572
		public void OnUpgradeDeckCards(Card[] cards, bool triggerVisual)
		{
			UiManager.GetPanel<GameRunVisualPanel>().PlayUpgradeDeckCardsEffect(cards, 1f + (float)cards.Length * 0.3f);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x0000538F File Offset: 0x0000358F
		public IEnumerator OnGainExhibit(Exhibit exhibit, bool triggerVisual, [MaybeNull] VisualSourceData sourceData)
		{
			SystemBoard panel = UiManager.GetPanel<SystemBoard>();
			if (triggerVisual && sourceData != null)
			{
				Vector3 vector;
				switch (sourceData.SourceType)
				{
				case VisualSourceType.Reward:
					vector = UiManager.GetPanel<RewardPanel>().OnGainExhibitReward(exhibit);
					break;
				case VisualSourceType.BossReward:
					vector = UiManager.GetPanel<BossExhibitPanel>().GetRewardWorldPosition(sourceData.Index);
					break;
				case VisualSourceType.Shop:
					vector = UiManager.GetPanel<ShopPanel>().GetExhibitPosition(sourceData.Index);
					break;
				case VisualSourceType.Vn:
					vector = UiManager.GetPanel<VnPanel>().GetOptionWorldPosition(sourceData.Index);
					break;
				case VisualSourceType.Gap:
					vector = UiManager.GetPanel<GapOptionsPanel>().GetOptionPosition(sourceData.Index);
					break;
				case VisualSourceType.Debug:
					vector = CameraController.ScenePositionToWorldPositionInUI(Vector3.zero);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				Vector3 vector2 = vector;
				panel.CreateExhibitGainVisual(exhibit, vector2, 0.6f);
				panel.OnExhibitAdded(exhibit, 0.6f);
				if (GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("Exhibit"))
				{
					GameMaster.MarkHintAsShown("Exhibit");
					yield return UiManager.GetPanel<HintPanel>().ShowAsync(new HintPayload
					{
						HintKey = "Exhibit",
						Target = panel.GetRectForHint(),
						Delay = 0.8f
					});
				}
			}
			else
			{
				panel.OnExhibitAdded(exhibit, 0f);
			}
			yield break;
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x000053AC File Offset: 0x000035AC
		public void OnLoseExhibit(Exhibit exhibit, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnExhibitRemoved(exhibit);
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x000053B9 File Offset: 0x000035B9
		public void OnSetBaseMana(ManaGroup mana, bool triggerVisual)
		{
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x000053BB File Offset: 0x000035BB
		public void OnGainBaseMana(ManaGroup mana, bool triggerVisual)
		{
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x000053BD File Offset: 0x000035BD
		public void OnLoseBaseMana(ManaGroup mana, bool triggerVisual)
		{
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x000053BF File Offset: 0x000035BF
		public void CustomVisual(string visualName, bool triggerVisual)
		{
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000E9 RID: 233 RVA: 0x000053C1 File Offset: 0x000035C1
		public static string SysFileName
		{
			get
			{
				return "sys.sav";
			}
		}

		// Token: 0x060000EA RID: 234 RVA: 0x000053C8 File Offset: 0x000035C8
		private static string GetProfileFileName(int index)
		{
			return string.Format("save{0}.sav", index);
		}

		// Token: 0x060000EB RID: 235 RVA: 0x000053DA File Offset: 0x000035DA
		private static string GetGameRunFileName(int index)
		{
			return string.Format("game{0}.sav", index);
		}

		// Token: 0x060000EC RID: 236 RVA: 0x000053EC File Offset: 0x000035EC
		private static string GetHistoryFileName(int index)
		{
			return string.Format("history{0}.sav", index);
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000ED RID: 237 RVA: 0x000053FE File Offset: 0x000035FE
		public int? CurrentSaveIndex
		{
			get
			{
				return this._currentSaveIndex;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000EE RID: 238 RVA: 0x00005406 File Offset: 0x00003606
		public ProfileSaveData CurrentProfile
		{
			get
			{
				return this._currentProfileSaveData;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000EF RID: 239 RVA: 0x0000540E File Offset: 0x0000360E
		public int CurrentProfileLevel
		{
			get
			{
				if (this._currentProfileSaveData == null)
				{
					return 0;
				}
				return ExpHelper.GetLevelForTotalExp(this._currentProfileSaveData.Exp);
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000F0 RID: 240 RVA: 0x0000542A File Offset: 0x0000362A
		// (set) Token: 0x060000F1 RID: 241 RVA: 0x00005432 File Offset: 0x00003632
		public GameRunSaveData GameRunSaveData { get; private set; }

		// Token: 0x060000F2 RID: 242 RVA: 0x0000543C File Offset: 0x0000363C
		public static void DebugAddExp(int exp)
		{
			ProfileSaveData currentProfileSaveData = Singleton<GameMaster>.Instance._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				Debug.LogError("No profile data");
				return;
			}
			currentProfileSaveData.Exp += Math.Min(currentProfileSaveData.Exp + exp, ExpHelper.MaxExp);
			currentProfileSaveData.BluePoint += exp;
			Singleton<GameMaster>.Instance.SaveProfile();
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x0000549C File Offset: 0x0000369C
		public static void SaveSys()
		{
			SysSaveData sysSaveData = new SysSaveData
			{
				SaveIndex = Singleton<GameMaster>.Instance._currentSaveIndex,
				Locale = Localization.CurrentLocale.ToString()
			};
			GameMaster.WriteSaveData(GameMaster.SysFileName, SaveDataHelper.SerializeSys(sysSaveData, true));
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x000054EC File Offset: 0x000036EC
		public static ProfileSaveData CreateAndSelectProfile(int index, string name)
		{
			ProfileSaveData profileSaveData = new ProfileSaveData
			{
				Name = name,
				CreationTimestamp = Utils.ToIso8601Timestamp(DateTime.UtcNow),
				GameVersion = VersionInfo.Current.Version,
				GameRevision = VersionInfo.Current.Revision,
				Settings = 
				{
					PreferWideTooltips = L10nManager.Info.PreferWideTooltip
				}
			};
			GameMaster.TryDeleteSaveData(GameMaster.GetGameRunFileName(index), false);
			GameMaster.TryDeleteSaveData(GameMaster.GetHistoryFileName(index), false);
			GameMaster.WriteSaveData(GameMaster.GetProfileFileName(index), SaveDataHelper.SerializeProfile(profileSaveData, true));
			GameMaster.SelectProfile(new int?(index));
			UiManager.GetPanel<MuseumPanel>().Initialization();
			return profileSaveData;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00005590 File Offset: 0x00003790
		public static ProfileSaveData SetProfileName(int index, string name)
		{
			int? currentSaveIndex = Singleton<GameMaster>.Instance._currentSaveIndex;
			ProfileSaveData profileSaveData;
			if ((index == currentSaveIndex.GetValueOrDefault()) & (currentSaveIndex != null))
			{
				Singleton<GameMaster>.Instance._currentProfileSaveData.Name = name;
				GameMaster.WriteSaveData(GameMaster.GetProfileFileName(index), SaveDataHelper.SerializeProfile(Singleton<GameMaster>.Instance._currentProfileSaveData, true));
			}
			else if (GameMaster.TryLoadProfileSaveData(index, out profileSaveData))
			{
				if (profileSaveData != null)
				{
					profileSaveData.Name = name;
					GameMaster.WriteSaveData(GameMaster.GetProfileFileName(index), SaveDataHelper.SerializeProfile(profileSaveData, true));
					return profileSaveData;
				}
				Debug.LogWarning(string.Format("SelectProfile({0}): profile not found", index));
			}
			return null;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000562C File Offset: 0x0000382C
		public static void SelectProfile(int? index)
		{
			if (index != null)
			{
				int valueOrDefault = index.GetValueOrDefault();
				ProfileSaveData profileSaveData;
				if (GameMaster.TryLoadProfileSaveData(valueOrDefault, out profileSaveData))
				{
					if (profileSaveData != null)
					{
						Singleton<GameMaster>.Instance._currentSaveIndex = index;
						Singleton<GameMaster>.Instance._currentProfileSaveData = profileSaveData;
						GameRunSaveData gameRunSaveData;
						GameMaster.GameRunSaveDataLoadResult gameRunSaveDataLoadResult = GameMaster.TryLoadGameRunSaveData(valueOrDefault, out gameRunSaveData);
						if (gameRunSaveDataLoadResult == GameMaster.GameRunSaveDataLoadResult.Success)
						{
							Singleton<GameMaster>.Instance.GameRunSaveData = gameRunSaveData;
						}
						else if (gameRunSaveDataLoadResult == GameMaster.GameRunSaveDataLoadResult.NotFound)
						{
							Singleton<GameMaster>.Instance.GameRunSaveData = null;
						}
						else if (gameRunSaveDataLoadResult == GameMaster.GameRunSaveDataLoadResult.VersionMismatch)
						{
							Singleton<GameMaster>.Instance.GameRunSaveData = null;
							GameMaster.TryDeleteSaveData(GameMaster.GetGameRunFileName(valueOrDefault), true);
							profileSaveData.HasClearBonus = true;
							Singleton<GameMaster>.Instance.SaveProfile();
							GameMaster.ShowWarningDialogWithKey("GameRunSaveDiscarded", "GameRunSaveDiscardedSubText");
						}
						else if (gameRunSaveDataLoadResult == GameMaster.GameRunSaveDataLoadResult.Failed)
						{
							Singleton<GameMaster>.Instance.GameRunSaveData = null;
							GameMaster.TryDeleteSaveData(GameMaster.GetGameRunFileName(valueOrDefault), true);
							GameMaster.ShowErrorDialogWithKey("CorruptedGameRunSaveData");
						}
						foreach (CharacterStatsSaveData characterStatsSaveData in profileSaveData.CharacterStats)
						{
							GameDifficulty? highestPerfectSuccessDifficulty = characterStatsSaveData.HighestPerfectSuccessDifficulty;
							GameDifficulty gameDifficulty = GameDifficulty.Lunatic;
							if ((highestPerfectSuccessDifficulty.GetValueOrDefault() == gameDifficulty) & (highestPerfectSuccessDifficulty != null))
							{
								GameMaster.UnlockAchievement("Normal" + characterStatsSaveData.CharacterId);
								GameMaster.UnlockAchievement("True" + characterStatsSaveData.CharacterId);
								GameMaster.UnlockAchievement("Lunatic" + characterStatsSaveData.CharacterId);
							}
							else if (characterStatsSaveData.HighestPerfectSuccessDifficulty != null)
							{
								GameMaster.UnlockAchievement("Normal" + characterStatsSaveData.CharacterId);
								GameMaster.UnlockAchievement("True" + characterStatsSaveData.CharacterId);
							}
							else if (characterStatsSaveData.HighestSuccessDifficulty != null)
							{
								GameMaster.UnlockAchievement("Normal" + characterStatsSaveData.CharacterId);
							}
						}
					}
					else
					{
						Debug.LogWarning(string.Format("SelectProfile({0}): profile not found", index));
						Singleton<GameMaster>.Instance._currentSaveIndex = default(int?);
						Singleton<GameMaster>.Instance._currentProfileSaveData = null;
					}
				}
			}
			else
			{
				Singleton<GameMaster>.Instance._currentSaveIndex = default(int?);
				Singleton<GameMaster>.Instance._currentProfileSaveData = null;
			}
			GameMaster.SaveSys();
			UiManager.GetPanel<MainMenuPanel>().RefreshProfile();
			ProfileSaveData currentProfileSaveData = Singleton<GameMaster>.Instance._currentProfileSaveData;
			if (currentProfileSaveData != null)
			{
				GameMaster.SetTurboMode(currentProfileSaveData.Settings.IsTurboMode);
				UiManager.LoadKeyboardBindings(currentProfileSaveData.Settings.KeyboardBindings);
				return;
			}
			GameMaster.SetTurboMode(false);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00005888 File Offset: 0x00003A88
		public static void DeleteProfile(int index)
		{
			GameMaster.DeleteSaveData(GameMaster.GetProfileFileName(index));
			GameMaster.TryDeleteSaveData(GameMaster.GetGameRunFileName(index), false);
			GameMaster.TryDeleteSaveData(GameMaster.GetHistoryFileName(index), false);
			int? currentSaveIndex = Singleton<GameMaster>.Instance._currentSaveIndex;
			if ((index == currentSaveIndex.GetValueOrDefault()) & (currentSaveIndex != null))
			{
				Singleton<GameMaster>.Instance._currentProfileSaveData = null;
				Singleton<GameMaster>.Instance._currentSaveIndex = default(int?);
				Singleton<GameMaster>.Instance.GameRunSaveData = null;
			}
			UiManager.GetPanel<MainMenuPanel>().RefreshProfile();
			GameMaster.SaveSys();
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00005910 File Offset: 0x00003B10
		private static void WriteSaveData(string filename, byte[] data)
		{
			try
			{
				string saveDataFolder = GameMaster.PlatformHandler.GetSaveDataFolder();
				if (!Directory.Exists(saveDataFolder))
				{
					Directory.CreateDirectory(saveDataFolder);
				}
				File.WriteAllBytes(Path.Combine(saveDataFolder, filename), data);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to save " + filename);
				Debug.LogException(ex);
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00005970 File Offset: 0x00003B70
		private static void DeleteSaveData(string filename)
		{
			try
			{
				File.Delete(Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), filename));
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to save " + filename);
				Debug.LogException(ex);
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x000059BC File Offset: 0x00003BBC
		private static bool TryDeleteSaveData(string filename, bool backup)
		{
			bool flag;
			try
			{
				string text = Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), filename);
				if (File.Exists(text))
				{
					if (backup)
					{
						File.Copy(text, text + "_backup", true);
					}
					File.Delete(text);
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00005A1C File Offset: 0x00003C1C
		private void AppendGameRunHistory(GameRunRecordSaveData record, int bluePoint)
		{
			int? currentSaveIndex = this.CurrentSaveIndex;
			if (currentSaveIndex != null)
			{
				int valueOrDefault = currentSaveIndex.GetValueOrDefault();
				string text = Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), GameMaster.GetHistoryFileName(valueOrDefault));
				List<GameRunRecordSaveData> list;
				try
				{
					list = (File.Exists(text) ? SaveDataHelper.DeserializeGameRunHistory(File.ReadAllBytes(text)) : new List<GameRunRecordSaveData>());
				}
				catch (Exception ex)
				{
					Debug.LogError("[GameMaster] Failed to load game-run history");
					Debug.LogException(ex);
					list = new List<GameRunRecordSaveData>();
				}
				record.SaveTimestamp = Utils.ToIso8601Timestamp(DateTime.Now);
				record.BluePoint = bluePoint;
				list.Add(record);
				if (list.Count > 50)
				{
					list.RemoveRange(0, list.Count - 50);
				}
				try
				{
					File.WriteAllBytes(text, SaveDataHelper.SerializeGameRunHistory(list, true));
				}
				catch (Exception ex2)
				{
					Debug.LogError("[GameMaster] Failed to save game-run history");
					Debug.LogException(ex2);
				}
				return;
			}
			Debug.LogError("[GameMaster] Cannot save game-run history while profile not selected.");
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00005B10 File Offset: 0x00003D10
		private void SaveProfile()
		{
			if (this.CurrentSaveIndex == null)
			{
				Debug.LogError("SaveIndex is not set, profile not saved");
				return;
			}
			ProfileSaveData currentProfileSaveData = this._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				Debug.LogError("CurrentProfileSaveData is null, profile not saved");
				return;
			}
			string text = Utils.ToIso8601Timestamp(DateTime.UtcNow);
			currentProfileSaveData.SaveTimestamp = text;
			currentProfileSaveData.GameVersion = VersionInfo.Current.Version;
			currentProfileSaveData.GameRevision = VersionInfo.Current.Revision;
			GameMaster.WriteSaveData(GameMaster.GetProfileFileName(this.CurrentSaveIndex.Value), SaveDataHelper.SerializeProfile(currentProfileSaveData, true));
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00005BA0 File Offset: 0x00003DA0
		private void SaveProfileWithEndingGameRun(GameRunController gameRun, int bluePoint, GameRunRecordSaveData gameRunRecord)
		{
			if (this.CurrentSaveIndex == null)
			{
				Debug.LogError("SaveIndex is not set, profile not saved");
				return;
			}
			ProfileSaveData currentProfileSaveData = this._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				Debug.LogError("CurrentProfileSaveData is null, profile not saved");
				return;
			}
			if (gameRun.Status == GameRunStatus.Running)
			{
				Debug.LogError("Cannot save profile with running game-run");
				return;
			}
			string characterId = gameRun.Player.Id;
			CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(currentProfileSaveData.CharacterStats, (CharacterStatsSaveData s) => s.CharacterId == characterId);
			if (characterStatsSaveData == null)
			{
				characterStatsSaveData = new CharacterStatsSaveData
				{
					CharacterId = characterId
				};
				currentProfileSaveData.CharacterStats = Enumerable.ToArray<CharacterStatsSaveData>(Enumerable.OrderBy<CharacterStatsSaveData, string>(Enumerable.Concat<CharacterStatsSaveData>(currentProfileSaveData.CharacterStats, new CharacterStatsSaveData[] { characterStatsSaveData }), (CharacterStatsSaveData s) => s.CharacterId));
			}
			characterStatsSaveData.TotalPlaySeconds += gameRunRecord.TotalSeconds;
			characterStatsSaveData.TotalBluePoint += bluePoint;
			if (gameRun.IsAutoSeed && gameRunRecord.JadeBoxes.Empty<string>())
			{
				if (gameRunRecord.ResultType == GameResultType.Failure)
				{
					characterStatsSaveData.FailCount++;
				}
				else
				{
					GameResultType resultType = gameRunRecord.ResultType;
					if (resultType == GameResultType.NormalEnd || resultType == GameResultType.TrueEndFail)
					{
						characterStatsSaveData.SuccessCount++;
						characterStatsSaveData.PuzzleCount += PuzzleFlags.GetPuzzleLevel(gameRunRecord.Puzzles);
						GameDifficulty? gameDifficulty = characterStatsSaveData.HighestSuccessDifficulty;
						if (gameDifficulty != null)
						{
							GameDifficulty valueOrDefault = gameDifficulty.GetValueOrDefault();
							if (!gameRun.Difficulty.IsHigherThan(valueOrDefault))
							{
								goto IL_0200;
							}
						}
						characterStatsSaveData.HighestSuccessDifficulty = new GameDifficulty?(gameRun.Difficulty);
					}
					else if (gameRunRecord.ResultType == GameResultType.TrueEnd)
					{
						characterStatsSaveData.PerfectSuccessCount++;
						characterStatsSaveData.PuzzleCount += PuzzleFlags.GetPuzzleLevel(gameRunRecord.Puzzles);
						GameDifficulty? gameDifficulty = characterStatsSaveData.HighestPerfectSuccessDifficulty;
						if (gameDifficulty != null)
						{
							GameDifficulty valueOrDefault2 = gameDifficulty.GetValueOrDefault();
							if (!gameRun.Difficulty.IsHigherThan(valueOrDefault2))
							{
								goto IL_0200;
							}
						}
						characterStatsSaveData.HighestPerfectSuccessDifficulty = new GameDifficulty?(gameRun.Difficulty);
					}
				}
				IL_0200:
				string id = gameRun.Player.Id;
				if (gameRunRecord.ResultType != GameResultType.Failure)
				{
					GameMaster.UnlockAchievement("Normal" + id);
					if (gameRunRecord.Puzzles != PuzzleFlag.None)
					{
						GameMaster.UnlockAchievement(AchievementKey.PuzzleAny);
					}
					if (PuzzleFlags.IsAll(gameRunRecord.Puzzles))
					{
						GameMaster.UnlockAchievement(AchievementKey.Puzzle);
					}
					if (gameRunRecord.ResultType == GameResultType.TrueEnd)
					{
						GameMaster.UnlockAchievement("True" + id);
						if (PuzzleFlags.IsAll(gameRunRecord.Puzzles))
						{
							GameMaster.UnlockAchievement(AchievementKey.PuzzleTrue);
						}
						if (gameRunRecord.Difficulty == GameDifficulty.Lunatic)
						{
							GameMaster.UnlockAchievement("Lunatic" + id);
							if (PuzzleFlags.IsAll(gameRunRecord.Puzzles))
							{
								GameMaster.UnlockAchievement(AchievementKey.PuzzleLunatic);
							}
						}
						if (gameRunRecord.TotalSeconds <= 3600)
						{
							GameMaster.UnlockAchievement(AchievementKey.SpeedRun);
						}
					}
					ManaGroup mana = gameRun.BaseMana;
					if (mana.White >= 5)
					{
						GameMaster.UnlockAchievement(AchievementKey.White);
					}
					if (mana.Blue >= 5)
					{
						GameMaster.UnlockAchievement(AchievementKey.Blue);
					}
					if (mana.Black >= 5)
					{
						GameMaster.UnlockAchievement(AchievementKey.Black);
					}
					if (mana.Red >= 5)
					{
						GameMaster.UnlockAchievement(AchievementKey.Red);
					}
					if (mana.Green >= 5)
					{
						GameMaster.UnlockAchievement(AchievementKey.Green);
					}
					if (Enumerable.All<ManaColor>(ManaColors.WUBRG, (ManaColor c) => mana[c] > 0))
					{
						GameMaster.UnlockAchievement(AchievementKey.FiveColor);
					}
					if (gameRun.Stats.ShopConsumed == 0)
					{
						GameMaster.UnlockAchievement(AchievementKey.UseNoMoney);
					}
					if (gameRun.BaseDeck.Count <= 8)
					{
						GameMaster.UnlockAchievement(AchievementKey.LowCardsGameRun);
					}
					if (gameRun.Stats.NoExhibitFlag)
					{
						GameMaster.UnlockAchievement(AchievementKey.NoExhibits);
					}
				}
			}
			if (gameRun.JadeBoxes.NotEmpty<JadeBox>() && gameRunRecord.ResultType != GameResultType.Failure)
			{
				GameMaster.UnlockAchievement(AchievementKey.JadeBox);
			}
			currentProfileSaveData.Exp = Math.Min(currentProfileSaveData.Exp + bluePoint, ExpHelper.MaxExp);
			SortedSet<string> sortedSet = new SortedSet<string>(currentProfileSaveData.CardsRevealed);
			foreach (string text in gameRun.Stats.CardsRevealed)
			{
				if (CardConfig.FromId(text) != null)
				{
					sortedSet.Add(text);
				}
			}
			currentProfileSaveData.CardsRevealed = Enumerable.ToList<string>(sortedSet);
			SortedSet<string> sortedSet2 = new SortedSet<string>(currentProfileSaveData.ExhibitsRevealed);
			foreach (string text2 in gameRun.Stats.ExhibitsRevealed)
			{
				if (ExhibitConfig.FromId(text2) != null)
				{
					sortedSet2.Add(text2);
				}
			}
			currentProfileSaveData.ExhibitsRevealed = Enumerable.ToList<string>(sortedSet2);
			GameMaster.WriteSaveData(GameMaster.GetProfileFileName(this.CurrentSaveIndex.Value), SaveDataHelper.SerializeProfile(currentProfileSaveData, true));
			GameMaster.TryDeleteSaveData(GameMaster.GetGameRunFileName(this.CurrentSaveIndex.Value), false);
			this.GameRunSaveData = null;
			this.AppendGameRunHistory(gameRunRecord, bluePoint);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x000060A8 File Offset: 0x000042A8
		private void SaveProfileWithSettings(GameSettingsSaveData settings)
		{
			if (this.CurrentSaveIndex == null)
			{
				Debug.LogError("SaveIndex is not set, profile not saved");
				return;
			}
			ProfileSaveData currentProfileSaveData = this._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				Debug.LogError("CurrentProfileSaveData is null, profile not saved");
				return;
			}
			currentProfileSaveData.Settings = settings;
			GameMaster.WriteSaveData(GameMaster.GetProfileFileName(this.CurrentSaveIndex.Value), SaveDataHelper.SerializeProfile(currentProfileSaveData, true));
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0000610C File Offset: 0x0000430C
		public static bool TryLoadProfileSaveData(int index, out ProfileSaveData saveData)
		{
			string profileFileName = GameMaster.GetProfileFileName(index);
			if (!File.Exists(Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), profileFileName)))
			{
				saveData = null;
				return true;
			}
			bool flag;
			try
			{
				byte[] array = File.ReadAllBytes(Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), profileFileName));
				saveData = SaveDataHelper.DeserializeProfile(array);
				flag = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to load " + profileFileName);
				Debug.LogException(ex);
				saveData = null;
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000618C File Offset: 0x0000438C
		public static GameMaster.GameRunSaveDataLoadResult TryLoadGameRunSaveData(int index, out GameRunSaveData saveData)
		{
			string gameRunFileName = GameMaster.GetGameRunFileName(index);
			string text = Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), gameRunFileName);
			if (!File.Exists(text))
			{
				saveData = null;
				return GameMaster.GameRunSaveDataLoadResult.NotFound;
			}
			GameMaster.GameRunSaveDataLoadResult gameRunSaveDataLoadResult;
			try
			{
				byte[] array = File.ReadAllBytes(text);
				saveData = SaveDataHelper.DeserializeGameRun(array);
				if (!GameMaster.IsVersionMatches(saveData))
				{
					gameRunSaveDataLoadResult = GameMaster.GameRunSaveDataLoadResult.VersionMismatch;
				}
				else
				{
					gameRunSaveDataLoadResult = GameMaster.GameRunSaveDataLoadResult.Success;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to load " + gameRunFileName);
				Debug.LogException(ex);
				saveData = null;
				gameRunSaveDataLoadResult = GameMaster.GameRunSaveDataLoadResult.Failed;
			}
			return gameRunSaveDataLoadResult;
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0000620C File Offset: 0x0000440C
		private static bool IsVersionMatches(GameRunSaveData saveData)
		{
			SemVer semVer;
			if (!SemVer.TryParse(saveData.GameVersion, out semVer))
			{
				Debug.Log("[Profile] Cannot get game-run save data version, discarded.");
				return false;
			}
			SemVer semVer2;
			if (!SemVer.TryParse(VersionInfo.Current.Version, out semVer2))
			{
				Debug.Log("[Profile] Cannot get player version, game-run save data discarded");
				return false;
			}
			if (!semVer.EqualsWithoutCount(semVer2))
			{
				Debug.Log(string.Format("[Profile] Game-run save data major version mismatch ({0} vs {1}), save data discarded", semVer, semVer2));
				return false;
			}
			return true;
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00006270 File Offset: 0x00004470
		private void SaveGameRun(GameRunSaveData data, bool normalSave = true)
		{
			data.SaveTimestamp = Utils.ToIso8601Timestamp(DateTime.Now);
			if (normalSave)
			{
				data.PlayedSeconds = Singleton<GameMaster>.Instance.CurrentGameRunPlayedSeconds;
			}
			VersionInfo versionInfo = VersionInfo.Current;
			data.GameVersion = versionInfo.Version;
			data.GameRevision = versionInfo.Revision;
			if (this.CurrentSaveIndex == null)
			{
				Debug.LogError("SaveIndex is not set, game-run not saved");
				return;
			}
			GameMaster.WriteSaveData(GameMaster.GetGameRunFileName(this.CurrentSaveIndex.Value), SaveDataHelper.SerializeGameRun(data, true));
			this.GameRunSaveData = data;
			if (normalSave)
			{
				UiManager.GetPanel<SystemBoard>().ShowGameSaveHint();
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x0000630C File Offset: 0x0000450C
		private static void RevealCardRecursive(ProfileSaveData profile, string id)
		{
			if (id.EndsWith('+'))
			{
				string text = id;
				id = text.Substring(0, text.Length - 1);
			}
			CardConfig cardConfig = CardConfig.FromId(id);
			if (cardConfig == null)
			{
				Debug.LogError("Cannot reveal card " + id + " which not exists");
				return;
			}
			List<string> cardsRevealed = profile.CardsRevealed;
			int num = cardsRevealed.LowerBound(id);
			if (num >= cardsRevealed.Count || !cardsRevealed[num].Equals(id))
			{
				cardsRevealed.Insert(num, id);
				foreach (string text2 in cardConfig.RelativeCards)
				{
					GameMaster.RevealCardRecursive(profile, text2);
				}
			}
		}

		// Token: 0x06000104 RID: 260 RVA: 0x000063CC File Offset: 0x000045CC
		public static void RevealCard(string cardId)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal card while profile is null");
			}
			GameMaster.RevealCardRecursive(currentProfile, cardId);
		}

		// Token: 0x06000105 RID: 261 RVA: 0x000063EC File Offset: 0x000045EC
		public static void RevealExhibit(string exhibitId)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal exhibit while profile is null");
			}
			List<string> exhibitsRevealed = currentProfile.ExhibitsRevealed;
			int num = exhibitsRevealed.LowerBound(exhibitId);
			if (num >= exhibitsRevealed.Count || !exhibitsRevealed[num].Equals(exhibitId))
			{
				exhibitsRevealed.Insert(num, exhibitId);
			}
			foreach (string text in ExhibitConfig.FromId(exhibitId).RelativeCards)
			{
				GameMaster.RevealCardRecursive(currentProfile, text);
			}
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00006488 File Offset: 0x00004688
		public static void RevealEnemyGroup(string enemyGroupId)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal enemy-group while profile is null");
			}
			List<string> enemyGroupRevealed = currentProfile.EnemyGroupRevealed;
			int num = enemyGroupRevealed.LowerBound(enemyGroupId);
			if (num >= enemyGroupRevealed.Count || !enemyGroupRevealed[num].Equals(enemyGroupId))
			{
				enemyGroupRevealed.Insert(num, enemyGroupId);
			}
		}

		// Token: 0x06000107 RID: 263 RVA: 0x000064DB File Offset: 0x000046DB
		public static bool ShouldShowHint(string key)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal enemy-group while profile is null");
			}
			return !currentProfile.HintStatus.ShownHints.Contains(key);
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00006508 File Offset: 0x00004708
		public static void MarkHintAsShown(string key)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal enemy-group while profile is null");
			}
			if (currentProfile.HintStatus.ShownHints.Add(key))
			{
				Singleton<GameMaster>.Instance.SaveProfile();
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000653E File Offset: 0x0000473E
		public static void ResetHints()
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal enemy-group while profile is null");
			}
			currentProfile.HintStatus.BattleHintShown = false;
			currentProfile.HintStatus.ShownHints.Clear();
			Singleton<GameMaster>.Instance.SaveProfile();
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00006580 File Offset: 0x00004780
		public static List<GameRunRecordSaveData> GetGameRunHistory()
		{
			int? currentSaveIndex = Singleton<GameMaster>.Instance.CurrentSaveIndex;
			if (currentSaveIndex != null)
			{
				int valueOrDefault = currentSaveIndex.GetValueOrDefault();
				string text = Path.Combine(GameMaster.PlatformHandler.GetSaveDataFolder(), GameMaster.GetHistoryFileName(valueOrDefault));
				List<GameRunRecordSaveData> list;
				try
				{
					list = (File.Exists(text) ? SaveDataHelper.DeserializeGameRunHistory(File.ReadAllBytes(text)) : new List<GameRunRecordSaveData>());
				}
				catch (Exception ex)
				{
					Debug.LogError("[GameMaster] Failed to load game-run history at '" + text + "'");
					Debug.LogException(ex);
					list = new List<GameRunRecordSaveData>();
				}
				return list;
			}
			Debug.LogError("[GameMaster] Cannot get game-run history while profile is null");
			return new List<GameRunRecordSaveData>();
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00006624 File Offset: 0x00004824
		public static void UnlockDifficulty()
		{
			ProfileSaveData currentProfileSaveData = Singleton<GameMaster>.Instance._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				Debug.LogError("CurrentProfileSaveData is null, profile not saved");
				return;
			}
			PlayerUnit[] selectablePlayers = LBoL.Core.Library.GetSelectablePlayers();
			for (int i = 0; i < selectablePlayers.Length; i++)
			{
				PlayerUnit chara = selectablePlayers[i];
				CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(currentProfileSaveData.CharacterStats, (CharacterStatsSaveData s) => s.CharacterId == chara.Id);
				if (characterStatsSaveData == null)
				{
					characterStatsSaveData = new CharacterStatsSaveData
					{
						CharacterId = chara.Id,
						HighestPerfectSuccessDifficulty = new GameDifficulty?(GameDifficulty.Lunatic)
					};
					currentProfileSaveData.CharacterStats = Enumerable.ToArray<CharacterStatsSaveData>(Enumerable.OrderBy<CharacterStatsSaveData, string>(Enumerable.Concat<CharacterStatsSaveData>(currentProfileSaveData.CharacterStats, new CharacterStatsSaveData[] { characterStatsSaveData }), (CharacterStatsSaveData s) => s.CharacterId));
				}
				else
				{
					characterStatsSaveData.HighestPerfectSuccessDifficulty = new GameDifficulty?(GameDifficulty.Lunatic);
				}
			}
			Singleton<GameMaster>.Instance.SaveProfile();
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00006712 File Offset: 0x00004912
		private void TriggerSettingsChanged(GameSettingsSaveData settings)
		{
			this.SaveProfileWithSettings(settings);
			Action<GameSettingsSaveData> settingsChanged = GameMaster.SettingsChanged;
			if (settingsChanged == null)
			{
				return;
			}
			settingsChanged.Invoke(settings);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000672C File Offset: 0x0000492C
		private GameSettingsSaveData TryGetGameSettings()
		{
			ProfileSaveData currentProfileSaveData = this._currentProfileSaveData;
			if (currentProfileSaveData == null)
			{
				Debug.LogError("Cannot fetch game settings while profile is empty");
				return null;
			}
			return currentProfileSaveData.Settings;
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x0600010E RID: 270 RVA: 0x00006755 File Offset: 0x00004955
		// (set) Token: 0x0600010F RID: 271 RVA: 0x0000676C File Offset: 0x0000496C
		public static bool IsTurboMode
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.IsTurboMode;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.IsTurboMode = value;
				GameMaster.SetTurboMode(value);
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000110 RID: 272 RVA: 0x0000679C File Offset: 0x0000499C
		// (set) Token: 0x06000111 RID: 273 RVA: 0x000067B4 File Offset: 0x000049B4
		public static bool ShowVerboseKeywords
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData == null || gameSettingsSaveData.ShowVerboseKeywords;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowVerboseKeywords = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000112 RID: 274 RVA: 0x000067DE File Offset: 0x000049DE
		// (set) Token: 0x06000113 RID: 275 RVA: 0x000067F8 File Offset: 0x000049F8
		public static bool ShowIllustrator
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.ShowIllustrator;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowIllustrator = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00006822 File Offset: 0x00004A22
		// (set) Token: 0x06000115 RID: 277 RVA: 0x0000683C File Offset: 0x00004A3C
		public static bool IsLargeTooltips
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.IsLargeTooltips;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.IsLargeTooltips = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00006866 File Offset: 0x00004A66
		// (set) Token: 0x06000117 RID: 279 RVA: 0x00006888 File Offset: 0x00004A88
		public static bool PreferWideTooltips
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				if (gameSettingsSaveData == null)
				{
					return L10nManager.Info.PreferWideTooltip;
				}
				return gameSettingsSaveData.PreferWideTooltips;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.PreferWideTooltips = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000118 RID: 280 RVA: 0x000068B2 File Offset: 0x00004AB2
		// (set) Token: 0x06000119 RID: 281 RVA: 0x000068CC File Offset: 0x00004ACC
		public static bool RightClickCancel
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData == null || gameSettingsSaveData.RightClickCancel;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.RightClickCancel = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600011A RID: 282 RVA: 0x000068F6 File Offset: 0x00004AF6
		// (set) Token: 0x0600011B RID: 283 RVA: 0x00006910 File Offset: 0x00004B10
		public static bool IsLoopOrder
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.IsLoopOrder;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.IsLoopOrder = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600011C RID: 284 RVA: 0x0000693A File Offset: 0x00004B3A
		// (set) Token: 0x0600011D RID: 285 RVA: 0x00006954 File Offset: 0x00004B54
		public static bool SingleEnemyAutoSelect
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.SingleEnemyAutoSelect;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.SingleEnemyAutoSelect = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600011E RID: 286 RVA: 0x0000697E File Offset: 0x00004B7E
		// (set) Token: 0x0600011F RID: 287 RVA: 0x00006998 File Offset: 0x00004B98
		public static QuickPlayLevel QuickPlayLevel
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				if (gameSettingsSaveData == null)
				{
					return QuickPlayLevel.Default;
				}
				return gameSettingsSaveData.QuickPlayLevel;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.QuickPlayLevel = value;
				Singleton<GameMaster>.Instance.SaveProfile();
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000120 RID: 288 RVA: 0x000069CC File Offset: 0x00004BCC
		// (set) Token: 0x06000121 RID: 289 RVA: 0x000069E4 File Offset: 0x00004BE4
		public static bool ShowXCostEmptyUseWarning
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.ShowXCostEmptyUseWarning;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowXCostEmptyUseWarning = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000122 RID: 290 RVA: 0x00006A0E File Offset: 0x00004C0E
		// (set) Token: 0x06000123 RID: 291 RVA: 0x00006A28 File Offset: 0x00004C28
		public static bool ShowShortcut
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.ShowShortcut;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowShortcut = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000124 RID: 292 RVA: 0x00006A52 File Offset: 0x00004C52
		// (set) Token: 0x06000125 RID: 293 RVA: 0x00006A6C File Offset: 0x00004C6C
		public static bool ShowCardOrder
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.ShowCardOrder;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowCardOrder = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000126 RID: 294 RVA: 0x00006A96 File Offset: 0x00004C96
		// (set) Token: 0x06000127 RID: 295 RVA: 0x00006AB0 File Offset: 0x00004CB0
		public static bool ShowReload
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.ShowReload;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowReload = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000128 RID: 296 RVA: 0x00006ADA File Offset: 0x00004CDA
		// (set) Token: 0x06000129 RID: 297 RVA: 0x00006AF4 File Offset: 0x00004CF4
		public static bool Shake
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.Shake;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.Shake = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600012A RID: 298 RVA: 0x00006B1E File Offset: 0x00004D1E
		// (set) Token: 0x0600012B RID: 299 RVA: 0x00006B38 File Offset: 0x00004D38
		public static bool CostMoreLeft
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.CostMoreLeft;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.CostMoreLeft = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x0600012C RID: 300 RVA: 0x00006B62 File Offset: 0x00004D62
		// (set) Token: 0x0600012D RID: 301 RVA: 0x00006B7C File Offset: 0x00004D7C
		public static HintLevel HintLevel
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				if (gameSettingsSaveData == null)
				{
					return HintLevel.Detailed;
				}
				return gameSettingsSaveData.HintLevel;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.HintLevel = value;
				Singleton<GameMaster>.Instance.SaveProfile();
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600012E RID: 302 RVA: 0x00006BB0 File Offset: 0x00004DB0
		// (set) Token: 0x0600012F RID: 303 RVA: 0x00006BC8 File Offset: 0x00004DC8
		public static bool DefaultShowRandomResult
		{
			get
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				return gameSettingsSaveData != null && gameSettingsSaveData.ShowRandomResult;
			}
			set
			{
				GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
				gameSettingsSaveData.ShowRandomResult = value;
				Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000130 RID: 304 RVA: 0x00006BF2 File Offset: 0x00004DF2
		public static bool ShowDetailedHint
		{
			get
			{
				return GameMaster.HintLevel == HintLevel.Detailed;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000131 RID: 305 RVA: 0x00006BFC File Offset: 0x00004DFC
		public static bool ShowBriefHint
		{
			get
			{
				HintLevel hintLevel = GameMaster.HintLevel;
				return hintLevel == HintLevel.Detailed || hintLevel == HintLevel.Brief;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00006C1F File Offset: 0x00004E1F
		public static bool EnableKeyboard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00006C24 File Offset: 0x00004E24
		public static void SaveKeyboardBindings(string actionId, string inputPath)
		{
			GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
			if (inputPath != null)
			{
				gameSettingsSaveData.KeyboardBindings[actionId] = inputPath;
			}
			else
			{
				gameSettingsSaveData.KeyboardBindings.Remove(actionId);
			}
			Singleton<GameMaster>.Instance.SaveProfile();
			Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00006C70 File Offset: 0x00004E70
		public static void ClearKeyboardBindings()
		{
			GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
			gameSettingsSaveData.KeyboardBindings.Clear();
			Singleton<GameMaster>.Instance.SaveProfile();
			Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			UiManager.LoadKeyboardBindings(gameSettingsSaveData.KeyboardBindings);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00006CB4 File Offset: 0x00004EB4
		public static void SetPreferredCardIllustrator(string cardId, string illustratorId)
		{
			GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
			if (gameSettingsSaveData == null)
			{
				return;
			}
			CardConfig cardConfig = CardConfig.FromId(cardId);
			if (cardConfig == null)
			{
				Debug.LogError("Card config for " + cardId + " not found");
				return;
			}
			if (illustratorId == cardConfig.Illustrator || illustratorId == "")
			{
				gameSettingsSaveData.PreferredCardIllustrators.Remove(cardId);
			}
			else
			{
				if (!Enumerable.Contains<string>(cardConfig.SubIllustrator, illustratorId))
				{
					Debug.LogError(illustratorId + " not in " + cardId + " sub-illustrators");
					return;
				}
				gameSettingsSaveData.PreferredCardIllustrators[cardId] = illustratorId;
			}
			Singleton<GameMaster>.Instance.SaveProfile();
			Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00006D63 File Offset: 0x00004F63
		public static string GetPreferredCardIllustrator(Card card)
		{
			return GameMaster.GetPreferredCardIllustrator(card.Id);
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00006D70 File Offset: 0x00004F70
		private static string GetPreferredCardIllustrator(string cardId)
		{
			GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
			if (gameSettingsSaveData == null)
			{
				return null;
			}
			CardConfig cardConfig = CardConfig.FromId(cardId);
			if (cardConfig == null)
			{
				Debug.LogError("Card config for " + cardId + " not found");
				return null;
			}
			string text;
			if (!gameSettingsSaveData.PreferredCardIllustrators.TryGetValue(cardId, ref text))
			{
				return null;
			}
			if (Enumerable.Contains<string>(cardConfig.SubIllustrator, text))
			{
				return text;
			}
			gameSettingsSaveData.PreferredCardIllustrators.Remove(cardId);
			Singleton<GameMaster>.Instance.SaveProfile();
			return null;
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000138 RID: 312 RVA: 0x00006DE8 File Offset: 0x00004FE8
		// (remove) Token: 0x06000139 RID: 313 RVA: 0x00006E1C File Offset: 0x0000501C
		public static event Action<GameSettingsSaveData> SettingsChanged;

		// Token: 0x0600013A RID: 314 RVA: 0x00006E4F File Offset: 0x0000504F
		private IEnumerator RunStationDialog(List<StationDialogSource> dialogSource)
		{
			if (dialogSource.Count > 0)
			{
				foreach (StationDialogSource stationDialogSource in dialogSource)
				{
					yield return UiManager.GetPanel<VnPanel>().RunDialog(stationDialogSource.DialogName, new DialogStorage(), this.CurrentGameRun.DialogLibrary, RuntimeCommandHandler.Create(stationDialogSource.CommandHandler), null, null, null);
				}
				List<StationDialogSource>.Enumerator enumerator = default(List<StationDialogSource>.Enumerator);
			}
			yield break;
			yield break;
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00006E65 File Offset: 0x00005065
		private IEnumerator SwitchToStation(Station station)
		{
			yield return Environment.Instance.LoadEnvironment(station.Stage.Id, station.Level);
			UiManager.GetPanel<VnPanel>().SetNextButton(false, default(int?), null);
			switch (station.Type)
			{
			case StationType.None:
				throw new InvalidOperationException("Enter station whose StationType == None");
			case StationType.Enemy:
				yield return this.PreloadEnemyGroup(((BattleStation)station).EnemyGroup);
				break;
			case StationType.EliteEnemy:
				yield return this.PreloadEnemyGroup(((BattleStation)station).EnemyGroup);
				break;
			case StationType.Supply:
				UiManager.GetPanel<BackgroundPanel>().Show("Adventure");
				break;
			case StationType.Gap:
				UiManager.GetPanel<GapOptionsPanel>().Show((GapStation)station);
				Environment.EnterGapRoom();
				break;
			case StationType.Shop:
				yield return GameDirector.LoadLoreCharacterAsync(typeof(Takane), delegate
				{
					UiManager.GetPanel<ShopPanel>().Show((ShopStation)station);
				}, 0).ToCoroutine(null, null);
				GameDirector.ShopChat();
				UiManager.GetPanel<UltimateSkillPanel>().HideInDialog();
				break;
			case StationType.Adventure:
				UiManager.GetPanel<BackgroundPanel>().Show("Adventure");
				break;
			case StationType.Entry:
			{
				EntryStation entryStation = (EntryStation)station;
				if (entryStation.DebutAdventure != null)
				{
					if (this.CurrentGameRun.KaguyaInDebut)
					{
						yield return GameDirector.LoadLoreCharacterAsync(typeof(Eirin), new Action(this.ClickOnEirin), 0).ToCoroutine(null, null);
						yield return GameDirector.LoadLoreCharacterAsync(typeof(Kaguya), new Action(this.ClickOnKaguya), 1).ToCoroutine(null, null);
					}
					else
					{
						yield return GameDirector.LoadLoreCharacterAsync(typeof(Eirin), new Action(this.ClickOnEirin), 0).ToCoroutine(null, null);
					}
				}
				if (entryStation.Stage.DontAutoOpenMapInEntry)
				{
					GameDirector.HidePlayer();
				}
				break;
			}
			case StationType.Select:
			case StationType.BattleAdvTest:
				break;
			case StationType.Trade:
				UiManager.GetPanel<BackgroundPanel>().Show("Adventure");
				break;
			case StationType.Boss:
				yield return this.PreloadEnemyGroup(((BattleStation)station).EnemyGroup);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00006E7B File Offset: 0x0000507B
		private IEnumerator SwitchToStationFromSave(Station station)
		{
			yield return Environment.Instance.LoadEnvironment(station.Stage.Id, station.Level);
			UiManager.GetPanel<VnPanel>().SetNextButton(false, default(int?), null);
			if (station is IAdventureStation)
			{
				UiManager.GetPanel<BackgroundPanel>().Show("Adventure");
			}
			yield break;
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00006E8A File Offset: 0x0000508A
		private void ClickOnEirin()
		{
			Debug.Log("Eirin Clicked.");
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00006E96 File Offset: 0x00005096
		private void ClickOnKaguya()
		{
			Debug.Log("Kaguya Clicked.");
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00006EA2 File Offset: 0x000050A2
		private IEnumerator RunInnerStation(Station station)
		{
			yield return this.RunStationDialog(station.PreDialogs);
			switch (station.Type)
			{
			case StationType.None:
				throw new InvalidOperationException("Enter station whose StationType == None");
			case StationType.Enemy:
				yield return this.BattleStationFlow((BattleStation)station);
				break;
			case StationType.EliteEnemy:
				yield return this.BattleStationFlow((BattleStation)station);
				break;
			case StationType.Supply:
				yield return GameMaster.AdventureFlow((SupplyStation)station);
				break;
			case StationType.Gap:
				yield return this.GapStationFlow((GapStation)station);
				break;
			case StationType.Shop:
				station.Finish();
				break;
			case StationType.Adventure:
				yield return GameMaster.AdventureFlow((AdventureStation)station);
				break;
			case StationType.Entry:
			{
				EntryStation entryStation = (EntryStation)station;
				bool flag = false;
				if (entryStation.DebutAdventure != null)
				{
					yield return GameMaster.AdventureFlow(entryStation);
					flag = true;
				}
				else
				{
					entryStation.Finish();
				}
				if (flag)
				{
					GameDirector.DebutChat(this.CurrentGameRun.KaguyaInDebut, this.CurrentGameRun.Player.GetName().ToString(UnitNameStyle.Short));
				}
				if (entryStation.Stage.DontAutoOpenMapInEntry)
				{
					yield return new WaitForSecondsRealtime(1f);
					GameDirector.RevealPlayer(false);
					GameDirector.PlayerDebutAnimation();
				}
				else
				{
					UiManager.GetPanel<MapPanel>().Show();
				}
				break;
			}
			case StationType.Select:
				yield return this.SelectStationFlow((SelectStation)station);
				break;
			case StationType.Trade:
				yield return GameMaster.AdventureFlow((TradeStation)station);
				break;
			case StationType.Boss:
				yield return this.BattleStationFlow((BattleStation)station);
				break;
			case StationType.BattleAdvTest:
				yield return this.BattleAdvTestStationFlow((BattleAdvTestStation)station);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (station.GameRun.Player.IsDead)
			{
				GameMaster.EndGameProcedure(station.GameRun);
			}
			else if (station.IsNormalEnd && !station.GameRun.CanEnterTrueEnding())
			{
				GameMaster.EndGameProcedure(station.GameRun);
			}
			else if (station.IsTrueEnd)
			{
				GameMaster.EndGameProcedure(station.GameRun);
			}
			else
			{
				yield return this.EndStationFlow(station, false);
			}
			yield break;
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00006EB8 File Offset: 0x000050B8
		private IEnumerator EndStationFlow(Station station, bool skipBossReward = false)
		{
			if (station.IsStageEnd)
			{
				UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(1), new Action(GameMaster.RequestEnterNextStage));
			}
			else if (station.GameRun.MapMode == GameRunMapMode.TeleportBoss)
			{
				UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(0), new Action(GameMaster.TeleportToBossNode));
			}
			else
			{
				UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(0), delegate
				{
					UiManager.Show<MapPanel>();
					int? nextButtonStringIndex = UiManager.GetPanel<VnPanel>().NextButtonStringIndex;
					int? num = nextButtonStringIndex;
					int num2 = 0;
					if (!((num.GetValueOrDefault() == num2) & (num != null)))
					{
						num = nextButtonStringIndex;
						num2 = 1;
						if (!((num.GetValueOrDefault() == num2) & (num != null)) && GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("SkipReward"))
						{
							UiManager.GetPanel<HintPanel>().Show(new HintPayload
							{
								HintKey = "SkipReward"
							});
							GameMaster.MarkHintAsShown("SkipReward");
						}
					}
				});
			}
			if (!station.GameRun.Player.IsDead)
			{
				BattleStation battleStation = station as BattleStation;
				if (battleStation != null)
				{
					BossStation bossStation = battleStation as BossStation;
					if (bossStation != null && !skipBossReward)
					{
						UiManager.GetPanel<VnPanel>().SetNextButton(false, default(int?), null);
						bossStation.GenerateBossRewards();
						Exhibit[] bossRewards = bossStation.BossRewards;
						if (bossRewards != null && !bossRewards.Empty<Exhibit>())
						{
							BossExhibitPanel bossPanel = UiManager.GetPanel<BossExhibitPanel>();
							bossPanel.Show(bossRewards);
							yield return new WaitWhile(() => bossPanel.IsVisible);
						}
					}
					battleStation.GenerateRewards();
					if (!battleStation.Rewards.Empty<StationReward>())
					{
						UiManager.GetPanel<RewardPanel>().Show(new ShowRewardContent
						{
							Station = station,
							ShowNextButton = false
						});
						if (GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("Reward"))
						{
							UiManager.GetPanel<HintPanel>().Show(new HintPayload
							{
								HintKey = "Reward"
							});
							GameMaster.MarkHintAsShown("Reward");
						}
					}
				}
			}
			if (station.Type == StationType.Shop)
			{
				UiManager.GetPanel<VnPanel>().SetNextButton(true, new int?(2), null);
			}
			if (station.Type == StationType.Gap)
			{
				List<StationReward> rewards = station.Rewards;
				if (rewards != null && rewards.Count > 0)
				{
					UiManager.GetPanel<RewardPanel>().Show(new ShowRewardContent
					{
						Station = station
					});
				}
			}
			yield break;
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00006ED0 File Offset: 0x000050D0
		private void LeaveStation(Station station)
		{
			UiManager.Hide<RewardPanel>(true);
			switch (station.Type)
			{
			case StationType.None:
				throw new InvalidOperationException("Enter station whose StationType == None");
			case StationType.Enemy:
				this.ClearUnitLayer();
				return;
			case StationType.EliteEnemy:
				this.ClearUnitLayer();
				return;
			case StationType.Supply:
				UiManager.Hide<BackgroundPanel>(true);
				UiManager.GetPanel<VnPanel>().HideContent();
				return;
			case StationType.Gap:
				UiManager.Hide<GapOptionsPanel>(false);
				Environment.LeaveGapRoom();
				return;
			case StationType.Shop:
				UiManager.Hide<ShopPanel>(false);
				GameDirector.StopLoreChat();
				this.ClearUnitLayer();
				UiManager.GetPanel<UltimateSkillPanel>().ShowInDialog();
				return;
			case StationType.Adventure:
				UiManager.Hide<BackgroundPanel>(true);
				UiManager.GetPanel<VnPanel>().HideContent();
				return;
			case StationType.Entry:
				UiManager.GetPanel<VnPanel>().HideContent();
				GameDirector.StopLoreChat();
				this.ClearUnitLayer();
				return;
			case StationType.Select:
				UiManager.GetPanel<VnPanel>().HideContent();
				return;
			case StationType.Trade:
				UiManager.Hide<BackgroundPanel>(true);
				UiManager.GetPanel<VnPanel>().HideContent();
				return;
			case StationType.Boss:
				this.ClearUnitLayer();
				return;
			case StationType.BattleAdvTest:
				this.ClearUnitLayer();
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00006FCD File Offset: 0x000051CD
		private IEnumerator PreloadEnemyGroup(EnemyGroup group)
		{
			GameDirector.MovePlayer(group.PlayerRootV2);
			foreach (EnemyUnit enemyUnit in group)
			{
				yield return GameDirector.LoadEnemyAsync(enemyUnit, group.FormationName, false, default(int?)).ToCoroutine(null, null);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			GameDirector.HideAll();
			yield break;
			yield break;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00006FDC File Offset: 0x000051DC
		private IEnumerator BattleStationFlow(BattleStation battleStation)
		{
			GameDirector.RevealPlayer(false);
			GameDirector.PlayerDebutAnimation();
			GameRunController gameRun = battleStation.GameRun;
			EnemyGroup enemyGroup = battleStation.EnemyGroup;
			if (!enemyGroup.Hidden)
			{
				GameDirector.RevealAllEnemies(false);
				GameDirector.AllEnemiesDebutAnimation();
			}
			yield return new WaitForSeconds(enemyGroup.DebutTime);
			string preBattleDialogName = enemyGroup.PreBattleDialogName;
			string postBattleDialogName = enemyGroup.PostBattleDialogName;
			DialogStorage storage = new DialogStorage();
			if (!string.IsNullOrWhiteSpace(preBattleDialogName))
			{
				yield return UiManager.GetPanel<VnPanel>().RunDialog("Battle/" + preBattleDialogName, storage, gameRun.DialogLibrary, RuntimeCommandHandler.Create(battleStation), null, null, null);
			}
			yield return GameMaster.BattleFlow(enemyGroup);
			if (gameRun.Player.IsAlive && !string.IsNullOrWhiteSpace(postBattleDialogName))
			{
				yield return UiManager.GetPanel<VnPanel>().RunDialog("Battle/" + postBattleDialogName, storage, gameRun.DialogLibrary, RuntimeCommandHandler.Create(battleStation), null, null, null);
			}
			battleStation.Finish();
			GameRunSaveData gameRunSaveData = gameRun.Save();
			gameRunSaveData.Timing = SaveTiming.BattleFinish;
			gameRunSaveData.EnteredStationType = new StationType?(battleStation.Type);
			gameRunSaveData.BattleStationEnemyGroup = battleStation.EnemyGroupEntry.Id;
			this.SaveGameRun(gameRunSaveData, true);
			yield break;
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00006FF2 File Offset: 0x000051F2
		private IEnumerator BattleStationFlowFromEndSave(BattleStation battleStation, SaveTiming saveTiming)
		{
			yield return this.EndStationFlow(battleStation, saveTiming == SaveTiming.AfterBossReward);
			yield break;
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000700F File Offset: 0x0000520F
		private IEnumerator BattleAdvTestStationFlow(BattleAdvTestStation station)
		{
			SelectDebugPanel panel = UiManager.GetPanel<SelectDebugPanel>();
			panel.Show(station);
			yield return new WaitWhile(() => panel.IsVisible);
			GameRunController gameRun = station.GameRun;
			EnemyGroup group = station.EnemyGroup;
			Adventure adventure = station.Adventure;
			if (group != null)
			{
				if (!group.Environment.IsNullOrEmpty())
				{
					yield return Environment.Instance.LoadEnvironment(group.Environment);
				}
				GameDirector.MovePlayer(group.PlayerRootV2);
				foreach (EnemyUnit enemyUnit in group)
				{
					yield return GameDirector.LoadEnemyAsync(enemyUnit, group.FormationName, false, default(int?)).ToCoroutine(null, null);
				}
				IEnumerator<EnemyUnit> enumerator = null;
				GameDirector.PlayerDebutAnimation();
				GameDirector.AllEnemiesDebutAnimation();
				EnemyGroup enemyGroup = station.EnemyGroup;
				yield return new WaitForSeconds(enemyGroup.DebutTime);
				string preBattleDialogName = enemyGroup.PreBattleDialogName;
				string postBattleDialogName = enemyGroup.PostBattleDialogName;
				DialogStorage storage = new DialogStorage();
				if (!string.IsNullOrWhiteSpace(preBattleDialogName))
				{
					yield return UiManager.GetPanel<VnPanel>().RunDialog("Battle/" + preBattleDialogName, storage, gameRun.DialogLibrary, RuntimeCommandHandler.Create(station), null, null, null);
				}
				yield return GameMaster.BattleFlow(enemyGroup);
				if (gameRun.Player.IsAlive && !string.IsNullOrWhiteSpace(postBattleDialogName))
				{
					yield return UiManager.GetPanel<VnPanel>().RunDialog("Battle/" + postBattleDialogName, storage, gameRun.DialogLibrary, RuntimeCommandHandler.Create(station), null, null, null);
				}
				station.Finish();
				enemyGroup = null;
				postBattleDialogName = null;
				storage = null;
			}
			else if (adventure != null)
			{
				UiManager.GetPanel<BackgroundPanel>().Show("Adventure");
				yield return GameMaster.AdventureFlow(station);
				UiManager.Hide<BackgroundPanel>(true);
				UiManager.GetPanel<VnPanel>().HideContent();
			}
			else
			{
				station.Finish();
			}
			if (station.GameRun.Player.IsAlive)
			{
				List<StationReward> rewards = station.Rewards;
				if (rewards != null && !rewards.Empty<StationReward>())
				{
					UiManager.GetPanel<RewardPanel>().Show(new ShowRewardContent
					{
						Station = station,
						ShowNextButton = false
					});
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0000701E File Offset: 0x0000521E
		private IEnumerator GapStationFlow(GapStation gapStation)
		{
			yield return UiManager.GetPanel<GapOptionsPanel>().WaitUntilOptionSelected();
			yield return this.RunStationDialog(gapStation.PostDialogs);
			gapStation.Finish();
			yield break;
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00007034 File Offset: 0x00005234
		private IEnumerator SelectStationFlow(SelectStation selectStation)
		{
			EnemyUnit[] opponents = selectStation.Opponents;
			DialogStorage storage = new DialogStorage();
			int num = 0;
			foreach (EnemyUnit enemyUnit in opponents)
			{
				num++;
				storage.SetValue(string.Format("$opponent{0}", num), enemyUnit.Name);
				storage.SetValue(string.Format("$name{0}", num), enemyUnit.GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default));
			}
			int num2 = this.CurrentGameRun.RootRng.NextInt(1, opponents.Length);
			storage.SetValue("$randomIndex", (float)num2);
			storage.SetValue("$randomName", opponents[num2 - 1].GetName().ToString(true, NounCase.Nominative, UnitNameStyle.Default));
			yield return UiManager.GetPanel<VnPanel>().RunDialog("SelectOpponent", storage, this.CurrentGameRun.DialogLibrary, null, null, null, null);
			selectStation.Finish();
			float num4;
			float? num3 = (storage.TryGetValue<float>("$selection", out num4) ? new float?(num4) : default(float?));
			if (num3 == null)
			{
				Debug.LogError("SelectOpponent ended without selection");
				yield break;
			}
			int num5 = (int)num3.Value - 1;
			if (num5 < 0 || num5 >= opponents.Length)
			{
				Debug.LogError(string.Format("SelectOpponent ended with selection({0}) out of range", num5));
				yield break;
			}
			string id = opponents[num5].Config.Id;
			selectStation.Stage.SetBoss(id);
			UiManager.GetPanel<MapPanel>().FinalWidget.SetBoss(id);
			yield break;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0000704A File Offset: 0x0000524A
		private void ClearUnitLayer()
		{
			GameDirector.ClearEnemies();
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00007090 File Offset: 0x00005290
		[CompilerGenerated]
		internal static void <EndGameStatistics>g__AddStatData|60_0(string id, ref GameMaster.<>c__DisplayClass60_0 A_1)
		{
			BluePointConfig bluePointConfig = BluePointConfig.FromId(id);
			int? num = ((bluePointConfig != null) ? bluePointConfig.BluePoint : default(int?));
			if (num != null)
			{
				int valueOrDefault = num.GetValueOrDefault();
				A_1.bluePoint += valueOrDefault;
				A_1.scoreData.Add(new ScoreData
				{
					Id = id,
					TotalBluePoint = valueOrDefault
				});
				return;
			}
			Debug.LogError("[GameResult] Cannot find BluePointConfig." + id + " or its BluePoint is not set");
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00007114 File Offset: 0x00005314
		[CompilerGenerated]
		internal static void <EndGameStatistics>g__AddCountableStatData|60_1(string id, int count, ref GameMaster.<>c__DisplayClass60_0 A_2)
		{
			BluePointConfig bluePointConfig = BluePointConfig.FromId(id);
			int? num = ((bluePointConfig != null) ? bluePointConfig.BluePoint : default(int?));
			if (num != null)
			{
				int valueOrDefault = num.GetValueOrDefault();
				A_2.bluePoint += count * valueOrDefault;
				A_2.scoreData.Add(new ScoreData
				{
					Id = id,
					Num = count,
					TotalBluePoint = count * valueOrDefault
				});
				return;
			}
			Debug.LogError("[GameResult] Cannot find BluePointConfig." + id + " or its BluePoint is not set");
		}

		// Token: 0x0600014D RID: 333 RVA: 0x000071A1 File Offset: 0x000053A1
		[CompilerGenerated]
		internal static UniTask <LoadSharedUiTo>g__LoadShared|70_0<T>(bool show = false, ref GameMaster.<>c__DisplayClass70_0 A_1) where T : UiPanelBase
		{
			A_1.ui.Add(typeof(T));
			return UiManager.LoadPanelAsync<T>(show);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000071C3 File Offset: 0x000053C3
		[CompilerGenerated]
		internal static UniTask <LoadMainMenuUiAsync>g__Load|72_0<T>(bool show = false) where T : UiPanelBase
		{
			GameMaster.MainMenuUiList.Add(typeof(T));
			return UiManager.LoadPanelAsync<T>("MainMenu", show);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x000071E9 File Offset: 0x000053E9
		[CompilerGenerated]
		internal static UniTask <CoLoadGameRunUi>g__Load|75_0<T>(bool show = false) where T : UiPanelBase
		{
			GameMaster.GameRunUiList.Add(typeof(T));
			return UiManager.LoadPanelAsync<T>("GameRun", show);
		}

		// Token: 0x0400003A RID: 58
		private DateTime _currentGameRunStartTime;

		// Token: 0x0400003D RID: 61
		private static readonly List<Type> MainMenuUiList = new List<Type>();

		// Token: 0x0400003E RID: 62
		private static readonly List<Type> GameRunUiList = new List<Type>();

		// Token: 0x0400003F RID: 63
		private static readonly Dictionary<Type, List<IAdventureHandler>> ExtraAdventureHandlers = new Dictionary<Type, List<IAdventureHandler>>();

		// Token: 0x04000040 RID: 64
		private static bool _quitConfirmed;

		// Token: 0x04000043 RID: 67
		private static readonly Vector2 CursorHotpot = new Vector2(4f, 2f);

		// Token: 0x04000044 RID: 68
		private const string UseLbolCursorName = "UseLbolCursor";

		// Token: 0x04000046 RID: 70
		private const string AnimatingEnvironmentName = "IsAnimatingEnvironmentEnabled";

		// Token: 0x04000047 RID: 71
		private static bool _isAnimatingEnvironmentEnabled;

		// Token: 0x04000048 RID: 72
		public const string VsyncCountName = "VsyncCount";

		// Token: 0x04000049 RID: 73
		public const string TargetFrameRateName = "TargetFrameRate";

		// Token: 0x0400004B RID: 75
		private const bool SaveCompressed = true;

		// Token: 0x0400004C RID: 76
		private const int MaxHistory = 50;

		// Token: 0x0400004D RID: 77
		private int? _currentSaveIndex;

		// Token: 0x0400004E RID: 78
		[MaybeNull]
		private ProfileSaveData _currentProfileSaveData;

		// Token: 0x0200012A RID: 298
		private class GameStatisticData
		{
			// Token: 0x04000C30 RID: 3120
			public int BluePoint;

			// Token: 0x04000C31 RID: 3121
			public List<ScoreData> ScoreDatas;

			// Token: 0x04000C32 RID: 3122
			public float DifficultyMultipler;

			// Token: 0x04000C33 RID: 3123
			public int DebugExp;
		}

		// Token: 0x0200012B RID: 299
		public enum GameMasterStatus
		{
			// Token: 0x04000C35 RID: 3125
			GameEntry,
			// Token: 0x04000C36 RID: 3126
			MainMenu,
			// Token: 0x04000C37 RID: 3127
			InGame
		}

		// Token: 0x0200012C RID: 300
		public sealed class MainMenuInputActionHandler : IInputActionHandler
		{
			// Token: 0x06001042 RID: 4162 RVA: 0x0004C970 File Offset: 0x0004AB70
			public void OnCancel()
			{
				VnPanel panel = UiManager.GetPanel<VnPanel>();
				if (panel.IsRunning)
				{
					if (panel.CanSkipAll)
					{
						UiManager.GetDialog<MessageDialog>().Show(new MessageContent
						{
							TextKey = "SkipDialog",
							Buttons = DialogButtons.ConfirmCancel,
							OnConfirm = new Action(panel.SkipAll)
						});
					}
					return;
				}
				MainMenuPanel panel2 = UiManager.GetPanel<MainMenuPanel>();
				if (panel2.IsEarlyAccessShowing)
				{
					panel2.UI_SwitchEarlyAccessHint(false);
					return;
				}
				if (panel2.IsMainMenuGourp)
				{
					panel2.UI_QuitGame();
					return;
				}
				panel2.UI_ShowMainMenu();
			}

			// Token: 0x06001043 RID: 4163 RVA: 0x0004C9F4 File Offset: 0x0004ABF4
			public void OnRightClickCancel()
			{
				VnPanel panel = UiManager.GetPanel<VnPanel>();
				if (panel.IsRunning)
				{
					if (panel.CanSkipAll)
					{
						UiManager.GetDialog<MessageDialog>().Show(new MessageContent
						{
							TextKey = "SkipDialog",
							Buttons = DialogButtons.ConfirmCancel,
							OnConfirm = new Action(panel.SkipAll)
						});
					}
					return;
				}
				MainMenuPanel panel2 = UiManager.GetPanel<MainMenuPanel>();
				if (panel2.IsEarlyAccessShowing)
				{
					panel2.UI_SwitchEarlyAccessHint(false);
					return;
				}
				if (!panel2.IsMainMenuGourp)
				{
					panel2.UI_ShowMainMenu();
				}
			}

			// Token: 0x06001044 RID: 4164 RVA: 0x0004CA6F File Offset: 0x0004AC6F
			public void BeginSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(true);
			}

			// Token: 0x06001045 RID: 4165 RVA: 0x0004CA7C File Offset: 0x0004AC7C
			public void EndSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(false);
			}
		}

		// Token: 0x0200012D RID: 301
		private sealed class GameRunInputActionHandler : IInputActionHandler
		{
			// Token: 0x06001047 RID: 4167 RVA: 0x0004CA91 File Offset: 0x0004AC91
			public void OnNavigate(NavigateDirection dir)
			{
				if (UiManager.GetPanel<VnPanel>().HandleNavigateAction(dir))
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleNavigateFromKey(dir);
			}

			// Token: 0x06001048 RID: 4168 RVA: 0x0004CAAD File Offset: 0x0004ACAD
			public void OnConfirm()
			{
				if (UiManager.GetPanel<VnPanel>().HandleConfirmAction())
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleConfirmAction();
			}

			// Token: 0x06001049 RID: 4169 RVA: 0x0004CAC7 File Offset: 0x0004ACC7
			public void OnCancel()
			{
				if (UiManager.GetPanel<VnPanel>().HandleCancelAction())
				{
					return;
				}
				if (UiManager.GetPanel<PlayBoard>().HandleCancelAction())
				{
					return;
				}
				UiManager.GetPanel<SettingPanel>().Show(SettingsPanelType.InGame);
			}

			// Token: 0x0600104A RID: 4170 RVA: 0x0004CAEE File Offset: 0x0004ACEE
			public void OnRightClickCancel()
			{
				if (UiManager.GetPanel<VnPanel>().HandleCancelAction())
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleCancelAction();
			}

			// Token: 0x0600104B RID: 4171 RVA: 0x0004CB08 File Offset: 0x0004AD08
			public void BeginSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(true);
			}

			// Token: 0x0600104C RID: 4172 RVA: 0x0004CB15 File Offset: 0x0004AD15
			public void EndSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(false);
			}

			// Token: 0x0600104D RID: 4173 RVA: 0x0004CB22 File Offset: 0x0004AD22
			public void BeginShowEnemyMoveOrder()
			{
				GameDirector.SetEnemyMoveOrderVisible(true);
			}

			// Token: 0x0600104E RID: 4174 RVA: 0x0004CB2A File Offset: 0x0004AD2A
			public void EndShowEnemyMoveOrder()
			{
				GameDirector.SetEnemyMoveOrderVisible(false);
			}

			// Token: 0x0600104F RID: 4175 RVA: 0x0004CB32 File Offset: 0x0004AD32
			public void OnSelect(int i)
			{
				if (UiManager.GetPanel<VnPanel>().HandleSelectionFromKey(i))
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleSelectionFromKey(i);
			}

			// Token: 0x06001050 RID: 4176 RVA: 0x0004CB4D File Offset: 0x0004AD4D
			public void OnPoolMana(ManaColor color)
			{
				UiManager.GetPanel<PlayBoard>().HandlePoolMana(color);
			}

			// Token: 0x06001051 RID: 4177 RVA: 0x0004CB5A File Offset: 0x0004AD5A
			public void OnUseUs()
			{
				UiManager.GetPanel<PlayBoard>().HandleUseUsFromKey();
			}

			// Token: 0x06001052 RID: 4178 RVA: 0x0004CB66 File Offset: 0x0004AD66
			public void OnEndTurn()
			{
				UiManager.GetPanel<PlayBoard>().HandleEndTurnFromKey();
			}

			// Token: 0x06001053 RID: 4179 RVA: 0x0004CB72 File Offset: 0x0004AD72
			public void OnToggleMap()
			{
				UiManager.GetPanel<SystemBoard>().ToggleMapPanel();
			}

			// Token: 0x06001054 RID: 4180 RVA: 0x0004CB7E File Offset: 0x0004AD7E
			public void OnToggleBaseDeck()
			{
				UiManager.GetPanel<SystemBoard>().ShowBaseDeck();
			}

			// Token: 0x06001055 RID: 4181 RVA: 0x0004CB8A File Offset: 0x0004AD8A
			public void OnToggleDrawZone()
			{
				UiManager.GetPanel<PlayBoard>().ShowDrawZone();
			}

			// Token: 0x06001056 RID: 4182 RVA: 0x0004CB96 File Offset: 0x0004AD96
			public void OnToggleDiscardZone()
			{
				UiManager.GetPanel<PlayBoard>().ShowDiscardZone();
			}

			// Token: 0x06001057 RID: 4183 RVA: 0x0004CBA2 File Offset: 0x0004ADA2
			public void OnToggleExileZone()
			{
				UiManager.GetPanel<PlayBoard>().ShowExileZone();
			}

			// Token: 0x06001058 RID: 4184 RVA: 0x0004CBAE File Offset: 0x0004ADAE
			public void OnToggleMinimize()
			{
				if (UiManager.GetPanel<SelectCardPanel>().SwitchMinimized())
				{
					return;
				}
				UiManager.GetPanel<SelectBaseManaPanel>().SwitchMinimized();
			}
		}

		// Token: 0x0200012E RID: 302
		public enum GameRunSaveDataLoadResult
		{
			// Token: 0x04000C39 RID: 3129
			Success,
			// Token: 0x04000C3A RID: 3130
			NotFound,
			// Token: 0x04000C3B RID: 3131
			VersionMismatch,
			// Token: 0x04000C3C RID: 3132
			Failed
		}
	}
}
