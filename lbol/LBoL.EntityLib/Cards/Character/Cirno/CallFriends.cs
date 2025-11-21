using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Character.Cirno.Friend;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x0200049C RID: 1180
	[UsedImplicitly]
	public sealed class CallFriends : Card
	{
		// Token: 0x06000FC4 RID: 4036 RVA: 0x0001C0FE File Offset: 0x0001A2FE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> cards = Enumerable.ToList<Card>(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyFriend, false), base.Value1, (CardConfig config) => config.Cost.Amount < 5));
			if (this.IsUpgraded)
			{
				Card card;
				List<Type> list = Enumerable.ToList<Type>(Enumerable.Where<Type>(this._types, (Type type) => Enumerable.All<Card>(cards, (Card card) => card.GetType() != type)));
				if (list.Count > 0)
				{
					list.Shuffle(base.GameRun.BattleRng);
					cards.Add(Library.CreateCard(Enumerable.First<Type>(list)));
				}
			}
			foreach (Card card in cards)
			{
				Card card;
				card.Summon();
			}
			if (cards.Count > 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(cards, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card selectedCard = interaction.SelectedCard;
				yield return new AddCardsToHandAction(new Card[] { selectedCard });
				interaction = null;
			}
			yield break;
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x0001C110 File Offset: 0x0001A310
		public CallFriends()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(SunnyFriend));
			list.Add(typeof(LunaFriend));
			list.Add(typeof(StarFriend));
			list.Add(typeof(ClownpieceFriend));
			this._types = list;
			base..ctor();
		}

		// Token: 0x0400010B RID: 267
		private readonly List<Type> _types;
	}
}
