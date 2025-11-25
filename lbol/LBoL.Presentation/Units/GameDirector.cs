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
	public class GameDirector : Singleton<GameDirector>
	{
		private Dictionary<string, EnemyFormation> Formations { get; set; }
		private static string ActiveFormation { get; set; }
		public UnitView PlayerUnitView { get; private set; }
		private List<UnitView> EnemyUnitViews { get; } = new List<UnitView>();
		public static UnitView Player
		{
			get
			{
				return Singleton<GameDirector>.Instance.PlayerUnitView;
			}
		}
		public static IReadOnlyList<UnitView> Enemies
		{
			get
			{
				return Singleton<GameDirector>.Instance.EnemyUnitViews;
			}
		}
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
		public static void MovePlayer(Vector2 v2)
		{
			if (Singleton<GameDirector>.Instance.PlayerUnitView == null)
			{
				Debug.LogError("There's no PlayerUnit in UnitDirector.");
				return;
			}
			Singleton<GameDirector>.Instance.playerRoot.localPosition = v2;
		}
		public static UniTask<UnitView> LoadPlayerAsync(PlayerUnit player, bool hidden = false)
		{
			return Singleton<GameDirector>.Instance.InternalLoadPlayerAsync(player, hidden);
		}
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
		public static void ClearEnemies()
		{
			Singleton<GameDirector>.Instance.InternalClearEnemies();
		}
		public static void ClearAll()
		{
			Singleton<GameDirector>.Instance.InternalClearAll();
		}
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
		private void InternalClearEnemies()
		{
			foreach (UnitView unitView in this.EnemyUnitViews)
			{
				Object.Destroy(unitView.gameObject);
			}
			this.EnemyUnitViews.Clear();
		}
		private void InternalClearAll()
		{
			GameDirector.StopLoreChat();
			Object.Destroy(this.PlayerUnitView.gameObject);
			this.PlayerUnitView = null;
			this.InternalClearEnemies();
			GameDirector.ClearDoremyEnvironment();
		}
		public static void HidePlayer()
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.IsHidden = true;
			}
		}
		public static void HideAll()
		{
			GameDirector.HidePlayer();
			foreach (UnitView unitView in Singleton<GameDirector>.Instance.EnemyUnitViews)
			{
				unitView.IsHidden = true;
			}
		}
		public static void RevealPlayer(bool withStatus)
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.Show(withStatus);
			}
		}
		public static void RevealAll(bool withStatus)
		{
			GameDirector.RevealPlayer(withStatus);
			GameDirector.RevealAllEnemies(withStatus);
		}
		public static void RevealAllEnemies(bool withStatus)
		{
			foreach (UnitView unitView in Singleton<GameDirector>.Instance.EnemyUnitViews)
			{
				unitView.Show(withStatus);
			}
		}
		public static void RevealEnemy(int index, bool withStatus)
		{
			UnitView enemy = GameDirector.GetEnemy(index);
			if (enemy != null)
			{
				enemy.Show(withStatus);
			}
		}
		public static void RevealEnemy(EnemyUnit enemy, bool withStatus)
		{
			UnitView enemy2 = GameDirector.GetEnemy(enemy);
			if (enemy2 != null)
			{
				enemy2.Show(withStatus);
			}
		}
		public static void FadeInEnemyStatus()
		{
			foreach (UnitView unitView in Singleton<GameDirector>.Instance.EnemyUnitViews)
			{
				unitView.SetStatusVisible(true, false);
			}
		}
		public static void FadeInPlayerStatus()
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.SetStatusVisible(true, false);
			}
		}
		public static void FadeOutPlayerStatus()
		{
			UnitView playerUnitView = Singleton<GameDirector>.Instance.PlayerUnitView;
			if (playerUnitView)
			{
				playerUnitView.SetStatusVisible(false, false);
			}
		}
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
		public static UnitView GetEnemy(int index)
		{
			return Singleton<GameDirector>.Instance.EnemyUnitViews.TryGetValue(index);
		}
		public static UnitView GetEnemyByRootIndex(int rootIndex)
		{
			return Enumerable.FirstOrDefault<UnitView>(Singleton<GameDirector>.Instance.EnemyUnitViews, (UnitView enemy) => enemy.RootIndex == rootIndex);
		}
		public static UnitView GetEnemy(EnemyUnit enemy)
		{
			return Enumerable.FirstOrDefault<UnitView>(Singleton<GameDirector>.Instance.EnemyUnitViews, (UnitView e) => e.Unit == enemy);
		}
		public static void PlayerDebutAnimation()
		{
			GameDirector.Player.DebutAnimation();
		}
		public static void EnemyDebutAnimation(int index)
		{
			UnitView enemy = GameDirector.GetEnemy(index);
			if (enemy == null)
			{
				return;
			}
			enemy.DebutAnimation();
		}
		public static void EnemyDebutAnimation(EnemyUnit enemy)
		{
			UnitView enemy2 = GameDirector.GetEnemy(enemy);
			if (enemy2 == null)
			{
				return;
			}
			enemy2.DebutAnimation();
		}
		public static void AllEnemiesDebutAnimation()
		{
			foreach (UnitView unitView in GameDirector.Enemies)
			{
				unitView.DebutAnimation();
			}
		}
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
		private static Coroutine LoreChatRunner { get; set; }
		public static void DebutChat(bool hasKaguya, string playerName)
		{
			GameDirector.LoreChatRunner = Singleton<GameDirector>.Instance.StartCoroutine(GameDirector.DebutFlow(hasKaguya, playerName));
		}
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
		public static void ShopChat()
		{
			GameDirector.LoreChatRunner = Singleton<GameDirector>.Instance.StartCoroutine(GameDirector.ShopFlow());
		}
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
		public static void StopLoreChat()
		{
			if (GameDirector.LoreChatRunner != null)
			{
				Singleton<GameDirector>.Instance.StopCoroutine(GameDirector.LoreChatRunner);
			}
			GameDirector.LoreChatRunner = null;
		}
		private static IEnumerator WaitCoroutineViewer(WaitForCoroutineAction action)
		{
			yield return action.Coroutine;
			yield break;
		}
		private static IEnumerator WaitYieldInstructionViewer(WaitForYieldInstructionAction action)
		{
			yield return action.Instruction;
			yield break;
		}
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
		public static void OnGunHit()
		{
			GameDirector.GunHitPresentation(GameDirector._gunHitArgs);
		}
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
		private static IEnumerator CrashingTimer()
		{
			GameDirector._someoneCrashing = true;
			yield return new WaitForSeconds(0.3f);
			GameDirector._someoneCrashing = false;
			yield break;
		}
		private static int ShakingDamage { get; set; }
		private static void ShakeWithDamage(float damage, float shakePower)
		{
			GameDirector.ShakeWithDamage((damage * shakePower).ToInt());
		}
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
		private static IEnumerator EndShootActionViewer(EndShootAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.SourceUnit);
			if (unit != null)
			{
				yield return unit.ForceEndShoot();
			}
			yield break;
		}
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
		private static IEnumerator LoseAllExhibitsViewer(LoseAllExhibitsAction action)
		{
			UiManager.GetPanel<PlayBoard>().FindActionSourceWorldPosition(action.Source);
			yield return UiManager.GetPanel<SystemBoard>().LoseAllExhibits(action.Args.Exhibits, action.Source);
			yield break;
		}
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
		public static void AddDeremyEnvironment(EffectWidget widget)
		{
			GameDirector.DoremyEnvironmentEffects.Add(widget);
		}
		public static void ClearDoremyEnvironment()
		{
			foreach (EffectWidget effectWidget in GameDirector.DoremyEnvironmentEffects)
			{
				Object.Destroy(effectWidget.gameObject);
			}
			GameDirector.DoremyEnvironmentEffects.Clear();
		}
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
		private static IEnumerator ForceKillViewer(ForceKillAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.Args.Target);
			if (unit != null)
			{
				unit.OnForceKill();
			}
			yield break;
		}
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
		private static IEnumerator LoseBlockShieldViewer(LoseBlockShieldAction action)
		{
			UnitView unit = GameDirector.GetUnit(action.Args.Target);
			if (!(unit == null))
			{
				return unit.LoseBlockShieldViewer(action);
			}
			return null;
		}
		private IEnumerator InstantWinViewer(InstantWinAction action)
		{
			GameDirector.ClearEnemies();
			yield break;
		}
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
		public static float TimeStep { get; set; }
		private float CurrentTime { get; set; }
		private float Accumulator { get; set; }
		private static void SetTimeStep()
		{
			GameDirector.TimeStep = 0.016666668f;
			if ((double)GameDirector.TimeStep <= 0.001)
			{
				Debug.LogError("TimeSteptoo fast!");
			}
		}
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
		private const string DefaultFormation = "Triangle";
		[SerializeField]
		private Transform unitRoot;
		[SerializeField]
		private Transform playerRoot;
		[SerializeField]
		private GameObject unitPrefab;
		private static readonly Vector2 PlayerPosition = new Vector2(-3f, 0.5f);
		private static readonly Vector3 EirinPosition = new Vector3(3f, 0.5f);
		private static readonly Vector3 KaguyaPosition = new Vector3(4.4f, 0.5f);
		private static bool _enemyMoveOrderVisible;
		[SerializeField]
		private Transform lore0;
		[SerializeField]
		private Transform lore1;
		private static GunHitArgs _gunHitArgs;
		private static bool _someoneCrashing;
		private static readonly List<int> ShakeFloors;
		private const float DefaultShakeScale = 0.08f;
		private static Vector3 _cameraOffset;
		private static readonly List<EffectWidget> DoremyEnvironmentEffects;
		private const float MaxFrameTime = 0.25f;
	}
}
