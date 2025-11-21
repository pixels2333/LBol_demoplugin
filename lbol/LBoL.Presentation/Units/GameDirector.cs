using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.Effect;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;

namespace LBoL.Presentation.Units
{
	// Token: 0x02000018 RID: 24
	public class GameDirector : Singleton<GameDirector>
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060001AB RID: 427 RVA: 0x000087EE File Offset: 0x000069EE
		// (set) Token: 0x060001AC RID: 428 RVA: 0x000087F6 File Offset: 0x000069F6
		private Dictionary<string, EnemyFormation> Formations { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060001AD RID: 429 RVA: 0x000087FF File Offset: 0x000069FF
		// (set) Token: 0x060001AE RID: 430 RVA: 0x00008806 File Offset: 0x00006A06
		private static string ActiveFormation { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060001AF RID: 431 RVA: 0x0000880E File Offset: 0x00006A0E
		// (set) Token: 0x060001B0 RID: 432 RVA: 0x00008816 File Offset: 0x00006A16
		public UnitView PlayerUnitView { get; private set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060001B1 RID: 433 RVA: 0x0000881F File Offset: 0x00006A1F
		private List<UnitView> EnemyUnitViews { get; } = new List<UnitView>();

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060001B2 RID: 434 RVA: 0x00008827 File Offset: 0x00006A27
		public static UnitView Player
		{
			get
			{
				return Singleton<GameDirector>.Instance.PlayerUnitView;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060001B3 RID: 435 RVA: 0x00008833 File Offset: 0x00006A33
		public static IReadOnlyList<UnitView> Enemies
		{
			get
			{
				return Singleton<GameDirector>.Instance.EnemyUnitViews;
			}
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00008840 File Offset: 0x00006A40
		private void Awake()
		{
			this.Formations = new Dictionary<string, EnemyFormation>();
			foreach (object obj in this.unitRoot)
			{
				EnemyFormation component = ((Transform)obj).GetComponent<EnemyFormation>();
				if (component)
				{
					this.Formations.Add(component.name, component);
				}
			}
			GameDirector.SetTimeStep();
			GameDirector._cameraOffset = CameraController.MainCamera.transform.localPosition;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x000088D8 File Offset: 0x00006AD8
		public void EnterBattle(BattleController battle)
		{
			battle.ActionViewer.Register<WaitForCoroutineAction>(new BattleActionViewer<WaitForCoroutineAction>(GameDirector.WaitCoroutineViewer), null);
			battle.ActionViewer.Register<WaitForYieldInstructionAction>(new BattleActionViewer<WaitForYieldInstructionAction>(GameDirector.WaitYieldInstructionViewer), null);
			battle.ActionViewer.Register<SpawnEnemyAction>(new BattleActionViewer<SpawnEnemyAction>(GameDirector.SpawnViewer), null);
			battle.ActionViewer.Register<DamageAction>(new BattleActionViewer<DamageAction>(GameDirector.DamageActionViewer), null);
			battle.ActionViewer.Register<ExplodeAction>(new BattleActionViewer<ExplodeAction>(GameDirector.ExplodeActionViewer), null);
			battle.ActionViewer.Register<StatisticalTotalDamageAction>(new BattleActionViewer<StatisticalTotalDamageAction>(GameDirector.StatisticalTotalDamageViewer), null);
			battle.ActionViewer.Register<EndShootAction>(new BattleActionViewer<EndShootAction>(GameDirector.EndShootActionViewer), null);
			battle.ActionViewer.Register<HealAction>(new BattleActionViewer<HealAction>(GameDirector.HealActionViewer), null);
			battle.ActionViewer.Register<EnemyMoveAction>(new BattleActionViewer<EnemyMoveAction>(GameDirector.EnemyMoveViewer), null);
			battle.ActionViewer.Register<LoseAllExhibitsAction>(new BattleActionViewer<LoseAllExhibitsAction>(GameDirector.LoseAllExhibitsViewer), null);
			battle.ActionViewer.Register<PerformAction>(new BattleActionViewer<PerformAction>(GameDirector.PerformViewer), null);
			battle.ActionViewer.Register<CastBlockShieldAction>(new BattleActionViewer<CastBlockShieldAction>(GameDirector.CastBlockShieldViewer), null);
			battle.ActionViewer.Register<LoseBlockShieldAction>(new BattleActionViewer<LoseBlockShieldAction>(GameDirector.LoseBlockShieldViewer), null);
			battle.ActionViewer.Register<ForceKillAction>(new BattleActionViewer<ForceKillAction>(GameDirector.ForceKillViewer), null);
			battle.ActionViewer.Register<EscapeAction>(new BattleActionViewer<EscapeAction>(GameDirector.EscapeViewer), null);
			battle.ActionViewer.Register<DieAction>(new BattleActionViewer<DieAction>(GameDirector.MultipleDieViewer), null);
			battle.ActionViewer.Register<InstantWinAction>(new BattleActionViewer<InstantWinAction>(this.InstantWinViewer), null);
			battle.ActionViewer.Register<ApplyStatusEffectAction>(new BattleActionViewer<ApplyStatusEffectAction>(this.ApplyStatusEffectViewer), null);
			battle.ActionViewer.Register<RemoveStatusEffectAction>(new BattleActionViewer<RemoveStatusEffectAction>(this.RemoveStatusEffectViewer), null);
			battle.GlobalStatusChanged += new Action(this.OnGlobalStatusChanged);
			GameDirector.Player.EnterBattle(battle);
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00008ACC File Offset: 0x00006CCC
		public void LeaveBattle(BattleController battle)
		{
			battle.ActionViewer.Unregister<WaitForCoroutineAction>(new BattleActionViewer<WaitForCoroutineAction>(GameDirector.WaitCoroutineViewer));
			battle.ActionViewer.Unregister<WaitForYieldInstructionAction>(new BattleActionViewer<WaitForYieldInstructionAction>(GameDirector.WaitYieldInstructionViewer));
			battle.ActionViewer.Unregister<SpawnEnemyAction>(new BattleActionViewer<SpawnEnemyAction>(GameDirector.SpawnViewer));
			battle.ActionViewer.Unregister<DamageAction>(new BattleActionViewer<DamageAction>(GameDirector.DamageActionViewer));
			battle.ActionViewer.Unregister<ExplodeAction>(new BattleActionViewer<ExplodeAction>(GameDirector.ExplodeActionViewer));
			battle.ActionViewer.Unregister<StatisticalTotalDamageAction>(new BattleActionViewer<StatisticalTotalDamageAction>(GameDirector.StatisticalTotalDamageViewer));
			battle.ActionViewer.Unregister<EndShootAction>(new BattleActionViewer<EndShootAction>(GameDirector.EndShootActionViewer));
			battle.ActionViewer.Unregister<HealAction>(new BattleActionViewer<HealAction>(GameDirector.HealActionViewer));
			battle.ActionViewer.Unregister<EnemyMoveAction>(new BattleActionViewer<EnemyMoveAction>(GameDirector.EnemyMoveViewer));
			battle.ActionViewer.Unregister<LoseAllExhibitsAction>(new BattleActionViewer<LoseAllExhibitsAction>(GameDirector.LoseAllExhibitsViewer));
			battle.ActionViewer.Unregister<PerformAction>(new BattleActionViewer<PerformAction>(GameDirector.PerformViewer));
			battle.ActionViewer.Unregister<CastBlockShieldAction>(new BattleActionViewer<CastBlockShieldAction>(GameDirector.CastBlockShieldViewer));
			battle.ActionViewer.Unregister<LoseBlockShieldAction>(new BattleActionViewer<LoseBlockShieldAction>(GameDirector.LoseBlockShieldViewer));
			battle.ActionViewer.Unregister<ForceKillAction>(new BattleActionViewer<ForceKillAction>(GameDirector.ForceKillViewer));
			battle.ActionViewer.Unregister<EscapeAction>(new BattleActionViewer<EscapeAction>(GameDirector.EscapeViewer));
			battle.ActionViewer.Unregister<DieAction>(new BattleActionViewer<DieAction>(GameDirector.MultipleDieViewer));
			battle.ActionViewer.Unregister<InstantWinAction>(new BattleActionViewer<InstantWinAction>(this.InstantWinViewer));
			battle.ActionViewer.Unregister<ApplyStatusEffectAction>(new BattleActionViewer<ApplyStatusEffectAction>(this.ApplyStatusEffectViewer));
			battle.ActionViewer.Unregister<RemoveStatusEffectAction>(new BattleActionViewer<RemoveStatusEffectAction>(this.RemoveStatusEffectViewer));
			battle.GlobalStatusChanged -= new Action(this.OnGlobalStatusChanged);
			GameDirector.Player.LeaveBattle(battle);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00008CAC File Offset: 0x00006EAC
		private void OnGlobalStatusChanged()
		{
			if (this.PlayerUnitView)
			{
				this.PlayerUnitView.OnGlobalStatusChanged();
			}
			foreach (UnitView unitView in this.EnemyUnitViews)
			{
				if (unitView)
				{
					unitView.OnGlobalStatusChanged();
					unitView.UpdateIntentions();
				}
			}
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00008D24 File Offset: 0x00006F24
		public static void MovePlayer(Vector2 v2)
		{
			if (Singleton<GameDirector>.Instance.PlayerUnitView == null)
			{
				Debug.LogError("There's no PlayerUnit in UnitDirector.");
				return;
			}
			Singleton<GameDirector>.Instance.playerRoot.localPosition = v2;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00008D52 File Offset: 0x00006F52
		public static UniTask<UnitView> LoadPlayerAsync(PlayerUnit player, bool hidden = false)
		{
			return Singleton<GameDirector>.Instance.InternalLoadPlayerAsync(player, hidden);
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00008D60 File Offset: 0x00006F60
		private async UniTask<UnitView> InternalLoadPlayerAsync(PlayerUnit player, bool hidden)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.unitPrefab, this.playerRoot);
			UnitView playerView = gameObject.GetComponent<UnitView>();
			playerView.Unit = player;
			playerView.SetStatusWidget(UiManager.GetPanel<UnitStatusHud>().CreateStatusWidget(player), 1f);
			playerView.SetInfoWidget(UiManager.GetPanel<UnitStatusHud>().CreateInfoWidget(player), 1f);
			playerView.IsHidden = hidden;
			await playerView.LoadUnitModelAsync(player.ModelName, true, default(float?));
			player.SetView(playerView);
			playerView.SetPlayerHpBarLength(player.MaxHp);
			this.PlayerUnitView = playerView;
			return playerView;
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00008DB4 File Offset: 0x00006FB4
		public static UniTask<UnitView> LoadEnemyAsync(EnemyUnit enemy, string formationName = null, bool hidden = false, int? forceRootIndex = null)
		{
			if (formationName == null)
			{
				formationName = "Triangle";
			}
			EnemyFormation enemyFormation;
			if (Singleton<GameDirector>.Instance.Formations.TryGetValue(formationName, ref enemyFormation))
			{
				GameDirector.ActiveFormation = formationName;
				return Singleton<GameDirector>.Instance.InternalLoadEnemyAsync(enemy, enemyFormation, hidden, forceRootIndex);
			}
			GameDirector.ActiveFormation = "Triangle";
			Debug.LogWarning("Formation name '" + formationName + "' not found, using default.");
			return Singleton<GameDirector>.Instance.InternalLoadEnemyAsync(enemy, Singleton<GameDirector>.Instance.Formations["Triangle"], hidden, forceRootIndex);
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00008E34 File Offset: 0x00007034
		public static void ClearEnemies()
		{
			Singleton<GameDirector>.Instance.InternalClearEnemies();
		}

		// Token: 0x060001BD RID: 445 RVA: 0x00008E40 File Offset: 0x00007040
		public static void ClearAll()
		{
			Singleton<GameDirector>.Instance.InternalClearAll();
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00008E4C File Offset: 0x0000704C
		private async UniTask<UnitView> InternalLoadEnemyAsync(EnemyUnit enemy, EnemyFormation formation, bool hidden, int? forceRootIndex)
		{
			int rootIndex = forceRootIndex ?? enemy.RootIndex;
			if (rootIndex < 0 || rootIndex >= formation.enemyLocations.Count)
			{
				throw new ArgumentException(string.Format("Invalid root-index: {0} (formation size: {1})", rootIndex, formation.enemyLocations.Count), "rootIndex");
			}
			GameObject gameObject = Object.Instantiate<GameObject>(this.unitPrefab, Enumerable.ToList<Transform>(formation.enemyLocations.Values)[rootIndex]);
			UnitView enemyView = gameObject.GetComponent<UnitView>();
			enemyView.Unit = enemy;
			enemyView.SetStatusWidget(UiManager.GetPanel<UnitStatusHud>().CreateStatusWidget(enemy), 0f);
			enemyView.SetInfoWidget(UiManager.GetPanel<UnitStatusHud>().CreateInfoWidget(enemy), (float)(hidden ? 0 : 1));
			enemyView.IsHidden = hidden;
			float? hpLength = EnemyUnitConfig.FromId(enemy.Id).HpLength;
			await enemyView.LoadUnitModelAsync(enemy.ModelName, false, hpLength);
			enemy.SetView(enemyView);
			this.EnemyUnitViews.Add(enemyView);
			enemyView.RootIndex = rootIndex;
			enemyView.AngleIndex = Enumerable.ToList<int>(formation.enemyLocations.Keys)[rootIndex];
			return enemyView;
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00008EB0 File Offset: 0x000070B0
		public static async UniTask<UnitView> LoadLoreCharacterAsync(Type characterType, Action clickAction, int index = 0)
		{
			EnemyUnit enemyUnit = Library.CreateEnemyUnit(characterType);
			GameDirector.MovePlayer(GameDirector.PlayerPosition);
			Singleton<GameDirector>.Instance.lore0.localPosition = GameDirector.EirinPosition;
			Singleton<GameDirector>.Instance.lore0.localScale = Vector3.one;
			Singleton<GameDirector>.Instance.lore1.localPosition = GameDirector.KaguyaPosition;
			Singleton<GameDirector>.Instance.lore1.localScale = Vector3.one;
			object obj = await GameDirector.LoadEnemyAsync(enemyUnit, "Lore", false, new int?(index));
			obj.ClickHandler = clickAction;
			return obj;
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00008F04 File Offset: 0x00007104
		private void InternalClearEnemies()
		{
			foreach (UnitView unitView in this.EnemyUnitViews)
			{
				Object.Destroy(unitView.gameObject);
			}
			this.EnemyUnitViews.Clear();
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00008F64 File Offset: 0x00007164
		private void InternalClearAll()
		{
			GameDirector.StopLoreChat();
			Object.Destroy(this.PlayerUnitView.gameObject);
			this.PlayerUnitView = null;
			this.InternalClearEnemies();
			GameDirector.ClearDoremyEnvironment();
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00008F90 File Offset: 0x00007190
		public static void HidePlayer()
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.IsHidden = true;
			}
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00008FB8 File Offset: 0x000071B8
		public static void HideAll()
		{
			GameDirector.HidePlayer();
			foreach (UnitView unitView in Singleton<GameDirector>.Instance.EnemyUnitViews)
			{
				unitView.IsHidden = true;
			}
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00009014 File Offset: 0x00007214
		public static void RevealPlayer(bool withStatus)
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.Show(withStatus);
			}
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000903B File Offset: 0x0000723B
		public static void RevealAll(bool withStatus)
		{
			GameDirector.RevealPlayer(withStatus);
			GameDirector.RevealAllEnemies(withStatus);
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000904C File Offset: 0x0000724C
		public static void RevealAllEnemies(bool withStatus)
		{
			foreach (UnitView unitView in Singleton<GameDirector>.Instance.EnemyUnitViews)
			{
				unitView.Show(withStatus);
			}
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x000090A4 File Offset: 0x000072A4
		public static void RevealEnemy(int index, bool withStatus)
		{
			UnitView enemy = GameDirector.GetEnemy(index);
			if (enemy != null)
			{
				enemy.Show(withStatus);
			}
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x000090C8 File Offset: 0x000072C8
		public static void RevealEnemy(EnemyUnit enemy, bool withStatus)
		{
			UnitView enemy2 = GameDirector.GetEnemy(enemy);
			if (enemy2 != null)
			{
				enemy2.Show(withStatus);
			}
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x000090EC File Offset: 0x000072EC
		public static void FadeInEnemyStatus()
		{
			foreach (UnitView unitView in Singleton<GameDirector>.Instance.EnemyUnitViews)
			{
				unitView.SetStatusVisible(true, false);
			}
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00009144 File Offset: 0x00007344
		public static void FadeInPlayerStatus()
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.SetStatusVisible(true, false);
			}
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000916C File Offset: 0x0000736C
		public static void FadeOutPlayerStatus()
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.SetStatusVisible(false, false);
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00009194 File Offset: 0x00007394
		private static void UpdateEnemyMoveOrder()
		{
			if (GameDirector._enemyMoveOrderVisible)
			{
				foreach (ValueTuple<int, UnitView> valueTuple in Enumerable.ToList<UnitView>(Enumerable.Select(Enumerable.ThenBy(Enumerable.OrderBy(Enumerable.Where(Enumerable.Select(GameDirector.Enemies, (UnitView enemyView) => new
				{
					enemyView = enemyView,
					enemy = (EnemyUnit)enemyView.Unit
				}), <>h__TransparentIdentifier0 => <>h__TransparentIdentifier0.enemy.IsAlive), <>h__TransparentIdentifier0 => <>h__TransparentIdentifier0.enemy.MovePriority), <>h__TransparentIdentifier0 => <>h__TransparentIdentifier0.enemy.RootIndex), <>h__TransparentIdentifier0 => <>h__TransparentIdentifier0.enemyView)).WithIndices<UnitView>())
				{
					int item = valueTuple.Item1;
					valueTuple.Item2.SetMoveOrder(item + 1);
				}
			}
		}

		// Token: 0x060001CD RID: 461 RVA: 0x000092B4 File Offset: 0x000074B4
		public static void SetEnemyMoveOrderVisible(bool visible)
		{
			GameDirector._enemyMoveOrderVisible = visible;
			if (visible)
			{
				GameDirector.UpdateEnemyMoveOrder();
				using (IEnumerator<UnitView> enumerator = GameDirector.Enemies.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						UnitView unitView = enumerator.Current;
						if (unitView.Unit.IsAlive)
						{
							unitView.SetMoveOrderVisible(true);
						}
					}
					return;
				}
			}
			foreach (UnitView unitView2 in GameDirector.Enemies)
			{
				unitView2.SetMoveOrderVisible(false);
			}
		}

		// Token: 0x060001CE RID: 462 RVA: 0x00009354 File Offset: 0x00007554
		public static UnitView GetUnit(Unit unit)
		{
			if (unit == GameDirector.Player.Unit)
			{
				return GameDirector.Player;
			}
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				return GameDirector.GetEnemy(enemyUnit);
			}
			return null;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00009386 File Offset: 0x00007586
		public static UnitView GetEnemy(int index)
		{
			return Singleton<GameDirector>.Instance.EnemyUnitViews.TryGetValue(index);
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00009398 File Offset: 0x00007598
		public static UnitView GetEnemyByRootIndex(int rootIndex)
		{
			return Enumerable.FirstOrDefault<UnitView>(Singleton<GameDirector>.Instance.EnemyUnitViews, (UnitView enemy) => enemy.RootIndex == rootIndex);
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x000093D0 File Offset: 0x000075D0
		public static UnitView GetEnemy(EnemyUnit enemy)
		{
			return Enumerable.FirstOrDefault<UnitView>(Singleton<GameDirector>.Instance.EnemyUnitViews, (UnitView e) => e.Unit == enemy);
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00009405 File Offset: 0x00007605
		public static void PlayerDebutAnimation()
		{
			GameDirector.Player.DebutAnimation();
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00009411 File Offset: 0x00007611
		public static void EnemyDebutAnimation(int index)
		{
			UnitView enemy = GameDirector.GetEnemy(index);
			if (enemy == null)
			{
				return;
			}
			enemy.DebutAnimation();
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x00009423 File Offset: 0x00007623
		public static void EnemyDebutAnimation(EnemyUnit enemy)
		{
			UnitView enemy2 = GameDirector.GetEnemy(enemy);
			if (enemy2 == null)
			{
				return;
			}
			enemy2.DebutAnimation();
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x00009438 File Offset: 0x00007638
		public static void AllEnemiesDebutAnimation()
		{
			foreach (UnitView unitView in GameDirector.Enemies)
			{
				unitView.DebutAnimation();
			}
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x00009484 File Offset: 0x00007684
		public static IEnumerable<UnitView> EnumeratePotentialTargets(TargetType targetType)
		{
			if (targetType == TargetType.All || targetType == TargetType.Self)
			{
				yield return GameDirector.Player;
			}
			if (targetType == TargetType.All || targetType == TargetType.SingleEnemy || targetType == TargetType.RandomEnemy || targetType == TargetType.AllEnemies)
			{
				foreach (UnitView unitView in GameDirector.Enemies)
				{
					if (unitView.Unit.IsAlive)
					{
						yield return unitView;
					}
				}
				IEnumerator<UnitView> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060001D7 RID: 471 RVA: 0x00009494 File Offset: 0x00007694
		// (set) Token: 0x060001D8 RID: 472 RVA: 0x0000949B File Offset: 0x0000769B
		private static Coroutine LoreChatRunner { get; set; }

		// Token: 0x060001D9 RID: 473 RVA: 0x000094A3 File Offset: 0x000076A3
		public static void DebutChat(bool hasKaguya, string playerName)
		{
			GameDirector.LoreChatRunner = Singleton<GameDirector>.Instance.StartCoroutine(GameDirector.DebutFlow(hasKaguya, playerName));
		}

		// Token: 0x060001DA RID: 474 RVA: 0x000094BB File Offset: 0x000076BB
		private static IEnumerator DebutFlow(bool hasKaguya, string playerName)
		{
			UnitView eirin = GameDirector.Enemies[0];
			yield return new WaitForSeconds(2f);
			if (hasKaguya)
			{
				UnitView kaguya = GameDirector.Enemies[1];
				List<string> kaguyaChats = Enumerable.ToList<string>("Debut.KaguyaChats".LocalizeStrings(true));
				Transform erinTrans = Singleton<GameDirector>.Instance.lore0;
				Transform kaguyaTrans = Singleton<GameDirector>.Instance.lore1;
				erinTrans.DOKill(true);
				erinTrans.DOScaleX(-1f, 0.3f);
				kaguya.Chat(kaguyaChats[0], 3f, ChatWidget.CloudType.RightTalk, 0f);
				yield return new WaitForSecondsRealtime(3.5f);
				kaguya.Chat(kaguyaChats[1], 3f, ChatWidget.CloudType.RightTalk, 0f);
				yield return new WaitForSecondsRealtime(3.1f);
				kaguyaTrans.DOKill(true);
				DOTween.Sequence().Append(kaguyaTrans.DOScaleX(-1f, 0.2f)).Append(kaguyaTrans.DOLocalMoveX(5f, 2f, false).SetRelative(true).SetEase(Ease.InSine))
					.SetUpdate(true)
					.SetAutoKill(true);
				yield return new WaitForSecondsRealtime(0.4f);
				eirin.Chat(kaguyaChats[2], 3f, ChatWidget.CloudType.LeftTalk, 0f);
				yield return new WaitForSecondsRealtime(4f);
				erinTrans.DOScaleX(1f, 0.3f);
				yield return new WaitForSecondsRealtime(0.5f);
				kaguya = null;
				kaguyaChats = null;
				erinTrans = null;
				kaguyaTrans = null;
			}
			List<string> eirinChats = Enumerable.ToList<string>("Debut.EirinChats".LocalizeStrings(true));
			eirinChats[0] = string.Format(eirinChats[0], playerName);
			for (;;)
			{
				foreach (string text in eirinChats)
				{
					eirin.Chat(text, 3f, ChatWidget.CloudType.RightTalk, 0f);
					yield return new WaitForSecondsRealtime(7f);
				}
				List<string>.Enumerator enumerator = default(List<string>.Enumerator);
			}
			yield break;
			yield break;
		}

		// Token: 0x060001DB RID: 475 RVA: 0x000094D1 File Offset: 0x000076D1
		public static void ShopChat()
		{
			GameDirector.LoreChatRunner = Singleton<GameDirector>.Instance.StartCoroutine(GameDirector.ShopFlow());
		}

		// Token: 0x060001DC RID: 476 RVA: 0x000094E7 File Offset: 0x000076E7
		private static IEnumerator ShopFlow()
		{
			UnitView takane = GameDirector.Enemies[0];
			takane.PlayAnimation("debut");
			IList<string> list = "Shop.Chats".LocalizeStrings(true);
			if (list.Count == 0)
			{
				yield break;
			}
			yield return new WaitForSecondsRealtime(2f);
			for (;;)
			{
				foreach (string text in list)
				{
					takane.Chat(text, 3f, ChatWidget.CloudType.RightTalk, 0f);
					yield return new WaitForSecondsRealtime(7f);
				}
				IEnumerator<string> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x000094EF File Offset: 0x000076EF
		public static void StopLoreChat()
		{
			if (GameDirector.LoreChatRunner != null)
			{
				Singleton<GameDirector>.Instance.StopCoroutine(GameDirector.LoreChatRunner);
			}
			GameDirector.LoreChatRunner = null;
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000950D File Offset: 0x0000770D
		private static IEnumerator WaitCoroutineViewer(WaitForCoroutineAction action)
		{
			yield return action.Coroutine;
			yield break;
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000951C File Offset: 0x0000771C
		private static IEnumerator WaitYieldInstructionViewer(WaitForYieldInstructionAction action)
		{
			yield return action.Instruction;
			yield break;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000952C File Offset: 0x0000772C
		private static IEnumerator DamageActionViewer(DamageAction action)
		{
			DamageEventArgs[] damageArgs = action.DamageArgs;
			Unit source = Enumerable.First<DamageEventArgs>(damageArgs).Source;
			UnitView unit = GameDirector.GetUnit(source);
			if (source != null && unit == null)
			{
				Debug.LogWarning("Damaging source " + source.DebugName + " not found in GameDirector");
				return null;
			}
			List<ValueTuple<UnitView, DamageInfo>> list = new List<ValueTuple<UnitView, DamageInfo>>();
			foreach (DamageEventArgs damageEventArgs in damageArgs)
			{
				UnitView unit2 = GameDirector.GetUnit(damageEventArgs.Target);
				if (damageEventArgs.Target == null)
				{
					Debug.LogWarning("Damaging target is null");
				}
				else if (unit2 == null)
				{
					Debug.LogWarning("Damaging target " + damageEventArgs.Target.DebugName + " not found in GameDirector");
				}
				else
				{
					list.Add(new ValueTuple<UnitView, DamageInfo>(unit2, damageEventArgs.DamageInfo));
				}
			}
			ActionCause cause = action.Cause;
			if (cause == ActionCause.Card || cause == ActionCause.Us || cause == ActionCause.EnemyAction || cause == ActionCause.StatusEffect || cause == ActionCause.Exhibit || cause == ActionCause.JadeBox || cause == ActionCause.Unit)
			{
				string text = action.GunName;
				if (action.GunName == null)
				{
					string text2 = "Action [";
					GameEntity source2 = action.Source;
					Debug.LogWarning(text2 + (((source2 != null) ? source2.DebugName : null) ?? "<null>") + "]: GunName is null");
					text = "Empty";
				}
				else if (text.IsNullOrEmpty() || text == "Empty")
				{
					string text3 = "Action [";
					GameEntity source3 = action.Source;
					Debug.LogWarning(text3 + (((source3 != null) ? source3.DebugName : null) ?? "<null>") + "]: GunName is empty");
				}
				return GameDirector.GunShootAction(unit, list, text, action.GunType);
			}
			Doll doll = action.Source as Doll;
			if (doll == null)
			{
				foreach (ValueTuple<UnitView, DamageInfo> valueTuple in list)
				{
					UnitView item = valueTuple.Item1;
					DamageInfo item2 = valueTuple.Item2;
					if (item.Unit is PlayerUnit)
					{
						UiManager.GetPanel<SystemBoard>().OnHpChanged();
					}
					item.OnDamageReceived(item2);
				}
				return null;
			}
			DollView doll2 = unit.GetDoll(doll);
			if (doll2)
			{
				string text4 = action.GunName;
				if (action.GunName == null)
				{
					string text5 = "Action [";
					GameEntity source4 = action.Source;
					Debug.LogWarning(text5 + (((source4 != null) ? source4.DebugName : null) ?? "<null>") + "]: GunName is null");
					text4 = "Empty";
				}
				else if (text4.IsNullOrEmpty() || text4 == "Empty")
				{
					string text6 = "Action [";
					GameEntity source5 = action.Source;
					Debug.LogWarning(text6 + (((source5 != null) ? source5.DebugName : null) ?? "<null>") + "]: GunName is empty");
				}
				return GameDirector.GunShootAction(doll2, list, text4, action.GunType);
			}
			return null;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00009808 File Offset: 0x00007A08
		private static IEnumerator ExplodeActionViewer(ExplodeAction action)
		{
			DamageEventArgs[] damageArgs = action.DamageArgs;
			Unit source = Enumerable.First<DamageEventArgs>(damageArgs).Source;
			UnitView unit = GameDirector.GetUnit(source);
			if (source != null && unit == null)
			{
				Debug.LogWarning("Damaging source " + source.DebugName + " not found in GameDirector");
			}
			List<ValueTuple<UnitView, DamageInfo>> list = new List<ValueTuple<UnitView, DamageInfo>>();
			foreach (DamageEventArgs damageEventArgs in damageArgs)
			{
				UnitView unit2 = GameDirector.GetUnit(damageEventArgs.Target);
				if (damageEventArgs.Target == null)
				{
					Debug.LogWarning("Damaging target is null");
				}
				else if (unit2 == null)
				{
					Debug.LogWarning("Damaging target " + damageEventArgs.Target.DebugName + " not found in GameDirector");
				}
				else
				{
					list.Add(new ValueTuple<UnitView, DamageInfo>(unit2, damageEventArgs.DamageInfo));
				}
			}
			if (action.Cause != ActionCause.Card && action.Cause != ActionCause.Us && action.Cause != ActionCause.EnemyAction && action.Cause != ActionCause.StatusEffect)
			{
				foreach (ValueTuple<UnitView, DamageInfo> valueTuple in list)
				{
					UnitView item = valueTuple.Item1;
					DamageInfo item2 = valueTuple.Item2;
					if (item.Unit is PlayerUnit)
					{
						UiManager.GetPanel<SystemBoard>().OnHpChanged();
					}
					item.OnDamageReceived(item2);
				}
				return null;
			}
			string text = action.GunName;
			if (action.GunName == null)
			{
				string text2 = "Action [";
				GameEntity source2 = action.Source;
				Debug.LogWarning(text2 + (((source2 != null) ? source2.DebugName : null) ?? "<null>") + "]: GunName is null");
				text = "Empty";
			}
			else if (text.IsNullOrEmpty() || text == "Empty")
			{
				string text3 = "Action [";
				GameEntity source3 = action.Source;
				Debug.LogWarning(text3 + (((source3 != null) ? source3.DebugName : null) ?? "<null>") + "]: GunName is empty");
			}
			GunType gunType = action.GunType;
			Unit unit3 = action.DieArgs.Unit;
			int i = action.DieArgs.Power;
			int bluePoint = action.DieArgs.BluePoint;
			int num = i;
			int num2 = bluePoint;
			return GameDirector.<ExplodeActionViewer>g__InternalExplodeViewer|76_0(unit, list, text, gunType, unit3, num, num2);
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x00009A38 File Offset: 0x00007C38
		private static IEnumerator GunShootAction(UnitView source, [TupleElementNames(new string[] { "target", "damageInfo" })] IList<ValueTuple<UnitView, DamageInfo>> pairs, string gunName, GunType type)
		{
			source.Targets = Enumerable.ToList<UnitView>(Enumerable.Select<ValueTuple<UnitView, DamageInfo>, UnitView>(pairs, ([TupleElementNames(new string[] { "target", "damageInfo" })] ValueTuple<UnitView, DamageInfo> p) => p.Item1));
			int count = source.Targets.Count;
			if (count <= 2)
			{
				source.Target = Enumerable.First<UnitView>(source.Targets);
			}
			else
			{
				int num = (count - 1) / 2;
				List<UnitView> list = Enumerable.ToList<UnitView>(source.Targets);
				List<UnitView> list2 = new List<UnitView>();
				for (int i = 0; i <= num; i++)
				{
					UnitView smallOne = Enumerable.First<UnitView>(list);
					IEnumerable<UnitView> enumerable = list;
					Func<UnitView, bool> func;
					Func<UnitView, bool> <>9__2;
					if ((func = <>9__2) == null)
					{
						func = (<>9__2 = (UnitView u) => u.AngleIndex < smallOne.AngleIndex);
					}
					foreach (UnitView unitView in Enumerable.Where<UnitView>(enumerable, func))
					{
						smallOne = unitView;
					}
					list2.Add(smallOne);
					list.Remove(smallOne);
				}
				source.Target = list2[num];
			}
			foreach (ValueTuple<UnitView, DamageInfo> valueTuple in pairs)
			{
				UnitView item = valueTuple.Item1;
				DamageInfo item2 = valueTuple.Item2;
				item.ComingDamage = item2;
			}
			GameDirector._gunHitArgs = new GunHitArgs(source == GameDirector.Player, pairs, gunName);
			yield return source.Shoot(gunName, type);
			yield return new WaitWhile(() => GameDirector._someoneCrashing);
			yield break;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00009A5C File Offset: 0x00007C5C
		private static IEnumerator GunShootAction(DollView doll, [TupleElementNames(new string[] { "target", "damageInfo" })] IList<ValueTuple<UnitView, DamageInfo>> pairs, string gunName, GunType type)
		{
			List<UnitView> list = Enumerable.ToList<UnitView>(Enumerable.Select<ValueTuple<UnitView, DamageInfo>, UnitView>(pairs, ([TupleElementNames(new string[] { "target", "damageInfo" })] ValueTuple<UnitView, DamageInfo> p) => p.Item1));
			UnitView unitView = Enumerable.First<UnitView>(list);
			if (list.Count > 2)
			{
				int num = (list.Count - 1) / 2;
				List<UnitView> list2 = Enumerable.ToList<UnitView>(list);
				List<UnitView> list3 = new List<UnitView>();
				for (int i = 0; i <= num; i++)
				{
					UnitView smallOne = Enumerable.First<UnitView>(list2);
					IEnumerable<UnitView> enumerable = list2;
					Func<UnitView, bool> func;
					Func<UnitView, bool> <>9__2;
					if ((func = <>9__2) == null)
					{
						func = (<>9__2 = (UnitView u) => u.AngleIndex < smallOne.AngleIndex);
					}
					foreach (UnitView unitView2 in Enumerable.Where<UnitView>(enumerable, func))
					{
						smallOne = unitView2;
					}
					list3.Add(smallOne);
					list2.Remove(smallOne);
				}
				unitView = list3[num];
			}
			foreach (ValueTuple<UnitView, DamageInfo> valueTuple in pairs)
			{
				UnitView item = valueTuple.Item1;
				DamageInfo item2 = valueTuple.Item2;
				item.ComingDamage = item2;
			}
			GameDirector._gunHitArgs = new GunHitArgs(true, pairs, gunName);
			yield return doll.Shoot(gunName, type, unitView, list);
			yield return new WaitWhile(() => GameDirector._someoneCrashing);
			yield break;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00009A80 File Offset: 0x00007C80
		public static void OnGunHit()
		{
			GameDirector.GunHitPresentation(GameDirector._gunHitArgs);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x00009A8C File Offset: 0x00007C8C
		private static void GunHitPresentation(GunHitArgs gunHitArgs)
		{
			float num = 0f;
			float num2 = 1f;
			GunConfig gunConfig = GunConfig.FromName(gunHitArgs.GunName);
			if (gunConfig != null)
			{
				num2 = gunConfig.ShakePower;
			}
			foreach (ValueTuple<UnitView, DamageInfo> valueTuple in gunHitArgs.Pairs)
			{
				UnitView item = valueTuple.Item1;
				DamageInfo item2 = valueTuple.Item2;
				if (num < item2.Amount)
				{
					num = item2.Amount;
				}
				item.HitEnd();
				if (item.Unit is PlayerUnit)
				{
					UiManager.GetPanel<SystemBoard>().OnHpChanged();
				}
				item.OnDamageReceived(item2);
				PopupHud.Instance.DamagePopupFromScene(item2, item.transform.position, gunHitArgs.SourceIsPlayer);
			}
			if (GameMaster.Shake)
			{
				GameDirector.ShakeWithDamage(num, num2);
			}
			if (Enumerable.Any<ValueTuple<UnitView, DamageInfo>>(gunHitArgs.Pairs, ([TupleElementNames(new string[] { "target", "damageInfo" })] ValueTuple<UnitView, DamageInfo> pair) => pair.Item1.WillCrash))
			{
				Singleton<GameDirector>.Instance.StartCoroutine(GameDirector.CrashingTimer());
			}
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x00009BA4 File Offset: 0x00007DA4
		private static IEnumerator CrashingTimer()
		{
			GameDirector._someoneCrashing = true;
			yield return new WaitForSeconds(0.3f);
			GameDirector._someoneCrashing = false;
			yield break;
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060001E7 RID: 487 RVA: 0x00009BAC File Offset: 0x00007DAC
		// (set) Token: 0x060001E8 RID: 488 RVA: 0x00009BB3 File Offset: 0x00007DB3
		private static int ShakingDamage { get; set; }

		// Token: 0x060001E9 RID: 489 RVA: 0x00009BBB File Offset: 0x00007DBB
		private static void ShakeWithDamage(float damage, float shakePower)
		{
			GameDirector.ShakeWithDamage((damage * shakePower).ToInt());
		}

		// Token: 0x060001EA RID: 490 RVA: 0x00009BCC File Offset: 0x00007DCC
		private static void ShakeWithDamage(int damage)
		{
			GameDirector.ShakingDamage += damage;
			if (GameDirector.ShakingDamage >= GameDirector.ShakeFloors[4])
			{
				GameDirector.Shake(4, true);
				return;
			}
			if (GameDirector.ShakingDamage >= GameDirector.ShakeFloors[3])
			{
				GameDirector.Shake(3, true);
				return;
			}
			if (GameDirector.ShakingDamage >= GameDirector.ShakeFloors[2])
			{
				GameDirector.Shake(2, true);
				return;
			}
			if (GameDirector.ShakingDamage >= GameDirector.ShakeFloors[1])
			{
				GameDirector.Shake(1, true);
				return;
			}
			if (GameDirector.ShakingDamage >= GameDirector.ShakeFloors[0])
			{
				GameDirector.Shake(0, true);
			}
		}

		// Token: 0x060001EB RID: 491 RVA: 0x00009C68 File Offset: 0x00007E68
		public static void Shake(int level = 0, bool withDamage = true)
		{
			if (level >= 4)
			{
				GameDirector.ShakeHandler(0.5f, 4f, withDamage);
				return;
			}
			switch (level)
			{
			case 0:
				GameDirector.ShakeHandler(0.3f, 1f, withDamage);
				return;
			case 1:
				GameDirector.ShakeHandler(0.3f, 1.5f, withDamage);
				return;
			case 2:
				GameDirector.ShakeHandler(0.4f, 2f, withDamage);
				return;
			case 3:
				GameDirector.ShakeHandler(0.4f, 3f, withDamage);
				return;
			default:
				return;
			}
		}

		// Token: 0x060001EC RID: 492 RVA: 0x00009CE4 File Offset: 0x00007EE4
		private static void ShakeHandler(float duration, float strength, bool withDamage)
		{
			Transform trans = CameraController.MainCamera.transform;
			trans.DOKill(false);
			trans.localPosition = GameDirector._cameraOffset;
			trans.DOShakePosition(duration, new Vector3(strength, strength) * 0.08f, 10, 90f, false, true, ShakeRandomnessMode.Full).SetUpdate(true).OnComplete(delegate
			{
				trans.localPosition = GameDirector._cameraOffset;
				if (withDamage)
				{
					GameDirector.ShakingDamage = 0;
				}
			});
		}

		// Token: 0x060001ED RID: 493 RVA: 0x00009D6C File Offset: 0x00007F6C
		public static void ShakeUi(int level = 0)
		{
			if (level >= 4)
			{
				GameDirector.ShakeUiHandler(0.5f, 800f);
				return;
			}
			switch (level)
			{
			case 0:
				GameDirector.ShakeUiHandler(0.3f, 200f);
				return;
			case 1:
				GameDirector.ShakeUiHandler(0.3f, 300f);
				return;
			case 2:
				GameDirector.ShakeUiHandler(0.4f, 400f);
				return;
			case 3:
				GameDirector.ShakeUiHandler(0.4f, 600f);
				return;
			default:
				return;
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x00009DE4 File Offset: 0x00007FE4
		private static void ShakeUiHandler(float duration, float strength)
		{
			RectTransform trans = CameraController.UiRoot;
			trans.DOKill(false);
			trans.localPosition = Vector3.zero;
			trans.DOShakePosition(duration, new Vector3(strength, strength) * 0.08f, 10, 90f, false, true, ShakeRandomnessMode.Full).SetUpdate(true).OnComplete(delegate
			{
				trans.localPosition = Vector3.zero;
			});
		}

		// Token: 0x060001EF RID: 495 RVA: 0x00009E5E File Offset: 0x0000805E
		private static IEnumerator StatisticalTotalDamageViewer(StatisticalTotalDamageAction action)
		{
			GameRunController gameRun = action.Battle.GameRun;
			if (gameRun.IsAutoSeed && gameRun.JadeBoxes.Empty<JadeBox>())
			{
				Unit unit;
				IReadOnlyList<DamageEventArgs> readOnlyList;
				if (action.Source is YinyangCardBase)
				{
					foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in action.Args.ArgsTable)
					{
						keyValuePair.Deconstruct(ref unit, ref readOnlyList);
						Unit unit2 = unit;
						IReadOnlyList<DamageEventArgs> readOnlyList2 = readOnlyList;
						if (unit2.IsDead)
						{
							int num = 0;
							foreach (DamageEventArgs damageEventArgs in readOnlyList2)
							{
								num += damageEventArgs.DamageInfo.Amount.RoundToInt(1);
							}
							if (num >= 100)
							{
								GameMaster.UnlockAchievement(AchievementKey.Yinyang);
								break;
							}
						}
					}
				}
				if (action.Source is Card)
				{
					int num2 = 0;
					foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in action.Args.ArgsTable)
					{
						keyValuePair.Deconstruct(ref unit, ref readOnlyList);
						foreach (DamageEventArgs damageEventArgs2 in readOnlyList)
						{
							num2 += damageEventArgs2.DamageInfo.Amount.RoundToInt(1);
						}
					}
					if (num2 >= 300)
					{
						GameMaster.UnlockAchievement(AchievementKey.Starlight);
					}
				}
			}
			yield break;
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x00009E6D File Offset: 0x0000806D
		private static IEnumerator EndShootActionViewer(EndShootAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.SourceUnit);
			if (unit != null)
			{
				yield return unit.ForceEndShoot();
			}
			yield break;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x00009E7C File Offset: 0x0000807C
		private static IEnumerator HealActionViewer(HealAction action)
		{
			HealEventArgs args = action.Args;
			UnitView unit = GameDirector.GetUnit(args.Source);
			UnitView target = GameDirector.GetUnit(args.Target);
			if (unit == null)
			{
				Debug.LogWarning("Healing source " + args.Source.Name + " not found in GameDirector");
				yield break;
			}
			if (target == null)
			{
				Debug.LogWarning("Healing target " + args.Target.Name + " not found in GameDirector");
				yield break;
			}
			int healAmount = args.Amount.ToInt();
			bool large = healAmount > 12;
			if (args.HealType == HealType.Vampire)
			{
				Vector3 position = unit.transform.position;
				for (int i = 0; i < 2; i++)
				{
					EffectManager.CreateEffectBullet(new VampireBlood
					{
						TargetPosition = target.transform.position
					}, position, null);
				}
				yield return new WaitForSeconds(1.2f);
			}
			PopupHud.Instance.HealPopupFromScene(healAmount, target.transform.position);
			EffectManager.CreateEffect(large ? "UnitHealLarge" : "UnitHeal", target.EffectRoot, true);
			AudioManager.PlayUi(large ? "HealLarge" : "Heal", false);
			if (target.Unit is PlayerUnit)
			{
				UiManager.GetPanel<SystemBoard>().OnHpChanged();
			}
			target.OnHealingReceived(args.Amount.ToInt());
			yield return new WaitForSeconds(action.WaitTime);
			yield break;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x00009E8B File Offset: 0x0000808B
		private static IEnumerator EnemyMoveViewer(EnemyMoveAction action)
		{
			EnemyUnit enemy = action.Enemy;
			string moveName = action.MoveName;
			UnitView enemy2 = GameDirector.GetEnemy(enemy);
			if (enemy2)
			{
				enemy2.ShowMovePopup(moveName, action.CloseMoveName);
			}
			yield break;
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x00009E9A File Offset: 0x0000809A
		private static IEnumerator LoseAllExhibitsViewer(LoseAllExhibitsAction action)
		{
			UiManager.GetPanel<PlayBoard>().FindActionSourceWorldPosition(action.Source);
			yield return UiManager.GetPanel<SystemBoard>().LoseAllExhibits(action.Args.Exhibits, action.Source);
			yield break;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00009EA9 File Offset: 0x000080A9
		private static IEnumerator PerformViewer(PerformAction action)
		{
			PerformAction.PerformArgs args = action.Args;
			PerformAction.ViewCardArgs viewCardArgs = args as PerformAction.ViewCardArgs;
			if (viewCardArgs == null)
			{
				PerformAction.GunArgs gunArgs = args as PerformAction.GunArgs;
				if (gunArgs == null)
				{
					PerformAction.DollArgs dollArgs = args as PerformAction.DollArgs;
					if (dollArgs == null)
					{
						PerformAction.AnimationArgs animationArgs = args as PerformAction.AnimationArgs;
						if (animationArgs == null)
						{
							PerformAction.SfxArgs sfxArgs = args as PerformAction.SfxArgs;
							if (sfxArgs == null)
							{
								PerformAction.UiSoundArgs uiSoundArgs = args as PerformAction.UiSoundArgs;
								if (uiSoundArgs == null)
								{
									PerformAction.ChatArgs chatArgs = args as PerformAction.ChatArgs;
									if (chatArgs == null)
									{
										PerformAction.SpellArgs spellArgs = args as PerformAction.SpellArgs;
										if (spellArgs == null)
										{
											PerformAction.EffectArgs effectArgs = args as PerformAction.EffectArgs;
											if (effectArgs == null)
											{
												PerformAction.EffectMessageArgs effectMessageArgs = args as PerformAction.EffectMessageArgs;
												if (effectMessageArgs == null)
												{
													PerformAction.SePopArgs sePopArgs = args as PerformAction.SePopArgs;
													if (sePopArgs == null)
													{
														PerformAction.SummonFriendArgs summonFriendArgs = args as PerformAction.SummonFriendArgs;
														if (summonFriendArgs == null)
														{
															PerformAction.TransformModelArgs transformModelArgs = args as PerformAction.TransformModelArgs;
															if (transformModelArgs == null)
															{
																PerformAction.DeathAnimationArgs deathAnimationArgs = args as PerformAction.DeathAnimationArgs;
																if (deathAnimationArgs == null)
																{
																	PerformAction.WaitArgs waitArgs = args as PerformAction.WaitArgs;
																	if (waitArgs == null)
																	{
																		throw new ArgumentOutOfRangeException();
																	}
																	if (waitArgs.Unscale)
																	{
																		yield return new WaitForSecondsRealtime(waitArgs.Time);
																	}
																	else
																	{
																		yield return new WaitForSeconds(waitArgs.Time);
																	}
																}
																else
																{
																	UnitView unit = GameDirector.GetUnit(deathAnimationArgs.Source);
																	if (!unit)
																	{
																		Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + deathAnimationArgs.Source.DebugName);
																		yield break;
																	}
																	unit.DeathAnimation();
																}
															}
															else
															{
																UnitView unit2 = GameDirector.GetUnit(transformModelArgs.Source);
																if (!unit2)
																{
																	Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + transformModelArgs.Source.DebugName);
																	yield break;
																}
																yield return unit2.LoadUnitModelAsync(transformModelArgs.ModelName, false, default(float?));
															}
														}
														else
														{
															UiManager.GetPanel<PlayBoard>().CardUi.PlaySummonEffect(summonFriendArgs.Card);
															yield return new WaitForSeconds(0.5f);
														}
													}
													else
													{
														UnitView unit3 = GameDirector.GetUnit(sePopArgs.Source);
														if (!unit3)
														{
															Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + sePopArgs.Source.DebugName);
															yield break;
														}
														if (unit3.Unit.IsAlive)
														{
															unit3.ShowSePopup(sePopArgs.PopContent, StatusEffectType.Positive, 0, UnitInfoWidget.SePopType.Amulet);
														}
													}
												}
												else
												{
													UnitView unit4 = GameDirector.GetUnit(effectMessageArgs.Source);
													if (!unit4)
													{
														Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + effectMessageArgs.Source.DebugName);
														yield break;
													}
													unit4.SendEffectMessage(effectMessageArgs.EffectName, effectMessageArgs.Message, effectMessageArgs.Args);
												}
											}
											else
											{
												UnitView unit5 = GameDirector.GetUnit(effectArgs.Source);
												if (!unit5)
												{
													Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + effectArgs.Source.DebugName);
													yield break;
												}
												switch (effectArgs.EffectType)
												{
												case PerformAction.EffectBehavior.PlayOneShot:
													unit5.PlayEffectOneShot(effectArgs.EffectName, effectArgs.Delay);
													break;
												case PerformAction.EffectBehavior.Add:
													unit5.PlayEffectLoop(effectArgs.EffectName);
													break;
												case PerformAction.EffectBehavior.Remove:
													unit5.EndEffectLoop(effectArgs.EffectName, true);
													break;
												case PerformAction.EffectBehavior.DieOut:
													unit5.EndEffectLoop(effectArgs.EffectName, false);
													break;
												default:
													throw new ArgumentOutOfRangeException();
												}
												if (!effectArgs.SfxId.IsNullOrEmpty())
												{
													AudioManager.PlaySfxDelay(effectArgs.SfxId, effectArgs.SfxDelay);
												}
												yield return new WaitForSeconds(effectArgs.WaitTime);
											}
										}
										else
										{
											UnitView unit6 = GameDirector.GetUnit(spellArgs.Source);
											if (!unit6)
											{
												Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + spellArgs.Source.DebugName);
												yield break;
											}
											yield return unit6.SpellDeclare(spellArgs.SpellName);
										}
									}
									else
									{
										UnitView unit7 = GameDirector.GetUnit(chatArgs.Source);
										if (!unit7)
										{
											Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + chatArgs.Source.DebugName);
											yield break;
										}
										if (unit7.Unit.IsAlive && !unit7.IsHidden)
										{
											unit7.Chat(chatArgs.Content, chatArgs.ChatTime, chatArgs.Talk ? ((unit7 == GameDirector.Player) ? ChatWidget.CloudType.LeftTalk : ChatWidget.CloudType.RightTalk) : ((unit7 == GameDirector.Player) ? ChatWidget.CloudType.LeftThink : ChatWidget.CloudType.RightThink), chatArgs.Delay);
											yield return new WaitForSecondsRealtime(chatArgs.WaitTime);
										}
									}
								}
								else
								{
									AudioManager.PlayUi(uiSoundArgs.Id, false);
								}
							}
							else
							{
								AudioManager.PlaySfxDelay(sfxArgs.Id, sfxArgs.Delay);
							}
						}
						else
						{
							if (!animationArgs.SfxId.IsNullOrEmpty())
							{
								AudioManager.PlaySfxDelay(animationArgs.SfxId, animationArgs.SfxDelay);
							}
							if (animationArgs.ShakeLevel >= 0)
							{
								GameDirector.Shake(animationArgs.ShakeLevel, true);
							}
							if (animationArgs.AnimationName.IsNullOrEmpty())
							{
								Debug.LogWarning("Action need a animation name.");
								yield break;
							}
							UnitView unit8 = GameDirector.GetUnit(animationArgs.Source);
							if (unit8 == null)
							{
								Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + animationArgs.Source.DebugName);
								yield break;
							}
							unit8.PlayAnimation(animationArgs.AnimationName);
							yield return new WaitForSeconds(animationArgs.WaitTime);
						}
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(dollArgs.DebugString))
						{
							Debug.Log("[PerformViewer] Doll perform: " + dollArgs.DebugString);
						}
						yield return new WaitForSeconds(dollArgs.WaitTime);
					}
				}
				else
				{
					if (gunArgs.GunId.IsNullOrEmpty() || gunArgs.GunId == "Empty")
					{
						Debug.LogWarning("Action need a gunID.");
						yield break;
					}
					if (GunConfig.FromName(gunArgs.GunId) == null)
					{
						Debug.LogError("No such gun named: " + gunArgs.GunId);
						yield break;
					}
					UnitView unit9 = GameDirector.GetUnit(gunArgs.Source);
					UnitView unit10 = GameDirector.GetUnit(gunArgs.Target);
					if (unit9 == null || unit10 == null)
					{
						Debug.LogWarning("[GameDirector] Perform: cannot get source view for " + gunArgs.Source.DebugName + " or target view for " + gunArgs.Target.DebugName);
						yield break;
					}
					unit9.Target = unit10;
					unit9.Targets.Clear();
					unit9.Targets.Add(unit10);
					unit10.ComingDamage = DamageInfo.Attack(0f, false);
					unit9.PerformShoot(gunArgs.GunId);
					yield return new WaitForSeconds(gunArgs.WaitTime);
				}
			}
			else
			{
				yield return UiManager.GetPanel<PlayBoard>().CardUi.ViewCardFromZone(viewCardArgs.Card, viewCardArgs.Zone);
			}
			yield break;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x00009EB8 File Offset: 0x000080B8
		public static void AddDeremyEnvironment(EffectWidget widget)
		{
			GameDirector.DoremyEnvironmentEffects.Add(widget);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00009EC8 File Offset: 0x000080C8
		public static void ClearDoremyEnvironment()
		{
			foreach (EffectWidget effectWidget in GameDirector.DoremyEnvironmentEffects)
			{
				Object.Destroy(effectWidget.gameObject);
			}
			GameDirector.DoremyEnvironmentEffects.Clear();
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00009F28 File Offset: 0x00008128
		private static IEnumerator SpawnViewer(SpawnEnemyAction action)
		{
			EnemyUnit enemyUnit = (EnemyUnit)action.Args.Unit;
			yield return GameDirector.LoadEnemyAsync(enemyUnit, GameDirector.ActiveFormation, false, default(int?)).ToCoroutine(null, null);
			GameDirector.RevealEnemy(enemyUnit, true);
			UnitView view = GameDirector.GetEnemy(enemyUnit);
			view.SetStatusVisible(true, false);
			yield return new WaitForSeconds(action.WaitTime);
			if (GameDirector._enemyMoveOrderVisible)
			{
				GameDirector.UpdateEnemyMoveOrder();
				view.SetMoveOrderVisible(true);
			}
			yield break;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00009F37 File Offset: 0x00008137
		private static IEnumerator ForceKillViewer(ForceKillAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.Args.Target);
			if (unit != null)
			{
				unit.OnForceKill();
			}
			yield break;
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00009F46 File Offset: 0x00008146
		private static IEnumerator EscapeViewer(EscapeAction action)
		{
			Unit unit = action.Args.Unit;
			UnitView unit2 = GameDirector.GetUnit(unit);
			if (unit2 != null)
			{
				if (unit is LoveGirl || unit.HasStatusEffect<DreamServant>())
				{
					yield return unit2.EscapeViewer(1);
				}
				else if (unit is FraudRabbit)
				{
					yield return unit2.EscapeViewer(2);
				}
				else
				{
					yield return unit2.EscapeViewer(0);
				}
				if (unit is LoveGirl)
				{
					yield return new WaitForSecondsRealtime(1.5f);
				}
			}
			GameDirector.UpdateEnemyMoveOrder();
			yield break;
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00009F55 File Offset: 0x00008155
		private static IEnumerator InternalDieViewer(DieEventArgs args)
		{
			Unit unit = args.Unit;
			UnitView unitView = GameDirector.GetUnit(unit);
			if (unitView != null)
			{
				yield return unitView.DieViewer();
			}
			if (unit is EnemyUnit)
			{
				int power = args.Power;
				int bluePoint = args.BluePoint;
				int money = args.Money;
				int num = power;
				int num2 = bluePoint;
				int num3 = money;
				yield return GameDirector.EnemyDeathPoints(unitView.transform, num, num2, num3);
			}
			GameRunController gameRun = unit.GameRun;
			if (gameRun.IsAutoSeed && gameRun.JadeBoxes.Empty<JadeBox>())
			{
				if (unit is Aya && unit.TurnCounter == 0)
				{
					GameMaster.UnlockAchievement(AchievementKey.AyaSpecial);
				}
				else
				{
					if (unit is Youmu)
					{
						LouguanJianSe statusEffect = unit.GetStatusEffect<LouguanJianSe>();
						if (statusEffect != null && statusEffect.TriggerCount == 0)
						{
							GameMaster.UnlockAchievement(AchievementKey.YoumuSpecial);
							goto IL_01A5;
						}
					}
					Doremy doremy = unit as Doremy;
					if (doremy != null && doremy.IsWokeUp)
					{
						GameMaster.UnlockAchievement(AchievementKey.DoremySpecial);
					}
					else if (unit is Sakuya && gameRun.CurrentStage is BambooForest && gameRun.Player.IsExtraTurn)
					{
						GameMaster.UnlockAchievement(AchievementKey.SakuyaSpecial);
					}
				}
			}
			IL_01A5:
			yield break;
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00009F64 File Offset: 0x00008164
		private static IEnumerator MultipleDieViewer(DieAction action)
		{
			List<Coroutine> list = new List<Coroutine>();
			foreach (DieEventArgs dieEventArgs in action.ArgsList)
			{
				if (GameDirector.GetUnit(dieEventArgs.Unit) != null)
				{
					list.Add(Singleton<GameDirector>.Instance.StartCoroutine(GameDirector.InternalDieViewer(dieEventArgs)));
				}
			}
			foreach (Coroutine coroutine in list)
			{
				yield return coroutine;
			}
			List<Coroutine>.Enumerator enumerator = default(List<Coroutine>.Enumerator);
			GameDirector.UpdateEnemyMoveOrder();
			yield break;
			yield break;
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00009F73 File Offset: 0x00008173
		private static IEnumerator EnemyDeathPoints(Transform enemyTrans, int red, int bluePoint, int money)
		{
			int i = red;
			int num = 0;
			int j = bluePoint;
			int num2 = 0;
			while (i > 8)
			{
				i -= 5;
				num++;
			}
			while (j > 8)
			{
				j -= 5;
				num2++;
			}
			for (int k = 0; k < i; k++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.Power,
					TargetPosition = CameraController.ScenePositionToWorldPositionInUI(GameDirector.Player.transform.position)
				}, CameraController.ScenePositionToWorldPositionInUI(enemyTrans.position), UiManager.GetPanel<GameRunVisualPanel>().transform);
			}
			for (int l = 0; l < num; l++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.BigPower,
					TargetPosition = CameraController.ScenePositionToWorldPositionInUI(GameDirector.Player.transform.position)
				}, CameraController.ScenePositionToWorldPositionInUI(enemyTrans.position), UiManager.GetPanel<GameRunVisualPanel>().transform);
			}
			for (int m = 0; m < j; m++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.Blue,
					TargetPosition = GameDirector.Player.transform.position
				}, enemyTrans.position, null);
			}
			for (int n = 0; n < num2; n++)
			{
				EffectManager.CreateEffectBullet(new Point
				{
					Type = Point.PointType.BigBlue,
					TargetPosition = GameDirector.Player.transform.position
				}, enemyTrans.position, null);
			}
			if (money > 0)
			{
				GameRunVisualPanel panel = UiManager.GetPanel<GameRunVisualPanel>();
				UiManager.GetPanel<SystemBoard>().CreateMoneyGainVisual(CameraController.ScenePositionToWorldPositionInUI(enemyTrans.position), money, panel.transform, 1f);
			}
			yield return new WaitForSeconds(1f);
			UiManager.GetPanel<UltimateSkillPanel>().GainPower(red);
			if (money > 0)
			{
				UiManager.GetPanel<SystemBoard>().OnMoneyChanged();
			}
			yield break;
		}

		// Token: 0x060001FD RID: 509 RVA: 0x00009F97 File Offset: 0x00008197
		private static IEnumerator CastBlockShieldViewer(CastBlockShieldAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.Args.Source);
			UnitView unit2 = GameDirector.GetUnit(action.Args.Target);
			if (unit2 == null)
			{
				yield break;
			}
			if (unit2 == unit)
			{
				yield return unit2.GainBlockShieldToSelfViewer(action);
			}
			else
			{
				unit.CastShieldToOther(action.Cast);
				yield return unit2.ReceiveShieldViewer(action);
			}
			yield break;
		}

		// Token: 0x060001FE RID: 510 RVA: 0x00009FA8 File Offset: 0x000081A8
		private static IEnumerator LoseBlockShieldViewer(LoseBlockShieldAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.Args.Target);
			if (!(unit == null))
			{
				return unit.LoseBlockShieldViewer(action);
			}
			return null;
		}

		// Token: 0x060001FF RID: 511 RVA: 0x00009FD8 File Offset: 0x000081D8
		private IEnumerator InstantWinViewer(InstantWinAction action)
		{
			GameDirector.ClearEnemies();
			yield break;
		}

		// Token: 0x06000200 RID: 512 RVA: 0x00009FE0 File Offset: 0x000081E0
		private IEnumerator ApplyStatusEffectViewer(ApplyStatusEffectAction action)
		{
			StatusEffectApplyEventArgs args = action.Args;
			StatusEffect effect = args.Effect;
			UnitView unit = GameDirector.GetUnit(action.Args.Unit);
			if (unit != null && unit.Unit.IsAlive)
			{
				int num = 0;
				if (effect.HasLevel && args.Level != null)
				{
					num = args.Level.Value;
				}
				else if (effect.HasDuration && args.Duration != null)
				{
					num = args.Duration.Value;
				}
				unit.ShowSePopup(effect.Name, effect.Type, num, UnitInfoWidget.SePopType.Add);
				unit.OnAddStatusEffect(args.Effect, args.AddResult.Value);
				if (effect.Config.SFX.IsNullOrEmpty() || effect.Config.SFX == "Default")
				{
					switch (effect.Config.Type)
					{
					case StatusEffectType.Positive:
						AudioManager.PlaySfx("Buff", -1f);
						break;
					case StatusEffectType.Negative:
						AudioManager.PlaySfx("Debuff", -1f);
						break;
					case StatusEffectType.Special:
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				else
				{
					AudioManager.PlaySfx(effect.Config.SFX, -1f);
				}
				if (!effect.Config.VFX.IsNullOrEmpty() && !(effect.Config.VFX == "Default"))
				{
					EffectManager.CreateEffect(effect.Config.VFX, unit.EffectRoot, 0f, default(float?), false, true);
				}
				string unitEffectName = effect.UnitEffectName;
				if (unitEffectName != null)
				{
					StatusEffectAddResult? addResult = args.AddResult;
					StatusEffectAddResult statusEffectAddResult = StatusEffectAddResult.Added;
					if ((addResult.GetValueOrDefault() == statusEffectAddResult) & (addResult != null))
					{
						unit.TryPlayEffectLoop(unitEffectName);
						unit.SendEffectMessage(unitEffectName, "OnPropertyChanged", effect);
						yield return new WaitForSeconds(0.2f);
					}
				}
			}
			yield return new WaitForSeconds(args.WaitTime);
			yield break;
		}

		// Token: 0x06000201 RID: 513 RVA: 0x00009FEF File Offset: 0x000081EF
		private IEnumerator RemoveStatusEffectViewer(RemoveStatusEffectAction action)
		{
			StatusEffectEventArgs args = action.Args;
			StatusEffect effect = args.Effect;
			UnitView unit = GameDirector.GetUnit(action.Args.Unit);
			if (unit != null)
			{
				if (unit.Unit.IsAlive)
				{
					unit.ShowSePopup(effect.Name, effect.Type, 0, UnitInfoWidget.SePopType.Remove);
				}
				unit.OnRemoveStatusEffect(effect);
				string unitEffectName = effect.UnitEffectName;
				if (unitEffectName != null)
				{
					unit.EndEffectLoop(unitEffectName, true);
				}
			}
			yield return new WaitForSeconds(args.WaitTime);
			yield break;
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000202 RID: 514 RVA: 0x00009FFE File Offset: 0x000081FE
		// (set) Token: 0x06000203 RID: 515 RVA: 0x0000A005 File Offset: 0x00008205
		public static float TimeStep { get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000204 RID: 516 RVA: 0x0000A00D File Offset: 0x0000820D
		// (set) Token: 0x06000205 RID: 517 RVA: 0x0000A015 File Offset: 0x00008215
		private float CurrentTime { get; set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000206 RID: 518 RVA: 0x0000A01E File Offset: 0x0000821E
		// (set) Token: 0x06000207 RID: 519 RVA: 0x0000A026 File Offset: 0x00008226
		private float Accumulator { get; set; }

		// Token: 0x06000208 RID: 520 RVA: 0x0000A02F File Offset: 0x0000822F
		private static void SetTimeStep()
		{
			GameDirector.TimeStep = 0.016666668f;
			if ((double)GameDirector.TimeStep <= 0.001)
			{
				Debug.LogError("TimeSteptoo fast!");
			}
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000A058 File Offset: 0x00008258
		private void Update()
		{
			float time = Time.time;
			float num = time - this.CurrentTime;
			if (num > 0.25f)
			{
				num = 0.25f;
			}
			this.CurrentTime = time;
			this.Accumulator += num;
			while (this.Accumulator >= GameDirector.TimeStep)
			{
				this.MasterTick();
				this.Accumulator -= GameDirector.TimeStep;
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000A0C0 File Offset: 0x000082C0
		private void MasterTick()
		{
			if (this.PlayerUnitView)
			{
				this.PlayerUnitView.Tick();
				foreach (DollView dollView in this.PlayerUnitView.Dolls)
				{
					dollView.Tick();
				}
			}
			foreach (UnitView unitView in this.EnemyUnitViews)
			{
				unitView.Tick();
			}
			Singleton<GunManager>.Instance.Tick();
			Singleton<EffectManager>.Instance.Tick();
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000A194 File Offset: 0x00008394
		// Note: this type is marked as 'beforefieldinit'.
		static GameDirector()
		{
			List<int> list = new List<int>();
			list.Add(5);
			list.Add(10);
			list.Add(15);
			list.Add(20);
			list.Add(30);
			GameDirector.ShakeFloors = list;
			GameDirector.DoremyEnvironmentEffects = new List<EffectWidget>();
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000A218 File Offset: 0x00008418
		[CompilerGenerated]
		internal static IEnumerator <ExplodeActionViewer>g__InternalExplodeViewer|76_0(UnitView source, IList<ValueTuple<UnitView, DamageInfo>> targetPairs, string gunName, GunType gunType, Unit explodingUnit, int power, int bluepoint)
		{
			yield return GameDirector.GunShootAction(source, targetPairs, gunName, gunType);
			UnitView unitView = GameDirector.GetUnit(explodingUnit);
			if (unitView != null)
			{
				yield return unitView.DieViewer();
			}
			if (explodingUnit is EnemyUnit)
			{
				yield return GameDirector.EnemyDeathPoints(unitView.transform, power, bluepoint, 0);
			}
			yield break;
		}

		// Token: 0x04000098 RID: 152
		private const string DefaultFormation = "Triangle";

		// Token: 0x04000099 RID: 153
		[SerializeField]
		private Transform unitRoot;

		// Token: 0x0400009A RID: 154
		[SerializeField]
		private Transform playerRoot;

		// Token: 0x0400009B RID: 155
		[SerializeField]
		private GameObject unitPrefab;

		// Token: 0x040000A0 RID: 160
		private static readonly Vector2 PlayerPosition = new Vector2(-3f, 0.5f);

		// Token: 0x040000A1 RID: 161
		private static readonly Vector3 EirinPosition = new Vector3(3f, 0.5f);

		// Token: 0x040000A2 RID: 162
		private static readonly Vector3 KaguyaPosition = new Vector3(4.4f, 0.5f);

		// Token: 0x040000A3 RID: 163
		private static bool _enemyMoveOrderVisible;

		// Token: 0x040000A4 RID: 164
		[SerializeField]
		private Transform lore0;

		// Token: 0x040000A5 RID: 165
		[SerializeField]
		private Transform lore1;

		// Token: 0x040000A7 RID: 167
		private static GunHitArgs _gunHitArgs;

		// Token: 0x040000A8 RID: 168
		private static bool _someoneCrashing;

		// Token: 0x040000A9 RID: 169
		private static readonly List<int> ShakeFloors;

		// Token: 0x040000AB RID: 171
		private const float DefaultShakeScale = 0.08f;

		// Token: 0x040000AC RID: 172
		private static Vector3 _cameraOffset;

		// Token: 0x040000AD RID: 173
		private static readonly List<EffectWidget> DoremyEnvironmentEffects;

		// Token: 0x040000AE RID: 174
		private const float MaxFrameTime = 0.25f;
	}
}
