using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004D9 RID: 1241
	[UsedImplicitly]
	public sealed class LarvaFriend : Card
	{
		// Token: 0x0600106E RID: 4206 RVA: 0x0001CF84 File Offset: 0x0001B184
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x0001CF8C File Offset: 0x0001B18C
		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!base.Summoned || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			base.Loyalty += base.PassiveCost;
			int num;
			for (int i = 0; i < base.Battle.FriendPassiveTimes; i = num + 1)
			{
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				yield return PerformAction.Sfx("FairySupport", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "LarvaFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.DebuffAction<Poison>(base.Battle.RandomAliveEnemy, base.Value1, 0, 0, 0, true, 0.2f);
				num = i;
			}
			yield break;
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x0001CF9C File Offset: 0x0001B19C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return base.UpgradeAllHandsAction();
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				yield return PerformAction.Gun(base.Battle.Player, Enumerable.FirstOrDefault<EnemyUnit>(base.Battle.AllAliveEnemies), "LarvaUlt", 1f);
				foreach (BattleAction battleAction in base.DebuffAction<Poison>(base.Battle.AllAliveEnemies, base.Value2, 0, 0, 0, true, 0.2f))
				{
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
				foreach (EnemyUnit enemyUnit in Enumerable.Where<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.HasStatusEffect<Poison>()))
				{
					if (base.Battle.BattleShouldEnd)
					{
						yield break;
					}
					foreach (BattleAction battleAction2 in enemyUnit.GetStatusEffect<Poison>().TakeEffect())
					{
						yield return battleAction2;
					}
					enumerator = null;
				}
				IEnumerator<EnemyUnit> enumerator2 = null;
			}
			yield break;
			yield break;
		}
	}
}
