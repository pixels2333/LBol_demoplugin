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
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class MoguMaster : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list = new List<Card>();
			this._types.Shuffle(base.GameRun.BattleCardRng);
			for (int i = 0; i < base.Value1; i++)
			{
				list.Add(Library.CreateCard(this._types[i]));
			}
			SelectCardInteraction interaction = new SelectCardInteraction(base.Value2, base.Value2, list, SelectedCardHandling.DoNothing)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			yield return new AddCardsToHandAction(interaction.SelectedCards, AddCardsType.Normal);
			yield break;
		}
		public MoguMaster()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(RedMogu));
			list.Add(typeof(BlueMogu));
			list.Add(typeof(GreenMogu));
			list.Add(typeof(PurpleMogu));
			this._types = list;
			base..ctor();
		}
		private readonly List<Type> _types;
	}
}
