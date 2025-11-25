using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
namespace LBoL.Core.Battle.BattleActions
{
	public class FollowAttackAction : EventBattleAction<FollowAttackEventArgs>
	{
		public FollowAttackAction(UnitSelector sourceSelector, bool randomFiller = false)
		{
			base.Args = new FollowAttackEventArgs
			{
				SourceSelector = sourceSelector,
				Count = 1,
				RandomFiller = randomFiller
			};
		}
		public FollowAttackAction(UnitSelector sourceSelector, int count, bool randomFiller = false)
		{
			base.Args = new FollowAttackEventArgs
			{
				SourceSelector = sourceSelector,
				Count = count,
				RandomFiller = randomFiller
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<FollowAttackEventArgs>("FollowAttacking", base.Args, base.Battle.FollowAttacking);
			int num;
			for (int i = 0; i < base.Args.Count; i = num)
			{
				Card card3 = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone, (Card card) => card.IsFollowCard);
				if (card3 != null)
				{
					FollowAttackAction.<>c__DisplayClass2_0 CS$<>8__locals1 = new FollowAttackAction.<>c__DisplayClass2_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.play = new PlayCardAction(card3, base.Args.SourceSelector);
					yield return base.CreatePhase("FollowAttack", delegate
					{
						CS$<>8__locals1.<>4__this.React(CS$<>8__locals1.play, null, default(ActionCause?));
					}, false);
					base.Args.Cards.Add(CS$<>8__locals1.play.Args.Card);
					CS$<>8__locals1 = null;
				}
				else if (base.Args.RandomFiller)
				{
					Card card2 = Enumerable.FirstOrDefault<Card>(base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), 1, (CardConfig config) => (config.Keywords & Keyword.FollowCard) > Keyword.None));
					if (card2 != null)
					{
						FollowAttackAction.<>c__DisplayClass2_1 CS$<>8__locals2 = new FollowAttackAction.<>c__DisplayClass2_1();
						CS$<>8__locals2.<>4__this = this;
						card2.IsPlayTwiceToken = true;
						CS$<>8__locals2.play = new PlayCardAction(card2, base.Args.SourceSelector);
						yield return base.CreatePhase("FollowAttack", delegate
						{
							CS$<>8__locals2.<>4__this.React(CS$<>8__locals2.play, null, default(ActionCause?));
						}, false);
						base.Args.Cards.Add(CS$<>8__locals2.play.Args.Card);
						CS$<>8__locals2 = null;
					}
				}
				else if (base.Battle.FollowAttackFillerLevel > 0)
				{
					FollowAttackAction.<>c__DisplayClass2_2 CS$<>8__locals3 = new FollowAttackAction.<>c__DisplayClass2_2();
					CS$<>8__locals3.<>4__this = this;
					CS$<>8__locals3.play = new PlayCardAction(Library.CreateCard<FollowAttackFiller>(), base.Args.SourceSelector);
					yield return base.CreatePhase("FollowAttack", delegate
					{
						CS$<>8__locals3.<>4__this.React(CS$<>8__locals3.play, null, default(ActionCause?));
					}, false);
					base.Args.Cards.Add(CS$<>8__locals3.play.Args.Card);
					CS$<>8__locals3 = null;
				}
				num = i + 1;
			}
			if (base.Args.Cards.Count > 0)
			{
				yield return base.CreatePhase("Record", delegate
				{
					base.Battle.RecordCardFollowAttack(base.Args.Cards);
				}, false);
			}
			yield return base.CreateEventPhase<FollowAttackEventArgs>("FollowAttacked", base.Args, base.Battle.FollowAttacked);
			yield break;
		}
	}
}
