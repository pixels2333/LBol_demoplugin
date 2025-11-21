using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002BB RID: 699
	[UsedImplicitly]
	public sealed class YinyangQueen : Card
	{
		// Token: 0x06000AB9 RID: 2745 RVA: 0x00016101 File Offset: 0x00014301
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x00016109 File Offset: 0x00014309
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
				yield return base.DefenseAction(base.Value2, base.Value2, BlockShieldType.Direct, false);
				num = i;
			}
			yield break;
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x00016119 File Offset: 0x00014319
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				List<Card> list = new List<Card>();
				this._types.Shuffle(base.GameRun.BattleCardRng);
				for (int i = 0; i < base.Value1; i++)
				{
					list.Add(Library.CreateCard(this._types[i]));
				}
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				yield return base.SpellAnime;
				yield return base.BuffAction<YinyangQueenSe>(1, 0, 0, 0, 0.2f);
				List<Card> list2 = new List<Card>();
				this._types.Shuffle(base.GameRun.BattleCardRng);
				for (int j = 0; j < base.Value1; j++)
				{
					list2.Add(Library.CreateCard(this._types[j]));
				}
				yield return new AddCardsToHandAction(list2, AddCardsType.Normal);
			}
			yield break;
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x00016130 File Offset: 0x00014330
		public YinyangQueen()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(YinyangCard));
			list.Add(typeof(ShuihuoCard));
			list.Add(typeof(FengleiCard));
			list.Add(typeof(ShanyuCard));
			this._types = list;
			base..ctor();
		}

		// Token: 0x040000F1 RID: 241
		private readonly List<Type> _types;
	}
}
