using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002EE RID: 750
	[UsedImplicitly]
	public sealed class ModuoluoSeasons : Card
	{
		// Token: 0x06000B33 RID: 2867 RVA: 0x000169DE File Offset: 0x00014BDE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Value1 > 0)
			{
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			}
			int total = base.Battle.CardsToFull;
			List<Type> types = Enumerable.ToList<Type>(this._types);
			while (total > 0)
			{
				int num = Math.Min(5, total);
				total -= num;
				List<Card> list = new List<Card>();
				for (int i = 0; i < num; i++)
				{
					types.Shuffle(base.GameRun.BattleCardRng);
					list.Add(Library.CreateCard(types[0]));
				}
				yield return new AddCardsToHandAction(list, AddCardsType.OneByOne);
			}
			yield break;
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x000169F0 File Offset: 0x00014BF0
		public ModuoluoSeasons()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(UManaCard));
			list.Add(typeof(RManaCard));
			list.Add(typeof(SummerFlower));
			this._types = list;
			base..ctor();
		}

		// Token: 0x040000F6 RID: 246
		private readonly List<Type> _types;
	}
}
