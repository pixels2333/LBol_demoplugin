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
	// Token: 0x02000432 RID: 1074
	[UsedImplicitly]
	public sealed class MoguMaster : Card
	{
		// Token: 0x06000EAC RID: 3756 RVA: 0x0001AC79 File Offset: 0x00018E79
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

		// Token: 0x06000EAD RID: 3757 RVA: 0x0001AC8C File Offset: 0x00018E8C
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

		// Token: 0x04000109 RID: 265
		private readonly List<Type> _types;
	}
}
