using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class MingzhuAntou : Card
	{
		protected override ManaGroup AdditionalCost
		{
			get
			{
				return base.Mana * -this.YinyangCount;
			}
		}
		private int YinyangCount
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return Enumerable.Count<Card>(base.Battle.EnumerateAllCards(), (Card card) => card is YinyangCardBase);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> list = new List<Card>();
			this._types.Shuffle(base.GameRun.BattleCardRng);
			for (int i = 0; i < base.Value1; i++)
			{
				list.Add(Library.CreateCard(this._types[i]));
			}
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, false, false, false)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			Card selectedCard = interaction.SelectedCard;
			yield return new AddCardsToHandAction(new Card[] { selectedCard });
			yield break;
		}
		public MingzhuAntou()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(YinyangCard));
			list.Add(typeof(ShuihuoCard));
			list.Add(typeof(FengleiCard));
			list.Add(typeof(ShanyuCard));
			this._types = list;
			base..ctor();
		}
		private readonly List<Type> _types;
	}
}
