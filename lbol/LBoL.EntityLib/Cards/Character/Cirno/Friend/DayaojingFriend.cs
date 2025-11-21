using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno.Friend
{
	// Token: 0x020004D8 RID: 1240
	[UsedImplicitly]
	public sealed class DayaojingFriend : Card
	{
		// Token: 0x0600106A RID: 4202 RVA: 0x0001CF4D File Offset: 0x0001B14D
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x0001CF55 File Offset: 0x0001B155
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
				yield return PerformAction.Effect(base.Battle.Player, "DaiyoFairy", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.AttackAction(base.Battle.RandomAliveEnemy);
				num = i;
			}
			yield break;
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x0001CF65 File Offset: 0x0001B165
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.DefenseAction(true);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				yield return base.SkillAnime;
				yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value1, this.IsUpgraded), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
