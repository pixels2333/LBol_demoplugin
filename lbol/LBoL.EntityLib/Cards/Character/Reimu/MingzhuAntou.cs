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
	// Token: 0x020003E1 RID: 993
	[UsedImplicitly]
	public sealed class MingzhuAntou : Card
	{
		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000DE9 RID: 3561 RVA: 0x00019E04 File Offset: 0x00018004
		protected override ManaGroup AdditionalCost
		{
			get
			{
				return base.Mana * -this.YinyangCount;
			}
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000DEA RID: 3562 RVA: 0x00019E18 File Offset: 0x00018018
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

		// Token: 0x06000DEB RID: 3563 RVA: 0x00019E53 File Offset: 0x00018053
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

		// Token: 0x06000DEC RID: 3564 RVA: 0x00019E6C File Offset: 0x0001806C
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

		// Token: 0x04000107 RID: 263
		private readonly List<Type> _types;
	}
}
