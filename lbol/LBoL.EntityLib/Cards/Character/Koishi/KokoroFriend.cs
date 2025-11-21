using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000480 RID: 1152
	[UsedImplicitly]
	public sealed class KokoroFriend : Card
	{
		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000F6C RID: 3948 RVA: 0x0001B9BD File Offset: 0x00019BBD
		[UsedImplicitly]
		public override FriendCostInfo FriendU
		{
			get
			{
				return new FriendCostInfo(base.UltimateCost, FriendCostType.Active);
			}
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x0001B9CB File Offset: 0x00019BCB
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06000F6E RID: 3950 RVA: 0x0001B9D3 File Offset: 0x00019BD3
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
				yield return new DreamCardsAction(base.Value1, 0);
				num = i;
			}
			yield break;
		}

		// Token: 0x06000F6F RID: 3951 RVA: 0x0001B9E3 File Offset: 0x00019BE3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			}
			else if (((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active2)
			{
				base.Loyalty += base.ActiveCost2;
				yield return base.SkillAnime;
				yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				yield return base.SkillAnime;
				yield return base.BuffAction<MoodEpiphany>(0, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
