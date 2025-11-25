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
	public class GameMaster : Singleton<GameMaster>, IGameRunAchievementHandler, IGameRunVisualTrigger
	{
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
		public static void UnlockAchievement(AchievementKey achievementKey)
		{
			GameMaster.UnlockAchievement(achievementKey.ToString());
		}
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
		void IGameRunAchievementHandler.IncreaseStats(ProfileStatsKey statsKey)
		{
			GameMaster.IncreaseStats(statsKey);
		}
		void IGameRunAchievementHandler.UnlockAchievement(AchievementKey achievementKey)
		{
			GameMaster.UnlockAchievement(achievementKey);
		}
		public GameRunController CurrentGameRun { get; private set; }
		public static bool ShowPoseAnimation { get; set; }
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
		public static bool ShowRandomResult
		{
			get
			{
				return Singleton<GameMaster>.Instance.CurrentGameRun != null && Singleton<GameMaster>.Instance.CurrentGameRun.ShowRandomResult;
			}
		}
		public static PlatformHandler PlatformHandler
		{
			get
			{
				return PlatformHandlerRunner.Instance.PlatformHandler;
			}
		}
		private static void ShowErrorDialog(string error)
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				Text = error,
				Icon = MessageIcon.Error
			});
		}
		private static void ShowErrorDialogWithKey(string errorKey)
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = errorKey,
				Icon = MessageIcon.Error
			});
		}
		private static void ShowWarningDialogWithKey(string warningKey, [MaybeNull] string warningSubTextKey = null)
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = warningKey,
				SubTextKey = warningSubTextKey,
				Icon = MessageIcon.Warning
			});
		}
		public static void StartupEnterMainMenu(int? saveIndex)
		{
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoStartupEnterMainMenu(saveIndex));
		}
		public static bool ShowCardId { get; set; }
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
		public static void StartGame(GameDifficulty difficulty, PuzzleFlag puzzles, PlayerUnit player, PlayerType playerType, Exhibit initExhibit, int? initMoneyOverride, IEnumerable<Card> deck, IEnumerable<Stage> stages, Type debutAdventureType, IEnumerable<JadeBox> jadeBoxes, GameMode gameMode = GameMode.FreeMode, bool showRandomResult = true)
		{
			GameMaster.StartGame(default(ulong?), difficulty, puzzles, player, playerType, initExhibit, initMoneyOverride, deck, stages, debutAdventureType, jadeBoxes, gameMode, showRandomResult);
		}
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
		private IEnumerator CoNewGameRun(GameRunController gameRun)
		{
			yield return this.CoSetupGameRun(gameRun);
			yield return this.CoEnterNextStage();
			GameMaster.Status = GameMaster.GameMasterStatus.InGame;
			GameMaster.ShowPoseAnimation = true;
			yield break;
		}
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
		private IEnumerator CoTeleportToNode(MapNode mapNode)
		{
			yield return UiManager.ShowLoading(0.2f).ToCoroutine(null);
			MapPanel panel = UiManager.GetPanel<MapPanel>();
			MapNodeWidget mapNodeWidget = panel.GetMapNodeWidget(mapNode.X, mapNode.Y);
			panel.EnterNode(mapNodeWidget);
			yield return this.CoEnterMapNode(mapNode, false, true);
			yield break;
		}
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
		private void PlayMusicFromSave(StationType stationType, int stageLevel)
		{
			if (stationType == StationType.Boss)
			{
				AudioManager.PlayAdventureBgm(2);
				return;
			}
			AudioManager.EnterStage(stageLevel, true);
		}
		private IEnumerator CoLeaveStation(Station station)
		{
			if (station != null)
			{
				this.LeaveStation(station);
			}
			yield break;
		}
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
		private static IEnumerator CoShowResultAndLeaveGameRun(GameResultData data)
		{
			UiManager.GetPanel<UltimateSkillPanel>().Hide();
			GameResultPanel panel = UiManager.GetPanel<GameResultPanel>();
			panel.Show(data);
			yield return new WaitWhile(() => panel.IsVisible);
			GameMaster.LeaveGameRun();
			yield break;
		}
		public static int? TempDebugExp { get; set; }
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
		public static void LeaveGameRun()
		{
			GameDirector.StopLoreChat();
			Singleton<GameMaster>.Instance.StopAllCoroutines();
			Singleton<GameMaster>.Instance.StartCoroutine(Singleton<GameMaster>.Instance.CoLeaveGameRun());
		}
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
		private static UniTask LoadSharedUiTo(IList<Type> ui)
		{
			GameMaster.<LoadSharedUiTo>d__70 <LoadSharedUiTo>d__;
			<LoadSharedUiTo>d__.<>t__builder = AsyncUniTaskMethodBuilder.Create();
			<LoadSharedUiTo>d__.ui = ui;
			<LoadSharedUiTo>d__.<>1__state = -1;
			<LoadSharedUiTo>d__.<>t__builder.Start<GameMaster.<LoadSharedUiTo>d__70>(ref <LoadSharedUiTo>d__);
			return <LoadSharedUiTo>d__.<>t__builder.Task;
		}
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
		private static void UnloadMainMenuUi()
		{
			foreach (Type type in GameMaster.MainMenuUiList)
			{
				UiManager.UnloadPanel(type);
			}
			GameMaster.MainMenuUiList.Clear();
		}
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
		private static void UnloadGameRunUi()
		{
			foreach (Type type in GameMaster.GameRunUiList)
			{
				UiManager.UnloadPanel(type);
			}
			GameMaster.GameRunUiList.Clear();
		}
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
		public static void RegisterExtraAdventureHandlers<TAdventure>(IAdventureHandler handlerInstance) where TAdventure : Adventure
		{
			GameMaster.RegisterExtraAdventureHandlers(typeof(TAdventure), handlerInstance);
		}
		public static void UnregisterExtraAdventureHandlers(Type adventureType, IAdventureHandler handlerInstance)
		{
			List<IAdventureHandler> list;
			if (GameMaster.ExtraAdventureHandlers.TryGetValue(adventureType, ref list))
			{
				list.Remove(handlerInstance);
			}
		}
		public static void UnregisterExtraAdventureHandlers<TAdventure>(IAdventureHandler handlerInstance) where TAdventure : Adventure
		{
			GameMaster.UnregisterExtraAdventureHandlers(typeof(TAdventure), handlerInstance);
		}
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
		public static void QuitGame()
		{
			GameMaster._quitConfirmed = true;
			Application.Quit();
		}
		private static void SetTurboMode(bool turboMode)
		{
			Time.timeScale = (turboMode ? 2f : 1f);
		}
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
		public static bool ShowAllCardsInMuseum { get; set; }
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
		private static Texture2D CursorTexture { get; set; }
		public static bool UseLbolCursor { get; private set; }
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
		public static GameMaster.GameMasterStatus Status { get; set; }
		public void OnGainMaxHp(int deltaMaxHp, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
			GameDirector.Player.OnMaxHpChanged();
		}
		public void OnLoseMaxHp(int deltaMaxHp, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
			GameDirector.Player.OnMaxHpChanged();
		}
		public void OnSetHpAndMaxHp(int hp, int maxHp, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
			GameDirector.Player.OnMaxHpChanged();
		}
		public void OnEnemySetHpAndMaxHp(int index, int hp, int maxHp, bool triggerVisual)
		{
			GameDirector.GetEnemyByRootIndex(index).OnMaxHpChanged();
		}
		public void OnDamage(DamageInfo damage, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnHpChanged();
			GameDirector.Player.OnDamageReceived(damage);
		}
		private static UnitView PlayerView
		{
			get
			{
				return Singleton<GameDirector>.Instance.PlayerUnitView;
			}
		}
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
		public void OnConsumeMoney(int value)
		{
			UiManager.GetPanel<SystemBoard>().OnMoneyChanged();
		}
		public void OnLoseMoney(int value)
		{
			UiManager.GetPanel<SystemBoard>().OnMoneyChanged();
		}
		public void OnGainPower(int value, bool triggerVisual)
		{
			UiManager.GetPanel<UltimateSkillPanel>().GainPower(value);
		}
		public void OnConsumePower(int value, bool triggerVisual)
		{
			UiManager.GetPanel<UltimateSkillPanel>().ConsumePower(value);
		}
		public void OnLosePower(int value, bool triggerVisual)
		{
			UiManager.GetPanel<UltimateSkillPanel>().LosePower(value);
		}
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
		public void OnUpgradeDeckCards(Card[] cards, bool triggerVisual)
		{
			UiManager.GetPanel<GameRunVisualPanel>().PlayUpgradeDeckCardsEffect(cards, 1f + (float)cards.Length * 0.3f);
		}
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
		public void OnLoseExhibit(Exhibit exhibit, bool triggerVisual)
		{
			UiManager.GetPanel<SystemBoard>().OnExhibitRemoved(exhibit);
		}
		public void OnSetBaseMana(ManaGroup mana, bool triggerVisual)
		{
		}
		public void OnGainBaseMana(ManaGroup mana, bool triggerVisual)
		{
		}
		public void OnLoseBaseMana(ManaGroup mana, bool triggerVisual)
		{
		}
		public void CustomVisual(string visualName, bool triggerVisual)
		{
		}
		public static string SysFileName
		{
			get
			{
				return "sys.sav";
			}
		}
		private static string GetProfileFileName(int index)
		{
			return string.Format("save{0}.sav", index);
		}
		private static string GetGameRunFileName(int index)
		{
			return string.Format("game{0}.sav", index);
		}
		private static string GetHistoryFileName(int index)
		{
			return string.Format("history{0}.sav", index);
		}
		public int? CurrentSaveIndex
		{
			get
			{
				return this._currentSaveIndex;
			}
		}
		public ProfileSaveData CurrentProfile
		{
			get
			{
				return this._currentProfileSaveData;
			}
		}
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
		public GameRunSaveData GameRunSaveData { get; private set; }
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
		public static void SaveSys()
		{
			SysSaveData sysSaveData = new SysSaveData
			{
				SaveIndex = Singleton<GameMaster>.Instance._currentSaveIndex,
				Locale = Localization.CurrentLocale.ToString()
			};
			GameMaster.WriteSaveData(GameMaster.SysFileName, SaveDataHelper.SerializeSys(sysSaveData, true));
		}
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
		public static void RevealCard(string cardId)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal card while profile is null");
			}
			GameMaster.RevealCardRecursive(currentProfile, cardId);
		}
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
		public static bool ShouldShowHint(string key)
		{
			ProfileSaveData currentProfile = Singleton<GameMaster>.Instance.CurrentProfile;
			if (currentProfile == null)
			{
				throw new InvalidOperationException("Cannot reveal enemy-group while profile is null");
			}
			return !currentProfile.HintStatus.ShownHints.Contains(key);
		}
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
		public static bool ShowDetailedHint
		{
			get
			{
				return GameMaster.HintLevel == HintLevel.Detailed;
			}
		}
		public static bool ShowBriefHint
		{
			get
			{
				HintLevel hintLevel = GameMaster.HintLevel;
				return hintLevel == HintLevel.Detailed || hintLevel == HintLevel.Brief;
			}
		}
		public static bool EnableKeyboard
		{
			get
			{
				return true;
			}
		}
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
		public static void ClearKeyboardBindings()
		{
			GameSettingsSaveData gameSettingsSaveData = Singleton<GameMaster>.Instance.TryGetGameSettings();
			gameSettingsSaveData.KeyboardBindings.Clear();
			Singleton<GameMaster>.Instance.SaveProfile();
			Singleton<GameMaster>.Instance.TriggerSettingsChanged(gameSettingsSaveData);
			UiManager.LoadKeyboardBindings(gameSettingsSaveData.KeyboardBindings);
		}
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
		public static string GetPreferredCardIllustrator(Card card)
		{
			return GameMaster.GetPreferredCardIllustrator(card.Id);
		}
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
		public static event Action<GameSettingsSaveData> SettingsChanged;
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
		private void ClickOnEirin()
		{
			Debug.Log("Eirin Clicked.");
		}
		private void ClickOnKaguya()
		{
			Debug.Log("Kaguya Clicked.");
		}
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
		private IEnumerator BattleStationFlowFromEndSave(BattleStation battleStation, SaveTiming saveTiming)
		{
			yield return this.EndStationFlow(battleStation, saveTiming == SaveTiming.AfterBossReward);
			yield break;
		}
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
		private IEnumerator GapStationFlow(GapStation gapStation)
		{
			yield return UiManager.GetPanel<GapOptionsPanel>().WaitUntilOptionSelected();
			yield return this.RunStationDialog(gapStation.PostDialogs);
			gapStation.Finish();
			yield break;
		}
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
		private void ClearUnitLayer()
		{
			GameDirector.ClearEnemies();
		}
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
		[CompilerGenerated]
		internal static UniTask <LoadSharedUiTo>g__LoadShared|70_0<T>(bool show = false, ref GameMaster.<>c__DisplayClass70_0 A_1) where T : UiPanelBase
		{
			A_1.ui.Add(typeof(T));
			return UiManager.LoadPanelAsync<T>(show);
		}
		[CompilerGenerated]
		internal static UniTask <LoadMainMenuUiAsync>g__Load|72_0<T>(bool show = false) where T : UiPanelBase
		{
			GameMaster.MainMenuUiList.Add(typeof(T));
			return UiManager.LoadPanelAsync<T>("MainMenu", show);
		}
		[CompilerGenerated]
		internal static UniTask <CoLoadGameRunUi>g__Load|75_0<T>(bool show = false) where T : UiPanelBase
		{
			GameMaster.GameRunUiList.Add(typeof(T));
			return UiManager.LoadPanelAsync<T>("GameRun", show);
		}
		private DateTime _currentGameRunStartTime;
		private static readonly List<Type> MainMenuUiList = new List<Type>();
		private static readonly List<Type> GameRunUiList = new List<Type>();
		private static readonly Dictionary<Type, List<IAdventureHandler>> ExtraAdventureHandlers = new Dictionary<Type, List<IAdventureHandler>>();
		private static bool _quitConfirmed;
		private static readonly Vector2 CursorHotpot = new Vector2(4f, 2f);
		private const string UseLbolCursorName = "UseLbolCursor";
		private const string AnimatingEnvironmentName = "IsAnimatingEnvironmentEnabled";
		private static bool _isAnimatingEnvironmentEnabled;
		public const string VsyncCountName = "VsyncCount";
		public const string TargetFrameRateName = "TargetFrameRate";
		private const bool SaveCompressed = true;
		private const int MaxHistory = 50;
		private int? _currentSaveIndex;
		[MaybeNull]
		private ProfileSaveData _currentProfileSaveData;
		private class GameStatisticData
		{
			public int BluePoint;
			public List<ScoreData> ScoreDatas;
			public float DifficultyMultipler;
			public int DebugExp;
		}
		public enum GameMasterStatus
		{
			GameEntry,
			MainMenu,
			InGame
		}
		public sealed class MainMenuInputActionHandler : IInputActionHandler
		{
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
			public void BeginSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(true);
			}
			public void EndSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(false);
			}
		}
		private sealed class GameRunInputActionHandler : IInputActionHandler
		{
			public void OnNavigate(NavigateDirection dir)
			{
				if (UiManager.GetPanel<VnPanel>().HandleNavigateAction(dir))
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleNavigateFromKey(dir);
			}
			public void OnConfirm()
			{
				if (UiManager.GetPanel<VnPanel>().HandleConfirmAction())
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleConfirmAction();
			}
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
			public void OnRightClickCancel()
			{
				if (UiManager.GetPanel<VnPanel>().HandleCancelAction())
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleCancelAction();
			}
			public void BeginSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(true);
			}
			public void EndSkipDialog()
			{
				UiManager.GetPanel<VnPanel>().UserSkipDialog(false);
			}
			public void BeginShowEnemyMoveOrder()
			{
				GameDirector.SetEnemyMoveOrderVisible(true);
			}
			public void EndShowEnemyMoveOrder()
			{
				GameDirector.SetEnemyMoveOrderVisible(false);
			}
			public void OnSelect(int i)
			{
				if (UiManager.GetPanel<VnPanel>().HandleSelectionFromKey(i))
				{
					return;
				}
				UiManager.GetPanel<PlayBoard>().HandleSelectionFromKey(i);
			}
			public void OnPoolMana(ManaColor color)
			{
				UiManager.GetPanel<PlayBoard>().HandlePoolMana(color);
			}
			public void OnUseUs()
			{
				UiManager.GetPanel<PlayBoard>().HandleUseUsFromKey();
			}
			public void OnEndTurn()
			{
				UiManager.GetPanel<PlayBoard>().HandleEndTurnFromKey();
			}
			public void OnToggleMap()
			{
				UiManager.GetPanel<SystemBoard>().ToggleMapPanel();
			}
			public void OnToggleBaseDeck()
			{
				UiManager.GetPanel<SystemBoard>().ShowBaseDeck();
			}
			public void OnToggleDrawZone()
			{
				UiManager.GetPanel<PlayBoard>().ShowDrawZone();
			}
			public void OnToggleDiscardZone()
			{
				UiManager.GetPanel<PlayBoard>().ShowDiscardZone();
			}
			public void OnToggleExileZone()
			{
				UiManager.GetPanel<PlayBoard>().ShowExileZone();
			}
			public void OnToggleMinimize()
			{
				if (UiManager.GetPanel<SelectCardPanel>().SwitchMinimized())
				{
					return;
				}
				UiManager.GetPanel<SelectBaseManaPanel>().SwitchMinimized();
			}
		}
		public enum GameRunSaveDataLoadResult
		{
			Success,
			NotFound,
			VersionMismatch,
			Failed
		}
	}
}
