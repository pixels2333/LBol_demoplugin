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
namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	[UsedImplicitly]
	public sealed class AnimalSpirit : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Value1 > 0)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			using (IEnumerator<Card> enumerator = base.Battle.HandZone.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Card card = enumerator.Current;
					List<AnimalSpirit.AnimalType> list = Enumerable.ToList<AnimalSpirit.AnimalType>(this._types);
					if (card.IsEcho || !card.CanBeDuplicated)
					{
						list.Remove(AnimalSpirit.AnimalType.Wolf);
					}
					if (card.IsReplenish)
					{
						list.Remove(AnimalSpirit.AnimalType.Otter);
					}
					if (card.Cost == base.Mana)
					{
						list.Remove(AnimalSpirit.AnimalType.Eagle);
					}
					if (list.Count > 0)
					{
						switch (list.Sample(base.BattleRng))
						{
						case AnimalSpirit.AnimalType.Wolf:
							card.IsEcho = true;
							break;
						case AnimalSpirit.AnimalType.Otter:
							card.IsReplenish = true;
							break;
						case AnimalSpirit.AnimalType.Eagle:
							card.SetTurnCost(base.Mana);
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
					}
				}
				yield break;
			}
			yield break;
		}
		public AnimalSpirit()
		{
			List<AnimalSpirit.AnimalType> list = new List<AnimalSpirit.AnimalType>();
			list.Add(AnimalSpirit.AnimalType.Wolf);
			list.Add(AnimalSpirit.AnimalType.Otter);
			list.Add(AnimalSpirit.AnimalType.Eagle);
			this._types = list;
			base..ctor();
		}
		private readonly List<AnimalSpirit.AnimalType> _types;
		private enum AnimalType
		{
			Wolf,
			Otter,
			Eagle
		}
	}
}
