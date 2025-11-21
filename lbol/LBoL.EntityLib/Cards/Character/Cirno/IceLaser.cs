using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004C4 RID: 1220
	[UsedImplicitly]
	public sealed class IceLaser : Card
	{
		// Token: 0x06001032 RID: 4146 RVA: 0x0001CB42 File Offset: 0x0001AD42
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageDealt), (GameEventPriority)0);
			this._coldingEnemies = new List<Unit>();
		}

		// Token: 0x06001033 RID: 4147 RVA: 0x0001CB74 File Offset: 0x0001AD74
		private void OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (args.ActionSource == this && !args.DamageInfo.IsGrazed)
			{
				Unit target = args.Target;
				this._coldingEnemies.Add(target);
			}
		}

		// Token: 0x06001034 RID: 4148 RVA: 0x0001CBAD File Offset: 0x0001ADAD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (this._coldingEnemies.Count > 0)
			{
				foreach (Unit unit in this._coldingEnemies)
				{
					if (unit.IsAlive)
					{
						yield return base.DebuffAction<Cold>(unit, 0, 0, 0, 0, true, 0.03f);
					}
				}
				List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
				this._coldingEnemies.Clear();
			}
			EnemyUnit target = Enumerable.Where<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy != selector.SelectedEnemy).SampleOrDefault(base.GameRun.BattleRng);
			if (target == null)
			{
				yield break;
			}
			Card card = Enumerable.LastOrDefault<Card>(base.Battle.DrawZone);
			if (card == null)
			{
				yield break;
			}
			yield return PerformAction.ViewCard(card);
			if (!Enumerable.Contains<ManaColor>(card.Config.Colors, ManaColor.Blue))
			{
				yield break;
			}
			yield return base.AttackAction(target);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (this._coldingEnemies.Count > 0)
			{
				foreach (Unit unit2 in this._coldingEnemies)
				{
					if (unit2.IsAlive)
					{
						yield return base.DebuffAction<Cold>(unit2, 0, 0, 0, 0, true, 0.03f);
					}
				}
				List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
				this._coldingEnemies.Clear();
			}
			yield break;
			yield break;
		}

		// Token: 0x0400010F RID: 271
		private List<Unit> _coldingEnemies;
	}
}
