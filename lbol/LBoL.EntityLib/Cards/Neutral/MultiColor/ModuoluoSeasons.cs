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
	[UsedImplicitly]
	public sealed class ModuoluoSeasons : Card
	{
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
		public ModuoluoSeasons()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(UManaCard));
			list.Add(typeof(RManaCard));
			list.Add(typeof(SummerFlower));
			this._types = list;
			base..ctor();
		}
		private readonly List<Type> _types;
	}
}
